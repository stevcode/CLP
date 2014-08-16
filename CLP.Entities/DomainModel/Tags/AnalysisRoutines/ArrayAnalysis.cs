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
            foreach (var tag in page.Tags.ToList().Where(tag => tag.Category == Category.Array && !(tag is ArrayTriedWrongDividerValuesTag)))
            {
                page.RemoveTag(tag);
            }

            var multiplicationRelationDefinitionTags = page.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
            var arrays = page.PageObjects.OfType<CLPArray>().ToList();
            if (!multiplicationRelationDefinitionTags.Any() ||
                !arrays.Any())
            {
                return;
            }

            foreach (var multiplicationRelationDefinition in multiplicationRelationDefinitionTags)
            {
                foreach (var array in arrays)
                {
                    InterpretOrientation(page, multiplicationRelationDefinition, array);
                    InterpretStrategies(page, array);
                    InterpretCorrectness(page, multiplicationRelationDefinition, array);
                }
            }
        }

        public static void AnalyzeHistory(CLPPage page)
        {
            var completeOrderedHistory = page.History.UndoItems.Reverse().Concat(page.History.RedoItems).ToList();

            //ArrayTriedWrongDividerValuesTag
            var divisionValueChangedHistoryForArrays = new Dictionary<string, List<CLPArrayDivisionValueChangedHistoryItem>>();
            foreach (var arrayDivisionValueChangedHistoryItem in completeOrderedHistory.OfType<CLPArrayDivisionValueChangedHistoryItem>())
            {
                if (!divisionValueChangedHistoryForArrays.ContainsKey(arrayDivisionValueChangedHistoryItem.ArrayID))
                {
                    divisionValueChangedHistoryForArrays.Add(arrayDivisionValueChangedHistoryItem.ArrayID,
                                                             new List<CLPArrayDivisionValueChangedHistoryItem>());
                }

                divisionValueChangedHistoryForArrays[arrayDivisionValueChangedHistoryItem.ArrayID].Add(arrayDivisionValueChangedHistoryItem);
            }

            foreach (var divisionValueChangedHistoryForArray in divisionValueChangedHistoryForArrays)
            {
                var array = page.GetPageObjectByID(divisionValueChangedHistoryForArray.Key) as CLPArray ??
                            page.History.GetPageObjectByID(divisionValueChangedHistoryForArray.Key) as CLPArray;
                if (array == null)
                {
                    continue;
                }

                var rowValueSum = array.HorizontalDivisions.Sum(x => x.Value);
                var columnValueSum = array.VerticalDivisions.Sum(x => x.Value);

                if (columnValueSum > array.Columns)
                {
                    page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                                                                    Origin.StudentPageObjectGenerated,
                                                                    array.ID,
                                                                    array.Rows,
                                                                    array.Columns,
                                                                    DividerValuesOrientation.Horizontal,
                                                                    array.HorizontalDivisions.Select(x => x.Value).ToList()));
                }

                if (rowValueSum > array.Rows)
                {
                    page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                                                                    Origin.StudentPageObjectGenerated,
                                                                    array.ID,
                                                                    array.Rows,
                                                                    array.Columns,
                                                                    DividerValuesOrientation.Vertical,
                                                                    array.VerticalDivisions.Select(x => x.Value).ToList()));
                }

                var divisionValueChangedHistory = divisionValueChangedHistoryForArray.Value;

                foreach (var arrayDivisionValueChangedHistoryItem in divisionValueChangedHistory)
                {
                    if (arrayDivisionValueChangedHistoryItem.IsHorizontalDivision)
                    {
                        rowValueSum -= array.HorizontalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                        rowValueSum += arrayDivisionValueChangedHistoryItem.PreviousValue;
                        if (rowValueSum > array.Rows)
                        {
                            var dividerValues =
                                array.HorizontalDivisions.Select(
                                                                 (t, i) =>
                                                                 arrayDivisionValueChangedHistoryItem.DivisionIndex == i
                                                                     ? arrayDivisionValueChangedHistoryItem.PreviousValue
                                                                     : t.Value).ToList();

                            page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                                                                            Origin.StudentPageObjectGenerated,
                                                                            array.ID,
                                                                            array.Rows,
                                                                            array.Columns,
                                                                            DividerValuesOrientation.Vertical,
                                                                            dividerValues));
                        }
                        rowValueSum += array.HorizontalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                        rowValueSum -= arrayDivisionValueChangedHistoryItem.PreviousValue;
                    }
                    else
                    {
                        columnValueSum -= array.VerticalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                        columnValueSum += arrayDivisionValueChangedHistoryItem.PreviousValue;
                        if (columnValueSum > array.Columns)
                        {
                            var dividerValues =
                                array.VerticalDivisions.Select(
                                                               (t, i) =>
                                                               arrayDivisionValueChangedHistoryItem.DivisionIndex == i
                                                                   ? arrayDivisionValueChangedHistoryItem.PreviousValue
                                                                   : t.Value).ToList();

                            page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                                                                            Origin.StudentPageObjectGenerated,
                                                                            array.ID,
                                                                            array.Rows,
                                                                            array.Columns,
                                                                            DividerValuesOrientation.Horizontal,
                                                                            dividerValues));
                        }
                        columnValueSum += array.VerticalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                        columnValueSum -= arrayDivisionValueChangedHistoryItem.PreviousValue;
                    }
                }
            }
        }

        public static void InterpretOrientation(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinition, CLPArray array)
        {
            if (multiplicationRelationDefinition.Factors.Count > 2)
            {
                return;
            }

            var firstFactor = multiplicationRelationDefinition.Factors[0];
            var secondFactor = multiplicationRelationDefinition.Factors[1];

            if (firstFactor == array.Columns &&
                secondFactor == array.Rows)
            {
                page.AddTag(new ArrayOrientationTag(page, Origin.StudentPageGenerated, ArrayOrientationTag.AcceptedValues.FirstFactorWidth));
            }
            else if (firstFactor == array.Rows &&
                     secondFactor == array.Columns)
            {
                page.AddTag(new ArrayOrientationTag(page, Origin.StudentPageGenerated, ArrayOrientationTag.AcceptedValues.FirstFactorHeight));
            }
            else
            {
                page.AddTag(new ArrayOrientationTag(page, Origin.StudentPageGenerated, ArrayOrientationTag.AcceptedValues.Unknown));
            }
        }

        public static void InterpretStrategies(CLPPage page, CLPArray array)
        {
            InterpretAxisStrategies(page, array, array.VerticalDivisions.Select(x => x.Value).ToList(), true);
            InterpretAxisStrategies(page, array, array.HorizontalDivisions.Select(x => x.Value).ToList(), false);
            InterpretRegionStrategies(page, array);
        }

        public static void InterpretAxisStrategies(CLPPage page, CLPArray array, List<int> dividerValues, bool isXAxisStrategy)
        {
            if (!dividerValues.Any())
            {
                return;
            }

            if (Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            {
                if (isXAxisStrategy)
                {
                    page.AddTag(new ArrayXAxisStrategyTag(page,
                                                          Origin.StudentPageGenerated,
                                                          ArrayXAxisStrategyTag.AcceptedValues.EvenSplit,
                                                          dividerValues));
                }
                else
                {
                    page.AddTag(new ArrayYAxisStrategyTag(page,
                                                          Origin.StudentPageGenerated,
                                                          ArrayYAxisStrategyTag.AcceptedValues.EvenSplit,
                                                          dividerValues));
                }
                return;
            }

            if (dividerValues.OrderBy(x => x).SequenceEqual(PlaceValueStrategyDivisions(isXAxisStrategy ? array.Columns : array.Rows).OrderBy(x => x)))
            {
                if (isXAxisStrategy)
                {
                    page.AddTag(new ArrayXAxisStrategyTag(page,
                                                          Origin.StudentPageGenerated,
                                                          ArrayXAxisStrategyTag.AcceptedValues.PlaceValue,
                                                          dividerValues));
                }
                else
                {
                    page.AddTag(new ArrayYAxisStrategyTag(page,
                                                          Origin.StudentPageGenerated,
                                                          ArrayYAxisStrategyTag.AcceptedValues.PlaceValue,
                                                          dividerValues));
                }
                return;
            }

            // HACK - This only compares the first 2 values to see if they are the same to determine Repeated Strategy. Find a way to determine this by frequency.
            if (dividerValues.First() == dividerValues.ElementAt(1))
            {
                if (isXAxisStrategy)
                {
                    page.AddTag(new ArrayXAxisStrategyTag(page,
                                                          Origin.StudentPageGenerated,
                                                          ArrayXAxisStrategyTag.AcceptedValues.Repeated,
                                                          dividerValues));
                }
                else
                {
                    page.AddTag(new ArrayYAxisStrategyTag(page,
                                                          Origin.StudentPageGenerated,
                                                          ArrayYAxisStrategyTag.AcceptedValues.Repeated,
                                                          dividerValues));
                }
                return;
            }

            if (isXAxisStrategy)
            {
                page.AddTag(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.Other, dividerValues));
            }
            else
            {
                page.AddTag(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.Other, dividerValues));
            }
        }

        public static void InterpretRegionStrategies(CLPPage page, CLPArray array)
        {
            //TODO
        }

        private static IEnumerable<int> PlaceValueStrategyDivisions(int startingValue)
        {
            var currentValue = startingValue;
            var currentPlace = 1;
            var output = new List<int>();
            while (currentValue > 0)
            {
                if (currentValue % 10 > 0)
                {
                    output.Add((currentValue % 10) * currentPlace);
                }
                currentValue /= 10;
                currentPlace *= 10;
            }
            output.Sort();

            return output;
        }

        public static void InterpretCorrectness(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinition, CLPArray array)
        {
            switch (array.ArrayType)
            {
                case ArrayTypes.Array:
                    InterpretArrayCorrectness(page, multiplicationRelationDefinition, array);
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

        public static void InterpretArrayCorrectness(CLPPage page,
                                                     MultiplicationRelationDefinitionTag multiplicationRelationDefinition,
                                                     CLPArray array)
        {
            var incorrectReasons = new List<ArrayIncorrectReason>();
            if (multiplicationRelationDefinition.Factors.Count > 2)
            {
                incorrectReasons.Add(ArrayIncorrectReason.Other);
                page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Incorrect, incorrectReasons));
                return;
            }

            var firstFactor = multiplicationRelationDefinition.Factors[0];
            var secondFactor = multiplicationRelationDefinition.Factors[1];

            if ((firstFactor == array.Rows && secondFactor == array.Columns) ||
                (firstFactor == array.Columns && secondFactor == array.Rows))
            {
                page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, incorrectReasons));
                return;
            }

            if (array.Rows == multiplicationRelationDefinition.Product ||
                array.Columns == multiplicationRelationDefinition.Product)
            {
                incorrectReasons.Add(ArrayIncorrectReason.ProductAsFactor);
            }
            
            if (firstFactor == array.Rows ||
                secondFactor == array.Rows ||
                firstFactor == array.Columns ||
                secondFactor == array.Columns)
            {
                incorrectReasons.Add(ArrayIncorrectReason.OneDimensionCorrect);
                page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.PartiallyCorrect, incorrectReasons));
                return;
            }

            if (!incorrectReasons.Any())
            {
                incorrectReasons.Add(ArrayIncorrectReason.WrongFactors);
            }

            page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Incorrect, incorrectReasons));
        }
    }
}