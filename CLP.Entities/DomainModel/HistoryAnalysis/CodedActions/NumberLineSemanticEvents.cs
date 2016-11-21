using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class NumberLineSemanticEvents
    {
        #region Static Methods

        public static ISemanticEvent EndPointsChange(CLPPage page, List<NumberLineEndPointsChangedHistoryAction> endPointsChangedHistoryActions)
        {
            if (page == null ||
                endPointsChangedHistoryActions == null ||
                !endPointsChangedHistoryActions.Any())
            {
                return null;
            }

            var numberLineID = endPointsChangedHistoryActions.First().NumberLineID;
            var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
            if (numberLine == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_NUMBER_LINE;
            var codedID = endPointsChangedHistoryActions.First().PreviousEndValue.ToString();
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);
            var eventInfo = endPointsChangedHistoryActions.Last().NewEndValue.ToString();
            var eventInfoIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, eventInfo);
            if (!string.IsNullOrWhiteSpace(eventInfoIncrementID))
            {
                eventInfo += " " + eventInfoIncrementID;
            }

            var semanticEvent = new SemanticEvent(page, endPointsChangedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = Codings.EVENT_NUMBER_LINE_CHANGE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = numberLineID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent JumpSizesChange(CLPPage page, List<NumberLineJumpSizesChangedHistoryAction> jumpSizesChangedHistoryActions)
        {
            if (page == null ||
                jumpSizesChangedHistoryActions == null ||
                !jumpSizesChangedHistoryActions.Any() ||
                jumpSizesChangedHistoryActions.Select(h => h.NumberLineID).Distinct().Count() != 1)
            {
                return null;
            }

            var numberLineID = jumpSizesChangedHistoryActions.First().NumberLineID;
            var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
            if (numberLine == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_NUMBER_LINE;
            var codedID = numberLine.GetCodedIDAtHistoryIndex(jumpSizesChangedHistoryActions.First().HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);
            var isAdding = jumpSizesChangedHistoryActions.First().JumpsAdded.Any() && !jumpSizesChangedHistoryActions.First().JumpsRemoved.Any();

            var allJumps = new List<NumberLineJumpSize>();
            foreach (var historyAction in jumpSizesChangedHistoryActions)
            {
                var jumps = isAdding ? historyAction.JumpsAdded : historyAction.JumpsRemoved;
                allJumps.AddRange(jumps);
            }

            if (!allJumps.Any())
            {
                return null;
            }

            var eventInfo = NumberLine.ConsolidateJumps(allJumps);

            var semanticEvent = new SemanticEvent(page, jumpSizesChangedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = isAdding ? Codings.EVENT_NUMBER_LINE_JUMP : Codings.EVENT_NUMBER_LINE_JUMP_ERASE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = numberLineID
                                };

            return semanticEvent;
        }

        #endregion // Static Methods
    }
}