using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CLP.Entities
{
    public static class DivisionToolAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            // First, clear out any old DivisionToolTags generated via Analysis.
            foreach (var tag in
                page.Tags.ToList()
                    .Where(
                           tag =>
                           tag is DivisionToolRepresentationCorrectnessTag || tag is DivisionToolCompletenessTag || tag is DivisionToolCorrectnessSummaryTag ||
                           tag is TroubleWithDivisionTag))
            {
                page.RemoveTag(tag);
            }

            var divisionDefinitionTags = page.Tags.OfType<DivisionRelationDefinitionTag>().ToList();
            var divisionTools = page.PageObjects.OfType<DivisionTool>().ToList();
            if (!divisionDefinitionTags.Any() ||
                !divisionTools.Any())  //BUG: Should probably mark as incorrect if no DTs on page.
            {
                return;
            }

            foreach (var divisionDefinitionTag in divisionDefinitionTags)
            {
                foreach (var divisionTool in divisionTools)
                {
                    AnalyzeRepresentationCorrectness(page, divisionDefinitionTag, divisionTool);
                }
            }

            AnalyzeDivisionToolTroubleWithDivision(page);
            AnalyzeDivisionToolCorrectness(page);
        }

        #region Analysis

        private class DivisionToolAndRemainder
        {
            public DivisionToolAndRemainder(DivisionTool divisionTool, int remainder)
            {
                DivisionTool = divisionTool;
                Remainder = remainder;
            }

            public readonly DivisionTool DivisionTool;
            public int Remainder;
        }

        public static List<string> GetListOfDivisionToolIDsInHistory(CLPPage page)
        {
            var completeOrderedHistory = page.History.UndoItems.Reverse().Concat(page.History.RedoItems).ToList();

            var divisionToolIDsInHistory = new List<string>();
            foreach (var pageObjectsAddedHistoryItem in completeOrderedHistory.OfType<PageObjectsAddedHistoryItem>())
            {
                divisionToolIDsInHistory.AddRange(from pageObjectID in pageObjectsAddedHistoryItem.PageObjectIDs
                                                      let divisionTool =
                                                          page.GetPageObjectByID(pageObjectID) as DivisionTool ?? page.History.GetPageObjectByID(pageObjectID) as DivisionTool
                                                      where divisionTool != null
                                                      select pageObjectID);
            }

            return divisionToolIDsInHistory;
        }

        public static void AnalyzeHistory(CLPPage page)
        {
            var completeOrderedHistory = page.History.UndoItems.Reverse().Concat(page.History.RedoItems).ToList();

            var divisionToolIDsInHistory = GetListOfDivisionToolIDsInHistory(page);

            //DivisionToolDeletedTag
            foreach (var historyItem in completeOrderedHistory.OfType<PageObjectsRemovedHistoryItem>())
            {
                foreach (var pageObjectID in historyItem.PageObjectIDs)
                {
                    var divisionTool = page.GetPageObjectByID(pageObjectID) as DivisionTool ?? page.History.GetPageObjectByID(pageObjectID) as DivisionTool;
                    if (divisionTool == null)
                    {
                        continue;
                    }

                    var arrayDimensions =
                        divisionTool.VerticalDivisions.Where(division => division.Value != 0).Select(division => divisionTool.Rows + "x" + division.Value).ToList();

                    page.AddTag(new DivisionToolDeletedTag(page,
                                                               Origin.StudentPageObjectGenerated,
                                                               divisionTool.ID,
                                                               divisionTool.Dividend,
                                                               divisionTool.Rows,
                                                               divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                               arrayDimensions));
                }
            }

            var divisionToolsOnPage = new List<DivisionToolAndRemainder>();
            var arraysOnPage = new List<CLPArray>();
            foreach (var historyItem in completeOrderedHistory)
            {
                var removedPageObjectsHistoryItem = historyItem as PageObjectsRemovedHistoryItem;
                if (removedPageObjectsHistoryItem != null)
                {
                    foreach (var pageObjectID in removedPageObjectsHistoryItem.PageObjectIDs)
                    {
                        divisionToolsOnPage.RemoveAll(x => x.DivisionTool.ID == pageObjectID);
                        arraysOnPage.RemoveAll(x => x.ID == pageObjectID);
                    }
                    continue;
                }

                var arraySnappedInHistoryItem = historyItem as DivisionToolArraySnappedInHistoryItem;
                if (arraySnappedInHistoryItem != null)
                {
                    var arrayToRemove = arraysOnPage.FirstOrDefault(x => x.ID == arraySnappedInHistoryItem.SnappedInArrayID);
                    var divisionToolAndRemainder = divisionToolsOnPage.FirstOrDefault(x => x.DivisionTool.ID == arraySnappedInHistoryItem.DivisionToolID);
                    if (divisionToolAndRemainder != null &&
                        arrayToRemove != null)
                    {
                        arraysOnPage.Remove(arrayToRemove);
                        divisionToolAndRemainder.Remainder -= (arrayToRemove.Rows * arrayToRemove.Columns);
                    }
                    continue;
                }

                //DivisionToolIncorrectArrayCreationTag
                var addedPageObjectHistoryItem = historyItem as PageObjectsAddedHistoryItem;
                if (addedPageObjectHistoryItem != null)
                {
                    foreach (var pageObject in
                        addedPageObjectHistoryItem.PageObjectIDs.Select(pageObjectID => page.GetPageObjectByID(pageObjectID) ?? page.History.GetPageObjectByID(pageObjectID)))
                    {
                        if (pageObject is DivisionTool)
                        {
                            var divisionTool = pageObject as DivisionTool;
                            divisionToolsOnPage.Add(new DivisionToolAndRemainder(divisionTool, divisionTool.Dividend));

                            //DivisionToolCreationErrorTag
                            var divisionDefinitions = page.Tags.OfType<DivisionRelationDefinitionTag>();

                            foreach (var divisionRelationDefinitionTag in divisionDefinitions)
                            {
                                if (divisionTool.Dividend == divisionRelationDefinitionTag.Dividend &&
                                    divisionTool.Rows == divisionRelationDefinitionTag.Divisor)
                                {
                                    continue;
                                }

                                ITag divisionCreationErrorTag = null;
                                if (divisionTool.Dividend == divisionRelationDefinitionTag.Divisor &&
                                    divisionTool.Rows == divisionRelationDefinitionTag.Dividend)
                                {
                                    divisionCreationErrorTag = new DivisionToolCreationErrorTag(page,
                                                                                                    Origin.StudentPageGenerated,
                                                                                                    divisionTool.ID,
                                                                                                    divisionTool.Dividend,
                                                                                                    divisionTool.Rows,
                                                                                                    divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                                                                    DivisionToolIncorrectCreationReasons.SwappedDividendAndDivisor);
                                }

                                if (divisionTool.Dividend == divisionRelationDefinitionTag.Dividend &&
                                    divisionTool.Rows != divisionRelationDefinitionTag.Divisor)
                                {
                                    divisionCreationErrorTag = new DivisionToolCreationErrorTag(page,
                                                                                                    Origin.StudentPageGenerated,
                                                                                                    divisionTool.ID,
                                                                                                    divisionTool.Dividend,
                                                                                                    divisionTool.Rows,
                                                                                                    divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                                                                    DivisionToolIncorrectCreationReasons.WrongDivisor);
                                }

                                if (divisionTool.Dividend != divisionRelationDefinitionTag.Dividend &&
                                    divisionTool.Rows == divisionRelationDefinitionTag.Divisor)
                                {
                                    divisionCreationErrorTag = new DivisionToolCreationErrorTag(page,
                                                                                                    Origin.StudentPageGenerated,
                                                                                                    divisionTool.ID,
                                                                                                    divisionTool.Dividend,
                                                                                                    divisionTool.Rows,
                                                                                                    divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                                                                    DivisionToolIncorrectCreationReasons.WrongDividend);
                                }

                                if (divisionTool.Dividend != divisionRelationDefinitionTag.Dividend &&
                                    divisionTool.Rows != divisionRelationDefinitionTag.Divisor)
                                {
                                    divisionCreationErrorTag = new DivisionToolCreationErrorTag(page,
                                                                                                    Origin.StudentPageGenerated,
                                                                                                    divisionTool.ID,
                                                                                                    divisionTool.Dividend,
                                                                                                    divisionTool.Rows,
                                                                                                    divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                                                                    DivisionToolIncorrectCreationReasons.WrongDividendAndDivisor);
                                }

                                if (divisionCreationErrorTag != null)
                                {
                                    page.AddTag(divisionCreationErrorTag);
                                }
                            }

                            continue;
                        }

                        var array = pageObject as CLPArray;
                        if (array != null)
                        {
                            arraysOnPage.Add(array);
                            foreach (var divisionToolAndRemainder in divisionToolsOnPage)
                            {
                                var divisionTool = divisionToolAndRemainder.DivisionTool;

                                if (divisionToolAndRemainder.Remainder != divisionTool.Dividend % divisionTool.Rows)
                                {
                                    var existingFactorPairErrorsTag =
                                        page.Tags.OfType<DivisionToolFactorPairErrorsTag>().FirstOrDefault(x => x.DivisionToolID == divisionTool.ID);
                                    var isArrayDimensionErrorsTagOnPage = true;

                                    if (existingFactorPairErrorsTag == null)
                                    {
                                        existingFactorPairErrorsTag = new DivisionToolFactorPairErrorsTag(page,
                                                                                                              Origin.StudentPageGenerated,
                                                                                                              divisionTool.ID,
                                                                                                              divisionTool.Dividend,
                                                                                                              divisionTool.Rows,
                                                                                                              divisionToolIDsInHistory.IndexOf(divisionTool.ID));
                                        isArrayDimensionErrorsTagOnPage = false;
                                    }

                                    if (array.Columns == divisionTool.Dividend ||
                                        array.Rows == divisionTool.Dividend)
                                    {
                                        existingFactorPairErrorsTag.CreateDividendAsDimensionDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                    }

                                    if (array.Rows != divisionTool.Rows)
                                    {
                                        if (array.Columns == divisionTool.Rows)
                                        {
                                            existingFactorPairErrorsTag.CreateWrongOrientationDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                        }
                                        else
                                        {
                                            existingFactorPairErrorsTag.CreateIncorrectDimensionDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                        }
                                    }

                                    var totalAreaOfArraysOnPage = arraysOnPage.Sum(x => x.Rows * x.Columns);
                                    if (totalAreaOfArraysOnPage > divisionToolAndRemainder.Remainder)
                                    {
                                        existingFactorPairErrorsTag.CreateArrayTooLargeDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                    }

                                    if (!isArrayDimensionErrorsTagOnPage &&
                                        existingFactorPairErrorsTag.ErrorAtemptsSum > 0)
                                    {
                                        page.AddTag(existingFactorPairErrorsTag);
                                    }
                                }
                                else
                                {
                                    var existingRemainderErrorsTag =
                                        page.Tags.OfType<DivisionToolRemainderErrorsTag>().FirstOrDefault(x => x.DivisionToolID == divisionTool.ID);
                                    var isRemainderErrorsTagOnPage = true;

                                    if (existingRemainderErrorsTag == null)
                                    {
                                        existingRemainderErrorsTag = new DivisionToolRemainderErrorsTag(page,
                                                                                                            Origin.StudentPageGenerated,
                                                                                                            divisionTool.ID,
                                                                                                            divisionTool.Dividend,
                                                                                                            divisionTool.Rows,
                                                                                                            divisionToolIDsInHistory.IndexOf(divisionTool.ID));
                                        isRemainderErrorsTagOnPage = false;
                                    }

                                    if (array.Columns == divisionTool.Dividend ||
                                        array.Rows == divisionTool.Dividend)
                                    {
                                        existingRemainderErrorsTag.CreateDividendAsDimensionDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                    }

                                    if (array.Rows != divisionTool.Rows)
                                    {
                                        if (array.Columns == divisionTool.Rows)
                                        {
                                            existingRemainderErrorsTag.CreateWrongOrientationDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                        }
                                        else
                                        {
                                            existingRemainderErrorsTag.CreateIncorrectDimensionDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                        }
                                    }

                                    var totalAreaOfArraysOnPage = arraysOnPage.Sum(x => x.Rows * x.Columns);
                                    if (totalAreaOfArraysOnPage > divisionToolAndRemainder.Remainder)
                                    {
                                        existingRemainderErrorsTag.CreateArrayTooLargeDimensions.Add(string.Format("{0}x{1}", array.Rows, array.Columns));
                                    }

                                    if (!isRemainderErrorsTagOnPage &&
                                        existingRemainderErrorsTag.ErrorAtemptsSum > 0)
                                    {
                                        page.AddTag(existingRemainderErrorsTag);
                                    }
                                }
                            }

                            continue;
                        }
                    }
                    continue;
                }

                //DivisionToolRemainderErrorsTag.OrientationChangedAttempts
                var arrayRotateHistoryItem = historyItem as CLPArrayRotateHistoryItem;
                if (arrayRotateHistoryItem != null)
                {
                    foreach (var divisionToolAndRemainder in divisionToolsOnPage)
                    {
                        // Only increase OrientationChanged attempt if Division Tool already full.
                        if (divisionToolAndRemainder.Remainder != divisionToolAndRemainder.DivisionTool.Dividend % divisionToolAndRemainder.DivisionTool.Rows)
                        {
                            continue;
                        }

                        var existingTroubleWithRemaindersTag =
                            page.Tags.OfType<DivisionToolRemainderErrorsTag>().FirstOrDefault(x => x.DivisionToolID == divisionToolAndRemainder.DivisionTool.ID);

                        if (existingTroubleWithRemaindersTag == null)
                        {
                            existingTroubleWithRemaindersTag = new DivisionToolRemainderErrorsTag(page,
                                                                                                      Origin.StudentPageGenerated,
                                                                                                      divisionToolAndRemainder.DivisionTool.ID,
                                                                                                      divisionToolAndRemainder.DivisionTool.Dividend,
                                                                                                      divisionToolAndRemainder.DivisionTool.Rows,
                                                                                                      divisionToolIDsInHistory.IndexOf(
                                                                                                                                           divisionToolAndRemainder
                                                                                                                                               .DivisionTool.ID));
                            page.AddTag(existingTroubleWithRemaindersTag);
                        }

                        ////existingTroubleWithRemaindersTag.OrientationChangedAttempts++;
                    }

                    continue;
                }

                //DivisionToolFailedSnapTag
                //var pageObjectMovedHistoryItem = historyItem as PageObjectMoveBatchHistoryItem;
                //if (pageObjectMovedHistoryItem != null)
                //{
                //    var array = page.GetPageObjectByID(pageObjectMovedHistoryItem.PageObjectID) as CLPArray ??
                //                page.History.GetPageObjectByID(pageObjectMovedHistoryItem.PageObjectID) as CLPArray;
                //    if (array == null)
                //    {
                //        continue;
                //    }

                //    var endPosition = pageObjectMovedHistoryItem.TravelledPositions.Last();
                //    foreach (var divisionToolAndRemainder in divisionToolsOnPage)
                //    {
                //        var DivisionTool = divisionToolAndRemainder.DivisionTool;

                //        var top = Math.Max(endPosition.Y + array.LabelLength, DivisionTool.YPosition + DivisionTool.LabelLength);
                //        var bottom = Math.Min(endPosition.Y + array.LabelLength + array.ArrayHeight,
                //                              persistingArray.YPosition + persistingArray.LabelLength + persistingArray.ArrayHeight);
                //        var verticalIntersectionLength = bottom - top;
                //        var isVerticalIntersection = verticalIntersectionLength > persistingArray.ArrayHeight / 2 || verticalIntersectionLength > snappingArray.ArrayHeight / 2;

                //        var left = Math.Max(snappingArray.XPosition + snappingArray.LabelLength, persistingArray.XPosition + persistingArray.LabelLength);
                //        var right = Math.Min(snappingArray.XPosition + snappingArray.LabelLength + snappingArray.ArrayWidth,
                //                             persistingArray.XPosition + persistingArray.LabelLength + persistingArray.ArrayWidth);
                //        var horizontalIntersectionLength = right - left;
                //        var isHorizontalIntersection = horizontalIntersectionLength > persistingArray.ArrayWidth / 2 || horizontalIntersectionLength > snappingArray.ArrayWidth / 2;

                //        if (isVerticalIntersection)
                //        {
                //            var diff = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.LabelLength + DivisionTool.LastDivisionPosition));
                //            if (diff < 50)
                //            {
                //                if (snappingArray.Rows != DivisionTool.Rows)
                //                {
                //                    var hasTag = false;
                //                    if (snappingArray.Columns == DivisionTool.Rows)
                //                    {
                //                        var existingTag =
                //                            pageObject.ParentPage.Tags.OfType<DivisionToolFailedSnapTag>()
                //                                      .FirstOrDefault(x => x.Value == DivisionToolFailedSnapTag.AcceptedValues.SnappedWrongOrientation);

                //                        var previousNumberOfAttempts = 0;
                //                        if (existingTag != null)
                //                        {
                //                            previousNumberOfAttempts = existingTag.NumberOfAttempts;
                //                            pageObject.ParentPage.RemoveTag(existingTag);
                //                        }
                //                        var newTag = new DivisionToolFailedSnapTag(pageObject.ParentPage,
                //                                                                       App.CurrentUserMode == App.UserMode.Student ? Origin.StudentPageObjectGenerated : Origin.TeacherPageObjectGenerated,
                //                                                                       DivisionToolFailedSnapTag.AcceptedValues.SnappedWrongOrientation,
                //                                                                       previousNumberOfAttempts + 1);
                //                        pageObject.ParentPage.AddTag(newTag);
                //                    }
                //                    else
                //                    {
                //                        var existingTag =
                //                            pageObject.ParentPage.Tags.OfType<DivisionToolFailedSnapTag>()
                //                                      .FirstOrDefault(x => x.Value == DivisionToolFailedSnapTag.AcceptedValues.SnappedIncorrectDimension);

                //                        var previousNumberOfAttempts = 0;
                //                        if (existingTag != null)
                //                        {
                //                            previousNumberOfAttempts = existingTag.NumberOfAttempts;
                //                            pageObject.ParentPage.RemoveTag(existingTag);
                //                        }
                //                        var newTag = new DivisionToolFailedSnapTag(pageObject.ParentPage,
                //                                                                       App.CurrentUserMode == App.UserMode.Student ? Origin.StudentPageObjectGenerated : Origin.TeacherPageObjectGenerated,
                //                                                                       DivisionToolFailedSnapTag.AcceptedValues.SnappedIncorrectDimension,
                //                                                                       previousNumberOfAttempts + 1);
                //                        pageObject.ParentPage.AddTag(newTag);
                //                    }

                //                    var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(DivisionTool);
                //                    foreach (var viewModel in factorCardViewModels)
                //                    {
                //                        (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                //                    }
                //                    continue;
                //                }
                //                if (DivisionTool.CurrentRemainder < DivisionTool.Rows * snappingArray.Columns)
                //                {
                //                    var existingTag =
                //                            pageObject.ParentPage.Tags.OfType<DivisionToolFailedSnapTag>()
                //                                      .FirstOrDefault(x => x.Value == DivisionToolFailedSnapTag.AcceptedValues.SnappedArrayTooLarge);

                //                    var previousNumberOfAttempts = 0;
                //                    if (existingTag != null)
                //                    {
                //                        previousNumberOfAttempts = existingTag.NumberOfAttempts;
                //                        pageObject.ParentPage.RemoveTag(existingTag);
                //                    }
                //                    var newTag = new DivisionToolFailedSnapTag(pageObject.ParentPage,
                //                                                                   App.CurrentUserMode == App.UserMode.Student ? Origin.StudentPageObjectGenerated : Origin.TeacherPageObjectGenerated,
                //                                                                   DivisionToolFailedSnapTag.AcceptedValues.SnappedArrayTooLarge,
                //                                                                   previousNumberOfAttempts + 1);
                //                    pageObject.ParentPage.AddTag(newTag);

                //                    var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(DivisionTool);
                //                    foreach (var viewModel in factorCardViewModels)
                //                    {
                //                        (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                //                    }
                //                    continue;
                //                }

                //                //If first division - update IsGridOn to match new array
                //                if (DivisionTool.LastDivisionPosition == 0)
                //                {
                //                    DivisionTool.IsGridOn = snappingArray.IsGridOn;
                //                }

                //                //Add a new division and remove snapping array
                //                PageObject.ParentPage.PageObjects.Remove(PageObject);
                //                DivisionTool.SnapInArray(snappingArray.Columns);

                //                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                //                                                           new DivisionToolArraySnappedInHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, pageObject.ID, snappingArray));
                //                return;
                //            }
                //        }
                //    }
                //}
            }
        }

        public static void AnalyzeStrategy(CLPPage page, DivisionTool divisionTool)
        {
            //var dividerValues = DivisionTool.VerticalDivisions.Select(x => x.Value).ToList();

            //if (!dividerValues.Any())
            //{
            //    return;
            //}

            //if (dividerValues.Count == 2)
            //{
            //    page.AddTag(new DivisionToolStrategyTag(page,
            //                                                Origin.StudentPageGenerated,
            //                                                DivisionToolStrategyTag.AcceptedValues.OneArray,
            //                                                dividerValues));
            //    return;
            //}

            //if (Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            //{
            //    page.AddTag(new DivisionToolStrategyTag(page,
            //                                                Origin.StudentPageGenerated,
            //                                                DivisionToolStrategyTag.AcceptedValues.EvenSplit,
            //                                                dividerValues));
            //    return;
            //}

            //// HACK - This only compares the first 2 values to see if they are the same to determine Repeated Strategy. Find a way to determine this by frequency.
            //if (dividerValues.First() == dividerValues.ElementAt(1))
            //{
            //    page.AddTag(new DivisionToolStrategyTag(page,
            //                                                Origin.StudentPageGenerated,
            //                                                DivisionToolStrategyTag.AcceptedValues.Repeated,
            //                                                dividerValues));
            //    return;
            //}

            //page.AddTag(new DivisionToolStrategyTag(page,
            //                                            Origin.StudentPageGenerated,
            //                                            DivisionToolStrategyTag.AcceptedValues.Other,
            //                                            dividerValues));
        }

        public static void AnalyzeRepresentationCorrectness(CLPPage page, DivisionRelationDefinitionTag divisionRelationDefinition, DivisionTool divisionTool)
        {
            var divisionToolIDsInHistory = GetListOfDivisionToolIDsInHistory(page);

            // Apply a Completeness tag.
            var isDivisionToolComplete = false;
            if (divisionTool.VerticalDivisions.Count < 2)
            {
                var tag = new DivisionToolCompletenessTag(page,
                                                              Origin.StudentPageGenerated,
                                                              divisionTool.ID,
                                                              divisionTool.Dividend,
                                                              divisionTool.Rows,
                                                              divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                              DivisionToolCompletenessValues.NoArrays);
                page.AddTag(tag);
            }
            else if (divisionTool.VerticalDivisions.Sum(x => x.Value) == divisionTool.Columns)
            {
                var tag = new DivisionToolCompletenessTag(page,
                                                              Origin.StudentPageGenerated,
                                                              divisionTool.ID,
                                                              divisionTool.Dividend,
                                                              divisionTool.Rows,
                                                              divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                              DivisionToolCompletenessValues.Complete);
                page.AddTag(tag);
                isDivisionToolComplete = true;
            }
            else
            {
                var tag = new DivisionToolCompletenessTag(page,
                                                              Origin.StudentPageGenerated,
                                                              divisionTool.ID,
                                                              divisionTool.Dividend,
                                                              divisionTool.Rows,
                                                              divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                              DivisionToolCompletenessValues.NotEnoughArrays);
                page.AddTag(tag);
            }

            // Apply a Correctness tag.
            var incorrectReasons = new List<DivisionToolIncorrectReason>();

            // Correct
            if (divisionRelationDefinition.Dividend == divisionTool.Dividend &&
                isDivisionToolComplete &&
                divisionRelationDefinition.Divisor == divisionTool.Rows &&
                divisionRelationDefinition.Quotient == divisionTool.Columns)
            {
                var correctTag = new DivisionToolRepresentationCorrectnessTag(page,
                                                                                  Origin.StudentPageGenerated,
                                                                                  divisionTool.ID,
                                                                                  divisionTool.Dividend,
                                                                                  divisionTool.Rows,
                                                                                  divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                                                  Correctness.Correct,
                                                                                  incorrectReasons);
                page.AddTag(correctTag);
                return;
            }

            // Incorrect
            if (!isDivisionToolComplete)
            {
                incorrectReasons.Add(DivisionToolIncorrectReason.Incomplete);
            }

            if (divisionRelationDefinition.Dividend != divisionTool.Dividend &&
                divisionRelationDefinition.Divisor != divisionTool.Rows)
            {
                incorrectReasons.Add(DivisionToolIncorrectReason.WrongDividendAndDivisor);
            }
            else
            {
                if (divisionRelationDefinition.Dividend != divisionTool.Dividend)
                {
                    incorrectReasons.Add(DivisionToolIncorrectReason.WrongDividend);
                }

                if (divisionRelationDefinition.Divisor != divisionTool.Rows)
                {
                    incorrectReasons.Add(DivisionToolIncorrectReason.WrongDivisor);
                }
            }

            if (!incorrectReasons.Any())
            {
                incorrectReasons.Add(DivisionToolIncorrectReason.Other);
            }

            var incorrectTag = new DivisionToolRepresentationCorrectnessTag(page,
                                                                                Origin.StudentPageGenerated,
                                                                                divisionTool.ID,
                                                                                divisionTool.Dividend,
                                                                                divisionTool.Rows,
                                                                                divisionToolIDsInHistory.IndexOf(divisionTool.ID),
                                                                                Correctness.Incorrect,
                                                                                incorrectReasons);
            page.AddTag(incorrectTag);
        }

        public static void AnalyzeDivisionToolCorrectness(CLPPage page)
        {
            var representationCorrectnessTags = page.Tags.OfType<DivisionToolRepresentationCorrectnessTag>().ToList();

            if (!representationCorrectnessTags.Any())
            {
                return;
            }

            var isCorrectOnce = false;
            var isPartiallyCorrectOnce = false;
            var isIncorrectOnce = false;

            foreach (var arrayRepresentationCorrectnessTag in representationCorrectnessTags)
            {
                switch (arrayRepresentationCorrectnessTag.Correctness)
                {
                    case Correctness.Correct:
                        isCorrectOnce = true;
                        break;
                    case Correctness.PartiallyCorrect:
                        isPartiallyCorrectOnce = true;
                        break;
                    case Correctness.Incorrect:
                        isIncorrectOnce = true;
                        break;
                    case Correctness.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var correctnessSum = Correctness.Unknown;
            if (isPartiallyCorrectOnce)
            {
                correctnessSum = Correctness.PartiallyCorrect;
            }
            else if (isCorrectOnce && isIncorrectOnce)
            {
                correctnessSum = Correctness.PartiallyCorrect;
            }
            else if (isCorrectOnce)
            {
                correctnessSum = Correctness.Correct;
            }
            else if (isIncorrectOnce)
            {
                correctnessSum = Correctness.Incorrect;
            }

            var correctnessSummaryTag = new DivisionToolCorrectnessSummaryTag(page, Origin.StudentPageGenerated, correctnessSum);
            correctnessSummaryTag.CorrectCount = representationCorrectnessTags.Count(x => x.Correctness == Correctness.Correct);
            correctnessSummaryTag.IncorrectCount = representationCorrectnessTags.Count(x => x.Correctness == Correctness.Incorrect);
            correctnessSummaryTag.PartiallyCorrectCount = representationCorrectnessTags.Count(x => x.Correctness == Correctness.PartiallyCorrect);
            page.AddTag(correctnessSummaryTag);
        }

        public static void AnalyzeDivisionToolTroubleWithDivision(CLPPage page)
        {
            var errorSum = TroubleWithDivisionTag.GetTroubleWithFactorPairsCount(page) + TroubleWithDivisionTag.GetTroubleWithRemaindersCount(page) +
                           TroubleWithDivisionTag.GetTroubleWithDivisionToolCreationCount(page);

            if (errorSum == 0)
            {
                return;
            }

            var troubleWithDivisionTag = new TroubleWithDivisionTag(page, Origin.StudentPageGenerated);
            page.AddTag(troubleWithDivisionTag);
        }

        #endregion //Analysis

        #region Interpretation

        #endregion //Interpretation
    }
}