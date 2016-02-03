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

            var referenceArrayID = inkAction.ReferencePageObjectID;
            if (referenceArrayID == null)
            {
                return null;
            }
            var array = page.GetPageObjectByIDOnPageOrInHistory(referenceArrayID) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var historyIndex = inkAction.HistoryItems.First().HistoryIndex;
            var codedObject = Codings.OBJECT_ARRAY;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID);

            // BUG: Doesn't deal with skip erase!
            var strokes = inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList();
            if (!strokes.Any())
            {
                return null;
            }
            var expectedRowValues = new List<int>();
            for (var i = 1; i <= array.Rows; i++)
            {
                expectedRowValues.Add(i * array.Columns);
            }

            var expectedColumnValues = new List<int>();
            for (var i = 1; i <= array.Columns; i++)
            {
                expectedColumnValues.Add(i * array.Rows);
            }

            #region Skip Counting Interpretation

            // Initialize StrokeCollection for each row
            var skipCountStrokes = new Dictionary<int, StrokeCollection>();
            for (var i = 1; i <= expectedRowValues.Count; i++)
            {
                skipCountStrokes.Add(i, new StrokeCollection());
            }

            // Place strokes in initial row groupings
            var currentRow = 1;
            var rowBufferSize = array.GridSquareSize * 0.2;
            var averageStrokeCenterX = strokes.Select(s => s.GetBounds().Center().X).Average();
            var arrayVisualRight = array.XPosition + array.LabelLength + array.ArrayWidth;
            var relativeAverageStrokeCenterX = averageStrokeCenterX - arrayVisualRight;
            var isSkipsOutsideArray = averageStrokeCenterX > arrayVisualRight;
            var rowBoundaryX = isSkipsOutsideArray ? arrayVisualRight - (0.2 * relativeAverageStrokeCenterX) : arrayVisualRight + (2.0 * relativeAverageStrokeCenterX);
            var rowBoundaryWidth = 2.4 * relativeAverageStrokeCenterX;
            var rowBoundaryHeight = array.GridSquareSize + (2.0 * rowBufferSize);
            for (var i = 0; i < strokes.Count; i++)
            {
                var stroke = strokes[i];
                var strokeBounds = stroke.GetBounds();

                for (var row = 1; row <= array.Rows; row++)
                {

                    var rowBoundary = new Rect
                                      {
                                          X = rowBoundaryX,
                                          Y = array.YPosition + array.LabelLength - rowBufferSize + ((row - 1) * array.GridSquareSize),
                                          Width = rowBoundaryWidth,
                                          Height = rowBoundaryHeight
                                      };
                }
            }

            var averageStrokeWidth = strokes.Select(s => s.GetBounds().Width).Average();
            var averageStrokeHeight = strokes.Select(s => s.GetBounds().Height).Average();
            var averageStrokeX = strokes.Select(s => s.GetBounds().X).Average();
            var probablyStrokeCountPerRow = Math.Round(strokes.Count % array.Rows * 1.0);
            var testBoundaryWidth = (probablyStrokeCountPerRow * averageStrokeWidth) + (1.5 * averageStrokeWidth);
            
            var testBoundaryX = arrayVisualRight <= averageStrokeX ? arrayVisualRight - (0.5 * averageStrokeWidth) : arrayVisualRight - (probablyStrokeCountPerRow * averageStrokeWidth);
            var testBoundaryBufferZoneSize = Math.Max(averageStrokeHeight, array.GridSquareSize) * 0.2;
            var testBoundaryInitialY = array.YPosition + array.LabelLength - testBoundaryBufferZoneSize;
            var testBoundaryMaxHeight = array.GridSquareSize + (2.0 * testBoundaryBufferZoneSize);

            
            // Test for skip counts on right side only.
            for (var i = 0; i < strokes.Count; i++)
            {
                var stroke = strokes[i];
                var strokeBounds = stroke.GetBounds();
                var strokeCenter = strokeBounds.Center();
                var strokeArea = strokeBounds.Area();

                // TODO: Check if stroke is inside acceptable bounds for right-side skip counting.
                // Do this outside of this for loop, if any strokes are outside acceptable bounds
                // create list of stroke collections before and after outside stroke, then produce
                // history action for each stroke collection, and IGNORE? the outside stroke. Also
                // check for strokes larger than GridSquareSize*2

                // TODO: Make InkCluster class and after clustering, create one for each cluster, use this to label each cluster, as property of class
                // also have flags for each ink cluster, so can flag this cluster as SKIP RIGHT on ARR uniqueID or ARR 8x8 (4x8 a), then can adaptively
                // move strokes to different clusters, for instance in the above situation where we'd change this single cluster to 2 cluster, the ink
                // in the skip and the ink out of the skip. can use to notice when erasing from a skp to generate ARR skip erase

                // Compare current stroke against previous stroke.
                var previousStroke = i <= 0 ? null : strokes[i - 1];
                if (previousStroke != null)
                {
                    var previousStrokeBounds = previousStroke.GetBounds();
                    var previousStrokeCenter = previousStrokeBounds.Center();
                    var previousStrokeArea = previousStrokeBounds.Area();
                    var angleToPreviousStroke = previousStrokeCenter.SlopeInDegrees(strokeCenter);
                }


                for (var row = 1; row <= array.Rows; row++)
                {
                    var strokesInPreviousRow = row <= 1 ? new StrokeCollection() : skipCountStrokes[row - 1];
                    var testBoundaryY = strokesInPreviousRow.Any() ? strokesInPreviousRow.GetBounds().Bottom : testBoundaryInitialY + ((row - 1) * array.GridSquareSize);
                    var averageHeightOfStrokesInARow = skipCountStrokes.Values.Where(x => x.Any()).Select(x => x.GetBounds().Height).Average();
                //    var testBoundaryMinBottom = array.YPosition + array.LabelLength + ((row - 1) * array.GridSquareSize) +

                    //var testBoundary = new Rect
                    //                   {
                    //                       X = testBoundaryX,
                    //                       Y = testBoundaryY,
                    //                       Width = testBoundaryWidth
                    //                   };
                }
            }

            var currentExpectedValue = expectedRowValues[currentRow - 1];


            //--------------------------------------------------------------------------
            Stroke prevStroke = null;
            var prevRow = -2;

            foreach (var stroke in strokes)
            {
                //Defines location variables
                var row = -2;
                var xpos = array.XPosition + array.LabelLength + array.ArrayWidth - 1.1 * array.GridSquareSize;
                var width = (4.5 * array.LabelLength) + (1.1 * array.GridSquareSize);
                var height = array.GridSquareSize;

                /*******************************/
                /*   INSIDE GENERAL AREA TEST  */
                /*******************************/
                bool cont = true; //false;
                //var generalBound = new Rect(array.XPosition + array.LabelLength,
                //                            array.YPosition + array.LabelLength - 0.1 * height,
                //                            array.ArrayWidth + 4.5 * array.LabelLength,
                //                            array.ArrayHeight + 0.2 * height);

                //if (stroke.HitTest(generalBound, 80))
                //{
                //    cont = true;
                //}

                /************************/
                /* PREVIOUS STROKE TEST */
                /************************/

                //Creates fixed stroke bounds
                var strokeBoundFixed = stroke.GetBounds();

                //Checks previous ink stroke's bounds
                if (prevStroke != null && cont) //&& prev_xpos == curr_xpos)
                {
                    var prevBound = new Rect(xpos, prevStroke.GetBounds().Y - 0.2 * height, width, 1.4 * height);

                    //Finds intersection
                    strokeBoundFixed.Intersect(prevBound);
                    var intersectArea = strokeBoundFixed.Height * strokeBoundFixed.Width;
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
                        strokeBoundFixed.Intersect(nextBound);
                        intersectArea = strokeBoundFixed.Height * strokeBoundFixed.Width;
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
                        var strokeBound = new Rect(stroke.GetBounds().X, stroke.GetBounds().Y, stroke.GetBounds().Width, stroke.GetBounds().Height);
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
                    skipCountStrokes[row].Add(stroke);
                    if (row > -1)
                    {
                        prevStroke = stroke;
                        prevRow = row;
                    }

                }


            }


            var skipCounts = new Dictionary<int, string>();
            //Writes row number and ink interpretation to txt file
            foreach (var row in skipCountStrokes.Keys)
            {
                var interpretation = InkInterpreter.StrokesToBestGuessText(skipCountStrokes[row]);
                skipCounts.Add(row, interpretation);
            }

            #endregion // Skip Counting Interpretation

            if (skipCounts.Keys.Any())
            {
                var location = inkAction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) ? "right" : "left";

                var keys = skipCounts.Keys.ToList();
                keys.Sort();
                var values = keys.Select(key => skipCounts[key]).ToList();
                var interpretedValues = string.Join(", ", values);

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

        public static List<IHistoryAction> InkDivide(CLPPage page, IHistoryAction inkAction)
        {
            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD || inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE) ||
                !inkAction.HistoryItems.All(h => h is ObjectsOnPageChangedHistoryItem))
            {
                return null;
            }

            var referenceArrayID = inkAction.ReferencePageObjectID;
            if (referenceArrayID == null)
            {
                return null;
            }
            var array = page.GetPageObjectByIDOnPageOrInHistory(referenceArrayID) as CLPArray;
            if (array == null ||
                array.ArrayType != ArrayTypes.Array ||
                !array.IsGridOn)
            {
                return null;
            }

            var historyIndex = inkAction.HistoryItems.First().HistoryIndex;
            var codedObject = Codings.OBJECT_ARRAY;
            var codedDescription = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? Codings.ACTION_ARRAY_DIVIDE_INK : Codings.ACTION_ARRAY_DIVIDE_INK_ERASE;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID); // TODO: Confirm increments correctly

            var interpretedActions = new List<IHistoryAction>();
            var historyItemBuffer = new List<IHistoryItem>();

            #region Ink Divide Interpretation

            var verticalDividers = new List<int> { 0 };
            var horizontalDividers = new List<int> { 0 };

            var arrayPosition = array.GetPositionAtHistoryIndex(historyIndex);
            var arrayDimensions = array.GetDimensionsAtHistoryIndex(historyIndex);
            var cuttableTop = arrayPosition.Y + array.LabelLength;
            var cuttableBottom = cuttableTop + arrayDimensions.Y - (2 * array.LabelLength);
            var cuttableLeft = arrayPosition.X + array.LabelLength;
            var cuttableRight = cuttableLeft + arrayDimensions.X - (2 * array.LabelLength);

            var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);

            foreach (var historyItem in inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>())
            {
                var strokesInHistoryItem = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? historyItem.StrokesAdded : historyItem.StrokesRemoved;
                if (strokesInHistoryItem.Count != 1)
                {
                    // TODO: Handles point erases
                    historyItemBuffer.Add(historyItem);
                    continue;
                }

                var stroke = strokesInHistoryItem.First();
                var referenceStrokeCopy = stroke.GetStrokeCopyAtHistoryIndex(page, historyIndex);

                var strokeTop = referenceStrokeCopy.GetBounds().Top;
                var strokeBottom = referenceStrokeCopy.GetBounds().Bottom;
                var strokeLeft = referenceStrokeCopy.GetBounds().Left;
                var strokeRight = referenceStrokeCopy.GetBounds().Right;

                const double SMALL_THRESHOLD = 5.0;
                const double LARGE_THRESHOLD = 15.0;

                if (Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
                    strokeRight <= cuttableRight &&
                    strokeLeft >= cuttableLeft &&
                    (strokeTop - cuttableTop <= SMALL_THRESHOLD ||
                    cuttableBottom - strokeBottom <= SMALL_THRESHOLD) &&
                    (strokeTop - cuttableTop <= LARGE_THRESHOLD &&
                    cuttableBottom - strokeBottom <= LARGE_THRESHOLD) &&
                    strokeBottom - strokeTop >= cuttableBottom - cuttableTop - LARGE_THRESHOLD &&
                    arrayColumnsAndRows.X > 1) //Vertical Stroke. Stroke must be within the bounds of the pageObject
                {
                    var average = (strokeRight + strokeLeft) / 2;
                    var relativeAverage = average - array.LabelLength - arrayPosition.X;
                    var dividerValue = (int)Math.Round(relativeAverage / array.GridSquareSize);
                    if (dividerValue == 0 ||
                        dividerValue == arrayColumnsAndRows.X)
                    {
                        historyItemBuffer.Add(historyItem);
                        continue;
                    }

                    var newInkAction = new HistoryAction(page, historyItemBuffer)
                                       {
                                           CodedObject = Codings.OBJECT_INK,
                                           CodedObjectAction = inkAction.CodedObjectAction,
                                           CodedObjectID = inkAction.CodedObjectID, // TODO: Increment correctly.
                                           CodedObjectActionID = inkAction.CodedObjectActionID
                                       };

                    historyItemBuffer.Clear();
                    interpretedActions.Add(newInkAction);

                    var inkDivideAction = new HistoryAction(page, historyItem)
                                          {
                                              CodedObject = codedObject,
                                              CodedObjectAction = codedDescription,
                                              CodedObjectID = codedID,
                                              CodedObjectIDIncrement = incrementID
                                          };

                    verticalDividers.Add(dividerValue);
                    verticalDividers.Add(array.Columns);  // TODO: Fix for if multiple ink dividers are made in a row
                    verticalDividers = verticalDividers.Distinct().OrderBy(x => x).ToList();
                    var verticalDivisions = verticalDividers.Zip(verticalDividers.Skip(1), (x, y) => y - x).Select(x => string.Format("{0}x{1}", arrayColumnsAndRows.Y, x));
                    inkDivideAction.CodedObjectActionID = string.Join(", ", verticalDivisions); // TODO: apply internal increments
                    inkDivideAction.MetaData.Add("REFERENCE_PAGE_OBJECT_ID", referenceArrayID);

                    interpretedActions.Add(inkDivideAction);
                }

                if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                         strokeBottom <= cuttableBottom &&
                         strokeTop >= cuttableTop &&
                         (cuttableRight - strokeRight <= SMALL_THRESHOLD ||
                         strokeLeft - cuttableLeft <= SMALL_THRESHOLD) &&
                         (cuttableRight - strokeRight <= LARGE_THRESHOLD &&
                         strokeLeft - cuttableLeft <= LARGE_THRESHOLD) &&
                         strokeRight - strokeLeft >= cuttableRight - cuttableLeft - LARGE_THRESHOLD &&
                         arrayColumnsAndRows.Y > 1) //Horizontal Stroke. Stroke must be within the bounds of the pageObject
                {
                    var average = (strokeTop + strokeBottom) / 2;
                    var relativeAverage = average - array.LabelLength - arrayPosition.Y;
                    var dividerValue = (int)Math.Round(relativeAverage / array.GridSquareSize);
                    if (dividerValue == 0 ||
                        dividerValue == arrayColumnsAndRows.Y)
                    {
                        historyItemBuffer.Add(historyItem);
                        continue;
                    }

                    var newInkAction = new HistoryAction(page, historyItemBuffer)
                                       {
                                           CodedObject = Codings.OBJECT_INK,
                                           CodedObjectAction = inkAction.CodedObjectAction,
                                           CodedObjectID = inkAction.CodedObjectID, // TODO: Increment correctly.
                                           CodedObjectActionID = inkAction.CodedObjectActionID
                                       };

                    historyItemBuffer.Clear();
                    interpretedActions.Add(newInkAction);

                    var inkDivideAction = new HistoryAction(page, historyItem)
                    {
                        CodedObject = codedObject,
                        CodedObjectAction = codedDescription,
                        CodedObjectID = codedID,
                        CodedObjectIDIncrement = incrementID
                    };

                    horizontalDividers.Add(dividerValue);
                    horizontalDividers.Add(array.Rows);
                    horizontalDividers = horizontalDividers.Distinct().OrderBy(x => x).ToList();
                    var horizontalDivisions = horizontalDividers.Zip(horizontalDividers.Skip(1), (x, y) => y - x).Select(x => string.Format("{0}x{1}", x, arrayColumnsAndRows.X));

                    inkDivideAction.CodedObjectActionID = string.Join(", ", horizontalDivisions); // TODO: apply internal increments
                    inkDivideAction.MetaData.Add("REFERENCE_PAGE_OBJECT_ID", referenceArrayID);
                    interpretedActions.Add(inkDivideAction);
                }
            }

            #endregion // Ink Divide Interpretation

            return interpretedActions;
        }

        #endregion // Static Methods
    }
}