using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Ink;
using Catel;
using Catel.Collections;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void GenerateSemanticEvents(CLPPage page)
        {
            Argument.IsNotNull(nameof(page), page);

            CLogger.AppendToLog($"Analyzing {page.Owner.FullName}'s Page {page.PageNumber}, Version Index {page.VersionIndex}");

            ObjectSemanticEvents.InitializeIncrementIDs();
            page.History.SemanticEvents.Clear();

            FixANSFIHistoryActions(page);

            // First Pass
            var initialSemanticEvents = GenerateInitialSemanticEvents(page);
            page.History.SemanticEventPasses.Add(new SemanticEventPass("Initialization", 1,  initialSemanticEvents));

            // Second Pass
            var clusteredInkSemanticEvents = ClusterInkSemanticEvents(page, initialSemanticEvents);
            page.History.SemanticEventPasses.Add(new SemanticEventPass("Ink Clustering", 2, clusteredInkSemanticEvents));

            // Third Pass
            var interpretedInkSemanticEvents = InterpretInkSemanticEvents(page, clusteredInkSemanticEvents);
            page.History.SemanticEventPasses.Add(new SemanticEventPass("Ink Interpretation", 3, interpretedInkSemanticEvents));

            // Fourth Pass
            //page.History.SemanticEvents.Add(new SemanticEvent(page, new List<IHistoryAction>())
            //                                {
            //                                    CodedObject = "\tPASS",
            //                                    CodedObjectID = "4",
            //                                    EventInformation = "Ink Interpretation",
            //                                    SemanticEventIndex = -4
            //                                });

            //var interpretedInkSemanticEvents = InterpretInkSemanticEvents(page, clusteredInkSemanticEvents);
            //eventIndex = 0;
            //foreach (var initialSemanticEvent in interpretedInkSemanticEvents)
            //{
            //    initialSemanticEvent.SemanticPassNumber = 3;
            //    initialSemanticEvent.SemanticEventIndex = eventIndex;
            //    eventIndex++;
            //}

            //page.History.SemanticEvents.AddRange(interpretedInkSemanticEvents);

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

        #region Zero Pass: Fix ANS FI HistoryActions

        private static void FixANSFIHistoryActions(CLPPage page)
        {
            #region Adjust Interpretation Regions

            switch (page.ID)
            {
                case "yzvpdIROIEOFrndOASGjvA": // Page 9 Assessment
                {
                    var interpretationRegion = page.PageObjects.OfType<InterpretationRegion>().First();
                    interpretationRegion.Height = 100.0;
                }
                    break;
                case "QHJ7pFHY3ECr8u6bSFRCkA": // Page 12 Assessment
                {
                    var interpretationRegion = page.PageObjects.OfType<InterpretationRegion>().First();
                    interpretationRegion.Height = 170.3069;
                    interpretationRegion.YPosition = 614.9809;
                }
                    break;
            }

            #endregion // Adjust Interpretation Regions

            var strokeChangedHistoryActions = page.History.UndoActions.OfType<ObjectsOnPageChangedHistoryAction>().Where(h => h.IsUsingStrokes).OrderBy(h => h.HistoryActionIndex).ToList();

            foreach (var strokeChangedHistoryAction in strokeChangedHistoryActions)
            {
                var isAdd = false;
                Stroke strokeChanged = null;
                if (strokeChangedHistoryAction.StrokeIDsAdded.Count == 1 &&
                    !strokeChangedHistoryAction.StrokeIDsRemoved.Any())
                {
                    isAdd = true;

                    var strokeID = strokeChangedHistoryAction.StrokeIDsAdded.First();
                    strokeChanged = page.GetStrokeByIDOnPageOrInHistory(strokeID);
                }
                else if (strokeChangedHistoryAction.StrokeIDsRemoved.Count == 1 &&
                         !strokeChangedHistoryAction.StrokeIDsAdded.Any())
                {
                    isAdd = false;

                    var strokeID = strokeChangedHistoryAction.StrokeIDsRemoved.First();
                    strokeChanged = page.GetStrokeByIDOnPageOrInHistory(strokeID);
                }

                if (strokeChanged == null)
                {
                    continue;
                }

                #region Check for Interpretation Region Fill-In

                var isOverInterpretationRegion = false;
                foreach (var interpretationRegion in page.PageObjects.OfType<InterpretationRegion>())
                {
                    var isStrokeOver = interpretationRegion.IsStrokeOverPageObject(strokeChanged);
                    if (!isStrokeOver)
                    {
                        continue;
                    }

                    isOverInterpretationRegion = true;

                    var strokesAdded = new List<Stroke>();
                    var strokesRemoved = new List<Stroke>();

                    if (isAdd)
                    {
                        strokesAdded.Add(strokeChanged);
                    }
                    else
                    {
                        strokesRemoved.Add(strokeChanged);
                    }

                    interpretationRegion.ChangeAcceptedStrokes(strokesAdded, strokesRemoved);
                    var fillInAnswerChangedHistoryAction = new FillInAnswerChangedHistoryAction(page,
                                                                                                page.Owner,
                                                                                                interpretationRegion,
                                                                                                strokesAdded,
                                                                                                strokesRemoved);

                    fillInAnswerChangedHistoryAction.HistoryActionIndex = strokeChangedHistoryAction.HistoryActionIndex;
                    fillInAnswerChangedHistoryAction.CachedFormattedValue = fillInAnswerChangedHistoryAction.FormattedValue;

                    var indexToReplace = page.History.UndoActions.IndexOf(strokeChangedHistoryAction);
                    page.History.UndoActions[indexToReplace] = fillInAnswerChangedHistoryAction;

                    break;
                }

                #endregion // Check for Interpretation Region Fill-In

                #region Check for Multiple Choice

                if (isOverInterpretationRegion)
                {
                    continue;
                }

                foreach (var multipleChoice in page.PageObjects.OfType<MultipleChoice>())
                {
                    var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(strokeChanged);
                    if (choiceBubbleStrokeIsOver == null)
                    {
                        continue;
                    }

                    var index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                    var strokesOverBubble = multipleChoice.StrokesOverChoiceBubble(choiceBubbleStrokeIsOver);

                    const int THRESHOLD = 80;
                    var status = ChoiceBubbleStatuses.PartiallyFilledIn;
                    var isStatusSet = false;
                    if (isAdd)
                    {
                        var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                        if (totalStrokeLength >= THRESHOLD)
                        {
                            status = ChoiceBubbleStatuses.AdditionalFilledIn;
                        }
                        else
                        {
                            totalStrokeLength += strokeChanged.StylusPoints.Count;
                            if (totalStrokeLength >= THRESHOLD)
                            {
                                status = ChoiceBubbleStatuses.FilledIn;
                                choiceBubbleStrokeIsOver.IsFilledIn = true;
                            }
                            else
                            {
                                status = ChoiceBubbleStatuses.PartiallyFilledIn;
                            }
                        }
                        isStatusSet = true;
                    }
                    else
                    {
                        var isRemovedStrokeOverBubble = strokesOverBubble.FirstOrDefault(s => s.GetStrokeID() == strokeChanged.GetStrokeID()) != null;
                        if (!isRemovedStrokeOverBubble)
                        {
                            // TODO: Log error
                            continue;
                        }
                        var otherStrokes = strokesOverBubble.Where(s => s.GetStrokeID() != strokeChanged.GetStrokeID()).ToList();
                        var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                        var otherStrokesStrokeLength = otherStrokes.Sum(s => s.StylusPoints.Count);

                        if (totalStrokeLength < THRESHOLD)
                        {
                            status = ChoiceBubbleStatuses.ErasedPartiallyFilledIn;
                        }
                        else
                        {
                            if (otherStrokesStrokeLength < THRESHOLD)
                            {
                                status = ChoiceBubbleStatuses.CompletelyErased;
                                choiceBubbleStrokeIsOver.IsFilledIn = false;
                            }
                            else
                            {
                                status = ChoiceBubbleStatuses.IncompletelyErased;
                            }
                        }
                        isStatusSet = true;
                    }

                    if (!isStatusSet ||
                        index == -1)
                    {
                        continue;
                    }

                    var strokesAdded = new List<Stroke>();
                    var strokesRemoved = new List<Stroke>();

                    if (isAdd)
                    {
                        strokesAdded.Add(strokeChanged);
                    }
                    else
                    {
                        strokesRemoved.Add(strokeChanged);
                    }

                    multipleChoice.ChangeAcceptedStrokes(strokesAdded, strokesRemoved);
                    var multipleChoiceBubbleStatusChangedHistoryAction =
                        new MultipleChoiceBubbleStatusChangedHistoryAction(page, page.Owner, multipleChoice, index, status, strokesAdded, strokesRemoved);

                    multipleChoiceBubbleStatusChangedHistoryAction.HistoryActionIndex = strokeChangedHistoryAction.HistoryActionIndex;
                    multipleChoiceBubbleStatusChangedHistoryAction.CachedFormattedValue = multipleChoiceBubbleStatusChangedHistoryAction.FormattedValue;

                    var indexToReplace = page.History.UndoActions.IndexOf(strokeChangedHistoryAction);
                    page.History.UndoActions[indexToReplace] = multipleChoiceBubbleStatusChangedHistoryAction;

                    break;
                }

                #endregion // Check for Multiple Choice
            }
        }

        #endregion // Zero Pass: Fix ANS FI HistoryActions

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
                    var singleSemanticEvents = VerifyAndGenerateSingleActionEvent(page, historyActionBuffer.First());
                    if (singleSemanticEvents.Any())
                    {
                        initialSemanticEvents.AddRange(singleSemanticEvents);
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

        private static List<ISemanticEvent> VerifyAndGenerateSingleActionEvent(CLPPage page, IHistoryAction historyAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(historyAction), historyAction);

            // TODO: Division Values Changed, DT Array Snapped/Removed, Animation Stop/Start

            var semanticEvents = new List<ISemanticEvent>();
            TypeSwitch.On(historyAction)
                      .Case<ObjectsOnPageChangedHistoryAction>(h =>
                                                               {
                                                                   var generatedSemanticEvents = ObjectSemanticEvents.Add(page, h);
                                                                   if (!generatedSemanticEvents.Any())
                                                                   {
                                                                       generatedSemanticEvents = ObjectSemanticEvents.Delete(page, h);
                                                                   }
                                                                   semanticEvents.AddRange(generatedSemanticEvents);
                                                               })
                      .Case<PartsValueChangedHistoryAction>(h =>
                                                                {
                                                                    var semanticEvent = StampSemanticEvents.PartsValueChanged(page, h);
                                                                    semanticEvents.Add(semanticEvent);
                                                                })
                      .Case<CLPArrayRotateHistoryAction>(h =>
                                                         {
                                                             var semanticEvent = ArraySemanticEvents.Rotate(page, h);
                                                             semanticEvents.Add(semanticEvent);
                                                         })
                      .Case<PageObjectCutHistoryAction>(h =>
                                                        {
                                                            var semanticEvent = ArraySemanticEvents.Cut(page, h);
                                                            semanticEvents.Add(semanticEvent);
                                                        })
                      .Case<CLPArraySnapHistoryAction>(h =>
                                                       {
                                                           var semanticEvent = ArraySemanticEvents.Snap(page, h);
                                                           semanticEvents.Add(semanticEvent);
                                                       })
                      .Case<CLPArrayDivisionsChangedHistoryAction>(h =>
                                                                   {
                                                                       var semanticEvent = ArraySemanticEvents.Divide(page, h);
                                                                       semanticEvents.Add(semanticEvent);
                                                                   });

            

            return semanticEvents;
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
                var correctness = bubble.IsACorrectValue ? Codings.CORRECTNESS_CODED_CORRECT : Codings.CORRECTNESS_CODED_INCORRECT;
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

            if (semanticEvent.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_BOTTOM_SKIP) &&
                semanticEvent.EventInformation.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedEvent = ArraySemanticEvents.BottomSkipCounting(page, semanticEvent);
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

        #region Fourth Pass: Consolidation

        public static List<ISemanticEvent> RefineSemanticEvents(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var buffer = new List<ISemanticEvent>();
            ISemanticEvent mostRecentSemanticEvent = null;

            #region Collapse INK ignore Events

            var collapsedInkIgnoreEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in semanticEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_INK &&
                    semanticEvent.EventType == Codings.EVENT_INK_IGNORE)
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (buffer.Any())
                {
  //                  semanticEvent.SemanticEvents.AddRange(buffer);
                    buffer.Clear();
                }

                mostRecentSemanticEvent = semanticEvent;
                collapsedInkIgnoreEvents.Add(semanticEvent);
            }

            if (buffer.Any() &&
                mostRecentSemanticEvent != null)
            {
  //              mostRecentSemanticEvent.SemanticEvents.AddRange(buffer);
                buffer.Clear();
            }

            #endregion // Collapse INK ignore Events

            #region Collapse Sequential ANS MC/FI

            #region Adds

            buffer.Clear();

            var collapsedAnsAddEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in collapsedInkIgnoreEvents)
            {
                if (semanticEvent.EventType == Codings.EVENT_FILL_IN_ADD ||
                    semanticEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ADD ||
                    semanticEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ADD_ADDITIONAL ||
                    semanticEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ADD_PARTIAL)
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (buffer.Any())
                {
                    var lastEventInBuffer = buffer.Last();

                    var collapsedEvent = new SemanticEvent(page, buffer)
                                         {
                                             CodedObject = lastEventInBuffer.CodedObject,
                                             EventType = lastEventInBuffer.CodedObject == Codings.OBJECT_FILL_IN
                                                             ? Codings.EVENT_FILL_IN_ADD
                                                             : Codings.EVENT_MULTIPLE_CHOICE_ADD,
                                             CodedObjectID = lastEventInBuffer.CodedObjectID,
                                             EventInformation = lastEventInBuffer.EventInformation,
                                             ReferencePageObjectID = lastEventInBuffer.ReferencePageObjectID
                                         };

                    collapsedAnsAddEvents.Add(collapsedEvent);
                    buffer.Clear();
                }

                collapsedAnsAddEvents.Add(semanticEvent);
            }

            if (buffer.Any())
            {
                var lastEventInBuffer = buffer.Last();

                var collapsedEvent = new SemanticEvent(page, buffer)
                                     {
                                         CodedObject = lastEventInBuffer.CodedObject,
                                         EventType = lastEventInBuffer.CodedObject == Codings.OBJECT_FILL_IN
                                                         ? Codings.EVENT_FILL_IN_ADD
                                                         : Codings.EVENT_MULTIPLE_CHOICE_ADD,
                                         CodedObjectID = lastEventInBuffer.CodedObjectID,
                                         EventInformation = lastEventInBuffer.EventInformation,
                                         ReferencePageObjectID = lastEventInBuffer.ReferencePageObjectID
                                     };

                collapsedAnsAddEvents.Add(collapsedEvent);
                buffer.Clear();
            }

            #endregion // Adds

            #region Erases

            buffer.Clear();

            var collapsedAnsEraseEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in collapsedAnsAddEvents)
            {
                if (semanticEvent.EventType == Codings.EVENT_FILL_IN_ERASE ||
                    semanticEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ERASE ||
                    semanticEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ERASE_PARTIAL ||
                    semanticEvent.EventType == Codings.EVENT_MULTIPLE_CHOICE_ERASE_INCOMPLETE)
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (buffer.Any())
                {
                    var lastEventInBuffer = buffer.Last();

                    var collapsedEvent = new SemanticEvent(page, buffer)
                                         {
                                             CodedObject = lastEventInBuffer.CodedObject,
                                             EventType = lastEventInBuffer.CodedObject == Codings.OBJECT_FILL_IN
                                                             ? Codings.EVENT_FILL_IN_ERASE
                                                             : Codings.EVENT_MULTIPLE_CHOICE_ERASE,
                                             CodedObjectID = lastEventInBuffer.CodedObjectID,
                                             EventInformation = lastEventInBuffer.EventInformation,
                                             ReferencePageObjectID = lastEventInBuffer.ReferencePageObjectID
                                         };

                    collapsedAnsEraseEvents.Add(collapsedEvent);
                    buffer.Clear();
                }

                collapsedAnsEraseEvents.Add(semanticEvent);
            }

            if (buffer.Any())
            {
                var lastEventInBuffer = buffer.Last();

                var collapsedEvent = new SemanticEvent(page, buffer)
                                     {
                                         CodedObject = lastEventInBuffer.CodedObject,
                                         EventType = lastEventInBuffer.CodedObject == Codings.OBJECT_FILL_IN
                                                         ? Codings.EVENT_FILL_IN_ERASE
                                                         : Codings.EVENT_MULTIPLE_CHOICE_ERASE,
                                         CodedObjectID = lastEventInBuffer.CodedObjectID,
                                         EventInformation = lastEventInBuffer.EventInformation,
                                         ReferencePageObjectID = lastEventInBuffer.ReferencePageObjectID
                                     };

                collapsedAnsEraseEvents.Add(collapsedEvent);
                buffer.Clear();
            }

            #endregion // Erases

            #endregion // Collapse Sequential ANS MC/FI

            #region Collapse Interwoven Skips and ARITHs

            buffer.Clear();

            var collapsedSkipPlusArithEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in collapsedAnsEraseEvents)
            {
                if (semanticEvent.EventType == Codings.EVENT_ARRAY_SKIP ||
                    semanticEvent.EventType == Codings.EVENT_ARRAY_SKIP_ERASE)
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (semanticEvent.CodedObject == Codings.OBJECT_ARITH &&
                    buffer.Any())
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (buffer.Any())
                {
                    var arithCount = buffer.Count(e => e.CodedObject == Codings.OBJECT_ARITH);
                    var eventType = $"{Codings.EVENT_ARRAY_SKIP_PLUS_ARITH} ({arithCount})";

                }

                collapsedSkipPlusArithEvents.Add(semanticEvent);
            }

            if (buffer.Any())
            {
                
            }

            #endregion // Collapse Interwoven Skips and ARITHs

            #region Collapse Sequential ARITHs

            #region Adds

            buffer.Clear();

            var collapsedArithAddEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in collapsedSkipPlusArithEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_ARITH &&
                    semanticEvent.EventType == Codings.EVENT_ARITH_ADD)
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (buffer.Any())
                {
                   var collapsedEvent = new SemanticEvent(page, buffer)
                                        {
                                            CodedObject = Codings.OBJECT_ARITH,
                                            EventType = Codings.EVENT_ARITH_ADD
                                        };

                    collapsedArithAddEvents.Add(collapsedEvent);
                    buffer.Clear();
                }

                collapsedArithAddEvents.Add(semanticEvent);
            }

            if (buffer.Any())
            {
                var collapsedEvent = new SemanticEvent(page, buffer)
                                     {
                                         CodedObject = Codings.OBJECT_ARITH,
                                         EventType = Codings.EVENT_ARITH_ADD
                                     };

                collapsedArithAddEvents.Add(collapsedEvent);
                buffer.Clear();
            }

            #endregion // Adds

            #region Erases

            buffer.Clear();

            var collapsedArithEraseEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in collapsedArithAddEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_ARITH &&
                    semanticEvent.EventType == Codings.EVENT_ARITH_ERASE)
                {
                    buffer.Add(semanticEvent);
                    continue;
                }

                if (buffer.Any())
                {
                    var collapsedEvent = new SemanticEvent(page, buffer)
                                         {
                                             CodedObject = Codings.OBJECT_ARITH,
                                             EventType = Codings.EVENT_ARITH_ERASE
                                         };

                    collapsedArithEraseEvents.Add(collapsedEvent);
                    buffer.Clear();
                }

                collapsedArithEraseEvents.Add(semanticEvent);
            }

            if (buffer.Any())
            {
                var collapsedEvent = new SemanticEvent(page, buffer)
                                     {
                                         CodedObject = Codings.OBJECT_ARITH,
                                         EventType = Codings.EVENT_ARITH_ERASE
                                     };

                collapsedArithEraseEvents.Add(collapsedEvent);
                buffer.Clear();
            }

            #endregion // Erases

            #endregion // Collapse Sequential ARITHs

            #region Remove Moves

            var withoutMoveEvents = new List<ISemanticEvent>();
            foreach (var semanticEvent in collapsedArithEraseEvents)
            {
                if (semanticEvent.EventType == Codings.EVENT_OBJECT_MOVE)
                {
                    continue;
                }

                withoutMoveEvents.Add(semanticEvent);
            }

            #endregion // Remove Moves

            return withoutMoveEvents;
        }

        #endregion // Fourth Pass: Consolidation

        #region Last Pass: Tag Generation

        public static void GenerateTags(CLPPage page, List<ISemanticEvent> semanticEvents, bool isRemovingExistingTags = true)
        {
            if (isRemovingExistingTags)
            {
                var existingTags = page.Tags.Where(t => t.Category != Category.Definition).ToList();
                foreach (var existingTag in existingTags)
                {
                    if (existingTag is MetaDataTag metaDataTag &&
                        metaDataTag.TagName == MetaDataTag.NAME_WORD_PROBLEM)
                    {
                        continue;
                    }

                    page.RemoveTag(existingTag);
                }
            }

            ProblemInformationTag.AttemptTagGeneration(page);
            AnswerRepresentationSequenceTag.AttemptTagGeneration(page, semanticEvents);

            var representationsUsedTag = RepresentationsUsedTag.AttemptTagGeneration(page, semanticEvents);
            var finalAnswerCorrectness = FinalAnswerCorrectnessTag.AttemptTagGeneration(page, semanticEvents);
            var representationCorrectness = FinalRepresentationCorrectnessTag.AttemptTagGeneration(page, representationsUsedTag);
            CorrectnessSummaryTag.AttemptTagGeneration(page, representationCorrectness, finalAnswerCorrectness);
            IntermediaryAnswerCorrectnessTag.AttemptTagGeneration(page, semanticEvents);
            
            //ArrayStrategyTag.IdentifyArrayStrategies(page, semanticEvents);

            NumberLineStrategyTag.IdentifyNumberLineStrategies(page, semanticEvents);
        }

        #endregion // Last Pass: Tag Generation
    }
}