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

            allJumps = allJumps.OrderBy(j => j.StartingTickIndex).ToList();
            var groupedJumps = new List<List<NumberLineJumpSize>>();

            var buffer = new List<NumberLineJumpSize>();
            for (int i = 0; i < allJumps.Count; i++)
            {
                var currentJump = allJumps[i];
                buffer.Add(currentJump);
                if (buffer.Count == 1)
                {
                    continue;
                }

                var nextJump = i + 1 < allJumps.Count ? allJumps[i + 1] : null;
                if (nextJump != null &&
                    currentJump.JumpSize != nextJump.JumpSize)
                {
                    groupedJumps.Add(buffer);
                    buffer.Clear();
                }
            }

            groupedJumps.Add(buffer);
            var jumpSegments = new List<string>();
            foreach (var jumps in groupedJumps)
            {
                var firstJump = jumps.First();
                var jumpSegment = string.Format("{0}, {1}-{2}", firstJump.JumpSize, firstJump.StartingTickIndex, firstJump.StartingTickIndex + (firstJump.JumpSize * jumps.Count));
                jumpSegments.Add(jumpSegment);
            }

            var codedActionID = string.Join("; ", jumpSegments);

            var historyAction = new HistoryAction(page, jumpSizesChangedHistoryItems.Cast<IHistoryItem>().ToList())
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = isAdding ? Codings.ACTION_NUMBER_LINE_JUMP : Codings.ACTION_NUMBER_LINE_JUMP_ERASE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID
                                };

            return historyAction;
        }

        #endregion // Static Methods
    }
}