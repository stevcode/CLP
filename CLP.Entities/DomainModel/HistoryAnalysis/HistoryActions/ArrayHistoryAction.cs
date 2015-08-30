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

            #region ACTION_ARRAY_ROTATE

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

            #endregion // ACTION_ARRAY_ROTATE

            #region ACTION_ARRAY_CUT

            if (cutHistoryItems.Count == historyItemCount &&
                cutHistoryItems.Count == 1)
            {
                var historyItem = cutHistoryItems.First();

                var cutArrayID = historyItem.CutPageObjectID;
                var cutArray = parentPage.GetPageObjectByIDOnPageOrInHistory(cutArrayID) as CLPArray;
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

            #endregion // ACTION_ARRAY_CUT

            #region ACTION_ARRAY_SNAP

            if (snapHistoryItems.Count == historyItemCount &&
                snapHistoryItems.Count == 1)
            {
                var historyItem = snapHistoryItems.First();
                var codedObject = Codings.OBJECT_ARRAY;
                var persistingArray = parentPage.GetPageObjectByIDOnPageOrInHistory(historyItem.PersistingArrayID) as CLPArray;
                var snappedArray = parentPage.GetPageObjectByIDOnPageOrInHistory(historyItem.SnappedArrayID) as CLPArray;
                if (persistingArray == null ||
                    snappedArray == null)
                {
                    return null;
                }

                // For consistency's sake, the persistingArray is always the CodedID.
                // It is the array that remains on the page, therefore it is the one
                // whose dimensions change. The SubID is the array that is snapped onto
                // the persistingArray then disappears.
                var codedID = persistingArray.GetCodedIDAtHistoryIndex(historyItem.HistoryIndex);
                var incrementID = GetIncrementID(codedObject, codedID);
                var codedSubID = snappedArray.GetCodedIDAtHistoryIndex(historyItem.HistoryIndex);
                var incrementSubID = GetIncrementID(codedObject, codedSubID);
                var codedActionID = persistingArray.GetCodedIDAtHistoryIndex(historyItem.HistoryIndex + 1);
                var codedActionIDIncrementID = IncrementAndGetIncrementID(codedObject, codedActionID);
                if (!string.IsNullOrWhiteSpace(codedActionIDIncrementID))
                {
                    codedActionID += " " + codedActionIDIncrementID;
                }

                var historyAction = new ArrayHistoryAction(parentPage, historyItems)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_ARRAY_SNAP,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectSubID = codedSubID,
                    CodedObjectSubIDIncrement = incrementSubID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            #endregion // ACTION_ARRAY_SNAP

            #region ACTION_ARRAY_DIVIDE

            if (divideHistoryItems.Count == historyItemCount &&
                divideHistoryItems.Count == 1)
            {
                var historyItem = divideHistoryItems.First();
                var codedObject = Codings.OBJECT_ARRAY;
                var dividedArrayID = historyItem.ArrayID;
                var dividedArray = parentPage.GetPageObjectByIDOnPageOrInHistory(dividedArrayID) as CLPArray;
                if (dividedArray == null ||
                    historyItem.IsColumnRegionsChange == null)
                {
                    return null;
                }

                var codedID = dividedArray.GetCodedIDAtHistoryIndex(historyItem.HistoryIndex);
                var incrementID = GetIncrementID(codedObject, codedID);
                var codedActionSegments = new List<string>();

                // QUESTION: Right now, only listing new regions created. Alternatively, can list all regions, or have SubID for the replaced region
                foreach (var region in historyItem.NewRegions)
                {
                    int regionRow;
                    int regionColumn;
                    if (historyItem.IsColumnRegionsChange.Value)
                    {
                        regionColumn = (int)dividedArray.GetColumnsAndRowsAtHistoryIndex(historyItem.HistoryIndex).X;
                        regionRow = region.Value;
                    }
                    else
                    {
                        regionColumn = region.Value;
                        regionRow = (int)dividedArray.GetColumnsAndRowsAtHistoryIndex(historyItem.HistoryIndex).Y;
                    }

                    var segmentID = string.Format("{0}x{1}", regionColumn, regionRow);
                    var segmentIncrementID = IncrementAndGetIncrementID(codedObject, segmentID); // TODO: add something like "_SUB to codedObject to give subarrays their own increment path
                    if (!string.IsNullOrWhiteSpace(segmentIncrementID))
                    {
                        segmentID += " " + segmentIncrementID;
                    }

                    codedActionSegments.Add(segmentID);
                }

                if (historyItem.IsColumnRegionsChange.Value)
                {
                    codedActionSegments.Add("v");
                }

                var codedActionID = string.Join(", ", codedActionSegments);

                var historyAction = new ArrayHistoryAction(parentPage, historyItems)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_ARRAY_DIVIDE,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            #endregion // ACTION_ARRAY_DIVIDE

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
