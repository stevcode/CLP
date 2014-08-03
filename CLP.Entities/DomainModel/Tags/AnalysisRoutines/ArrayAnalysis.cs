using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CLP.Entities
{
    public static class ArrayAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            // First, clear out any old ArrayTags generated via Analysis.
            foreach(var tag in page.Tags.ToList().Where(tag => tag.Category == Category.Array &&
                                                               !(tag is ArrayTriedWrongDividerValuesTag)))
            {
                page.Tags.Remove(tag);
            }

            var productDefinitionTags = page.Tags.OfType<ProductDefinitionTag>().ToList();
            var arrays = page.PageObjects.OfType<CLPArray>().ToList();
            if(!productDefinitionTags.Any() ||
               !arrays.Any())
            {
                return;
            }

            foreach(var productDefinitionTag in productDefinitionTags)
            {
                foreach(var array in arrays)
                {
                    InterpretOrientation(page, productDefinitionTag, array);
                    InterpretStrategies(page, array);
                    InterpretCorrectness(page, productDefinitionTag, array);
                }
            }
        }

        public static void InterpretOrientation(CLPPage page, ProductDefinitionTag productDefinition, CLPArray array)
        {
            if(productDefinition.FirstFactor == array.Columns &&
               productDefinition.SecondFactor == array.Rows)
            {
                page.Tags.Add(new ArrayOrientationTag(page, Origin.StudentPageGenerated, ArrayOrientationTag.AcceptedValues.FirstFactorWidth));
            }
            else if(productDefinition.FirstFactor == array.Rows &&
                    productDefinition.SecondFactor == array.Columns)
            {
                page.Tags.Add(new ArrayOrientationTag(page, Origin.StudentPageGenerated, ArrayOrientationTag.AcceptedValues.FirstFactorHeight));
            }
            else
            {
                page.Tags.Add(new ArrayOrientationTag(page, Origin.StudentPageGenerated, ArrayOrientationTag.AcceptedValues.Unknown));
            }
        }

        public static void InterpretStrategies(CLPPage page, CLPArray array)
        {
            InterpretAxisStrategies(page, array, array.HorizontalDivisions.Select(x => x.Value).ToList(), true);
            InterpretAxisStrategies(page, array, array.VerticalDivisions.Select(x => x.Value).ToList(), false);
            InterpretRegionStrategies(page, array);
        }

        public static void InterpretAxisStrategies(CLPPage page, CLPArray array, List<int> dividerValues, bool isXAxisStrategy)
        {
            if(!dividerValues.Any())
            {
                if(isXAxisStrategy)
                {
                    page.Tags.Add(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.NoDividers, dividerValues));
                }
                else
                {
                    page.Tags.Add(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.NoDividers, dividerValues));
                }
                return;
            }

            if(Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            {
                if(isXAxisStrategy)
                {
                    page.Tags.Add(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.EvenSplit, dividerValues));
                }
                else
                {
                    page.Tags.Add(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.EvenSplit, dividerValues));
                }
                return;
            }

            if(dividerValues.SequenceEqual(PlaceValueStrategyDivisions(isXAxisStrategy ? array.Columns : array.Rows)))
            {
                if(isXAxisStrategy)
                {
                    page.Tags.Add(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.PlaceValue, dividerValues));
                }
                else
                {
                    page.Tags.Add(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.PlaceValue, dividerValues));
                }
                return;
            }

            // HACK - This only compares the first 2 values to see if they are the same to determine Repeated Strategy. Find a way to determine this by frequency.
            if(dividerValues.First() == dividerValues.ElementAt(1))
            {
                if(isXAxisStrategy)
                {
                    page.Tags.Add(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.Repeated, dividerValues));
                }
                else
                {
                    page.Tags.Add(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.Repeated, dividerValues));
                }
                return;
            }

            if(isXAxisStrategy)
            {
                page.Tags.Add(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.Other, dividerValues));
            }
            else
            {
                page.Tags.Add(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.Other, dividerValues));
            }
        }

        public static void InterpretRegionStrategies(CLPPage page, CLPArray array)
        {
            
        }

        private static IEnumerable<int> PlaceValueStrategyDivisions(int startingValue)
        {
            var currentValue = startingValue;
            var currentPlace = 1;
            var output = new List<int>();
            while(currentValue > 0)
            {
                if(currentValue % 10 > 0)
                {
                    output.Add((currentValue % 10) * currentPlace);
                }
                currentValue /= 10;
                currentPlace *= 10;
            }
            output.Sort();

            return output;
        }

        public static void InterpretCorrectness(CLPPage page, ProductDefinitionTag productDefinition, CLPArray array)
        {
            switch(array.ArrayType)
            {
                case ArrayTypes.Array:
                    InterpretArrayCorrectness(page, productDefinition, array);
                    break;
                case ArrayTypes.ArrayCard:
                    // TODO
                    break;
                case ArrayTypes.FactorCard:
                    // TODO
                    break;
                case ArrayTypes.TenByTen:
                    // TODO
                    break;
            }
        }

        public static void InterpretArrayCorrectness(CLPPage page, ProductDefinitionTag productDefinition, CLPArray array)
        {
            var incorrectReasons = new List<ArrayIncorrectReason>();

            if(productDefinition.FirstFactor == array.Rows &&
               productDefinition.SecondFactor == array.Columns)
            {
                page.Tags.Add(new ArrayInterpretedCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, incorrectReasons));
                return;
            }

            if(productDefinition.SecondFactor == array.Rows &&
               productDefinition.FirstFactor == array.Columns)
            {
                if(productDefinition.ProductType != ProductType.Area) //HACK - This seems...weird to me. Order of Factors only matters when calculating area?
                {
                    page.Tags.Add(new ArrayInterpretedCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, incorrectReasons));
                    return;
                }

                incorrectReasons.Add(ArrayIncorrectReason.SwappedFactors);
            }

            var givenValues = new List<double?>();
            switch(productDefinition.UngivenProductPart)
            {
                case ProductPart.FirstFactor:
                    givenValues.Add(productDefinition.SecondFactor);
                    givenValues.Add(productDefinition.Product);
                    break;
                case ProductPart.SecondFactor:
                    givenValues.Add(productDefinition.FirstFactor);
                    givenValues.Add(productDefinition.Product);
                    break;
                case ProductPart.Product:
                    givenValues.Add(productDefinition.FirstFactor);
                    givenValues.Add(productDefinition.SecondFactor);
                    break;
            }

            var numbersUsed = new List<double?>
                              {
                                  array.Rows,
                                  array.Columns,
                                  array.Rows * array.Columns
                              };

            if(givenValues.Count == 2 &&
               numbersUsed.Contains(givenValues[0]) &&
               numbersUsed.Contains(givenValues[1]))
            {
                incorrectReasons.Add(ArrayIncorrectReason.MisusedGivens);
            }
            else
            {
                incorrectReasons.Add(ArrayIncorrectReason.WrongFactors);
            }

            page.Tags.Add(new ArrayInterpretedCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Incorrect, incorrectReasons));
        }
    }
}