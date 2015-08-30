﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public static class NumberLineCodedActions
    {
        #region Static Methods

        public static HistoryAction VerifyAndGenerate(CLPPage parentPage, List<IHistoryItem> historyItems)
        {
            if (!historyItems.All(h => h is NumberLineEndPointsChangedHistoryItem ||
                                       h is NumberLineJumpSizesChangedHistoryItem) || // TODO: allow inclusion of inkChanged historyItems and test for CHANGE_INK
                !historyItems.Any()) 
            {
                return null;
            }

            var jumpSizesChangedHistoryItems = GetNumberLineJumpSizesChangedHistoryItems(historyItems);
            var endPointsChangedHistoryItems = GetNumberLineEndPointsChangedHistoryItems(historyItems);

            if (endPointsChangedHistoryItems.Any() &&
                !jumpSizesChangedHistoryItems.Any())
            {
                var numberLineID = endPointsChangedHistoryItems.First().NumberLineID;
                var numberLine = parentPage.GetPageObjectByIDOnPageOrInHistory(numberLineID);
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

                var historyAction = new HistoryAction(parentPage, historyItems)
                                    {
                                        CodedObject = codedObject,
                                        CodedObjectAction = Codings.ACTION_NUMBER_LINE_CHANGE,
                                        CodedObjectID = codedID,
                                        CodedObjectIDIncrement = incrementID,
                                        CodedObjectActionID = codedActionID
                                    };

                return historyAction;
            }

            if (jumpSizesChangedHistoryItems.Any() &&
                jumpSizesChangedHistoryItems.Select(h => h.NumberLineID).Distinct().Count() == 1 &&
                !endPointsChangedHistoryItems.Any())
            {
                var numberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;
                var numberLine = parentPage.GetPageObjectByIDOnPageOrInHistory(numberLineID);
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






                if (!jumpSegments.Any())
                {
                    return null;
                }
                var codedActionID = string.Join("; ", jumpSegments);

                var historyAction = new HistoryAction(parentPage, historyItems)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_NUMBER_LINE_JUMP,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            return null;
        }

        public static List<NumberLineEndPointsChangedHistoryItem> GetNumberLineEndPointsChangedHistoryItems(List<IHistoryItem> historyItems)

        {
            return historyItems.OfType<NumberLineEndPointsChangedHistoryItem>().ToList();
        }

        public static List<NumberLineJumpSizesChangedHistoryItem> GetNumberLineJumpSizesChangedHistoryItems(List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<NumberLineJumpSizesChangedHistoryItem>().ToList();
        }

        #endregion // Static Methods
    }
}
