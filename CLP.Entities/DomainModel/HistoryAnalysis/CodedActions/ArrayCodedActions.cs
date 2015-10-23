using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using CLP.InkInterpretation;

namespace CLP.Entities
{
    public static class ArrayCodedActions
    {
        #region Static Methods

        public static IHistoryAction Rotate(CLPPage page, CLPArrayRotateHistoryItem rotateHistoryItem)
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

        public static IHistoryAction Cut(CLPPage page, PageObjectCutHistoryItem cutHistoryItem)
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
                codedActionSegments.Add(Codings.ACTIONID_ARRAY_CUT_VERTICAL);
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

        public static IHistoryAction Snap(CLPPage page, CLPArraySnapHistoryItem snapHistoryItem)
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

        public static IHistoryAction Divide(CLPPage page, CLPArrayDivisionsChangedHistoryItem divideHistoryItem)
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
                codedActionSegments.Add(Codings.ACTIONID_ARRAY_DIVIDER_VERTICAL);
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

        public static IHistoryAction SkipCounting(CLPPage page, IHistoryAction inkAction)
        {
            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ||
                  inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE))
            {
                return null;
            }

            var referenceArrayID = inkAction.MetaData["REFERENCE_PAGE_OBJECT_ID"];
            var array = page.GetPageObjectByIDOnPageOrInHistory(referenceArrayID) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var historyIndex = inkAction.HistoryItems.First().HistoryIndex;
            var codedObject = Codings.OBJECT_ARRAY;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID);

            var strokes = inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList();

            #region Skip Counting Interpretation

            var skipCountStrokes = new Dictionary<int, StrokeCollection>();
            Stroke prevStroke = null;
            var prevRow = -2;
            //var prev_xpos = -2.0; 

            var expandedArrayBounds = new Rect(array.XPosition - (array.LabelLength * 1.5),
                                               array.YPosition - (array.LabelLength * 1.5),
                                               array.Width + (array.LabelLength * 3),
                                               array.Height + (array.LabelLength * 3));
            var inkCloseToArray = strokes;

            foreach (var inkStroke in inkCloseToArray)
            {
                //Defines location variables
                var row = -2;
                var xpos = array.XPosition + array.LabelLength + array.ArrayWidth - 1.1 * array.GridSquareSize;
                var width = (4.5 * array.LabelLength) + (1.1 * array.GridSquareSize);
                var height = array.GridSquareSize;
                //var curr_xpos = xpos;

                /*******************************/
                /*   INSIDE GENERAL AREA TEST  */
                /*******************************/
                bool cont = false;
                var generalBound = new Rect(array.XPosition + array.LabelLength,
                                            array.YPosition + array.LabelLength - 0.1 * height,
                                            array.ArrayWidth + 4.5 * array.LabelLength,
                                            array.ArrayHeight + 0.2 * height);

                if (inkStroke.HitTest(generalBound, 80))
                {
                    cont = true;
                }
                else
                {
                    Console.WriteLine("Failed general bound test");
                }

                /************************/
                /* PREVIOUS STROKE TEST */
                /************************/

                //Creates fixed stroke bounds
                var strokeBoundFixed = new Rect(inkStroke.GetBounds().X, inkStroke.GetBounds().Y, inkStroke.GetBounds().Width, inkStroke.GetBounds().Height);

                //Checks previous ink stroke's bounds
                if (prevStroke != null && cont) //&& prev_xpos == curr_xpos)
                {
                    var prev_y = prevStroke.GetBounds().Y;
                    //var prev_height = prevStroke.GetBounds().Height;
                    var curr_y = inkStroke.GetBounds().Y;
                    //var curr_height = inkStroke.GetBounds().Height;

                    var prevBound = new Rect(xpos, prevStroke.GetBounds().Y - 0.2 * height, width, 1.4 * height);
      
                    //Finds intersection
                    var strokeBound = new Rect(inkStroke.GetBounds().X, inkStroke.GetBounds().Y, inkStroke.GetBounds().Width, inkStroke.GetBounds().Height);
                    strokeBound.Intersect(prevBound);
                    var intersectArea = strokeBound.Height * strokeBound.Width;
                    var strokeArea = strokeBoundFixed.Height * strokeBoundFixed.Width;
                    var percentIntersect = 100 * intersectArea / strokeArea;

                    //Checks if 80% inside row
                    if (percentIntersect >= 80 &&
                        percentIntersect <= 101)
                    {
                        row = prevRow;
                        cont = false;
                    }

                    //Check if in row after previous stroke
                    else
                    {
                        //Creates previous stroke's row bound
                        var nextBound = new Rect(xpos, prevStroke.GetBounds().Y + 0.8 * height, width, 1.4 * height);
     
                        //Finds intersection
                        strokeBound = new Rect(inkStroke.GetBounds().X, inkStroke.GetBounds().Y, inkStroke.GetBounds().Width, inkStroke.GetBounds().Height);
                        strokeBound.Intersect(nextBound);
                        intersectArea = strokeBound.Height * strokeBound.Width;
                        strokeArea = strokeBoundFixed.Height * strokeBoundFixed.Width;
                        percentIntersect = 100 * intersectArea / strokeArea;

                        //Checks if 80% inside row
                        if (percentIntersect >= 80 &&
                            percentIntersect <= 101)
                        {
                            row = prevRow + 1;
                            cont = false;
                        }
                    }
                }

                /************************/
                /*  ROW ITERATION TEST  */
                /************************/

                if (cont)
                {
                    for (int i = 0; i < array.Rows; i++)
                    {
                        //Creates array row bound
                        var ypos = array.YPosition + array.LabelLength + (array.GridSquareSize * i);
                        var rectBound = new Rect(xpos, ypos - 0.1 * height, width, 1.2 * height);
             
                        //Finds intersection
                        var strokeBound = new Rect(inkStroke.GetBounds().X, inkStroke.GetBounds().Y, inkStroke.GetBounds().Width, inkStroke.GetBounds().Height);
                        strokeBound.Intersect(rectBound);
                        var intersectArea = strokeBound.Height * strokeBound.Width;
                        var strokeArea = strokeBoundFixed.Height * strokeBoundFixed.Width;
                        var percentIntersect = 100 * intersectArea / strokeArea;
          
                        //Checks if 80% inside row
                        if (percentIntersect >= 80 &&
                            percentIntersect <= 101)
                        {
                            row = i;
                            cont = false;
                            break;
                        }
                    }
                }

                /************************/
                /*   INSIDE ARRAY TEST  */
                /************************/
                if (cont)
                {
                    var arrBound = new Rect(array.XPosition + array.LabelLength, array.YPosition + array.LabelLength, array.ArrayWidth, array.ArrayHeight);
                    if (inkStroke.HitTest(arrBound, 80))
                    {
                        row = -1;
                        //curr_xpos = xpos - array.GridSquareSize;
                    }
                }

                /***************/
                /*  ROW MATCH  */
                /***************/
                if (row > -2)
                {
                    //Adds stroke to dictionary
                    if (!skipCountStrokes.ContainsKey(row))
                    {
                        skipCountStrokes.Add(row, new StrokeCollection());
                    }
                    skipCountStrokes[row].Add(inkStroke);
                    if (row > -1)
                    {
                        prevStroke = inkStroke;
                        prevRow = row;
                        //prev_xpos = curr_xpos;
                    }

                }
            }

            var equation = string.Empty;
            var skipCounts = new Dictionary<int, string>();
            //Writes row number and ink interpretation to txt file
            foreach (var row in skipCountStrokes.Keys)
            {
                var interpretation = InkInterpreter.StrokesToBestGuessText(skipCountStrokes[row]);
                if (row == -1)
                {
                    equation = interpretation;
                }
                else
                {
                    skipCounts.Add(row, interpretation);
                }
            }

            #endregion // Skip Counting Interpretation

            if (skipCounts.Keys.Any())
            {
                var location = inkAction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) ? "right" : "left";

                var keys = skipCounts.Keys.ToList();
                keys.Sort();
                var values = keys.Select(key => skipCounts[key]).ToList();
                var interpretedValues = string.Join(",", values);

                var codedActionID = string.Format("{0}, \"{1}\"", location, interpretedValues);

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? Codings.ACTION_ARRAY_SKIP : Codings.ACTION_ARRAY_SKIP_ERASE,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            return null;
        }

        #endregion // Static Methods
    }
}