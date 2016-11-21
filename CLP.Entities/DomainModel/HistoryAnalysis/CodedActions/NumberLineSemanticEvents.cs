using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class NumberLineSemanticEvents
    {
        #region Static Methods

        public static ISemanticEvent EndPointsChange(CLPPage page, List<NumberLineEndPointsChangedHistoryAction> endPointsChangedHistoryItems)
        {
            if (page == null ||
                endPointsChangedHistoryItems == null ||
                !endPointsChangedHistoryItems.Any())
            {
                return null;
            }

            var numberLineID = endPointsChangedHistoryItems.First().NumberLineID;
            var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
            if (numberLine == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_NUMBER_LINE;
            var codedID = endPointsChangedHistoryItems.First().PreviousEndValue.ToString();
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);
            var eventInfo = endPointsChangedHistoryItems.Last().NewEndValue.ToString();
            var eventInfoIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, eventInfo);
            if (!string.IsNullOrWhiteSpace(eventInfoIncrementID))
            {
                eventInfo += " " + eventInfoIncrementID;
            }

            var semanticEvent = new SemanticEvent(page, endPointsChangedHistoryItems.Cast<IHistoryAction>().ToList())
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

        public static ISemanticEvent JumpSizesChange(CLPPage page, List<NumberLineJumpSizesChangedHistoryAction> jumpSizesChangedHistoryItems)
        {
            if (page == null ||
                jumpSizesChangedHistoryItems == null ||
                !jumpSizesChangedHistoryItems.Any() ||
                jumpSizesChangedHistoryItems.Select(h => h.NumberLineID).Distinct().Count() != 1)
            {
                return null;
            }

            var numberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;
            var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
            if (numberLine == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_NUMBER_LINE;
            var codedID = numberLine.GetCodedIDAtHistoryIndex(jumpSizesChangedHistoryItems.First().HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);
            var isAdding = jumpSizesChangedHistoryItems.First().JumpsAdded.Any() && !jumpSizesChangedHistoryItems.First().JumpsRemoved.Any();

            var allJumps = new List<NumberLineJumpSize>();
            foreach (var historyItem in jumpSizesChangedHistoryItems)
            {
                var jumps = isAdding ? historyItem.JumpsAdded : historyItem.JumpsRemoved;
                allJumps.AddRange(jumps);
            }

            if (!allJumps.Any())
            {
                return null;
            }

            var eventInfo = NumberLine.ConsolidateJumps(allJumps);

            var semanticEvent = new SemanticEvent(page, jumpSizesChangedHistoryItems.Cast<IHistoryAction>().ToList())
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