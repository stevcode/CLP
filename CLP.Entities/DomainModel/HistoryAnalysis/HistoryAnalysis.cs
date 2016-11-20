using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Collections;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void GenerateSemanticEvents(CLPPage page)
        {
            ObjectCodedActions.InitializeIncrementIDs();
            page.History.SemanticEvents.Clear();

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "SemanticEvents");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, page.DefaultZipEntryName + ".txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, "*****Coded Actions/Steps*****" + "\n\n");

            // First Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "1"
                                            });
            var initialSemanticEvents = GenerateInitialSemanticEvents(page);
            page.History.SemanticEvents.AddRange(initialSemanticEvents);

            File.AppendAllText(filePath, "PASS [1]" + "\n");
            foreach (var item in initialSemanticEvents)
            {
                var semi = item == initialSemanticEvents.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Second Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2"
                                            });
            var clusteredInkSemanticEvents = ClusterInkSemanticEvents(page, initialSemanticEvents);
            page.History.SemanticEvents.AddRange(clusteredInkSemanticEvents);

            File.AppendAllText(filePath, "\nPASS [2]" + "\n");
            foreach (var item in clusteredInkSemanticEvents)
            {
                var semi = item == clusteredInkSemanticEvents.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Third Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "3"
                                            });
            var interpretedInkSemanticEvents = InterpretInkSemanticEvents(page, clusteredInkSemanticEvents);
            page.History.SemanticEvents.AddRange(interpretedInkSemanticEvents);

            File.AppendAllText(filePath, "\nPASS [3]" + "\n");
            foreach (var item in interpretedInkSemanticEvents)
            {
                var semi = item == interpretedInkSemanticEvents.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Last Pass
            GenerateTags(page, interpretedInkSemanticEvents);

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

        public static List<ISemanticEvent> GenerateInitialSemanticEvents(CLPPage page)
        {
            var historyItemBuffer = new List<IHistoryItem>();
            var initialSemanticEvents = new List<ISemanticEvent>();
            var historyItems = page.History.CompleteOrderedHistoryItems;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var singleSemanticEvent = VerifyAndGenerateSingleItemAction(page, historyItemBuffer.First());
                    if (singleSemanticEvent != null)
                    {
                        initialSemanticEvents.Add(singleSemanticEvent);
                        historyItemBuffer.Clear();
                        continue;
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                var compoundSemanticEvent = VerifyAndGenerateCompoundItemAction(page, historyItemBuffer, nextHistoryItem);
                if (compoundSemanticEvent != null)
                {
                    initialSemanticEvents.Add(compoundSemanticEvent);
                    historyItemBuffer.Clear();
                }
            }

            return initialSemanticEvents;
        }

        public static ISemanticEvent VerifyAndGenerateSingleItemAction(CLPPage page, IHistoryItem historyItem)
        {
            if (historyItem == null)
            {
                return null;
            }

            ISemanticEvent semanticEvent = null;
            TypeSwitch.On(historyItem)
                      .Case<ObjectsOnPageChangedHistoryItem>(h => { semanticEvent = ObjectCodedActions.Add(page, h) ?? ObjectCodedActions.Delete(page, h); })
                      .Case<PartsValueChangedHistoryItem>(h =>
                                                          {
                                                              semanticEvent = new SemanticEvent(page, h)
                                                                              {
                                                                                  CodedObject = Codings.OBJECT_STAMP,
                                                                                  CodedObjectAction = "parts",
                                                                                  CodedObjectID = "CHANGED"
                                                                              };
                                                          })
                      .Case<CLPArrayRotateHistoryItem>(h => { semanticEvent = ArrayCodedActions.Rotate(page, h); })
                      .Case<PageObjectCutHistoryItem>(h => { semanticEvent = ArrayCodedActions.Cut(page, h); })
                      .Case<CLPArraySnapHistoryItem>(h => { semanticEvent = ArrayCodedActions.Snap(page, h); })
                      .Case<CLPArrayDivisionsChangedHistoryItem>(h => { semanticEvent = ArrayCodedActions.Divide(page, h); });

            return semanticEvent;
        }

        public static ISemanticEvent VerifyAndGenerateCompoundItemAction(CLPPage page, List<IHistoryItem> historyItems, IHistoryItem nextHistoryItem)
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

                    var semanticEvent = InkCodedActions.ChangeOrIgnore(page, objectsChangedHistoryItems);
                    return semanticEvent;
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

                    var semanticEvent = ObjectCodedActions.Move(page, objectsMovedHistoryItems);
                    return semanticEvent;
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

                    var semanticEvent = ObjectCodedActions.Resize(page, objectsResizedHistoryItems);
                    return semanticEvent;
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

                    var semanticEvent = NumberLineCodedActions.EndPointsChange(page, endPointsChangedHistoryItems);
                    return semanticEvent;
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

                    var semanticEvent = NumberLineCodedActions.JumpSizesChange(page, jumpSizesChangedHistoryItems);
                    return semanticEvent;
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
                    if (nextStatusChangedHistoryItems != null)
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
                    var semanticEvent = new SemanticEvent(page, historyItems)
                                        {
                                            CodedObject = Codings.OBJECT_MULTIPLE_CHOICE,
                                            CodedObjectAction = objectAction,
                                            IsObjectActionVisible = objectAction != Codings.ACTION_MULTIPLE_CHOICE_ADD,
                                            CodedObjectID = multipleChoice.CodedID,
                                            CodedObjectActionID = string.Format("{0}, {1}", bubble.BubbleCodedID, correctness)
                                        };
                    return semanticEvent;
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

        #region Second Pass: Ink Clustering

        public static List<ISemanticEvent> ClusterInkSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            InkCodedActions.InkClusters.Clear();
            var refinedInkActions = InkCodedActions.RefineInkDivideClusters(page, semanticEvents);
            // HACK: This should be taken care of at the historyItem level, assessment cache needs another conversion to handle that.
            refinedInkActions = InkCodedActions.RefineANS_FIClusters(page, refinedInkActions);

            InkCodedActions.GenerateInitialInkClusters(page, refinedInkActions);
            InkCodedActions.RefineSkipCountClusters(page, refinedInkActions);

            // TODO: Rename/fix - Refine Temporal Clusters
            var processedActions = new List<ISemanticEvent>();
            foreach (var semanticEvent in refinedInkActions)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_INK &&
                    semanticEvent.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var processedInkChangeActions = InkCodedActions.ProcessInkChangeSemanticEvent(page, semanticEvent);
                    processedActions.AddRange(processedInkChangeActions);
                }
                else
                {
                    processedActions.Add(semanticEvent);
                }
            }

            return processedActions;
        }

        #endregion // Second Pass: Ink Clustering

        #region Third Pass: Ink Interpretation

        public static List<ISemanticEvent> InterpretInkSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var allInterpretedSemanticEvents = new List<ISemanticEvent>();

            foreach (var semanticEvent in semanticEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_INK)
                {
                    var interpretedSemanticEvents = AttemptSemanticEventInterpretation(page, semanticEvent);
                    allInterpretedSemanticEvents.AddRange(interpretedSemanticEvents);
                }
                else
                {
                    allInterpretedSemanticEvents.Add(semanticEvent);
                }
            }

            return allInterpretedSemanticEvents;
        }

        public static List<ISemanticEvent> AttemptSemanticEventInterpretation(CLPPage page, ISemanticEvent semanticEvent)
        {
            var allInterpretedActions = new List<ISemanticEvent>();

            if (semanticEvent.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT_SKIP) &&
                semanticEvent.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedAction = ArrayCodedActions.SkipCounting(page, semanticEvent);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            if (semanticEvent.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER) &&
                semanticEvent.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedAction = ArrayCodedActions.ArrayEquation(page, semanticEvent);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            if (!semanticEvent.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER))
            {
                var interpretedAction = InkCodedActions.Arithmetic(page, semanticEvent);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                    return allInterpretedActions;
                }
            }

            // TODO: Attempt to interpret inked circles around a multiple choice bubbles

            if (!allInterpretedActions.Any())
            {
                allInterpretedActions.Add(semanticEvent);
            }

            return allInterpretedActions;
        }

        #endregion // Third Pass: Ink Interpretation

        #region Fourth Pass: Refinement

        public static List<ISemanticEvent> RefineSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var allRefinedSemanticEvents = new List<ISemanticEvent>();

            // TODO: Combine ARR skip + Ink ignore + ARR skip into single ARR skip
            //foreach (var semanticEvent in semanticEvents)
            //{
            //    if (semanticEvent.CodedObject == Codings.OBJECT_INK)
            //    {
            //        var refinedSemanticEvents = AttemptSemanticEventInterpretation(page, semanticEvent);
            //        allRefinedSemanticEvents.AddRange(refinedSemanticEvents);
            //    }
            //    else
            //    {
            //        allRefinedSemanticEvents.Add(semanticEvent);
            //    }
            //}

            return allRefinedSemanticEvents;
        }

        #endregion // Fourth Pass: Refinement

        #region Last Pass: Tag Generation

        public static void GenerateTags(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            ArrayStrategyTag.IdentifyArrayStrategies(page, semanticEvents);
            AttemptAnswerBeforeRepresentationTag(page, semanticEvents);
            AttemptAnswerChangedAfterRepresentationTag(page, semanticEvents);
            AttemptAnswerTag(page, semanticEvents);
            RepresentationsUsedTag.AttemptTagGeneration(page, semanticEvents);
            AttemptRepresentationCorrectness(page, semanticEvents);
        }

        // TODO: Move each Attempt method to the Tag's class

        public static void AttemptRepresentationCorrectness(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            //if (!semanticEvents.Any())
            //{
            //    return;
            //}

            #region Answer Definition Relation

            var relationDefinitionTag = page.Tags.FirstOrDefault(t => t is DivisionRelationDefinitionTag || t is MultiplicationRelationDefinitionTag || t is AdditionRelationDefinitionTag);

            if (relationDefinitionTag == null)
            {
                return;
            }

            var definitionRelation = new Relation();
            var otherDefinitionRelation = new Relation();
            var isOtherDefinitionUsed = false;
            var altDefinitionRelation = new Relation();
            var isAltDefinitionUsed = false;

            var div = relationDefinitionTag as DivisionRelationDefinitionTag;
            if (div != null)
            {
                definitionRelation.groupSize = div.Quotient;
                definitionRelation.numberOfGroups = div.Divisor;
                definitionRelation.product = div.Dividend;
                definitionRelation.isOrderedGroup = false;
                    // BUG: Actually a needed enhancement. There's no way to specify what type of division problem it is (dealing out, or partitivate or whatever), so group size is indeterminate.
                definitionRelation.isProductImportant = true;
            }

            var mult = relationDefinitionTag as MultiplicationRelationDefinitionTag;
            if (mult != null)
            {
                definitionRelation.numberOfGroups = mult.Factors.First().RelationPartAnswerValue;
                definitionRelation.groupSize = mult.Factors.Last().RelationPartAnswerValue;
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

            #endregion // Answer Definition Relation

            var keyIndexes =
                semanticEvents.Where(h => h.CodedObjectAction == Codings.ACTION_OBJECT_DELETE && (h.CodedObject == Codings.OBJECT_ARRAY || h.CodedObject == Codings.OBJECT_NUMBER_LINE))
                              .Select(h => h.HistoryItems.First().HistoryIndex - 1)
                              .ToList();
            if (!page.History.CompleteOrderedHistoryItems.Any())
            {
                return;
            }
            var lastHistoryIndex = page.History.CompleteOrderedHistoryItems.Last().HistoryIndex + 1;
            keyIndexes.Add(lastHistoryIndex);
            keyIndexes.Reverse();
            var usedPageObjectIDs = new List<string>();
            var finalPageObjectIDs = page.PageObjects.Where(p => p is CLPArray || p is NumberLine).Select(p => p.ID).ToList();
            var analysisCodes = new List<string>();
            foreach (var index in keyIndexes)
            {
                var pageObjectOnPage = page.GetPageObjectsOnPageAtHistoryIndex(index).Where(p => p is CLPArray || p is NumberLine || p is StampedObject || p is Bin).ToList();
                var stampedObjectGroups = new Dictionary<string, int>();
                foreach (var pageObject in pageObjectOnPage)
                {
                    var stampedObject = pageObject as StampedObject;
                    if (stampedObject != null &&
                        index == lastHistoryIndex)
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

                        usedPageObjectIDs.Add(stampedObject.ID);
                        continue;
                    }

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

                    if (representationRelation == null ||
                        usedPageObjectIDs.Contains(usedID))
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
                    else if (otherCorrectness == Correctness.PartiallyCorrect ||
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

                var binsOnPage = pageObjectOnPage.OfType<Bin>().Where(b => b.Parts > 0).ToList();
                if (binsOnPage.Any())
                {
                    var numberOfGroups = binsOnPage.Count();
                    var product = binsOnPage.Select(b => b.Parts).Sum();
                    var firstGroupSize = binsOnPage.First().Parts;
                    var isEqualGroups = binsOnPage.All(b => b.Parts == firstGroupSize);

                    var representationRelation = new Relation
                                                 {
                                                     groupSize = isEqualGroups ? firstGroupSize : -1,
                                                     numberOfGroups = numberOfGroups,
                                                     product = product,
                                                     isOrderedGroup = true,
                                                     isProductImportant = true
                                                 };

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
                    else if (otherCorrectness == Correctness.PartiallyCorrect ||
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

                    var codedObject = Codings.OBJECT_BINS;
                    var codedID = numberOfGroups;
                    var analysisCode = string.Format("{0} [{1}: {2}], final", codedObject, codedID, codedCorrectness);
                    analysisCodes.Add(analysisCode);
                }

                if (stampedObjectGroups.Keys.Any())
                {
                    foreach (var key in stampedObjectGroups.Keys)
                    {
                        var groupIDSections = key.Split(' ');
                        var parts = int.Parse(groupIDSections[0]);
                        var numberOfGroups = stampedObjectGroups[key];
                        var codedObject = Codings.OBJECT_STAMP;
                        var codedID = parts;
                        //var componentSection = string.Format(": {0} images", stampedObjectGroups[key]);
                        //var groupString = stampedObjectGroups[key] == 1 ? "group" : "groups";
                        //var englishValue = string.Format("{0} {1} of {2}", stampedObjectGroups[key], groupString, parts);
                        //var codedValue = string.Format("{0} [{1}{2}]\n  - {3}", obj, id, componentSection, englishValue);

                        var representationRelation = new Relation
                                                     {
                                                         groupSize = parts,
                                                         numberOfGroups = numberOfGroups,
                                                         product = numberOfGroups * parts,
                                                         isOrderedGroup = parts != 1,
                                                         isProductImportant = true
                                                     };

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
                        else if (otherCorrectness == Correctness.PartiallyCorrect ||
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

                        var analysisCode = string.Format("{0} [{1}, {2} images: {3}], final", codedObject, codedID, numberOfGroups, codedCorrectness);
                        analysisCodes.Add(analysisCode);
                    }
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

        public static void AttemptAnswerBeforeRepresentationTag(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var answerActions = semanticEvents.Where(Codings.IsAnswerObject).ToList();
            if (answerActions.Count < 1)
            {
                return;
            }

            var firstAnswer = semanticEvents.First(Codings.IsAnswerObject);
            var firstIndex = semanticEvents.IndexOf(firstAnswer);

            var beforeActions = semanticEvents.Take(firstIndex + 1).ToList();
            var isUsingRepresentationsBefore = beforeActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (isUsingRepresentationsBefore)
            {
                return;
            }

            var afterActions = semanticEvents.Skip(firstIndex).ToList();
            var isUsingRepresentationsAfter = afterActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (!isUsingRepresentationsAfter)
            {
                return;
            }

            // TODO: Derive this entire Analysis Code from ARA Tag and don't use this Tag
            var tag = new AnswerBeforeRepresentationTag(page, Origin.StudentPageGenerated, afterActions);
            page.AddTag(tag);
        }

        public static void AttemptAnswerChangedAfterRepresentationTag(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var answerActions = semanticEvents.Where(Codings.IsAnswerObject).ToList();
            if (answerActions.Count < 2)
            {
                return;
            }

            var firstAnswer = semanticEvents.First(Codings.IsAnswerObject);
            var firstIndex = semanticEvents.IndexOf(firstAnswer);
            var lastAnswer = semanticEvents.Last(Codings.IsAnswerObject);
            var lastIndex = semanticEvents.IndexOf(lastAnswer);

            var possibleTagActions = semanticEvents.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToList();
            var isUsingRepresentations = possibleTagActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (!isUsingRepresentations)
            {
                return;
            }

            var tag = new AnswerChangedAfterRepresentationTag(page, Origin.StudentPageGenerated, possibleTagActions);
            page.AddTag(tag);
        }

        public static void AttemptAnswerTag(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            // BUG: will miss instances where mc incorrect, mc correct, mc erase incorrect
            var lastAnswerAction = semanticEvents.LastOrDefault(Codings.IsAnswerObject);
            if (lastAnswerAction == null ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE_INCOMPLETE ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE_PARTIAL ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_FILL_IN_ERASE)
            {
                return;
            }

            var tag = new AnswerCorrectnessTag(page,
                                               Origin.StudentPageGenerated,
                                               new List<ISemanticEvent>
                                               {
                                                   lastAnswerAction
                                               });
            page.AddTag(tag);
        }

        #endregion // Last Pass: Tag Generation
    }
}