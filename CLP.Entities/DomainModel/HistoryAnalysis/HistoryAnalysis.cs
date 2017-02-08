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
                                                CodedObjectID = "1",
                                                EventInformation = "Initialization"
                                            });

            var initialSemanticEvents = GenerateInitialSemanticEvents(page);
            var eventIndex = 0;
            foreach (var initialSemanticEvent in initialSemanticEvents)
            {
                initialSemanticEvent.SemanticPassNumber = 1;
                initialSemanticEvent.SemanticEventIndex = eventIndex;
                eventIndex++;
            }

            page.History.SemanticEvents.AddRange(initialSemanticEvents);

            // Second Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryAction>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2",
                                                EventInformation = "Ink Clustering"
                                            });

            var clusteredInkSemanticEvents = ClusterInkSemanticEvents(page, initialSemanticEvents);
            eventIndex = 0;
            foreach (var initialSemanticEvent in clusteredInkSemanticEvents)
            {
                initialSemanticEvent.SemanticPassNumber = 2;
                initialSemanticEvent.SemanticEventIndex = eventIndex;
                eventIndex++;
            }

            page.History.SemanticEvents.AddRange(clusteredInkSemanticEvents);

            // Third Pass
            page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryAction>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "3",
                                                EventInformation = "Ink Interpretation"
                                            });

            var interpretedInkSemanticEvents = InterpretInkSemanticEvents(page, clusteredInkSemanticEvents);
            eventIndex = 0;
            foreach (var initialSemanticEvent in interpretedInkSemanticEvents)
            {
                initialSemanticEvent.SemanticPassNumber = 3;
                initialSemanticEvent.SemanticEventIndex = eventIndex;
                eventIndex++;
            }

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

        private static List<ISemanticEvent> GenerateInitialSemanticEvents(CLPPage page)
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

        private static ISemanticEvent VerifyAndGenerateSingleActionEvent(CLPPage page, IHistoryAction historyAction)
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

        private static ISemanticEvent VerifyAndGenerateCompoundActionEvent(CLPPage page, List<IHistoryAction> historyActions, IHistoryAction nextHistoryAction)
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

        private static List<ISemanticEvent> ClusterInkSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

            InkSemanticEvents.InkClusters.Clear();

            // Pass 2.0: Pre-Cluster
            var preProcessedSemanticEvents = InkSemanticEvents.GenerateArrayInkDivideSemanticEvents(page, semanticEvents);
            InkSemanticEvents.DefineMultipleChoiceClusters(page, preProcessedSemanticEvents);
            InkSemanticEvents.DefineFillInClusters(page, preProcessedSemanticEvents);
            InkSemanticEvents.DefineArrayInkDivideClusters(page, preProcessedSemanticEvents);
            InkSemanticEvents.DefineSkipCountClusters(page, preProcessedSemanticEvents);

            // Pass 2.1: OPTICS Clustering
            InkSemanticEvents.GenerateInitialInkClusters(preProcessedSemanticEvents);

            // Pass 2.2: Refine OPTICS Clusters 
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

            // TODO: Pass 2.3: Refine Temporal Clusters?

            return processedEvents;
        }

        #endregion // Second Pass: Ink Clustering

        #region Third Pass: Ink Interpretation

        private static List<ISemanticEvent> InterpretInkSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvents), semanticEvents);

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
                    var interpretedSemanticEvents = InkSemanticEvents.ProcessFinalAnswerFillInChangeSemanticEvent(page, semanticEvent);
                    allInterpretedSemanticEvents.AddRange(interpretedSemanticEvents);
                }
                else
                {
                    allInterpretedSemanticEvents.Add(semanticEvent);
                }
            }

            return allInterpretedSemanticEvents;
        }

        private static List<ISemanticEvent> AttemptSemanticEventInterpretation(CLPPage page, ISemanticEvent semanticEvent)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(semanticEvent), semanticEvent);

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
                // TODO: Attempt Dots/Lines interpretation
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

        #region Fourth Pass: Consolidation

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

        #endregion // Fourth Pass: Consolidation

        #region Last Pass: Tag Generation

        public static void GenerateTags(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            //ArrayStrategyTag.IdentifyArrayStrategies(page, semanticEvents);
            //AttemptAnswerBeforeRepresentationTag(page, semanticEvents);
            //AttemptAnswerChangedAfterRepresentationTag(page, semanticEvents);
            var finalAnswerCorrectness = FinalAnswerCorrectnessTag.AttemptTagGeneration(page, semanticEvents);
            var representationsUsedTag = RepresentationsUsedTag.AttemptTagGeneration(page, semanticEvents);
            var representationCorrectness = RepresentationCorrectnessTag.AttemptTagGeneration(page, representationsUsedTag);
            CorrectnessSummaryTag.AttemptTagGeneration(page, representationCorrectness, finalAnswerCorrectness);
            AnswerRepresentationSequence.AttemptTagGeneration(page, semanticEvents);
        }

        // TODO: Move each Attempt method to the Tag's class

        public static void AttemptAnswerBeforeRepresentationTag(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var answerEvents = semanticEvents.Where(Codings.IsFinalAnswerEvent).ToList();
            if (answerEvents.Count < 1)
            {
                return;
            }

            var firstAnswer = semanticEvents.First(Codings.IsFinalAnswerEvent);
            var firstIndex = semanticEvents.IndexOf(firstAnswer);

            var beforeEvents = semanticEvents.Take(firstIndex + 1).ToList();
            var isUsingRepresentationsBefore = beforeEvents.Any(h => Codings.IsRepresentationEvent(h) && h.EventType == Codings.EVENT_OBJECT_ADD);

            if (isUsingRepresentationsBefore)
            {
                return;
            }

            var afterEvents = semanticEvents.Skip(firstIndex).ToList();
            var isUsingRepresentationsAfter = afterEvents.Any(h => Codings.IsRepresentationEvent(h) && h.EventType == Codings.EVENT_OBJECT_ADD);

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
            var answerEvents = semanticEvents.Where(Codings.IsFinalAnswerEvent).ToList();
            if (answerEvents.Count < 2)
            {
                return;
            }

            var firstAnswer = semanticEvents.First(Codings.IsFinalAnswerEvent);
            var firstIndex = semanticEvents.IndexOf(firstAnswer);
            var lastAnswer = semanticEvents.Last(Codings.IsFinalAnswerEvent);
            var lastIndex = semanticEvents.IndexOf(lastAnswer);

            var possibleTagEvents = semanticEvents.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToList();
            var isUsingRepresentations = possibleTagEvents.Any(h => Codings.IsRepresentationEvent(h) && h.EventType == Codings.EVENT_OBJECT_ADD);

            if (!isUsingRepresentations)
            {
                return;
            }

            var tag = new AnswerChangedAfterRepresentationTag(page, Origin.StudentPageGenerated, possibleTagEvents);
            page.AddTag(tag);
        }

        #endregion // Last Pass: Tag Generation
    }
}