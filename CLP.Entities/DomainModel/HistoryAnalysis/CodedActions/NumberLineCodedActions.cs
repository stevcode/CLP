using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class NumberLineCodedActions
    {
        #region Static Methods

        public static IHistoryAction EndPointsChange(CLPPage page, List<NumberLineEndPointsChangedHistoryItem> endPointsChangedHistoryItems)
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
            var incrementID = HistoryAction.GetIncrementID(numberLine.ID, codedObject, codedID);
            var codedActionID = endPointsChangedHistoryItems.Last().NewEndValue.ToString();
            var codedActionIDIncrementID = HistoryAction.IncrementAndGetIncrementID(numberLine.ID, codedObject, codedActionID);
            if (!string.IsNullOrWhiteSpace(codedActionIDIncrementID))
            {
                codedActionID += " " + codedActionIDIncrementID;
            }

            var historyAction = new HistoryAction(page, endPointsChangedHistoryItems.Cast<IHistoryItem>().ToList())
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_NUMBER_LINE_CHANGE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID
                                };

            return historyAction;
        }

        public static IHistoryAction JumpSizesChange(CLPPage page, List<NumberLineJumpSizesChangedHistoryItem> jumpSizesChangedHistoryItems)
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
            var incrementID = HistoryAction.GetIncrementID(numberLine.ID, codedObject, codedID);
            var jumpSegments = new List<string>();
            foreach (var historyItem in jumpSizesChangedHistoryItems)
            {
                // historyItem. // TODO: not enough information to build, probably need to modify and re-convert historyItems to included needed info.
            }

            //if (!jumpSegments.Any())
            //{
            //    return null;
            //}
            var codedActionID = string.Join("; ", jumpSegments);

            var historyAction = new HistoryAction(page, jumpSizesChangedHistoryItems.Cast<IHistoryItem>().ToList())
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_NUMBER_LINE_JUMP,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID
                                };

            return historyAction;
        }

        #endregion // Static Methods
    }
}