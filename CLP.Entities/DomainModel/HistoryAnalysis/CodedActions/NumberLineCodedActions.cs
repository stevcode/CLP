using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class NumberLineCodedActions
    {
        #region Static Methods

        public static ISemanticEvent EndPointsChange(CLPPage page, List<NumberLineEndPointsChangedHistoryItem> endPointsChangedHistoryItems)
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
            var incrementID = ObjectCodedActions.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);
            var codedActionID = endPointsChangedHistoryItems.Last().NewEndValue.ToString();
            var codedActionIDIncrementID = ObjectCodedActions.SetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedActionID);
            if (!string.IsNullOrWhiteSpace(codedActionIDIncrementID))
            {
                codedActionID += " " + codedActionIDIncrementID;
            }

            var semanticEvent = new SemanticEvent(page, endPointsChangedHistoryItems.Cast<IHistoryItem>().ToList())
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_NUMBER_LINE_CHANGE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID,
                                    ReferencePageObjectID = numberLineID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent JumpSizesChange(CLPPage page, List<NumberLineJumpSizesChangedHistoryItem> jumpSizesChangedHistoryItems)
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
            var codedID = numberLine.GetCodedIDAtHistoryIndex(jumpSizesChangedHistoryItems.First().HistoryIndex);
            var incrementID = ObjectCodedActions.GetCurrentIncrementIDForPageObject(numberLine.ID, codedObject, codedID);
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

            var codedActionID = NumberLine.ConsolidateJumps(allJumps);

            var semanticEvent = new SemanticEvent(page, jumpSizesChangedHistoryItems.Cast<IHistoryItem>().ToList())
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = isAdding ? Codings.ACTION_NUMBER_LINE_JUMP : Codings.ACTION_NUMBER_LINE_JUMP_ERASE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID, 
                                    ReferencePageObjectID = numberLineID
                                };

            return semanticEvent;
        }

        #endregion // Static Methods
    }
}