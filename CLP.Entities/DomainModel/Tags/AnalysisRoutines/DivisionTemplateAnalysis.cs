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
                page.RemoveTag(tag);
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

        private class DivisionTemplateAndRemainder
        {
            public DivisionTemplateAndRemainder(FuzzyFactorCard divisionTemplate, int remainder)
            {
                DivisionTemplate = divisionTemplate;
                Remainder = remainder;
            }

            public FuzzyFactorCard DivisionTemplate;
            public int Remainder;
        }

        public static void AnalyzeHistory(CLPPage page)
        {
            var completeOrderedHistory = page.History.UndoItems.Reverse().Concat(page.History.RedoItems).ToList();

            //DivisionTemplateDeletedTag
            foreach(var historyItem in completeOrderedHistory.OfType<PageObjectsRemovedHistoryItem>())
            {
                foreach(var pageObjectID in historyItem.PageObjectIDs)
                {
                    var divisionTemplate = page.GetPageObjectByID(pageObjectID) as FuzzyFactorCard ?? page.History.GetPageObjectByID(pageObjectID) as FuzzyFactorCard;
                    if(divisionTemplate == null)
                    {
                        continue;
                    }

                    page.AddTag(new DivisionTemplateDeletedTag(page, Origin.StudentPageObjectGenerated, divisionTemplate.Dividend, divisionTemplate.Rows));
                }
            }

            //DivisionTemplateIncorrectArrayCreationTag
            var divisionTemplatesOnPage = new List<DivisionTemplateAndRemainder>();
            var arraysOnPage = new List<CLPArray>();
            foreach(var historyItem in completeOrderedHistory)
            {
                var removedPageObjectsHistoryItem = historyItem as PageObjectsRemovedHistoryItem;
                if(removedPageObjectsHistoryItem != null)
                {
                    foreach(var pageObjectID in removedPageObjectsHistoryItem.PageObjectIDs)
                    {
                        divisionTemplatesOnPage.RemoveAll(x => x.DivisionTemplate.ID == pageObjectID);
                        arraysOnPage.RemoveAll(x => x.ID == pageObjectID);
                    }
                    continue;
                }

                var arraySnappedInHistoryItem = historyItem as FFCArraySnappedInHistoryItem;
                if(arraySnappedInHistoryItem != null)
                {
                    var arrayToRemove = arraysOnPage.FirstOrDefault(x => x.ID == arraySnappedInHistoryItem.SnappedInArrayID);
                    var divisionTemplateAndRemainder = divisionTemplatesOnPage.FirstOrDefault(x => x.DivisionTemplate.ID == arraySnappedInHistoryItem.FuzzyFactorCardID);
                    if(divisionTemplateAndRemainder != null &&
                       arrayToRemove != null)
                    {
                        arraysOnPage.Remove(arrayToRemove);
                        divisionTemplateAndRemainder.Remainder -= (arrayToRemove.Rows * arrayToRemove.Columns);
                    }
                    continue;
                }

                var addedPageObjectHistoryItem = historyItem as PageObjectsAddedHistoryItem;
                if(addedPageObjectHistoryItem == null)
                {
                    continue;
                }

                foreach(var pageObjectID in addedPageObjectHistoryItem.PageObjectIDs)
                {
                    var pageObject = page.GetPageObjectByID(pageObjectID) ?? page.History.GetPageObjectByID(pageObjectID);
                    if(pageObject is FuzzyFactorCard)
                    {
                        divisionTemplatesOnPage.Add(new DivisionTemplateAndRemainder((pageObject as FuzzyFactorCard), (pageObject as FuzzyFactorCard).Dividend));
                        continue;
                    }

                    var array = pageObject as CLPArray;
                    if(array == null)
                    {
                        continue;
                    }

                    arraysOnPage.Add(array);
                    var arrayArea = arraysOnPage.Sum(x => x.Rows * x.Columns);

                    foreach(var divisionTemplateAndRemainder in divisionTemplatesOnPage)
                    {
                        if(array.Columns == divisionTemplateAndRemainder.DivisionTemplate.Dividend ||
                           (array.Rows == divisionTemplateAndRemainder.DivisionTemplate.Dividend))
                        {
                            var existingTag =
                                page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>()
                                    .FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.ProductAsDimension);

                            var previousNumberOfAttempts = 0;
                            if(existingTag != null)
                            {
                                previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                page.RemoveTag(existingTag);
                            }
                            var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                       Origin.StudentPageObjectGenerated,
                                                                                       DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.ProductAsDimension,
                                                                                       previousNumberOfAttempts + 1);
                            page.AddTag(newTag);
                        }

                        if(array.Rows != divisionTemplateAndRemainder.DivisionTemplate.Rows &&
                           array.Columns == divisionTemplateAndRemainder.DivisionTemplate.Rows)
                        {
                            var existingTag =
                                page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>().FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.WrongOrientation);

                            var previousNumberOfAttempts = 0;
                            if(existingTag != null)
                            {
                                previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                page.RemoveTag(existingTag);
                            }
                            var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                       Origin.StudentPageObjectGenerated,
                                                                                       DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.WrongOrientation,
                                                                                       previousNumberOfAttempts + 1);
                            page.AddTag(newTag);
                        }
                        else if(array.Rows != divisionTemplateAndRemainder.DivisionTemplate.Rows)
                        {
                            var existingTag =
                                page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>()
                                    .FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.IncorrectDimension);

                            var previousNumberOfAttempts = 0;
                            if(existingTag != null)
                            {
                                previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                page.RemoveTag(existingTag);
                            }
                            var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                       Origin.StudentPageObjectGenerated,
                                                                                       DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.IncorrectDimension,
                                                                                       previousNumberOfAttempts + 1);
                            page.AddTag(newTag);
                        }

                        if(arrayArea > divisionTemplateAndRemainder.Remainder)
                        {
                            var existingTag =
                                page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>().FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.ArrayTooLarge);

                            var previousNumberOfAttempts = 0;
                            if(existingTag != null)
                            {
                                previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                page.RemoveTag(existingTag);
                            }
                            var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                       Origin.StudentPageObjectGenerated,
                                                                                       DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.ArrayTooLarge,
                                                                                       previousNumberOfAttempts + 1);
                            page.AddTag(newTag);
                        }
                    }
                }
            }

            //DivisionTemplateFailedSnapTag
            //TODO: Doesn't seem to be enough information to re-create this Tag. Could possibly analyze PageObjectMoveBatchHistoryItem
            //to see if Array stops close to a DivisionTemplate, but it's not a guarantee.
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
                page.AddTag(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.OneArray, dividerValues));
                return;
            }

            if(Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            {
                page.AddTag(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.EvenSplit, dividerValues));
                return;
            }

            // HACK - This only compares the first 2 values to see if they are the same to determine Repeated Strategy. Find a way to determine this by frequency.
            if(dividerValues.First() == dividerValues.ElementAt(1))
            {
                page.AddTag(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.Repeated, dividerValues));
                return;
            }

            page.AddTag(new DivisionTemplateStrategyTag(page, Origin.StudentPageGenerated, DivisionTemplateStrategyTag.AcceptedValues.Other, dividerValues));
        }

        public static void InterpretCorrectness(CLPPage page, ProductDefinitionTag productDefinition, FuzzyFactorCard divisionTemplate)
        {
            // Apply a Completeness tag.
            var isDivisionTemplateComplete = false;
            if(divisionTemplate.VerticalDivisions.Count < 2)
            {
                var tag = new DivisionTemplateCompletenessTag(page, Origin.StudentPageGenerated, DivisionTemplateCompletenessTag.AcceptedValues.NoArrays);
                page.AddTag(tag);
            }
            else if(divisionTemplate.VerticalDivisions.Sum(x => x.Value) == divisionTemplate.Columns)
            {
                var tag = new DivisionTemplateCompletenessTag(page, Origin.StudentPageGenerated, DivisionTemplateCompletenessTag.AcceptedValues.Complete);
                page.AddTag(tag);
                isDivisionTemplateComplete = true;
            }
            else
            {
                var tag = new DivisionTemplateCompletenessTag(page, Origin.StudentPageGenerated, DivisionTemplateCompletenessTag.AcceptedValues.NotEnoughArrays);
                page.AddTag(tag);
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
                page.AddTag(correctTag);
                return;
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
            page.AddTag(incorrectTag);
        }
    }
}