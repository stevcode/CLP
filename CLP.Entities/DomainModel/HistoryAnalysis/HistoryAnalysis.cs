using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Collections;
using CLP.InkInterpretation;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void GenerateHistoryActions(CLPPage page)
        {
            HistoryAction.CurrentIncrementID.Clear();
            HistoryAction.MaxIncrementID.Clear();
            page.History.HistoryActions.Clear();

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "HistoryActions");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, PageNameComposite.ParsePage(page).ToFileName() + ".txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, "*****Coded Actions/Steps*****" + "\n\n");

            // First Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "1"
                                            });
            var initialHistoryActions = GenerateInitialHistoryActions(page);
            page.History.HistoryActions.AddRange(initialHistoryActions);
            
            File.AppendAllText(filePath, "PASS [1]" + "\n");
            foreach (var item in initialHistoryActions)
            {
                var semi = item == initialHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Second Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2"
                                            });
            var refinedInkHistoryActions = RefineInkHistoryActions(page, initialHistoryActions);
            page.History.HistoryActions.AddRange(refinedInkHistoryActions);

            File.AppendAllText(filePath, "\nPASS [2]" + "\n");
            foreach (var item in refinedInkHistoryActions)
            {
                var semi = item == refinedInkHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Third Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "3"
                                            });
            var interpretedHistoryActions = InterpretHistoryActions(page, refinedInkHistoryActions);
            page.History.HistoryActions.AddRange(interpretedHistoryActions);

            File.AppendAllText(filePath, "\nPASS [3]" + "\n");
            foreach (var item in interpretedHistoryActions)
            {
                var semi = item == interpretedHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Last Pass
            GenerateTags(page, interpretedHistoryActions);

            File.AppendAllText(filePath, "\n\n\n*****Tags*****" + "\n\n");
            foreach (var tag in page.Tags)
            {
                File.AppendAllText(filePath, "*" + tag.FormattedName + "*\n");
                File.AppendAllText(filePath, tag.FormattedValue + "\n\n");
            }

            File.AppendAllText(filePath, "\n*****History Items*****" + "\n\n");
            foreach (var historyItem in page.History.CompleteOrderedHistoryItems)
            {
                File.AppendAllText(filePath, historyItem.FormattedValue + "\n");
            }
        }

        #region First Pass: Initialization

        public static List<IHistoryAction> GenerateInitialHistoryActions(CLPPage page)
        {
            var historyItemBuffer = new List<IHistoryItem>();
            var initialHistoryActions = new List<IHistoryAction>();
            var historyItems = page.History.CompleteOrderedHistoryItems;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var singleHistoryAction = VerifyAndGenerateSingleItemAction(page, historyItemBuffer.First());
                    if (singleHistoryAction != null)
                    {
                        initialHistoryActions.Add(singleHistoryAction);
                        historyItemBuffer.Clear();
                        continue;
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                var compoundHistoryAction = VerifyAndGenerateCompoundItemAction(page, historyItemBuffer, nextHistoryItem);
                if (compoundHistoryAction != null)
                {
                    initialHistoryActions.Add(compoundHistoryAction);
                    historyItemBuffer.Clear();
                }
            }

            return initialHistoryActions;
        }

        public static IHistoryAction VerifyAndGenerateSingleItemAction(CLPPage page, IHistoryItem historyItem)
        {
            if (historyItem == null)
            {
                return null;
            }

            IHistoryAction historyAction = null;
            TypeSwitch.On(historyItem)
                      .Case<ObjectsOnPageChangedHistoryItem>(h => { historyAction = ObjectCodedActions.Add(page, h) ?? ObjectCodedActions.Delete(page, h); })
                      .Case<CLPArrayRotateHistoryItem>(h => { historyAction = ArrayCodedActions.Rotate(page, h); })
                      .Case<PageObjectCutHistoryItem>(h => { historyAction = ArrayCodedActions.Cut(page, h); })
                      .Case<CLPArraySnapHistoryItem>(h => { historyAction = ArrayCodedActions.Snap(page, h); })
                      .Case<CLPArrayDivisionsChangedHistoryItem>(h => { historyAction = ArrayCodedActions.Divide(page, h); });

            return historyAction;
        }

        public static IHistoryAction VerifyAndGenerateCompoundItemAction(CLPPage page, List<IHistoryItem> historyItems, IHistoryItem nextHistoryItem)
        {
            if (!historyItems.Any())
            {
                return null;
            }

            if (historyItems.All(h => h is ObjectsOnPageChangedHistoryItem))
            {
                var objectsChangedHistoryItems = historyItems.Cast<ObjectsOnPageChangedHistoryItem>().ToList();
                // TODO: Edge case that recognizes multiple bins added at once.

                if (objectsChangedHistoryItems.All(h => h.IsUsingStrokes && !h.IsUsingPageObjects))
                {
                    var nextObjectsChangedHistoryItem = nextHistoryItem as ObjectsOnPageChangedHistoryItem;
                    if (nextObjectsChangedHistoryItem != null &&
                        nextObjectsChangedHistoryItem.IsUsingStrokes &&
                        !nextObjectsChangedHistoryItem.IsUsingPageObjects)
                    {
                        // HACK: Another temp hack to recognize multiple choice box answers. Normally just return null.
                        var h = VerifyAndGenerateSingleItemAction(page, nextHistoryItem);
                        if (h == null)
                        {
                            return null;
                        }
                    }

                    var historyAction = InkCodedActions.ChangeOrIgnore(page, objectsChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is ObjectsMovedBatchHistoryItem))
            {
                var objectsMovedHistoryItems = historyItems.Cast<ObjectsMovedBatchHistoryItem>().ToList();

                var firstIDSequence = objectsMovedHistoryItems.First().PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList();
                if (objectsMovedHistoryItems.All(h => firstIDSequence.SequenceEqual(h.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList())))
                {
                    var nextMovedHistoryItem = nextHistoryItem as ObjectsMovedBatchHistoryItem;
                    if (nextMovedHistoryItem != null &&
                        firstIDSequence.SequenceEqual(nextMovedHistoryItem.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList()))
                    {
                        return null;
                    }

                    var historyAction = ObjectCodedActions.Move(page, objectsMovedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is PageObjectResizeBatchHistoryItem))
            {
                var objectsResizedHistoryItems = historyItems.Cast<PageObjectResizeBatchHistoryItem>().ToList();

                var firstID = objectsResizedHistoryItems.First().PageObjectID;
                if (objectsResizedHistoryItems.All(h => h.PageObjectID == firstID))
                {
                    var nextResizedHistoryItem = nextHistoryItem as PageObjectResizeBatchHistoryItem;
                    if (nextResizedHistoryItem != null &&
                        firstID == nextResizedHistoryItem.PageObjectID)
                    {
                        return null;
                    }

                    var historyAction = ObjectCodedActions.Resize(page, objectsResizedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is NumberLineEndPointsChangedHistoryItem))
            {
                var endPointsChangedHistoryItems = historyItems.Cast<NumberLineEndPointsChangedHistoryItem>().ToList();

                var firstNumberLineID = endPointsChangedHistoryItems.First().NumberLineID;
                if (endPointsChangedHistoryItems.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextEndPointsChangedHistoryItem = nextHistoryItem as NumberLineEndPointsChangedHistoryItem;
                    if (nextEndPointsChangedHistoryItem != null &&
                        nextEndPointsChangedHistoryItem.NumberLineID == firstNumberLineID)
                    {
                        return null;
                    }

                    var historyAction = NumberLineCodedActions.EndPointsChange(page, endPointsChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is NumberLineJumpSizesChangedHistoryItem))
            {
                var jumpSizesChangedHistoryItems = historyItems.Cast<NumberLineJumpSizesChangedHistoryItem>().ToList();

                var firstNumberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;
                var isAdding = jumpSizesChangedHistoryItems.First().JumpsAdded.Any() && !jumpSizesChangedHistoryItems.First().JumpsRemoved.Any();
                if (jumpSizesChangedHistoryItems.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextJumpsChangedHistoryItem = nextHistoryItem as NumberLineJumpSizesChangedHistoryItem;
                    if (nextJumpsChangedHistoryItem != null &&
                        nextJumpsChangedHistoryItem.NumberLineID == firstNumberLineID &&
                        isAdding == (nextJumpsChangedHistoryItem.JumpsAdded.Any() && !nextJumpsChangedHistoryItem.JumpsRemoved.Any()))
                    {
                        return null;
                    }

                    var historyAction = NumberLineCodedActions.JumpSizesChange(page, jumpSizesChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is MultipleChoiceBubbleStatusChangedHistoryItem))
            {
                var statusChangedHistoryItems = historyItems.Cast<MultipleChoiceBubbleStatusChangedHistoryItem>().ToList();
                var currentMultipleChoiceID = statusChangedHistoryItems.First().MultipleChoiceID;
                var currentBubbleIndex = statusChangedHistoryItems.First().ChoiceBubbleIndex;
                
                if (statusChangedHistoryItems.All(h => h.MultipleChoiceID == currentMultipleChoiceID))
                {
                    var nextStatusChangedHistoryItems = nextHistoryItem as MultipleChoiceBubbleStatusChangedHistoryItem;

                    if (_currentCompressedStatus == null)
                    {
                        _currentCompressedStatus = statusChangedHistoryItems.First().ChoiceBubbleStatus;
                    }

                    ChoiceBubbleStatuses? compressedStatus = null;
                    if (nextStatusChangedHistoryItems !=null)
                    {
                        compressedStatus = CompressMultipleChoiceStatuses(_currentCompressedStatus, nextStatusChangedHistoryItems.ChoiceBubbleStatus);
                    }

                    if (nextStatusChangedHistoryItems != null &&
                        nextStatusChangedHistoryItems.MultipleChoiceID == currentMultipleChoiceID &&
                        nextStatusChangedHistoryItems.ChoiceBubbleIndex == currentBubbleIndex &&
                        compressedStatus != null)
                    {
                        _currentCompressedStatus = compressedStatus;
                        return null;
                    }

                    var multipleChoice = page.GetPageObjectByIDOnPageOrInHistory(currentMultipleChoiceID);
                    if (multipleChoice == null)
                    {
                        return null;
                    }
                    var bubble = statusChangedHistoryItems.First().Bubble;
                    var correctness = bubble.IsACorrectValue ? "COR" : "INC";

                    var objectAction = string.Empty;
                    switch (_currentCompressedStatus)
                    {
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                            objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD_PARTIAL;
                            break;
                        case ChoiceBubbleStatuses.FilledIn:
                            objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD;
                            break;
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                            objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD_ADDITIONAL;
                            break;
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                            objectAction = Codings.ACTION_MULTIPLE_CHOICE_ERASE_PARTIAL;
                            break;
                        case ChoiceBubbleStatuses.IncompletelyErased:
                            objectAction = Codings.ACTION_MULTIPLE_CHOICE_ERASE_INCOMPLETE;
                            break;
                        case ChoiceBubbleStatuses.CompletelyErased:
                            objectAction = Codings.ACTION_MULTIPLE_CHOICE_ERASE;
                            break;
                        case null:
                            return null;
                        default:
                            return null;
                    }

                    _currentCompressedStatus = null;
                    var historyAction = new HistoryAction(page, historyItems)
                                        {
                                            CodedObject = Codings.OBJECT_MULTIPLE_CHOICE,
                                            CodedObjectAction = objectAction,
                                            IsObjectActionVisible = objectAction != Codings.ACTION_MULTIPLE_CHOICE_ADD,
                                            CodedObjectID = multipleChoice.CodedID,
                                            CodedObjectActionID = string.Format("{0}, {1}", bubble.BubbleCodedID, correctness)
                                        };
                    return historyAction;
                }
            }

            return null;
        }

        private static ChoiceBubbleStatuses? _currentCompressedStatus;

        private static ChoiceBubbleStatuses? CompressMultipleChoiceStatuses(ChoiceBubbleStatuses? currentStatus, ChoiceBubbleStatuses nextStatus)
        {
            switch (currentStatus)
            {
                case ChoiceBubbleStatuses.PartiallyFilledIn:
                    switch (nextStatus)
                    {
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                            return ChoiceBubbleStatuses.PartiallyFilledIn;
                        case ChoiceBubbleStatuses.FilledIn:
                            return ChoiceBubbleStatuses.FilledIn;
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                        case ChoiceBubbleStatuses.IncompletelyErased:
                        case ChoiceBubbleStatuses.CompletelyErased:
                            return null;
                    }
                    break;
                case ChoiceBubbleStatuses.FilledIn:
                    switch (nextStatus)
                    {
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                            return ChoiceBubbleStatuses.FilledIn;
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                        case ChoiceBubbleStatuses.FilledIn:
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                        case ChoiceBubbleStatuses.IncompletelyErased:
                        case ChoiceBubbleStatuses.CompletelyErased:
                            return null;
                    }
                    break;
                case ChoiceBubbleStatuses.AdditionalFilledIn:
                    switch (nextStatus)
                    {
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                            return ChoiceBubbleStatuses.AdditionalFilledIn;
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                        case ChoiceBubbleStatuses.FilledIn:
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                        case ChoiceBubbleStatuses.IncompletelyErased:
                        case ChoiceBubbleStatuses.CompletelyErased:
                            return null;
                    }
                    break;
                case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                    switch (nextStatus)
                    {
                        case ChoiceBubbleStatuses.FilledIn:
                            return ChoiceBubbleStatuses.FilledIn;
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                            return ChoiceBubbleStatuses.ErasedPartiallyFilledIn;
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                        case ChoiceBubbleStatuses.IncompletelyErased:
                        case ChoiceBubbleStatuses.CompletelyErased:
                            return null;
                    }
                    break;
                case ChoiceBubbleStatuses.IncompletelyErased:
                    switch (nextStatus)
                    {
                        case ChoiceBubbleStatuses.IncompletelyErased:
                            return ChoiceBubbleStatuses.IncompletelyErased;
                        case ChoiceBubbleStatuses.CompletelyErased:
                            return ChoiceBubbleStatuses.CompletelyErased;
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                        case ChoiceBubbleStatuses.FilledIn:
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                            return null;
                    }
                    break;
                case ChoiceBubbleStatuses.CompletelyErased:
                    switch (nextStatus)
                    {
                        case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                            return ChoiceBubbleStatuses.CompletelyErased;
                        case ChoiceBubbleStatuses.AdditionalFilledIn:
                        case ChoiceBubbleStatuses.PartiallyFilledIn:
                        case ChoiceBubbleStatuses.FilledIn:
                        case ChoiceBubbleStatuses.IncompletelyErased:
                        case ChoiceBubbleStatuses.CompletelyErased:
                            return null;
                    }
                    break;
                case null:
                    return null;
                default:
                    return null;
            }

            return null;
        }

        #endregion // First Pass: Initialization

        #region Second Pass: Ink Refinement

        public const double MAX_DISTANCE_Z_SCORE = 3.0;
        public const double DIMENSION_MULTIPLIER_THRESHOLD = 3.0;

        public static List<IHistoryAction> RefineInkHistoryActions(CLPPage page, List<IHistoryAction> historyActions)
        {
            InkCodedActions.GenerateInitialClusterings(page, historyActions);

            // TODO: Do this before clustering.
            var preProcessedInkActions = new List<IHistoryAction>();
            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var refinedInkActions = InkCodedActions.PreProcessInkChangeHistoryActions(page, historyAction);
                    preProcessedInkActions.AddRange(refinedInkActions);
                }
                else
                {
                    preProcessedInkActions.Add(historyAction);
                }
            }

            var refinedHistoryActions = new List<IHistoryAction>();
            foreach (var historyAction in preProcessedInkActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var refinedInkActions = InkCodedActions.ProcessInkChangeHistoryAction(page, historyAction);
                    refinedHistoryActions.AddRange(refinedInkActions);
                }
                else
                {
                    refinedHistoryActions.Add(historyAction);
                }
            }

            return refinedHistoryActions;
        }

        #endregion // Second Pass: Ink Refinement

        #region Third Pass: Interpretation

        public static List<IHistoryAction> InterpretHistoryActions(CLPPage page, List<IHistoryAction> historyActions)
        {
            var allInterpretedHistoryActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK)
                {
                    var interpretedHistoryActions = AttemptHistoryActionInterpretation(page, historyAction);
                    allInterpretedHistoryActions.AddRange(interpretedHistoryActions);
                }
                else
                {
                    allInterpretedHistoryActions.Add(historyAction);
                }
            }

            return allInterpretedHistoryActions;
        }

        public static List<IHistoryAction> AttemptHistoryActionInterpretation(CLPPage page, IHistoryAction historyaction)
        {
            var allInterpretedActions = new List<IHistoryAction>();

            if (historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_FILL_IN))
            {
                // HACK: discuss structure of history action

                var interpretedAction = InkCodedActions.FillInInterpretation(page, historyaction); // TODO: Potentionally needs a recursive pass through.
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            if ((historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) ||
                historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER)) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedAction = ArrayCodedActions.SkipCounting(page, historyaction);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            if (historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedAction = ArrayCodedActions.ArrayEquation(page, historyaction);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            if (!historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER))
            {
                var interpretedAction = InkCodedActions.Arithmetic(page, historyaction);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            // TODO: Attempt to interpret inked circles around a multiple choice bubbles

            if (!allInterpretedActions.Any())
            {
                allInterpretedActions.Add(historyaction);
            }

            return allInterpretedActions;
        }

        #endregion // Third Pass: Interpretation

        // 4th pass: simple pattern interpretations

        // 5th pass: complex pattern interpretations

        // 6th pass: Tag generation

        #region Last Pass: Tag Generation

        public static void GenerateTags(CLPPage page, List<IHistoryAction> historyActions)
        {
            AttemptAnswerBeforeRepresentationTag(page, historyActions);
            AttemptAnswerChangedAfterRepresentationTag(page, historyActions);
            AttemptAnswerTag(page, historyActions);
            AttemptRepresentationsUsedTag(page, historyActions);
            AttemptArrayStrategiesTag(page, historyActions);
            AttemptRepresentationCorrectness(page, historyActions);
        }

        // TODO: Move each Attempt method to the Tag's class

        public static void AttemptRepresentationCorrectness(CLPPage page, List<IHistoryAction> historyActions)
        {
            if (!historyActions.Any())
            {
                return;
            }

            var relationDefinitionTag = page.Tags.FirstOrDefault(t => t is DivisionRelationDefinitionTag ||
                                                           t is MultiplicationRelationDefinitionTag ||
                                                           t is AdditionRelationDefinitionTag);

            var definitionRelation = new Relation();
            var otherDefinitionRelation = new Relation();
            var isOtherDefinitionUsed = false;
            var altDefinitionRelation = new Relation();
            var isAltDefinitionUsed = false;

            var div = relationDefinitionTag as DivisionRelationDefinitionTag;
            if (div != null)
            {
                definitionRelation.groupSize = div.Divisor;
                definitionRelation.numberOfGroups = div.Quotient;
                definitionRelation.product = div.Dividend;
                definitionRelation.isOrderedGroup = true;
                definitionRelation.isProductImportant = true;
            }

            var mult = relationDefinitionTag as MultiplicationRelationDefinitionTag;
            if (mult != null)
            {
                definitionRelation.groupSize = mult.Factors.First().RelationPartAnswerValue;
                definitionRelation.numberOfGroups = mult.Factors.Last().RelationPartAnswerValue;
                definitionRelation.product = mult.Product;
                definitionRelation.isOrderedGroup = mult.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups;
                definitionRelation.isProductImportant = true;
            }

            var add = relationDefinitionTag as AdditionRelationDefinitionTag;
            if (add != null)
            {
                var m1 = add.Addends.First() as MultiplicationRelationDefinitionTag;
                var m2 = add.Addends.Last() as MultiplicationRelationDefinitionTag;

                definitionRelation.groupSize = m1.Factors.Last().RelationPartAnswerValue;
                definitionRelation.numberOfGroups = m1.Factors.First().RelationPartAnswerValue;
                definitionRelation.product = m1.Product;
                definitionRelation.isOrderedGroup = m1.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups;
                definitionRelation.isProductImportant = true;

                isOtherDefinitionUsed = true;
                otherDefinitionRelation.groupSize = m2.Factors.Last().RelationPartAnswerValue;
                otherDefinitionRelation.numberOfGroups = m2.Factors.First().RelationPartAnswerValue;
                otherDefinitionRelation.product = m2.Product;
                otherDefinitionRelation.isOrderedGroup = m2.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups;
                otherDefinitionRelation.isProductImportant = true;

                if (definitionRelation.groupSize == otherDefinitionRelation.groupSize)
                {
                    isAltDefinitionUsed = true;
                    altDefinitionRelation.groupSize = definitionRelation.groupSize;
                    altDefinitionRelation.numberOfGroups = definitionRelation.numberOfGroups + otherDefinitionRelation.numberOfGroups;
                    altDefinitionRelation.product = altDefinitionRelation.groupSize * altDefinitionRelation.numberOfGroups;
                    altDefinitionRelation.isOrderedGroup = true;
                    altDefinitionRelation.isProductImportant = true;
                }
            }

            var keyIndexes =
                historyActions.Where(h => h.CodedObjectAction == Codings.ACTION_OBJECT_DELETE && (h.CodedObject == Codings.OBJECT_ARRAY || h.CodedObject == Codings.OBJECT_NUMBER_LINE))
                              .Select(h => h.HistoryItems.First().HistoryIndex - 1).ToList();
            if (!page.History.CompleteOrderedHistoryItems.Any())
            {
                return;
            }
            keyIndexes.Add(page.History.CompleteOrderedHistoryItems.Last().HistoryIndex);
            keyIndexes.Reverse();
            var usedPageObjectIDs = new List<string>();
            var finalPageObjectIDs = page.PageObjects.Where(p => p is CLPArray || p is NumberLine).Select(p => p.ID).ToList();
            var analysisCodes = new List<string>();
            foreach (var index in keyIndexes)
            {
                var pageObjectOnPage = ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, index).Where(p => p is CLPArray || p is NumberLine).ToList();
                foreach (var pageObject in pageObjectOnPage)
                {
                    Relation representationRelation = null;
                    var usedID = string.Empty;
                    var codedObject = string.Empty;
                    var codedID = string.Empty;
                    var array = pageObject as CLPArray;
                    if (array != null)
                    {
                        codedObject = Codings.OBJECT_ARRAY;
                        codedID = array.GetCodedIDAtHistoryIndex(index);
                        var colsAndRows = array.GetColumnsAndRowsAtHistoryIndex(index);
                        usedID = array.ID;
                        representationRelation = new Relation
                        {
                            groupSize = colsAndRows.X,
                            numberOfGroups = colsAndRows.Y,
                            product = colsAndRows.X * colsAndRows.Y,
                            isOrderedGroup = false,
                            isProductImportant = false
                        };
                    }

                    var numberLine = pageObject as NumberLine;
                    if (numberLine != null)
                    {
                        codedObject = Codings.OBJECT_NUMBER_LINE;
                        codedID = numberLine.GetCodedIDAtHistoryIndex(index);
                        usedID = numberLine.ID;
                        var firstGroupSize = -1;
                        var firstJump = numberLine.JumpSizes.FirstOrDefault();
                        if (firstJump != null)
                        {
                            firstGroupSize = firstJump.JumpSize;
                        }
                        var isEqualGroups = numberLine.JumpSizes.All(j => j.JumpSize == firstGroupSize);

                        var product = -1;
                        var lastJump = numberLine.JumpSizes.LastOrDefault();
                        if (lastJump != null)
                        {
                            product = lastJump.StartingTickIndex + lastJump.JumpSize;
                        }

                        var jumpSizesIgnoringOverlaps = numberLine.JumpSizes.GroupBy(j => j.StartingTickIndex).Select(g => g.First()).ToList();

                        representationRelation = new Relation
                        {
                            groupSize = isEqualGroups ? firstGroupSize : -1,
                            numberOfGroups = jumpSizesIgnoringOverlaps.Count,
                            product = product,
                            isOrderedGroup = true,
                            isProductImportant = true
                        };
                    }

                    if (representationRelation == null || usedPageObjectIDs.Contains(usedID))
                    {
                        continue;
                    }

                    usedPageObjectIDs.Add(usedID);
                    var isFinal = finalPageObjectIDs.Contains(usedID);

                    var altCorrectness = Correctness.Unknown;
                    var otherCorrectness = Correctness.Unknown;
                    if (isAltDefinitionUsed)
                    {
                        altCorrectness = CompareRelationToRepresentations(representationRelation, altDefinitionRelation);
                    }
                    if (isOtherDefinitionUsed)
                    {
                        otherCorrectness = CompareRelationToRepresentations(representationRelation, otherDefinitionRelation);
                    }
                    var relationCorrectness = CompareRelationToRepresentations(representationRelation, definitionRelation);

                    Correctness correctness;
                    if (altCorrectness == Correctness.Correct ||
                        otherCorrectness == Correctness.Correct ||
                        relationCorrectness == Correctness.Correct)
                    {
                        correctness = Correctness.Correct;
                    }
                    else if(otherCorrectness == Correctness.PartiallyCorrect ||
                            relationCorrectness == Correctness.PartiallyCorrect)
                    {
                        correctness = Correctness.PartiallyCorrect;
                    }
                    else
                    {
                        correctness = relationCorrectness;
                    }

                    var codedCorrectness = string.Empty;
                    switch (correctness)
                    {
                        case Correctness.Correct:
                            codedCorrectness = Codings.CORRECTNESS_CORRECT;
                            break;
                        case Correctness.PartiallyCorrect:
                            codedCorrectness = Codings.CORRECTNESS_PARTIAL;
                            break;
                        case Correctness.Incorrect:
                            codedCorrectness = Codings.CORRECTNESS_INCORRECT;
                            break;
                        case Correctness.Unknown:
                            codedCorrectness = "UNKNOWN";
                            break;
                    }

                    var analysisCode = string.Format("{0} [{1}: {2}]{3}", codedObject, codedID, codedCorrectness, isFinal ? ", final" : string.Empty);
                    analysisCodes.Add(analysisCode);
                }
            }

            if (!analysisCodes.Any())
            {
                return;
            }

            analysisCodes.Reverse();

            var tag = new RepresentationCorrectnessTag(page, Origin.StudentPageGenerated, analysisCodes);
            page.AddTag(tag);
        }

        private class Relation
        {
            public double groupSize;
            public double numberOfGroups;
            public double product;
            public bool isOrderedGroup;
            public bool isProductImportant;
        }

        private static Correctness CompareRelationToRepresentations(Relation representationRelation, Relation definitionRelation)
        {
            if (representationRelation.isOrderedGroup &&
                definitionRelation.isOrderedGroup)
            {
                if (representationRelation.groupSize == definitionRelation.groupSize &&
                    representationRelation.numberOfGroups == definitionRelation.numberOfGroups)
                {
                    if (representationRelation.isProductImportant &&
                        definitionRelation.isProductImportant)
                    {
                        return representationRelation.product == definitionRelation.product ? Correctness.Correct : Correctness.PartiallyCorrect;
                    }

                    return Correctness.Correct;
                }

                if (representationRelation.groupSize == definitionRelation.groupSize ||
                    representationRelation.numberOfGroups == definitionRelation.numberOfGroups ||
                    representationRelation.groupSize == definitionRelation.numberOfGroups ||
                    representationRelation.numberOfGroups == definitionRelation.groupSize)
                {
                    return Correctness.PartiallyCorrect;
                }

                return Correctness.Incorrect;
            }

            if ((representationRelation.groupSize == definitionRelation.groupSize && representationRelation.numberOfGroups == definitionRelation.numberOfGroups) ||
                (representationRelation.groupSize == definitionRelation.numberOfGroups && representationRelation.numberOfGroups == definitionRelation.groupSize))
            {
                if (representationRelation.isProductImportant &&
                    definitionRelation.isProductImportant)
                {
                    return representationRelation.product == definitionRelation.product ? Correctness.Correct : Correctness.PartiallyCorrect;
                }

                return Correctness.Correct;
            }

            if (representationRelation.groupSize == definitionRelation.groupSize ||
                representationRelation.numberOfGroups == definitionRelation.numberOfGroups ||
                representationRelation.groupSize == definitionRelation.numberOfGroups ||
                representationRelation.numberOfGroups == definitionRelation.groupSize)
            {
                return Correctness.PartiallyCorrect;
            }

            return Correctness.Incorrect;
        }

        public static void AttemptAnswerBeforeRepresentationTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var answerActions = historyActions.Where(Codings.IsAnswerObject).ToList();
            if (answerActions.Count < 1)
            {
                return;
            }

            var firstAnswer = historyActions.First(Codings.IsAnswerObject);
            var firstIndex = historyActions.IndexOf(firstAnswer);

            var beforeActions = historyActions.Take(firstIndex + 1).ToList();
            var isUsingRepresentationsBefore = beforeActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (isUsingRepresentationsBefore)
            {
                return;
            }

            var afterActions = historyActions.Skip(firstIndex).ToList();
            var isUsingRepresentationsAfter = afterActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (!isUsingRepresentationsAfter)
            {
                return;
            }

            // TODO: Derive this entire Analysis Code from ARA Tag and don't use this Tag
            var tag = new AnswerBeforeRepresentationTag(page, Origin.StudentPageGenerated, afterActions);
            page.AddTag(tag);
        }

        public static void AttemptAnswerChangedAfterRepresentationTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var answerActions = historyActions.Where(Codings.IsAnswerObject).ToList();
            if (answerActions.Count < 2)
            {
                return;
            }

            var firstAnswer = historyActions.First(Codings.IsAnswerObject);
            var firstIndex = historyActions.IndexOf(firstAnswer);
            var lastAnswer = historyActions.Last(Codings.IsAnswerObject);
            var lastIndex = historyActions.IndexOf(lastAnswer);

            var possibleTagActions = historyActions.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToList();
            var isUsingRepresentations = possibleTagActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (!isUsingRepresentations)
            {
                return;
            }

            var tag = new AnswerChangedAfterRepresentationTag(page, Origin.StudentPageGenerated, possibleTagActions);
            page.AddTag(tag);
        }

        public static void AttemptAnswerTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            // BUG: will miss instances where mc incorrect, mc correct, mc erase incorrect
            var lastAnswerAction = historyActions.LastOrDefault(Codings.IsAnswerObject);
            if (lastAnswerAction == null ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE_INCOMPLETE ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE_PARTIAL ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_FILL_IN_ERASE)
            {
                return;
            }

            var tag = new AnswerCorrectnessTag(page, Origin.StudentPageGenerated, new List<IHistoryAction>
                                                                                  {
                                                                                      lastAnswerAction
                                                                                  });
            page.AddTag(tag);
        }

        public static void AttemptRepresentationsUsedTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var allRepresentations = new List<string>();
            var deletedCodedRepresentations = new List<string>();

            var stampedObjectGroups = new Dictionary<string, int>();
            var maxStampedObjectGroups = new Dictionary<string, int>();
            var jumpGroups = new Dictionary<string,List<NumberLineJumpSize>>();
            var subArrayGroups = new Dictionary<string,List<string>>();
            foreach (var historyAction in historyActions)
            {
                #region Stamps

                if (historyAction.CodedObject == Codings.OBJECT_STAMPED_OBJECTS)
                {
                    if (historyAction.CodedObjectAction == Codings.ACTION_OBJECT_ADD)
                    {
                        var historyItem = historyAction.HistoryItems.First();
                        var objectsChanged = historyItem as ObjectsOnPageChangedHistoryItem;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var stampedObject = objectsChanged.PageObjectsAdded.First() as StampedObject;
                        if (stampedObject == null)
                        {
                            continue;
                        }

                        var parts = stampedObject.Parts;
                        var parentStampID = stampedObject.ParentStampID;
                        var groupID = string.Format("{0} {1}", parts, parentStampID);
                        if (stampedObjectGroups.ContainsKey(groupID))
                        {
                            stampedObjectGroups[groupID]++;
                        }
                        else
                        {
                            stampedObjectGroups.Add(groupID, 1);
                        }

                        maxStampedObjectGroups = stampedObjectGroups;
                    }

                    if (historyAction.CodedObjectAction == Codings.ACTION_OBJECT_DELETE)
                    {
                        var historyItem = historyAction.HistoryItems.First();
                        var objectsChanged = historyItem as ObjectsOnPageChangedHistoryItem;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var stampedObject = objectsChanged.PageObjectsRemoved.First() as StampedObject;
                        if (stampedObject == null)
                        {
                            continue;
                        }

                        var parts = stampedObject.Parts;
                        var parentStampID = stampedObject.ParentStampID;
                        var groupID = string.Format("{0} {1}", parts, parentStampID);
                        stampedObjectGroups[groupID]--;
                        if (stampedObjectGroups[groupID] <= 0)
                        {
                            stampedObjectGroups.Remove(groupID);
                        }

                        if (stampedObjectGroups.Keys.Count == 0)
                        {
                            // TODO: Ideally, build entirely off info inside history action.
                            // Also just use this after the top level for-loop as an end case
                            // test to generate the final reps used.
                            foreach (var key in maxStampedObjectGroups.Keys)
                            {
                                var groupIDSections = key.Split(' ');
                                var stampParts = groupIDSections[0];
                                var obj = Codings.OBJECT_STAMP;
                                var id = stampParts;
                                var componentSection = string.Format(": {0} images", stampedObjectGroups[key]);
                                var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);
                                deletedCodedRepresentations.Add(codedValue);
                                allRepresentations.Add(obj);
                            }
                        }
                    }
                }

                #endregion // Stamps

                #region Number Line

                if (historyAction.CodedObject == Codings.OBJECT_NUMBER_LINE)
                {
                    if (historyAction.CodedObjectAction == Codings.ACTION_NUMBER_LINE_JUMP)
                    {
                        var jumpSizesChangedHistoryItems = historyAction.HistoryItems.Where(h => h is NumberLineJumpSizesChangedHistoryItem).Cast<NumberLineJumpSizesChangedHistoryItem>().ToList();
                        if (jumpSizesChangedHistoryItems == null ||
                            !jumpSizesChangedHistoryItems.Any())
                        {
                            continue;
                        }

                        var numberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;

                        var allJumps = new List<NumberLineJumpSize>();
                        foreach (var historyItem in jumpSizesChangedHistoryItems)
                        {
                            allJumps.AddRange(historyItem.JumpsAdded);
                        }

                        if (!jumpGroups.ContainsKey(numberLineID))
                        {
                            jumpGroups.Add(numberLineID, allJumps);
                        }
                        else
                        {
                            jumpGroups[numberLineID].AddRange(allJumps);
                        }
                    }

                    if (historyAction.CodedObjectAction == Codings.ACTION_NUMBER_LINE_JUMP_ERASE)
                    {
                        var jumpSizesChangedHistoryItems = historyAction.HistoryItems.Where(h => h is NumberLineJumpSizesChangedHistoryItem).Cast<NumberLineJumpSizesChangedHistoryItem>().ToList();
                        if (jumpSizesChangedHistoryItems == null ||
                            !jumpSizesChangedHistoryItems.Any())
                        {
                            continue;
                        }

                        var numberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;

                        var allJumps = new List<NumberLineJumpSize>();
                        foreach (var historyItem in jumpSizesChangedHistoryItems)
                        {
                            allJumps.AddRange(historyItem.JumpsRemoved);
                        }
                        
                        var jumpsToRemove = (from jump in allJumps
                                             from currentJump in jumpGroups[numberLineID]
                                             where jump.JumpSize == currentJump.JumpSize && jump.StartingTickIndex == currentJump.StartingTickIndex
                                             select currentJump).ToList();

                        foreach (var jump in jumpsToRemove)
                        {
                            // BUG: Natalie page 12 has errors here if you don't check ContainsKey, shouldn't happen.
                            if (jumpGroups.ContainsKey(numberLineID))
                            {
                                jumpGroups[numberLineID].Remove(jump);
                                
                                if (!jumpGroups[numberLineID].Any())
                                {
                                    jumpGroups.Remove(numberLineID);
                                }
                            }
                        }
                    }

                    if (historyAction.CodedObjectAction == Codings.ACTION_OBJECT_DELETE)
                    {
                        var historyItem = historyAction.HistoryItems.First();
                        var objectsChanged = historyItem as ObjectsOnPageChangedHistoryItem;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var numberLine = objectsChanged.PageObjectsRemoved.First() as NumberLine;
                        if (numberLine == null)
                        {
                            continue;
                        }

                        // TODO: Just like Stamps, use this as end-case to generate final reps
                        var numberLineID = numberLine.ID;

                        var obj = numberLine.CodedName;
                        var id = historyAction.CodedObjectID;
                        var components = jumpGroups.ContainsKey(numberLineID) ? NumberLine.ConsolidateJumps(jumpGroups[numberLineID].ToList()) : string.Empty;
                        var componentSection = string.IsNullOrEmpty(components) ? string.Empty : string.Format(": {0}", components);
                        var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);
                        deletedCodedRepresentations.Add(codedValue);
                        if (!string.IsNullOrEmpty(componentSection))
                        {
                            allRepresentations.Add(obj);
                        }
                    }
                    
                }

                #endregion // Number Line

                #region Array

                if (historyAction.CodedObject == Codings.OBJECT_ARRAY)
                {
                    if (historyAction.CodedObjectAction == Codings.ACTION_ARRAY_DIVIDE_INK)
                    {
                        var historyItem = historyAction.HistoryItems.First();
                        var objectsChanged = historyItem as ObjectsOnPageChangedHistoryItem;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var referenceArrayID = historyAction.MetaData["REFERENCE_PAGE_OBJECT_ID"];
                        var actionID = historyAction.CodedObjectActionID;
                        var subArrays = actionID.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                        if (!subArrayGroups.ContainsKey(referenceArrayID))
                        {
                            subArrayGroups.Add(referenceArrayID, subArrays);
                        }
                        else
                        {
                            subArrayGroups[referenceArrayID].AddRange(subArrays);
                        }
                    }

                    if (historyAction.CodedObjectAction == Codings.ACTION_ARRAY_DIVIDE_INK_ERASE)
                    {
                        var historyItem = historyAction.HistoryItems.First();
                        var objectsChanged = historyItem as ObjectsOnPageChangedHistoryItem;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var referenceArrayID = historyAction.MetaData["REFERENCE_PAGE_OBJECT_ID"];
                        var actionID = historyAction.CodedObjectActionID;
                        var subArrays = actionID.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                        foreach (var subArray in subArrays)
                        {
                            if (subArrayGroups.ContainsKey(referenceArrayID))
                            {
                                if (subArrayGroups[referenceArrayID].Contains(subArray))
                                {
                                    subArrayGroups[referenceArrayID].Remove(subArray);
                                    if (!subArrayGroups[referenceArrayID].Any())
                                    {
                                        subArrayGroups.Remove(referenceArrayID);
                                    }
                                }
                            }
                        }
                    }

                    if (historyAction.CodedObjectAction == Codings.ACTION_OBJECT_DELETE)
                    {
                        var historyItem = historyAction.HistoryItems.First();
                        var objectsChanged = historyItem as ObjectsOnPageChangedHistoryItem;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var array = objectsChanged.PageObjectsRemoved.First() as CLPArray;
                        if (array == null)
                        {
                            continue;
                        }

                        var obj = array.CodedName;
                        var id = historyAction.CodedObjectID;
                        var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : string.Format(": {0}", string.Join(", ", subArrayGroups[array.ID]));

                        var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);
                        deletedCodedRepresentations.Add(codedValue);
                        allRepresentations.Add(obj);
                    }
                }

                #endregion // Array
            }

            var finalCodedRepresentations = new List<string>();
            stampedObjectGroups.Clear();
            foreach (var pageObject in page.PageObjects)
            {
                var array = pageObject as CLPArray;
                if (array != null)
                {
                    var obj = array.CodedName;
                    var id = array.CodedID;
                    var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : string.Format(": {0}", string.Join(", ", subArrayGroups[array.ID]));

                    var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);

                    var formattedSkips = ArrayCodedActions.StaticSkipCountAnalysis(page, array);
                    if (!string.IsNullOrEmpty(formattedSkips))
                    {
                        var skipCodedValue = string.Format("\n  - skip [\"{0}\"]", formattedSkips);
                        codedValue = string.Format("{0}{1}", codedValue, skipCodedValue);
                    }

                    finalCodedRepresentations.Add(codedValue);
                    allRepresentations.Add(obj);
                }

                var numberLine = pageObject as NumberLine;
                if (numberLine != null)
                {
                    var obj = numberLine.CodedName;
                    var id = numberLine.CodedID;
                    var components = NumberLine.ConsolidateJumps(numberLine.JumpSizes.ToList());
                    var componentSection = string.IsNullOrEmpty(components) ? string.Empty : string.Format(": {0}", components);
                    var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);
                    finalCodedRepresentations.Add(codedValue);
                    if (!string.IsNullOrEmpty(componentSection))
                    {
                        allRepresentations.Add(obj);
                    }
                }

                var stampedObject = pageObject as StampedObject;
                if (stampedObject != null)
                {
                    var parts = stampedObject.Parts;
                    var parentStampID = stampedObject.ParentStampID;
                    var groupID = string.Format("{0} {1}", parts, parentStampID);
                    if (stampedObjectGroups.ContainsKey(groupID))
                    {
                        stampedObjectGroups[groupID]++;
                    }
                    else
                    {
                        stampedObjectGroups.Add(groupID, 1);
                    }
                }
            }

            foreach (var key in stampedObjectGroups.Keys)
            {
                var groupIDSections = key.Split(' ');
                var parts = groupIDSections[0];
                var obj = Codings.OBJECT_STAMP;
                var id = parts;
                var componentSection = string.Format(": {0} images", stampedObjectGroups[key]);
                var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);
                finalCodedRepresentations.Add(codedValue);
                allRepresentations.Add(obj);
            }

            allRepresentations = allRepresentations.Distinct().ToList();
            var tag = new RepresentationsUsedTag(page, Origin.StudentPageGenerated, allRepresentations, deletedCodedRepresentations, finalCodedRepresentations);
            page.AddTag(tag);
        }

        public static void AttemptArrayStrategiesTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var relevantHistoryactions = new List<IHistoryAction>();
            var strategyCodes = new List<string>();
            var ignoredHistoryIndexes = new List<int>();

            for (var i = 0; i < historyActions.Count; i++)
            {
                if (ignoredHistoryIndexes.Contains(i))
                {
                    continue;
                }

                var currentHistoryAction = historyActions[i];
                var isLastHistoryAction = i + 1 >= historyActions.Count;

                if (currentHistoryAction.CodedObject == Codings.OBJECT_ARRAY)
                {
                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_DIVIDE)
                    {
                        relevantHistoryactions.Add(currentHistoryAction);
                        var code = string.Format("{0} [{1}: {2}]", Codings.STRATEGY_ARRAY_DIVIDE, currentHistoryAction.CodedObjectID, currentHistoryAction.CodedObjectActionID);
                        strategyCodes.Add(code);
                        continue;
                    }

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_DIVIDE_INK)
                    {
                        relevantHistoryactions.Add(currentHistoryAction);
                        var code = string.Format("{0} [{1}: {2}]", Codings.STRATEGY_ARRAY_DIVIDE_INK, currentHistoryAction.CodedObjectID, currentHistoryAction.CodedObjectActionID);
                        strategyCodes.Add(code);
                        continue;
                    }

                    // HACK: Removed for demo.
                    //if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_SKIP)
                    //{
                    //    if (!isLastHistoryAction)
                    //    {
                    //        var nextHistoryAction = historyActions[i + 1];
                    //        if (nextHistoryAction.CodedObject == Codings.OBJECT_ARITH &&
                    //            nextHistoryAction.CodedObjectAction == Codings.ACTION_ARITH_ADD)
                    //        {
                    //            relevantHistoryactions.Add(currentHistoryAction);
                    //            relevantHistoryactions.Add(nextHistoryAction);
                    //            var compoundCode = string.Format("{0} +arith [{1}]", Codings.STRATEGY_ARRAY_SKIP, currentHistoryAction.CodedObjectID);
                    //            strategyCodes.Add(compoundCode);
                    //            continue;
                    //        }
                    //    }

                    //    relevantHistoryactions.Add(currentHistoryAction);
                    //    var code = string.Format("{0} [{1}]", Codings.STRATEGY_ARRAY_SKIP, currentHistoryAction.CodedObjectID);
                    //    strategyCodes.Add(code);
                    //    continue;
                    //}

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_CUT)
                    {
                        for (int j = i + 1; j < historyActions.Count; j++)
                        {
                            var nextHistoryAction = historyActions[j];
                            if (nextHistoryAction.CodedObject == Codings.OBJECT_ARRAY &&
                                nextHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_SNAP &&
                                nextHistoryAction.CodedObjectID == currentHistoryAction.CodedObjectID)
                            {
                                relevantHistoryactions.Add(currentHistoryAction);
                                relevantHistoryactions.Add(nextHistoryAction);
                                ignoredHistoryIndexes.Add(j);
                                var compoundCode = string.Format("{0} [{1}: {2}]", Codings.STRATEGY_ARRAY_CUT_THEN_SNAP, currentHistoryAction.CodedObjectID, currentHistoryAction.CodedObjectActionID);
                                strategyCodes.Add(compoundCode);
                                continue;
                            }
                        }

                        relevantHistoryactions.Add(currentHistoryAction);
                        var code = string.Format("{0} [{1}: {2}]", Codings.STRATEGY_ARRAY_CUT, currentHistoryAction.CodedObjectID, currentHistoryAction.CodedObjectActionID);
                        strategyCodes.Add(code);
                        continue;
                    }

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_SNAP)
                    {
                        relevantHistoryactions.Add(currentHistoryAction);
                        var code = string.Format("{0} [{1}: {2}]", Codings.STRATEGY_ARRAY_SNAP, currentHistoryAction.CodedObjectID, currentHistoryAction.CodedObjectActionID);
                        strategyCodes.Add(code);
                        continue;
                    }
                }
            }

            if (!strategyCodes.Any())
            {
                return;
            }

            var tag = new ArrayStrategiesTag(page, Origin.StudentPageGenerated, relevantHistoryactions, strategyCodes);
            page.AddTag(tag);
        }

        #endregion // Last Pass: Tag Generation

        // TODO: Refactor this to someplace more relevant
        public static string FindColorName(Color color)
        {
            var leastDifference = 0;
            var colorName = string.Empty;

            foreach (var systemColor in typeof (Color).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                var systemColorValue = (Color)systemColor.GetValue(null, null);

                if (systemColorValue == color)
                {
                    colorName = systemColor.Name;
                    break;
                }

                int a = color.A - systemColorValue.A, r = color.R - systemColorValue.R, g = color.G - systemColorValue.G, b = color.B - systemColorValue.B, difference = a * a + r * r + g * g + b * b;

                if (difference >= leastDifference)
                {
                    continue;
                }

                colorName = systemColor.Name;
                leastDifference = difference;
            }

            return colorName;
        }
    }
}