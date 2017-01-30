using System.Collections.Generic;
using System.Linq;
using Catel;

namespace CLP.Entities
{
    public static class NumberLineSemanticEvents
    {
        #region Initialization

        public static ISemanticEvent EndPointsChange(CLPPage page, List<NumberLineEndPointsChangedHistoryAction> endPointsChangedHistoryActions)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(endPointsChangedHistoryActions), endPointsChangedHistoryActions);

            if (!endPointsChangedHistoryActions.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, endPointsChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_EMPTY_LIST, "EndPointsChange, No Actions");
            }

            if (endPointsChangedHistoryActions.Select(h => h.NumberLineID).Distinct().Count() != 1)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, endPointsChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_MIXED_LIST, "EndPointsChange, Mixed Number Line IDs");
            }

            var numberLineID = endPointsChangedHistoryActions.First().NumberLineID;
            var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
            if (numberLine == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, endPointsChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "EndPointsChange, Number Line NULL");
            }

            var codedObject = Codings.OBJECT_NUMBER_LINE;
            var eventType = Codings.EVENT_NUMBER_LINE_CHANGE;
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
                                    EventType = eventType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = numberLineID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent JumpSizesChange(CLPPage page, List<NumberLineJumpSizesChangedHistoryAction> jumpSizesChangedHistoryActions)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(jumpSizesChangedHistoryActions), jumpSizesChangedHistoryActions);

            if (!jumpSizesChangedHistoryActions.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, jumpSizesChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_EMPTY_LIST, "JumpSizesChange, No Actions");
            }

            if (jumpSizesChangedHistoryActions.Select(h => h.NumberLineID).Distinct().Count() != 1)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, jumpSizesChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_MIXED_LIST, "JumpSizesChange, Mixed Number Line IDs");
            }

            var numberLineID = jumpSizesChangedHistoryActions.First().NumberLineID;
            var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
            if (numberLine == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, jumpSizesChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "JumpSizesChange, Number Line NULL");
            }

            var codedObject = Codings.OBJECT_NUMBER_LINE;
            var isAdding = jumpSizesChangedHistoryActions.First().JumpsAdded.Any() && !jumpSizesChangedHistoryActions.First().JumpsRemoved.Any();
            var eventType = isAdding ? Codings.EVENT_NUMBER_LINE_JUMP : Codings.EVENT_NUMBER_LINE_JUMP_ERASE;
            var codedID = numberLine.GetCodedIDAtHistoryIndex(jumpSizesChangedHistoryActions.First().HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);

            var allJumps = new List<NumberLineJumpSize>();
            foreach (var historyAction in jumpSizesChangedHistoryActions)
            {
                var jumps = isAdding ? historyAction.JumpsAdded : historyAction.JumpsRemoved;
                allJumps.AddRange(jumps);
            }

            if (!allJumps.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, jumpSizesChangedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_EMPTY_LIST, "JumpSizesChange, No Jumps Added or Removed");
            }

            var eventInfo = NumberLine.ConsolidateJumps(allJumps);

            var semanticEvent = new SemanticEvent(page, jumpSizesChangedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = numberLineID
                                };

            return semanticEvent;
        }

        #endregion // Initialization
    }
}