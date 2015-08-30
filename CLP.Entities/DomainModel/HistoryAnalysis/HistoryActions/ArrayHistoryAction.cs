using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class ArrayHistoryAction : AHistoryActionBase
    {
        #region Constructors

        public ArrayHistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            : base(parentPage, historyItems) { }

        /// <summary>Initializes <see cref="ArrayHistoryAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayHistoryAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Calculated Properties

        public List<PageObjectCutHistoryItem> ArrayCutActions
        {
            get { return HistoryItems.OfType<PageObjectCutHistoryItem>().ToList(); }
        }

        public List<CLPArrayDivisionsChangedHistoryItem> ArrayDivisionActions
        {
            get { return HistoryItems.OfType<CLPArrayDivisionsChangedHistoryItem>().ToList();}
        }

        #endregion //Calculated Properties

        #region Static Methods

        public static ArrayHistoryAction VerifyAndGenerate(CLPPage parentPage, List<IHistoryItem> historyItems)
        {
            if (!historyItems.All(h => h is CLPArrayRotateHistoryItem ||
                                       h is PageObjectCutHistoryItem ||
                                       h is CLPArraySnapHistoryItem ||
                                       h is CLPArrayDivisionsChangedHistoryItem) || // TODO: allow inclusion of inkChanged historyItems and test of DIVIDE_INK
                !historyItems.Any())
            {
                return null;
            }

            var rotateHistoryItems = GetArrayRotateHistoryItems(historyItems);
            var cutHistoryItems = GetArrayCutHistoryItems(parentPage, historyItems);
            var snapHistoryItems = GetArraySnapHistoryItems(historyItems);
            var divideHistoryItems = GetArrayDividedHistoryItems(historyItems);
            var historyItemCount = historyItems.Count;

            if (rotateHistoryItems.Count == historyItemCount &&
                rotateHistoryItems.Count == 1)
            {
                var historyItem = rotateHistoryItems.First();
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = string.Format("{0}x{1}", historyItem.OldRows, historyItem.OldColumns);
                var incrementID = GetIncrementID(codedObject, codedID);
                var codedActionID = string.Format("{0}x{1}", historyItem.OldColumns, historyItem.OldRows);
                var codedActionIDIncrementID = IncrementAndGetIncrementID(codedObject, codedActionID);
                if (!string.IsNullOrWhiteSpace(codedActionIDIncrementID))
                {
                    codedActionID += " " + codedActionIDIncrementID;
                }

                var historyAction = new ArrayHistoryAction(parentPage, historyItems)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_ARRAY_ROTATE,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            if (cutHistoryItems.Count == historyItemCount &&
                cutHistoryItems.Count == 1)
            {
                var historyItem = cutHistoryItems.First();

                var cutArrayID = historyItem.CutPageObjectID;
                var cutArray = parentPage.GetPageObjectByIDOnPageOrInHistory(cutArrayID);
                if (cutArray == null)
                {
                    return null;
                }

                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = cutArray.GetCodedIDAtHistoryIndex(historyItem.HistoryIndex);
                var incrementID = GetIncrementID(codedObject, codedID);
                var codedActionSegments = new List<string>();
                foreach (var halvedPageObjectID in historyItem.HalvedPageObjectIDs)
                {
                    var array = parentPage.GetPageObjectByIDOnPageOrInHistory(halvedPageObjectID) as CLPArray;
                    if (array == null)
                    {
                        return null;
                    }

                    var arrayCodedID = array.GetCodedIDAtHistoryIndex(historyItem.HistoryIndex + 1);
                    var arrayIncrementID = IncrementAndGetIncrementID(codedObject, arrayCodedID);
                    var actionSegment = string.IsNullOrWhiteSpace(arrayIncrementID) ? arrayCodedID : string.Format("{0} {1}", arrayCodedID, arrayIncrementID);
                    codedActionSegments.Add(actionSegment);
                }

                var cuttingStroke = parentPage.GetStrokeByIDOnPageOrInHistory(historyItem.CuttingStrokeID);
                if (cuttingStroke == null)
                {
                    return null;
                }

                var strokeTop = cuttingStroke.GetBounds().Top;
                var strokeBottom = cuttingStroke.GetBounds().Bottom;
                var strokeLeft = cuttingStroke.GetBounds().Left;
                var strokeRight = cuttingStroke.GetBounds().Right;

                var isVerticalStrokeCut = Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom);
                if (isVerticalStrokeCut)
                {
                    codedActionSegments.Add("v");
                }

                var codedActionID = string.Join(", ", codedActionSegments);

                var historyAction = new ArrayHistoryAction(parentPage, historyItems)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_ARRAY_CUT,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            return null;
        }

        public static List<CLPArrayRotateHistoryItem> GetArrayRotateHistoryItems(List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<CLPArrayRotateHistoryItem>().ToList();
        }

        public static List<PageObjectCutHistoryItem> GetArrayCutHistoryItems(CLPPage parentPage, List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<PageObjectCutHistoryItem>().Where(h => parentPage.GetPageObjectByIDOnPageOrInHistory(h.CutPageObjectID) is CLPArray).ToList();
        }

        public static List<CLPArraySnapHistoryItem> GetArraySnapHistoryItems(List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<CLPArraySnapHistoryItem>().ToList();
        }

        public static List<CLPArrayDivisionsChangedHistoryItem> GetArrayDividedHistoryItems(List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<CLPArrayDivisionsChangedHistoryItem>().ToList();
            // TODO: Limit by only divided history items?
        }

        #endregion // Static Methods
    }
}
