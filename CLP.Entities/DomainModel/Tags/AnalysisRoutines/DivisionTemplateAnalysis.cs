using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CLP.Entities
{
    public static class DivisionTemplateAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            // First, clear out any old DivisionTemplateTags generated via Analysis.
            foreach(var tag in page.Tags.ToList().Where(tag => tag is DivisionTemplateInterpretedCorrectnessTag || tag is DivisionTemplateStrategyTag || tag is DivisionTemplateCompletenessTag))
            {
                page.Tags.Remove(tag);
            }

            var productDefinitionTags = page.Tags.OfType<ProductDefinitionTag>().ToList();
            var divisionTemplates = page.PageObjects.OfType<FuzzyFactorCard>().ToList();
            if(!productDefinitionTags.Any() ||
               !divisionTemplates.Any())
            {
                return;
            }

            foreach(var productDefinitionTag in productDefinitionTags)
            {
                foreach(var divisionTemplate in divisionTemplates)
                {
                    InterpretStrategy(page, divisionTemplate);
                    InterpretCorrectness(page, productDefinitionTag, divisionTemplate);
                }
            }
        }

        public static void AnalyzeHistory(CLPPage page)
        {
            
        }

        public static void InterpretStrategy(CLPPage page, FuzzyFactorCard divisionTemplate)
        {
            var dividerValues = divisionTemplate.VerticalDivisions.Select(x => x.Value).ToList();

            if(!dividerValues.Any())
            {
                return;
            }

            if(dividerValues.Count == 2)
            {
                page.Tags.Add(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.OneArray, dividerValues));
                return;
            }

            if(Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            {
                page.Tags.Add(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.EvenSplit, dividerValues));
                return;
            }


            // HACK - This only compares the first 2 values to see if they are the same to determine Repeated Strategy. Find a way to determine this by frequency.
            if(dividerValues.First() == dividerValues.ElementAt(1))
            {
                page.Tags.Add(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.Repeated, dividerValues));
                return;
            }

            page.Tags.Add(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.Other, dividerValues));
        }

        public static void InterpretCorrectness(CLPPage page, ProductDefinitionTag productDefinition, FuzzyFactorCard divisionTemplate)
        {
            // Apply a Completeness tag.
            var isDivisionTemplateComplete = false;
            if(divisionTemplate.VerticalDivisions.Count < 2)
            {
                var tag = new DivisionTemplateCompletenessTag(page, Origin.StudentPageGenerated, DivisionTemplateCompletenessTag.AcceptedValues.NoArrays);
                page.Tags.Add(tag);
            }
            else if(divisionTemplate.VerticalDivisions.Sum(x => x.Value) == divisionTemplate.Columns)
            {
                var tag = new DivisionTemplateCompletenessTag(page, Origin.StudentPageGenerated, DivisionTemplateCompletenessTag.AcceptedValues.Complete);
                page.Tags.Add(tag);
                isDivisionTemplateComplete = true;
            }
            else
            {
                var tag = new DivisionTemplateCompletenessTag(page, Origin.StudentPageGenerated, DivisionTemplateCompletenessTag.AcceptedValues.NotEnoughArrays);
                page.Tags.Add(tag);
            }

            // Apply a Correctness tag.
            var incorrectReasons = new List<DivisionTemplateIncorrectReason>();

            // Correct
            if(productDefinition.Product == divisionTemplate.Dividend &&
               isDivisionTemplateComplete &&
               ((productDefinition.FirstFactor == divisionTemplate.Rows && productDefinition.UngivenProductPart != ProductPart.FirstFactor) ||
               (productDefinition.SecondFactor == divisionTemplate.Rows && productDefinition.UngivenProductPart != ProductPart.SecondFactor)))
            {
                var correctTag = new DivisionTemplateInterpretedCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, incorrectReasons);
                page.Tags.Add(correctTag);
            }

            // Incorrect
            if(!isDivisionTemplateComplete)
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.Incomplete);
            }
            
            if(productDefinition.Product != divisionTemplate.Dividend)
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.WrongProduct);
            }

            if((productDefinition.FirstFactor == divisionTemplate.Rows && productDefinition.UngivenProductPart != ProductPart.FirstFactor) ||
               (productDefinition.SecondFactor == divisionTemplate.Rows && productDefinition.UngivenProductPart != ProductPart.SecondFactor))
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.WrongFactor);
            }

            if(!incorrectReasons.Any())
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.Other);
            }

            var incorrectTag = new DivisionTemplateInterpretedCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Incorrect, incorrectReasons);
            page.Tags.Add(incorrectTag);
        }
    }
}