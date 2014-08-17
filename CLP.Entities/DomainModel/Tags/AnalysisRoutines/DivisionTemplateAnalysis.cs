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
            foreach (var tag in
                page.Tags.ToList()
                    .Where(
                           tag =>
                           tag is DivisionTemplateRepresentationCorrectnessTag || tag is DivisionTemplateStrategyTag ||
                           tag is DivisionTemplateCompletenessTag || tag is DivisionTemplateCorrectnessTag))
            {
                page.RemoveTag(tag);
            }

            var divisionDefinitionTags = page.Tags.OfType<DivisionRelationDefinitionTag>().ToList();
            var divisionTemplates = page.PageObjects.OfType<FuzzyFactorCard>().ToList();
            if (!divisionDefinitionTags.Any() ||
                !divisionTemplates.Any())
            {
                InterpretDivisionTemplateTroubleWithRemainders(page, true);
                return;
            }

            foreach (var divisionDefinitionTag in divisionDefinitionTags)
            {
                foreach (var divisionTemplate in divisionTemplates)
                {
                    AnalyzeStrategy(page, divisionTemplate);
                    AnalyzeRepresentationCorrectness(page, divisionDefinitionTag, divisionTemplate);
                }
            }

            InterpretDivisionTemplateTroubleWithRemainders(page);
            AnalyzeDivisionTemplateCorrectness(page);
        }

        #region Analysis

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
            foreach (var historyItem in completeOrderedHistory.OfType<PageObjectsRemovedHistoryItem>())
            {
                foreach (var pageObjectID in historyItem.PageObjectIDs)
                {
                    var divisionTemplate = page.GetPageObjectByID(pageObjectID) as FuzzyFactorCard ??
                                           page.History.GetPageObjectByID(pageObjectID) as FuzzyFactorCard;
                    if (divisionTemplate == null)
                    {
                        continue;
                    }

                    var arrayDimensions =
                        divisionTemplate.VerticalDivisions.Where(division => division.Value != 0)
                                        .Select(division => divisionTemplate.Rows + "x" + division.Value)
                                        .ToList();

                    page.AddTag(new DivisionTemplateDeletedTag(page,
                                                               Origin.StudentPageObjectGenerated,
                                                               divisionTemplate.ID,
                                                               divisionTemplate.Dividend,
                                                               divisionTemplate.Rows,
                                                               arrayDimensions));
                }
            }

            var divisionTemplatesOnPage = new List<DivisionTemplateAndRemainder>();
            var arraysOnPage = new List<CLPArray>();
            foreach (var historyItem in completeOrderedHistory)
            {
                var removedPageObjectsHistoryItem = historyItem as PageObjectsRemovedHistoryItem;
                if (removedPageObjectsHistoryItem != null)
                {
                    foreach (var pageObjectID in removedPageObjectsHistoryItem.PageObjectIDs)
                    {
                        divisionTemplatesOnPage.RemoveAll(x => x.DivisionTemplate.ID == pageObjectID);
                        arraysOnPage.RemoveAll(x => x.ID == pageObjectID);
                    }
                    continue;
                }

                var arraySnappedInHistoryItem = historyItem as FFCArraySnappedInHistoryItem;
                if (arraySnappedInHistoryItem != null)
                {
                    var arrayToRemove = arraysOnPage.FirstOrDefault(x => x.ID == arraySnappedInHistoryItem.SnappedInArrayID);
                    var divisionTemplateAndRemainder =
                        divisionTemplatesOnPage.FirstOrDefault(x => x.DivisionTemplate.ID == arraySnappedInHistoryItem.FuzzyFactorCardID);
                    if (divisionTemplateAndRemainder != null &&
                        arrayToRemove != null)
                    {
                        arraysOnPage.Remove(arrayToRemove);
                        divisionTemplateAndRemainder.Remainder -= (arrayToRemove.Rows * arrayToRemove.Columns);
                    }
                    continue;
                }

                //DivisionTemplateIncorrectArrayCreationTag
                var addedPageObjectHistoryItem = historyItem as PageObjectsAddedHistoryItem;
                if (addedPageObjectHistoryItem != null)
                {
                    foreach (var pageObject in
                        addedPageObjectHistoryItem.PageObjectIDs.Select(
                                                                        pageObjectID =>
                                                                        page.GetPageObjectByID(pageObjectID) ??
                                                                        page.History.GetPageObjectByID(pageObjectID)))
                    {
                        if (pageObject is FuzzyFactorCard)
                        {
                            divisionTemplatesOnPage.Add(new DivisionTemplateAndRemainder((pageObject as FuzzyFactorCard),
                                                                                         (pageObject as FuzzyFactorCard).Dividend));
                            continue;
                        }

                        var array = pageObject as CLPArray;
                        if (array == null)
                        {
                            continue;
                        }

                        arraysOnPage.Add(array);
                        var arrayArea = arraysOnPage.Sum(x => x.Rows * x.Columns);

                        foreach (var divisionTemplateAndRemainder in divisionTemplatesOnPage)
                        {
                            if (array.Columns == divisionTemplateAndRemainder.DivisionTemplate.Dividend ||
                                (array.Rows == divisionTemplateAndRemainder.DivisionTemplate.Dividend))
                            {
                                var existingTag =
                                    page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>()
                                        .FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.DividendAsDivisor);

                                var previousNumberOfAttempts = 0;
                                if (existingTag != null)
                                {
                                    previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                    page.RemoveTag(existingTag);
                                }
                                var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                           Origin.StudentPageObjectGenerated,
                                                                                           DivisionTemplateIncorrectArrayCreationTag.AcceptedValues
                                                                                                                                    .DividendAsDivisor,
                                                                                           previousNumberOfAttempts + 1);
                                page.AddTag(newTag);
                            }

                            if (array.Rows != divisionTemplateAndRemainder.DivisionTemplate.Rows &&
                                array.Columns == divisionTemplateAndRemainder.DivisionTemplate.Rows)
                            {
                                var existingTag =
                                    page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>()
                                        .FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.WrongOrientation);

                                var previousNumberOfAttempts = 0;
                                if (existingTag != null)
                                {
                                    previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                    page.RemoveTag(existingTag);
                                }
                                var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                           Origin.StudentPageObjectGenerated,
                                                                                           DivisionTemplateIncorrectArrayCreationTag.AcceptedValues
                                                                                                                                    .WrongOrientation,
                                                                                           previousNumberOfAttempts + 1);
                                page.AddTag(newTag);
                            }
                            else if (array.Rows != divisionTemplateAndRemainder.DivisionTemplate.Rows)
                            {
                                var existingTag =
                                    page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>()
                                        .FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.IncorrectDimension);

                                var previousNumberOfAttempts = 0;
                                if (existingTag != null)
                                {
                                    previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                    page.RemoveTag(existingTag);
                                }
                                var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                           Origin.StudentPageObjectGenerated,
                                                                                           DivisionTemplateIncorrectArrayCreationTag.AcceptedValues
                                                                                                                                    .IncorrectDimension,
                                                                                           previousNumberOfAttempts + 1);
                                page.AddTag(newTag);
                            }

                            if (arrayArea > divisionTemplateAndRemainder.Remainder)
                            {
                                var existingTag =
                                    page.Tags.OfType<DivisionTemplateIncorrectArrayCreationTag>()
                                        .FirstOrDefault(x => x.Value == DivisionTemplateIncorrectArrayCreationTag.AcceptedValues.ArrayTooLarge);

                                var previousNumberOfAttempts = 0;
                                if (existingTag != null)
                                {
                                    previousNumberOfAttempts = existingTag.NumberOfAttempts;
                                    page.RemoveTag(existingTag);
                                }
                                var newTag = new DivisionTemplateIncorrectArrayCreationTag(page,
                                                                                           Origin.StudentPageObjectGenerated,
                                                                                           DivisionTemplateIncorrectArrayCreationTag.AcceptedValues
                                                                                                                                    .ArrayTooLarge,
                                                                                           previousNumberOfAttempts + 1);
                                page.AddTag(newTag);
                            }
                        }
                    }
                    continue;
                }

                //DivisionTemplateFailedSnapTag
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
                //    foreach (var divisionTemplateAndRemainder in divisionTemplatesOnPage)
                //    {
                //        var divisionTemplate = divisionTemplateAndRemainder.DivisionTemplate;

                //        var top = Math.Max(endPosition.Y + array.LabelLength, divisionTemplate.YPosition + divisionTemplate.LabelLength);
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
                //            var diff = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.LabelLength + divisionTemplate.LastDivisionPosition));
                //            if (diff < 50)
                //            {
                //                if (snappingArray.Rows != divisionTemplate.Rows)
                //                {
                //                    var hasTag = false;
                //                    if (snappingArray.Columns == divisionTemplate.Rows)
                //                    {
                //                        var existingTag =
                //                            pageObject.ParentPage.Tags.OfType<DivisionTemplateFailedSnapTag>()
                //                                      .FirstOrDefault(x => x.Value == DivisionTemplateFailedSnapTag.AcceptedValues.SnappedWrongOrientation);

                //                        var previousNumberOfAttempts = 0;
                //                        if (existingTag != null)
                //                        {
                //                            previousNumberOfAttempts = existingTag.NumberOfAttempts;
                //                            pageObject.ParentPage.RemoveTag(existingTag);
                //                        }
                //                        var newTag = new DivisionTemplateFailedSnapTag(pageObject.ParentPage,
                //                                                                       App.CurrentUserMode == App.UserMode.Student ? Origin.StudentPageObjectGenerated : Origin.TeacherPageObjectGenerated,
                //                                                                       DivisionTemplateFailedSnapTag.AcceptedValues.SnappedWrongOrientation,
                //                                                                       previousNumberOfAttempts + 1);
                //                        pageObject.ParentPage.AddTag(newTag);
                //                    }
                //                    else
                //                    {
                //                        var existingTag =
                //                            pageObject.ParentPage.Tags.OfType<DivisionTemplateFailedSnapTag>()
                //                                      .FirstOrDefault(x => x.Value == DivisionTemplateFailedSnapTag.AcceptedValues.SnappedIncorrectDimension);

                //                        var previousNumberOfAttempts = 0;
                //                        if (existingTag != null)
                //                        {
                //                            previousNumberOfAttempts = existingTag.NumberOfAttempts;
                //                            pageObject.ParentPage.RemoveTag(existingTag);
                //                        }
                //                        var newTag = new DivisionTemplateFailedSnapTag(pageObject.ParentPage,
                //                                                                       App.CurrentUserMode == App.UserMode.Student ? Origin.StudentPageObjectGenerated : Origin.TeacherPageObjectGenerated,
                //                                                                       DivisionTemplateFailedSnapTag.AcceptedValues.SnappedIncorrectDimension,
                //                                                                       previousNumberOfAttempts + 1);
                //                        pageObject.ParentPage.AddTag(newTag);
                //                    }

                //                    var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(divisionTemplate);
                //                    foreach (var viewModel in factorCardViewModels)
                //                    {
                //                        (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                //                    }
                //                    continue;
                //                }
                //                if (divisionTemplate.CurrentRemainder < divisionTemplate.Rows * snappingArray.Columns)
                //                {
                //                    //TODO Liz - get old position - maybe from move batch? (Steve will email about this)
                //                    //var oldX = 10.0;
                //                    //var oldY = 10.0;
                //                    //APageObjectBaseViewModel.ChangePageObjectPosition(snappingArray, oldX, oldY, false);

                //                    var existingTag =
                //                            pageObject.ParentPage.Tags.OfType<DivisionTemplateFailedSnapTag>()
                //                                      .FirstOrDefault(x => x.Value == DivisionTemplateFailedSnapTag.AcceptedValues.SnappedArrayTooLarge);

                //                    var previousNumberOfAttempts = 0;
                //                    if (existingTag != null)
                //                    {
                //                        previousNumberOfAttempts = existingTag.NumberOfAttempts;
                //                        pageObject.ParentPage.RemoveTag(existingTag);
                //                    }
                //                    var newTag = new DivisionTemplateFailedSnapTag(pageObject.ParentPage,
                //                                                                   App.CurrentUserMode == App.UserMode.Student ? Origin.StudentPageObjectGenerated : Origin.TeacherPageObjectGenerated,
                //                                                                   DivisionTemplateFailedSnapTag.AcceptedValues.SnappedArrayTooLarge,
                //                                                                   previousNumberOfAttempts + 1);
                //                    pageObject.ParentPage.AddTag(newTag);

                //                    var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(divisionTemplate);
                //                    foreach (var viewModel in factorCardViewModels)
                //                    {
                //                        (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                //                    }
                //                    continue;
                //                }

                //                //If first division - update IsGridOn to match new array
                //                if (divisionTemplate.LastDivisionPosition == 0)
                //                {
                //                    divisionTemplate.IsGridOn = snappingArray.IsGridOn;
                //                }

                //                //Add a new division and remove snapping array
                //                PageObject.ParentPage.PageObjects.Remove(PageObject);
                //                divisionTemplate.SnapInArray(snappingArray.Columns);

                //                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                //                                                           new FFCArraySnappedInHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, pageObject.ID, snappingArray));
                //                return;
                //            }
                //        }
                //    }
                //}
            }
        }

        public static void AnalyzeStrategy(CLPPage page, FuzzyFactorCard divisionTemplate)
        {
            var dividerValues = divisionTemplate.VerticalDivisions.Select(x => x.Value).ToList();

            if (!dividerValues.Any())
            {
                return;
            }

            if (dividerValues.Count == 2)
            {
                page.AddTag(new DivisionTemplateStrategyTag(page,
                                                            Origin.StudentPageGenerated,
                                                            DivisionTemplateStrategyTag.AcceptedValues.OneArray,
                                                            dividerValues));
                return;
            }

            if (Math.Abs(dividerValues.First() - dividerValues.Average()) < 0.001)
            {
                page.AddTag(new DivisionTemplateStrategyTag(page,
                                                            Origin.StudentPageGenerated,
                                                            DivisionTemplateStrategyTag.AcceptedValues.EvenSplit,
                                                            dividerValues));
                return;
            }

            // HACK - This only compares the first 2 values to see if they are the same to determine Repeated Strategy. Find a way to determine this by frequency.
            if (dividerValues.First() == dividerValues.ElementAt(1))
            {
                page.AddTag(new DivisionTemplateStrategyTag(page,
                                                            Origin.StudentPageGenerated,
                                                            DivisionTemplateStrategyTag.AcceptedValues.Repeated,
                                                            dividerValues));
                return;
            }

            page.AddTag(new DivisionTemplateStrategyTag(page,
                                                        Origin.StudentPageGenerated,
                                                        DivisionTemplateStrategyTag.AcceptedValues.Other,
                                                        dividerValues));
        }

        public static void AnalyzeRepresentationCorrectness(CLPPage page,
                                                            DivisionRelationDefinitionTag divisionRelationDefinition,
                                                            FuzzyFactorCard divisionTemplate)
        {
            // Apply a Completeness tag.
            var isDivisionTemplateComplete = false;
            if (divisionTemplate.VerticalDivisions.Count < 2)
            {
                var tag = new DivisionTemplateCompletenessTag(page,
                                                              Origin.StudentPageGenerated,
                                                              DivisionTemplateCompletenessTag.AcceptedValues.NoArrays);
                page.AddTag(tag);
            }
            else if (divisionTemplate.VerticalDivisions.Sum(x => x.Value) == divisionTemplate.Columns)
            {
                var tag = new DivisionTemplateCompletenessTag(page,
                                                              Origin.StudentPageGenerated,
                                                              DivisionTemplateCompletenessTag.AcceptedValues.Complete);
                page.AddTag(tag);
                isDivisionTemplateComplete = true;
            }
            else
            {
                var tag = new DivisionTemplateCompletenessTag(page,
                                                              Origin.StudentPageGenerated,
                                                              DivisionTemplateCompletenessTag.AcceptedValues.NotEnoughArrays);
                page.AddTag(tag);
            }

            // Apply a Correctness tag.
            var incorrectReasons = new List<DivisionTemplateIncorrectReason>();

            // Correct
            if (divisionRelationDefinition.Dividend == divisionTemplate.Dividend &&
                isDivisionTemplateComplete &&
                divisionRelationDefinition.Divisor == divisionTemplate.Rows &&
                divisionRelationDefinition.Quotient == divisionTemplate.Columns)
            {
                var correctTag = new DivisionTemplateRepresentationCorrectnessTag(page,
                                                                                  Origin.StudentPageGenerated,
                                                                                  Correctness.Correct,
                                                                                  incorrectReasons);
                page.AddTag(correctTag);
                return;
            }

            // Incorrect
            if (!isDivisionTemplateComplete)
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.Incomplete);
            }

            if (divisionRelationDefinition.Dividend != divisionTemplate.Dividend &&
                divisionRelationDefinition.Divisor != divisionTemplate.Rows)
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.WrongDividendAndDivisor);
            }
            else
            {
                if (divisionRelationDefinition.Dividend != divisionTemplate.Dividend)
                {
                    incorrectReasons.Add(DivisionTemplateIncorrectReason.WrongDividend);
                }

                if (divisionRelationDefinition.Divisor != divisionTemplate.Rows)
                {
                    incorrectReasons.Add(DivisionTemplateIncorrectReason.WrongDivisor);
                }
            }

            if (!incorrectReasons.Any())
            {
                incorrectReasons.Add(DivisionTemplateIncorrectReason.Other);
            }

            var incorrectTag = new DivisionTemplateRepresentationCorrectnessTag(page,
                                                                                Origin.StudentPageGenerated,
                                                                                Correctness.Incorrect,
                                                                                incorrectReasons);
            page.AddTag(incorrectTag);
        }

        public static void AnalyzeDivisionTemplateCorrectness(CLPPage page)
        {
            var representationCorrectnessTags = page.Tags.OfType<DivisionTemplateRepresentationCorrectnessTag>().ToList();

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

            page.AddTag(new DivisionTemplateCorrectnessTag(page, Origin.StudentPageGenerated, correctnessSum));
        }

        #endregion //Analysis

        #region Interpretation

        public static void InterpretDivisionTemplateTroubleWithRemainders(CLPPage page, bool purgeAllTempTags = false)
        {
            var troubleWithRemaindersTags = page.Tags.OfType<DivisionTemplateTroubleWithRemaindersTag>().ToList();
            const int ATTEMPT_TOLERANCE = 4;

            foreach (var troubleWithRemaindersTag in troubleWithRemaindersTags)
            {
                var trackedVariablesSum = troubleWithRemaindersTag.ArrayTooLargeAttempts + troubleWithRemaindersTag.FailedSnapAttempts +
                                          troubleWithRemaindersTag.OrientationChangedAttempts;


                if (trackedVariablesSum < ATTEMPT_TOLERANCE ||
                    purgeAllTempTags)
                {
                    page.RemoveTag(troubleWithRemaindersTag);
                }
            }
        }

        #endregion //Interpretation
    }
}