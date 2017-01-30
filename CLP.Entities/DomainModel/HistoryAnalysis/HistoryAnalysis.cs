using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel;
using Catel.Collections;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void GenerateSemanticEvents(CLPPage page)
        {
            Argument.IsNotNull(nameof(page), page);

            ObjectSemanticEvents.InitializeIncrementIDs();
            page.History.SemanticEvents.Clear();

            // TODO: Pass 0 to "update" certain ink strokes over a Fill-In Ans to appropriate historyAction?

            // First Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryAction>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "1"
                                            });

            var initialSemanticEvents = GenerateInitialSemanticEvents(page);
            page.History.SemanticEvents.AddRange(initialSemanticEvents);

            // Second Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryAction>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2"
                                            });

            var clusteredInkSemanticEvents = ClusterInkSemanticEvents(page, initialSemanticEvents);
            page.History.SemanticEvents.AddRange(clusteredInkSemanticEvents);

            // Third Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryAction>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "3"
                                            });

            var interpretedInkSemanticEvents = InterpretInkSemanticEvents(page, clusteredInkSemanticEvents);
            page.History.SemanticEvents.AddRange(interpretedInkSemanticEvents);

            // Last Pass
            GenerateTags(page, interpretedInkSemanticEvents);

            #region Logging

            var isPrintingLogs = false;
            if (!isPrintingLogs)
            {
                return;
            }

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
            File.AppendAllText(filePath, "*****Semantic Events/Steps*****\n\n");

            File.AppendAllText(filePath, "PASS [1]\n");
            foreach (var semanticEvent in initialSemanticEvents)
            {
                var semi = semanticEvent == initialSemanticEvents.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, semanticEvent.CodedValue + semi);
            }

            File.AppendAllText(filePath, "\nPASS [2]\n");
            foreach (var semanticEvent in clusteredInkSemanticEvents)
            {
                var semi = semanticEvent == clusteredInkSemanticEvents.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, semanticEvent.CodedValue + semi);
            }

            File.AppendAllText(filePath, "\nPASS [3]\n");
            foreach (var semanticEvent in interpretedInkSemanticEvents)
            {
                var semi = semanticEvent == interpretedInkSemanticEvents.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, semanticEvent.CodedValue + semi);
            }

            File.AppendAllText(filePath, "\n\n\n*****Tags*****\n\n");
            foreach (var tag in page.Tags)
            {
                File.AppendAllText(filePath, $"*{tag.FormattedName}*\n");
                File.AppendAllText(filePath, $"{tag.FormattedValue}\n\n");
            }

            File.AppendAllText(filePath, "\n*****History Actions*****\n\n");
            foreach (var historyAction in page.History.CompleteOrderedHistoryActions)
            {
                File.AppendAllText(filePath, $"{historyAction.FormattedValue}\n");
            }

            #endregion // Logging
        }

        #region First Pass: Initialization

        public static List<ISemanticEvent> GenerateInitialSemanticEvents(CLPPage page)
        {
            Argument.IsNotNull(nameof(page), page);

            var historyActionBuffer = new List<IHistoryAction>();
            var initialSemanticEvents = new List<ISemanticEvent>();
            var historyActions = page.History.CompleteOrderedHistoryActions;

            for (var i = 0; i < historyActions.Count; i++)
            {
                var currentHistoryAction = historyActions[i];
                historyActionBuffer.Add(currentHistoryAction);
                if (historyActionBuffer.Count == 1)
                {
                    var singleSemanticEvent = VerifyAndGenerateSingleActionEvent(page, historyActionBuffer.First());
                    if (singleSemanticEvent != null)
                    {
                        initialSemanticEvents.Add(singleSemanticEvent);
                        historyActionBuffer.Clear();
                        continue;
                    }
                }

                var nextHistoryAction = i + 1 < historyActions.Count ? historyActions[i + 1] : null;
                var compoundSemanticEvent = VerifyAndGenerateCompoundActionEvent(page, historyActionBuffer, nextHistoryAction);
                if (compoundSemanticEvent != null)
                {
                    initialSemanticEvents.Add(compoundSemanticEvent);
                    historyActionBuffer.Clear();
                }
            }

            return initialSemanticEvents;
        }

        public static ISemanticEvent VerifyAndGenerateSingleActionEvent(CLPPage page, IHistoryAction historyAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(historyAction), historyAction);

            // TODO: Division Values Changed, DT Array Snapped/Removed, Animation Stop/Start

            ISemanticEvent semanticEvent = null;
            TypeSwitch.On(historyAction)
                      .Case<ObjectsOnPageChangedHistoryAction>(h =>
                                                               {
                                                                   semanticEvent = ObjectSemanticEvents.Add(page, h) ?? ObjectSemanticEvents.Delete(page, h);
                                                               })
                      .Case<PartsValueChangedHistoryAction>(h =>
                                                                {
                                                                    semanticEvent = StampSemanticEvents.PartsValueChanged(page, h);
                                                                })
                      .Case<CLPArrayRotateHistoryAction>(h =>
                                                         {
                                                             semanticEvent = ArraySemanticEvents.Rotate(page, h);
                                                         })
                      .Case<PageObjectCutHistoryAction>(h =>
                                                        {
                                                            semanticEvent = ArraySemanticEvents.Cut(page, h);
                                                        })
                      .Case<CLPArraySnapHistoryAction>(h =>
                                                       {
                                                           semanticEvent = ArraySemanticEvents.Snap(page, h);
                                                       })
                      .Case<CLPArrayDivisionsChangedHistoryAction>(h =>
                                                                   {
                                                                       semanticEvent = ArraySemanticEvents.Divide(page, h);
                                                                   });

            return semanticEvent;
        }

        public static ISemanticEvent VerifyAndGenerateCompoundActionEvent(CLPPage page, List<IHistoryAction> historyActions, IHistoryAction nextHistoryAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(historyActions), historyActions);

            if (!historyActions.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, historyActions, Codings.ERROR_TYPE_EMPTY_BUFFER, "Compound Action Attempt");
            }

            // TODO: GridToggle, DT Remainder Tiles Toggled

            #region INK Change

            if (historyActions.All(h => h is ObjectsOnPageChangedHistoryAction))
            {
                var objectsChangedHistoryActions = historyActions.Cast<ObjectsOnPageChangedHistoryAction>().ToList();

                if (objectsChangedHistoryActions.All(h => h.IsUsingStrokes && !h.IsUsingPageObjects))
                {
                    var nextObjectsChangedHistoryAction = nextHistoryAction as ObjectsOnPageChangedHistoryAction;
                    if (nextObjectsChangedHistoryAction != null &&
                        nextObjectsChangedHistoryAction.IsUsingStrokes &&
                        !nextObjectsChangedHistoryAction.IsUsingPageObjects)
                    {
                        return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                    }

                    var semanticEvent = InkSemanticEvents.ChangeOrIgnore(page, objectsChangedHistoryActions);
                    return semanticEvent;
                }
            }

            #endregion // INK Change

            #region PageObject Move

            if (historyActions.All(h => h is ObjectsMovedBatchHistoryAction))
            {
                var objectsMovedHistoryActions = historyActions.Cast<ObjectsMovedBatchHistoryAction>().ToList();

                var firstIDSequence = objectsMovedHistoryActions.First().PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList();
                if (objectsMovedHistoryActions.All(h => firstIDSequence.SequenceEqual(h.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList())))
                {
                    var nextMovedHistoryAction = nextHistoryAction as ObjectsMovedBatchHistoryAction;
                    if (nextMovedHistoryAction != null &&
                        firstIDSequence.SequenceEqual(nextMovedHistoryAction.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList()))
                    {
                        return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                    }

                    var semanticEvent = ObjectSemanticEvents.Move(page, objectsMovedHistoryActions);
                    return semanticEvent;
                }
            }

            #endregion // PageObject Move

            #region PageObject Resize

            if (historyActions.All(h => h is PageObjectResizeBatchHistoryAction))
            {
                var objectsResizedHistoryActions = historyActions.Cast<PageObjectResizeBatchHistoryAction>().ToList();

                var firstID = objectsResizedHistoryActions.First().PageObjectID;
                if (objectsResizedHistoryActions.All(h => h.PageObjectID == firstID))
                {
                    var nextResizedHistoryAction = nextHistoryAction as PageObjectResizeBatchHistoryAction;
                    if (nextResizedHistoryAction != null &&
                        firstID == nextResizedHistoryAction.PageObjectID)
                    {
                        return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                    }

                    var semanticEvent = ObjectSemanticEvents.Resize(page, objectsResizedHistoryActions);
                    return semanticEvent;
                }
            }

            #endregion // PageObject Resize

            #region Number Line End Points Changed

            if (historyActions.All(h => h is NumberLineEndPointsChangedHistoryAction))
            {
                var endPointsChangedHistoryActions = historyActions.Cast<NumberLineEndPointsChangedHistoryAction>().ToList();

                var firstNumberLineID = endPointsChangedHistoryActions.First().NumberLineID;
                if (endPointsChangedHistoryActions.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextEndPointsChangedHistoryAction = nextHistoryAction as NumberLineEndPointsChangedHistoryAction;
                    if (nextEndPointsChangedHistoryAction != null &&
                        nextEndPointsChangedHistoryAction.NumberLineID == firstNumberLineID)
                    {
                        return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                    }

                    var semanticEvent = NumberLineSemanticEvents.EndPointsChange(page, endPointsChangedHistoryActions);
                    return semanticEvent;
                }
            }

            #endregion // Number Line End Points Changed

            #region Number Line Jumps Add/Remove

            if (historyActions.All(h => h is NumberLineJumpSizesChangedHistoryAction))
            {
                var jumpSizesChangedHistoryActions = historyActions.Cast<NumberLineJumpSizesChangedHistoryAction>().ToList();

                var firstNumberLineID = jumpSizesChangedHistoryActions.First().NumberLineID;
                var isAdding = jumpSizesChangedHistoryActions.First().JumpsAdded.Any() && !jumpSizesChangedHistoryActions.First().JumpsRemoved.Any();
                if (jumpSizesChangedHistoryActions.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextJumpsChangedHistoryAction = nextHistoryAction as NumberLineJumpSizesChangedHistoryAction;
                    if (nextJumpsChangedHistoryAction != null &&
                        nextJumpsChangedHistoryAction.NumberLineID == firstNumberLineID &&
                        isAdding == (nextJumpsChangedHistoryAction.JumpsAdded.Any() && !nextJumpsChangedHistoryAction.JumpsRemoved.Any()))
                    {
                        return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                    }

                    var semanticEvent = NumberLineSemanticEvents.JumpSizesChange(page, jumpSizesChangedHistoryActions);
                    return semanticEvent;
                }
            }

            #endregion // Number Line Jumps Add/Remove

            #region Multiple Choice Bubble Status Changed

            if (historyActions.All(h => h is MultipleChoiceBubbleStatusChangedHistoryAction))
            {
                var statusChangedHistoryActions = historyActions.Cast<MultipleChoiceBubbleStatusChangedHistoryAction>().ToList();

                var currentMultipleChoiceID = statusChangedHistoryActions.First().MultipleChoiceID;
                var multipleChoice = page.GetPageObjectByIDOnPageOrInHistory(currentMultipleChoiceID);
                if (multipleChoice == null)
                {
                    return SemanticEvent.GetErrorSemanticEvent(page, historyActions, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "MultipleChoiceBubbleStatusChanged, Multiple Choice NULL");
                }

                var currentBubbleIndex = statusChangedHistoryActions.First().ChoiceBubbleIndex;
                if (statusChangedHistoryActions.Any(h => h.MultipleChoiceID != currentMultipleChoiceID))
                {
                    return SemanticEvent.GetErrorSemanticEvent(page, historyActions, Codings.ERROR_TYPE_MIXED_LIST, "MultipleChoiceBubbleStatusChanged, Mixed Multiple Choice IDs");
                }

                if (_currentCompressedStatus == null)
                {
                    _currentCompressedStatus = statusChangedHistoryActions.First().ChoiceBubbleStatus;
                }

                var nextStatusChangedHistoryAction = nextHistoryAction as MultipleChoiceBubbleStatusChangedHistoryAction;

                ChoiceBubbleStatuses? compressedStatus = null;
                if (nextStatusChangedHistoryAction != null)
                {
                    compressedStatus = CompressMultipleChoiceStatuses(_currentCompressedStatus, nextStatusChangedHistoryAction.ChoiceBubbleStatus);
                }

                if (nextStatusChangedHistoryAction != null &&
                    nextStatusChangedHistoryAction.MultipleChoiceID == currentMultipleChoiceID &&
                    nextStatusChangedHistoryAction.ChoiceBubbleIndex == currentBubbleIndex &&
                    compressedStatus != null)
                {
                    _currentCompressedStatus = compressedStatus;
                    return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                }

                var codedObject = Codings.OBJECT_MULTIPLE_CHOICE;
                var eventType = string.Empty;
                switch (_currentCompressedStatus)
                {
                    case ChoiceBubbleStatuses.PartiallyFilledIn:
                        eventType = Codings.EVENT_MULTIPLE_CHOICE_ADD_PARTIAL;
                        break;
                    case ChoiceBubbleStatuses.FilledIn:
                        eventType = Codings.EVENT_MULTIPLE_CHOICE_ADD;
                        break;
                    case ChoiceBubbleStatuses.AdditionalFilledIn:
                        eventType = Codings.EVENT_MULTIPLE_CHOICE_ADD_ADDITIONAL;
                        break;
                    case ChoiceBubbleStatuses.ErasedPartiallyFilledIn:
                        eventType = Codings.EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL;
                        break;
                    case ChoiceBubbleStatuses.IncompletelyErased:
                        eventType = Codings.EVENT_MULTIPLE_CHOICE_ERASE_INCOMPLETE;
                        break;
                    case ChoiceBubbleStatuses.CompletelyErased:
                        eventType = Codings.EVENT_MULTIPLE_CHOICE_ERASE;
                        break;
                    default:
                        return SemanticEvent.GetErrorSemanticEvent(page,
                                                                   historyActions,
                                                                   Codings.ERROR_TYPE_MULTIPLE_CHOICE_STATUS_INCONSISTANCY,
                                                                   "MultipleChoiceBubbleStatusChanged, _currentCompressedStatus NULL");
                }
                var codedID = multipleChoice.CodedID;

                var bubble = statusChangedHistoryActions.First().Bubble;
                var correctness = bubble.IsACorrectValue ? Codings.CORRECTNESS_CORRECT : Codings.CORRECTNESS_INCORRECT;
                var eventInfo = $"{bubble.BubbleCodedID}, {correctness}";

                _currentCompressedStatus = null;
                var semanticEvent = new SemanticEvent(page, historyActions)
                                    {
                                        CodedObject = codedObject,
                                        EventType = eventType,
                                        CodedObjectID = codedID,
                                        EventInformation = eventInfo,
                                        ReferencePageObjectID = multipleChoice.ID
                };

                return semanticEvent;
            }

            #endregion // Multiple Choice Bubble Status Changed

            #region Fill-In Status Changed

            if (historyActions.All(h => h is FillInAnswerChangedHistoryAction))
            {
                var statusChangedHistoryActions = historyActions.Cast<FillInAnswerChangedHistoryAction>().ToList();

                var currentInterpretationID = statusChangedHistoryActions.First().InterpretationRegionID;
                var interpretationRegion = page.GetPageObjectByIDOnPageOrInHistory(currentInterpretationID);
                if (interpretationRegion == null)
                {
                    return SemanticEvent.GetErrorSemanticEvent(page, historyActions, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "FillInAnswerChangedHistoryAction, Interpretation Region NULL");
                }

                if (statusChangedHistoryActions.Any(h => h.InterpretationRegionID != currentInterpretationID))
                {
                    return SemanticEvent.GetErrorSemanticEvent(page, historyActions, Codings.ERROR_TYPE_MIXED_LIST, "FillInAnswerChangedHistoryAction, Mixed Interpretation Region IDs");
                }

                var nextStatusChangedHistoryAction = nextHistoryAction as FillInAnswerChangedHistoryAction;
                if (nextStatusChangedHistoryAction != null &&
                    nextStatusChangedHistoryAction.InterpretationRegionID == currentInterpretationID)
                {
                    return null; // Confirmed nextHistoryAction belongs in this Semantic Event
                }

                var codedObject = Codings.OBJECT_FILL_IN;
                var eventType = Codings.EVENT_FILL_IN_CHANGE;

                var semanticEvent = new SemanticEvent(page, historyActions)
                                    {
                                        CodedObject = codedObject,
                                        EventType = eventType,
                                        ReferencePageObjectID = interpretationRegion.ID
                                    };

                return semanticEvent;
            }

            #endregion // Fill-In Status Changed

            return SemanticEvent.GetErrorSemanticEvent(page, historyActions, Codings.ERROR_TYPE_MIXED_BUFFER, "Compound Action Attempt");
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
            InkSemanticEvents.InkClusters.Clear();

            // Pass 2.0: Pre-Cluster
            var preProcessedSemanticEvents = InkSemanticEvents.GenerateArrayInkDivideSemanticEvents(page, semanticEvents);
            InkSemanticEvents.DefineMultipleChoiceClusters(page, preProcessedSemanticEvents);
            InkSemanticEvents.DefineFillInClusters(page, preProcessedSemanticEvents);
            InkSemanticEvents.DefineArrayInkDivideClusters(page, preProcessedSemanticEvents);

            // Pass 2.1: OPTICS Clustering
            InkSemanticEvents.GenerateInitialInkClusters(preProcessedSemanticEvents);


            InkSemanticEvents.RefineSkipCountClusters(page, preProcessedSemanticEvents);

            
            // TODO: Rename/fix - Refine Temporal Clusters
            var processedEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in preProcessedSemanticEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_INK &&
                    semanticEvent.EventType == Codings.EVENT_INK_CHANGE)
                {
                    var processedInkChangeEvents = InkSemanticEvents.ProcessInkChangeSemanticEvent(page, semanticEvent);
                    processedEvents.AddRange(processedInkChangeEvents);
                }
                else
                {
                    processedEvents.Add(semanticEvent);
                }
            }

            return processedEvents;
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
                else if (semanticEvent.CodedObject == Codings.OBJECT_FILL_IN &&
                         semanticEvent.EventType == Codings.EVENT_FILL_IN_CHANGE)
                {
                    // TODO: Include below like INK change processing
                    //preProcessedSemanticEvents = InkSemanticEvents.RefineANS_FIClusters(page, preProcessedSemanticEvents);
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
            var allInterpretedEvents = new List<ISemanticEvent>();

            if (semanticEvent.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_RIGHT_SKIP) &&
                semanticEvent.EventInformation.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedEvent = ArraySemanticEvents.SkipCounting(page, semanticEvent);
                if (interpretedEvent != null)
                {
                    allInterpretedEvents.Add(interpretedEvent);
                    return allInterpretedEvents;
                }
            }

            if (semanticEvent.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_OVER) &&
                semanticEvent.EventInformation.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedEvent = ArraySemanticEvents.ArrayEquation(page, semanticEvent);
                if (interpretedEvent != null)
                {
                    allInterpretedEvents.Add(interpretedEvent);
                    return allInterpretedEvents;
                }
            }

            if (!semanticEvent.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_OVER))
            {
                var interpretedEvent = InkSemanticEvents.Arithmetic(page, semanticEvent);
                if (interpretedEvent != null)
                {
                    allInterpretedEvents.Add(interpretedEvent);
                    return allInterpretedEvents;
                }
            }

            // TODO: Attempt to interpret inked circles around a multiple choice bubbles

            if (!allInterpretedEvents.Any())
            {
                allInterpretedEvents.Add(semanticEvent);
            }

            return allInterpretedEvents;
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
                definitionRelation.numberOfGroups = div.Divisor.RelationPartAnswerValue;
                definitionRelation.product = div.Dividend.RelationPartAnswerValue;
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
                definitionRelation.isOrderedGroup = mult.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;  // TODO: Was OrderedEqualGroups, confirm
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
                definitionRelation.isOrderedGroup = m1.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;  // TODO: Was OrderedEqualGroups, confirm
                definitionRelation.isProductImportant = true;

                isOtherDefinitionUsed = true;
                otherDefinitionRelation.groupSize = m2.Factors.Last().RelationPartAnswerValue;
                otherDefinitionRelation.numberOfGroups = m2.Factors.First().RelationPartAnswerValue;
                otherDefinitionRelation.product = m2.Product;
                otherDefinitionRelation.isOrderedGroup = m2.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;  // TODO: Was OrderedEqualGroups, confirm
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
                semanticEvents.Where(h => h.EventType == Codings.EVENT_OBJECT_DELETE && (h.CodedObject == Codings.OBJECT_ARRAY || h.CodedObject == Codings.OBJECT_NUMBER_LINE))
                              .Select(h => h.HistoryActions.First().HistoryActionIndex - 1)
                              .ToList();
            if (!page.History.CompleteOrderedHistoryActions.Any())
            {
                return;
            }
            var lastHistoryIndex = page.History.CompleteOrderedHistoryActions.Last().HistoryActionIndex + 1;
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
            var answerEvents = semanticEvents.Where(Codings.IsAnswerObject).ToList();
            if (answerEvents.Count < 1)
            {
                return;
            }

            var firstAnswer = semanticEvents.First(Codings.IsAnswerObject);
            var firstIndex = semanticEvents.IndexOf(firstAnswer);

            var beforeEvents = semanticEvents.Take(firstIndex + 1).ToList();
            var isUsingRepresentationsBefore = beforeEvents.Any(h => Codings.IsRepresentationObject(h) && h.EventType == Codings.EVENT_OBJECT_ADD);

            if (isUsingRepresentationsBefore)
            {
                return;
            }

            var afterEvents = semanticEvents.Skip(firstIndex).ToList();
            var isUsingRepresentationsAfter = afterEvents.Any(h => Codings.IsRepresentationObject(h) && h.EventType == Codings.EVENT_OBJECT_ADD);

            if (!isUsingRepresentationsAfter)
            {
                return;
            }

            // TODO: Derive this entire Analysis Code from ARA Tag and don't use this Tag
            var tag = new AnswerBeforeRepresentationTag(page, Origin.StudentPageGenerated, afterEvents);
            page.AddTag(tag);
        }

        public static void AttemptAnswerChangedAfterRepresentationTag(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var answerEvents = semanticEvents.Where(Codings.IsAnswerObject).ToList();
            if (answerEvents.Count < 2)
            {
                return;
            }

            var firstAnswer = semanticEvents.First(Codings.IsAnswerObject);
            var firstIndex = semanticEvents.IndexOf(firstAnswer);
            var lastAnswer = semanticEvents.Last(Codings.IsAnswerObject);
            var lastIndex = semanticEvents.IndexOf(lastAnswer);

            var possibleTagEvents = semanticEvents.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToList();
            var isUsingRepresentations = possibleTagEvents.Any(h => Codings.IsRepresentationObject(h) && h.EventType == Codings.EVENT_OBJECT_ADD);

            if (!isUsingRepresentations)
            {
                return;
            }

            var tag = new AnswerChangedAfterRepresentationTag(page, Origin.StudentPageGenerated, possibleTagEvents);
            page.AddTag(tag);
        }

        public static void AttemptAnswerTag(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            // BUG: will miss instances where mc incorrect, mc correct, mc erase incorrect
            var lastAnswerEvent = semanticEvents.LastOrDefault(Codings.IsAnswerObject);
            if (lastAnswerEvent == null ||
                lastAnswerEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ERASE ||
                lastAnswerEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ERASE_INCOMPLETE ||
                lastAnswerEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL ||
                lastAnswerEvent.EventType == Codings.EVENT_FILL_IN_ERASE)
            {
                return;
            }

            var tag = new AnswerCorrectnessTag(page,
                                               Origin.StudentPageGenerated,
                                               new List<ISemanticEvent>
                                               {
                                                   lastAnswerEvent
                                               });
            page.AddTag(tag);
        }

        #endregion // Last Pass: Tag Generation
    }
}