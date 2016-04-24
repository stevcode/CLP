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
            //foreach (var tag in page.Tags.ToList().Where(tag => tag.Category == Category.Array && !(tag is ArrayTriedWrongDividerValuesTag)))
            //{
            //    page.RemoveTag(tag);
            //}

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
                    AnalyzeOrientation(page, multiplicationRelationDefinition, array);
                    AnalyzeStrategies(page, array);
                    AnalyzeCorrectness(page, multiplicationRelationDefinition, array);
                }
            }

            AnalyzeArrayCorrectness(page);
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

                //if (columnValueSum > array.Columns)
                //{
                //    page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                //                                                    Origin.StudentPageObjectGenerated,
                //                                                    array.ID,
                //                                                    array.Rows,
                //                                                    array.Columns,
                //                                                    DividerValuesOrientation.Horizontal,
                //                                                    array.HorizontalDivisions.Select(x => x.Value).ToList()));
                //}

                //if (rowValueSum > array.Rows)
                //{
                //    page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                //                                                    Origin.StudentPageObjectGenerated,
                //                                                    array.ID,
                //                                                    array.Rows,
                //                                                    array.Columns,
                //                                                    DividerValuesOrientation.Vertical,
                //                                                    array.VerticalDivisions.Select(x => x.Value).ToList()));
                //}

                var divisionValueChangedHistory = divisionValueChangedHistoryForArray.Value;

                foreach (var arrayDivisionValueChangedHistoryItem in divisionValueChangedHistory)
                {
                    //if (arrayDivisionValueChangedHistoryItem.IsHorizontalDivision)
                    //{
                    //    rowValueSum -= array.HorizontalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                    //    rowValueSum += arrayDivisionValueChangedHistoryItem.PreviousValue;
                    //    if (rowValueSum > array.Rows)
                    //    {
                    //        var dividerValues =
                    //            array.HorizontalDivisions.Select(
                    //                                             (t, i) =>
                    //                                             arrayDivisionValueChangedHistoryItem.DivisionIndex == i
                    //                                                 ? arrayDivisionValueChangedHistoryItem.PreviousValue
                    //                                                 : t.Value).ToList();

                    //        page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                    //                                                        Origin.StudentPageObjectGenerated,
                    //                                                        array.ID,
                    //                                                        array.Rows,
                    //                                                        array.Columns,
                    //                                                        DividerValuesOrientation.Vertical,
                    //                                                        dividerValues));
                    //    }
                    //    rowValueSum += array.HorizontalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                    //    rowValueSum -= arrayDivisionValueChangedHistoryItem.PreviousValue;
                    //}
                    //else
                    //{
                    //    columnValueSum -= array.VerticalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                    //    columnValueSum += arrayDivisionValueChangedHistoryItem.PreviousValue;
                    //    if (columnValueSum > array.Columns)
                    //    {
                    //        var dividerValues =
                    //            array.VerticalDivisions.Select(
                    //                                           (t, i) =>
                    //                                           arrayDivisionValueChangedHistoryItem.DivisionIndex == i
                    //                                               ? arrayDivisionValueChangedHistoryItem.PreviousValue
                    //                                               : t.Value).ToList();

                    //        page.AddTag(new ArrayTriedWrongDividerValuesTag(page,
                    //                                                        Origin.StudentPageObjectGenerated,
                    //                                                        array.ID,
                    //                                                        array.Rows,
                    //                                                        array.Columns,
                    //                                                        DividerValuesOrientation.Horizontal,
                    //                                                        dividerValues));
                    //    }
                    //    columnValueSum += array.VerticalDivisions[arrayDivisionValueChangedHistoryItem.DivisionIndex].Value;
                    //    columnValueSum -= arrayDivisionValueChangedHistoryItem.PreviousValue;
                    //}
                }
            }
        }

        public static void AnalyzeOrientation(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinition, CLPArray array)
        {
            if (multiplicationRelationDefinition.Factors.Count > 2)
            {
                return;
            }

            var firstFactor = multiplicationRelationDefinition.Factors[0].RelationPartAnswerValue;
            var secondFactor = multiplicationRelationDefinition.Factors[1].RelationPartAnswerValue;

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

        public static void AnalyzeStrategies(CLPPage page, CLPArray array)
        {
            AnalyzeAxisStrategies(page, array, array.VerticalDivisions.Select(x => x.Value).ToList(), true);
            AnalyzeAxisStrategies(page, array, array.HorizontalDivisions.Select(x => x.Value).ToList(), false);

            AnalyzeRegionStrategies(page, array);
        }

        public static void AnalyzeAxisStrategies(CLPPage page, CLPArray array, List<int> dividerValues, bool isXAxisStrategy)
        {
            //if (!dividerValues.Any())
            //{
            //    return;
            //}

            //if (Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            //{
            //    if (isXAxisStrategy)
            //    {
            //        page.AddTag(new ArrayXAxisStrategyTag(page,
            //                                              Origin.StudentPageGenerated,
            //                                              ArrayXAxisStrategyTag.AcceptedValues.EvenSplit,
            //                                              dividerValues));
            //    }
            //    else
            //    {
            //        page.AddTag(new ArrayYAxisStrategyTag(page,
            //                                              Origin.StudentPageGenerated,
            //                                              ArrayYAxisStrategyTag.AcceptedValues.EvenSplit,
            //                                              dividerValues));
            //    }
            //    return;
            //}

            //if (dividerValues.OrderBy(x => x).SequenceEqual(PlaceValueStrategyDivisions(isXAxisStrategy ? array.Columns : array.Rows).OrderBy(x => x)))
            //{
            //    if (isXAxisStrategy)
            //    {
            //        page.AddTag(new ArrayXAxisStrategyTag(page,
            //                                              Origin.StudentPageGenerated,
            //                                              ArrayXAxisStrategyTag.AcceptedValues.PlaceValue,
            //                                              dividerValues));
            //    }
            //    else
            //    {
            //        page.AddTag(new ArrayYAxisStrategyTag(page,
            //                                              Origin.StudentPageGenerated,
            //                                              ArrayYAxisStrategyTag.AcceptedValues.PlaceValue,
            //                                              dividerValues));
            //    }
            //    return;
            //}

            //var mode = dividerValues.GroupBy(i => i).OrderByDescending(g => g.Count()).Select(g => g.Key).First();
            //var modeFrequency = dividerValues.Count(i => i == mode);

            //if (modeFrequency >= dividerValues.Count())
            //{
            //    if (isXAxisStrategy)
            //    {
            //        page.AddTag(new ArrayXAxisStrategyTag(page,
            //                                              Origin.StudentPageGenerated,
            //                                              ArrayXAxisStrategyTag.AcceptedValues.Repeated,
            //                                              dividerValues));
            //    }
            //    else
            //    {
            //        page.AddTag(new ArrayYAxisStrategyTag(page,
            //                                              Origin.StudentPageGenerated,
            //                                              ArrayYAxisStrategyTag.AcceptedValues.Repeated,
            //                                              dividerValues));
            //    }
            //    return;
            //}

            //if (isXAxisStrategy)
            //{
            //    page.AddTag(new ArrayXAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayXAxisStrategyTag.AcceptedValues.Other, dividerValues));
            //}
            //else
            //{
            //    page.AddTag(new ArrayYAxisStrategyTag(page, Origin.StudentPageGenerated, ArrayYAxisStrategyTag.AcceptedValues.Other, dividerValues));
            //}
        }

        public static void AnalyzeRegionStrategies(CLPPage page, CLPArray array)
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

        public static void AnalyzeCorrectness(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinition, CLPArray array)
        {
            switch (array.ArrayType)
            {
                case ArrayTypes.Array:
                    AnalyzeAxisDividerCorrectness(page, array, array.VerticalDivisions.Select(x => x.Value).ToList(), true);
                    AnalyzeAxisDividerCorrectness(page, array, array.HorizontalDivisions.Select(x => x.Value).ToList(), false);
                    AnalyzeRepresentationArrayCorrectness(page, multiplicationRelationDefinition, array);
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

        public static void AnalyzeAxisDividerCorrectness(CLPPage page, CLPArray array, List<int> dividerValues, bool isXAxisDivider)
        {
            if (!dividerValues.Any())
            {
                return;
            }

            if (dividerValues.Contains(0))
            {
                //if (isXAxisDivider)
                //{
                //    page.AddTag(new ArrayXAxisDividerCorrectnessTag(page,
                //                                                    Origin.StudentPageGenerated,
                //                                                    Correctness.Incorrect,
                //                                                    new List<ArrayAxisIncorrectReason>
                //                                                    {
                //                                                        ArrayAxisIncorrectReason.IncompleteDividerValues
                //                                                    }));
                //}
                //else
                //{
                //    page.AddTag(new ArrayYAxisDividerCorrectnessTag(page,
                //                                                    Origin.StudentPageGenerated,
                //                                                    Correctness.Incorrect,
                //                                                    new List<ArrayAxisIncorrectReason>
                //                                                    {
                //                                                        ArrayAxisIncorrectReason.IncompleteDividerValues
                //                                                    }));
                //}
                return;
            }

            if (isXAxisDivider && dividerValues.Sum() != array.Columns)
            {
                //page.AddTag(new ArrayXAxisDividerCorrectnessTag(page,
                //                                                Origin.StudentPageGenerated,
                //                                                Correctness.Incorrect,
                //                                                new List<ArrayAxisIncorrectReason>
                //                                                {
                //                                                    ArrayAxisIncorrectReason.WrongDividerSum
                //                                                }));
                return;
            }

            if (!isXAxisDivider &&
                dividerValues.Sum() != array.Rows)
            {
                //page.AddTag(new ArrayYAxisDividerCorrectnessTag(page,
                //                                                Origin.StudentPageGenerated,
                //                                                Correctness.Incorrect,
                //                                                new List<ArrayAxisIncorrectReason>
                //                                                {
                //                                                    ArrayAxisIncorrectReason.WrongDividerSum
                //                                                }));
                return;
            }

            //page.AddTag(new ArrayYAxisDividerCorrectnessTag(page,
            //                                                Origin.StudentPageGenerated,
            //                                                Correctness.Correct,
            //                                                new List<ArrayAxisIncorrectReason>()));
        }

        public static void AnalyzeRepresentationArrayCorrectness(CLPPage page,
                                                                   MultiplicationRelationDefinitionTag multiplicationRelationDefinition,
                                                                   CLPArray array)
        {
            //var incorrectReasons = new List<ArrayRepresentationIncorrectReason>();
            //if (multiplicationRelationDefinition.Factors.Count > 2)
            //{
            //    incorrectReasons.Add(ArrayRepresentationIncorrectReason.Other);
            //    page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Incorrect, incorrectReasons));
            //    return;
            //}

            //var firstFactor = multiplicationRelationDefinition.Factors[0].RelationPartAnswerValue;
            //var secondFactor = multiplicationRelationDefinition.Factors[1].RelationPartAnswerValue;

            //if ((firstFactor == array.Rows && secondFactor == array.Columns) ||
            //    (firstFactor == array.Columns && secondFactor == array.Rows))
            //{
            //    page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, incorrectReasons));
            //    return;
            //}

            //if (array.Rows == multiplicationRelationDefinition.Product ||
            //    array.Columns == multiplicationRelationDefinition.Product)
            //{
            //    incorrectReasons.Add(ArrayRepresentationIncorrectReason.ProductAsFactor);
            //}

            //if (firstFactor == array.Rows ||
            //    secondFactor == array.Rows ||
            //    firstFactor == array.Columns ||
            //    secondFactor == array.Columns)
            //{
            //    incorrectReasons.Add(ArrayRepresentationIncorrectReason.OneDimensionCorrect);
            //    page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.PartiallyCorrect, incorrectReasons));
            //    return;
            //}

            //if (!incorrectReasons.Any())
            //{
            //    incorrectReasons.Add(ArrayRepresentationIncorrectReason.WrongFactors);
            //}

            //page.AddTag(new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Incorrect, incorrectReasons));
        }

        public static void AnalyzeArrayCorrectness(CLPPage page)
        {
            //var representationCorrectnessTags = page.Tags.OfType<ArrayRepresentationCorrectnessTag>().ToList();
            //var xAxisCorrectnessTags = page.Tags.OfType<ArrayXAxisDividerCorrectnessTag>().ToList();
            //var yAxisCorrectnessTags = page.Tags.OfType<ArrayYAxisDividerCorrectnessTag>().ToList();


            //if (!representationCorrectnessTags.Any() &&
            //    !xAxisCorrectnessTags.Any() &&
            //    !yAxisCorrectnessTags.Any())
            //{
            //    return;
            //}


            //var incorrectReasons = new List<ArrayIncorrectReasons>();
            //var isCorrectOnce = false;
            //var isPartiallyCorrectOnce = false;
            //var isIncorrectOnce = false;
            //foreach (var arrayRepresentationCorrectnessTag in representationCorrectnessTags)
            //{
            //    switch (arrayRepresentationCorrectnessTag.Correctness)
            //    {
            //        case Correctness.Correct:
            //            isCorrectOnce = true;
            //            break;
            //        case Correctness.PartiallyCorrect:
            //            isPartiallyCorrectOnce = true;
            //            incorrectReasons.Add(ArrayIncorrectReasons.Representation);
            //            break;
            //        case Correctness.Incorrect:
            //            isIncorrectOnce = true;
            //            incorrectReasons.Add(ArrayIncorrectReasons.Representation);
            //            break;
            //        case Correctness.Unknown:
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}

            //foreach (var xAxisCorrectnessTag in xAxisCorrectnessTags)
            //{
            //    switch (xAxisCorrectnessTag.Correctness)
            //    {
            //        case Correctness.Correct:
            //            isCorrectOnce = true;
            //            break;
            //        case Correctness.PartiallyCorrect:
            //            isPartiallyCorrectOnce = true;
            //            incorrectReasons.Add(ArrayIncorrectReasons.XAxisDivider);
            //            break;
            //        case Correctness.Incorrect:
            //            isIncorrectOnce = true;
            //            incorrectReasons.Add(ArrayIncorrectReasons.XAxisDivider);
            //            break;
            //        case Correctness.Unknown:
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}

            //foreach (var yAxisCorrectnessTag in yAxisCorrectnessTags)
            //{
            //    switch (yAxisCorrectnessTag.Correctness)
            //    {
            //        case Correctness.Correct:
            //            isCorrectOnce = true;
            //            break;
            //        case Correctness.PartiallyCorrect:
            //            isPartiallyCorrectOnce = true;
            //            incorrectReasons.Add(ArrayIncorrectReasons.YAxisDivider);
            //            break;
            //        case Correctness.Incorrect:
            //            isIncorrectOnce = true;
            //            incorrectReasons.Add(ArrayIncorrectReasons.YAxisDivider);
            //            break;
            //        case Correctness.Unknown:
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}

            //incorrectReasons = incorrectReasons.Distinct().ToList();

            //var correctnessSum = Correctness.Unknown;
            //if (isPartiallyCorrectOnce)
            //{
            //    correctnessSum = Correctness.PartiallyCorrect;
            //}
            //else if (isCorrectOnce &&
            //         isIncorrectOnce)
            //{
            //    correctnessSum = Correctness.PartiallyCorrect;
            //}
            //else if (isCorrectOnce)
            //{
            //    correctnessSum = Correctness.Correct;
            //}
            //else if (isIncorrectOnce)
            //{
            //    correctnessSum = Correctness.Incorrect;
            //}

            //page.AddTag(new ArrayCorrectnessSummaryTag(page, Origin.StudentPageGenerated, correctnessSum, incorrectReasons));
        }
    }
}