using System;
using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class ArrayCodedActions
    {
        #region Static Methods

        public static HistoryAction Rotate(CLPPage page, CLPArrayRotateHistoryItem rotateHistoryItem)
        {
            if (page == null ||
                rotateHistoryItem == null)
            {
                return null;
            }

            var arrayID = rotateHistoryItem.ArrayID;
            var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var codedID = string.Format("{0}x{1}", rotateHistoryItem.OldRows, rotateHistoryItem.OldColumns);
            var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID);
            var codedActionID = string.Format("{0}x{1}", rotateHistoryItem.OldColumns, rotateHistoryItem.OldRows);
            var codedActionIDIncrementID = HistoryAction.IncrementAndGetIncrementID(array.ID, codedObject, codedActionID);
            if (!string.IsNullOrWhiteSpace(codedActionIDIncrementID))
            {
                codedActionID += " " + codedActionIDIncrementID;
            }

            var historyAction = new HistoryAction(page, rotateHistoryItem)
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_ARRAY_ROTATE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID
                                };

            return historyAction;
        }

        public static HistoryAction Cut(CLPPage page, PageObjectCutHistoryItem cutHistoryItem)
        {
            if (page == null ||
                cutHistoryItem == null)
            {
                return null;
            }

            var cutArrayID = cutHistoryItem.CutPageObjectID;
            var cutArray = page.GetPageObjectByIDOnPageOrInHistory(cutArrayID) as CLPArray;
            if (cutArray == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var codedID = cutArray.GetCodedIDAtHistoryIndex(cutHistoryItem.HistoryIndex);
            var incrementID = HistoryAction.GetIncrementID(cutArray.ID, codedObject, codedID);
            var codedActionSegments = new List<string>();
            foreach (var halvedPageObjectID in cutHistoryItem.HalvedPageObjectIDs)
            {
                var array = page.GetPageObjectByIDOnPageOrInHistory(halvedPageObjectID) as CLPArray;
                if (array == null)
                {
                    return null;
                }

                var arrayCodedID = array.GetCodedIDAtHistoryIndex(cutHistoryItem.HistoryIndex + 1);
                var arrayIncrementID = HistoryAction.IncrementAndGetIncrementID(array.ID, codedObject, arrayCodedID);
                var actionSegment = string.IsNullOrWhiteSpace(arrayIncrementID) ? arrayCodedID : string.Format("{0} {1}", arrayCodedID, arrayIncrementID);
                codedActionSegments.Add(actionSegment);
            }

            var cuttingStroke = page.GetStrokeByIDOnPageOrInHistory(cutHistoryItem.CuttingStrokeID);
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

            var historyAction = new HistoryAction(page, cutHistoryItem)
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_ARRAY_CUT,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID
                                };

            return historyAction;
        }

        public static HistoryAction Snap(CLPPage page, CLPArraySnapHistoryItem snapHistoryItem)
        {
            if (page == null ||
                snapHistoryItem == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var persistingArray = page.GetPageObjectByIDOnPageOrInHistory(snapHistoryItem.PersistingArrayID) as CLPArray;
            var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(snapHistoryItem.SnappedArrayID) as CLPArray;
            if (persistingArray == null ||
                snappedArray == null)
            {
                return null;
            }

            // For consistency's sake, the persistingArray is always the CodedID.
            // It is the array that remains on the page, therefore it is the one
            // whose dimensions change. The SubID is the array that is snapped onto
            // the persistingArray then disappears.
            var codedID = persistingArray.GetCodedIDAtHistoryIndex(snapHistoryItem.HistoryIndex);
            var incrementID = HistoryAction.GetIncrementID(persistingArray.ID, codedObject, codedID);
            var codedSubID = snappedArray.GetCodedIDAtHistoryIndex(snapHistoryItem.HistoryIndex);
            var incrementSubID = HistoryAction.GetIncrementID(snappedArray.ID, codedObject, codedSubID);
            var codedActionID = persistingArray.GetCodedIDAtHistoryIndex(snapHistoryItem.HistoryIndex + 1);
            var codedActionIDIncrementID = HistoryAction.IncrementAndGetIncrementID(persistingArray.ID, codedObject, codedActionID);
            if (!string.IsNullOrWhiteSpace(codedActionIDIncrementID))
            {
                codedActionID += " " + codedActionIDIncrementID;
            }

            var historyAction = new HistoryAction(page, snapHistoryItem)
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

        public static HistoryAction Divide(CLPPage page, CLPArrayDivisionsChangedHistoryItem divideHistoryItem)
        {
            if (page == null ||
                divideHistoryItem == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var dividedArrayID = divideHistoryItem.ArrayID;
            var dividedArray = page.GetPageObjectByIDOnPageOrInHistory(dividedArrayID) as CLPArray;
            if (dividedArray == null ||
                divideHistoryItem.IsColumnRegionsChange == null)
            {
                return null;
            }

            var codedID = dividedArray.GetCodedIDAtHistoryIndex(divideHistoryItem.HistoryIndex);
            var incrementID = HistoryAction.GetIncrementID(dividedArray.ID, codedObject, codedID);
            var codedActionSegments = new List<string>();

            // QUESTION: Right now, listing all regions after divide. Alternatively, can list just new regions and have SubID for the replaced region
            var index = 0;
            foreach (var region in divideHistoryItem.NewRegions.OrderBy(r => r.Position))
            {
                int regionRow;
                int regionColumn;
                if (divideHistoryItem.IsColumnRegionsChange.Value)
                {
                    regionColumn = (int)dividedArray.GetColumnsAndRowsAtHistoryIndex(divideHistoryItem.HistoryIndex).X;
                    regionRow = region.Value;
                }
                else
                {
                    regionColumn = region.Value;
                    regionRow = (int)dividedArray.GetColumnsAndRowsAtHistoryIndex(divideHistoryItem.HistoryIndex).Y;
                }

                var segmentID = string.Format("{0}x{1}", regionColumn, regionRow);
                var segmentIncrementID = HistoryAction.IncrementAndGetIncrementID(dividedArray.ID, codedObject, segmentID, index); 
                if (!string.IsNullOrWhiteSpace(segmentIncrementID))
                {
                    segmentID += " " + segmentIncrementID;
                }

                codedActionSegments.Add(segmentID);
                index++;
            }

            if (divideHistoryItem.IsColumnRegionsChange.Value)
            {
                codedActionSegments.Add("v");
            }

            var codedActionID = string.Join(", ", codedActionSegments);

            var historyAction = new HistoryAction(page, divideHistoryItem)
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_ARRAY_DIVIDE,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    CodedObjectActionID = codedActionID
                                };

            return historyAction;
        }

        #endregion // Static Methods
    }
}