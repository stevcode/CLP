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

        public static IHistoryAction InkDivide(CLPPage page, IHistoryItem historyItem)
        {
            if (page == null)
            {
                return null;
            }

            var objectsChangedHistoryItem = historyItem as ObjectsOnPageChangedHistoryItem;
            if (objectsChangedHistoryItem == null ||
                objectsChangedHistoryItem.IsUsingPageObjects ||
                !objectsChangedHistoryItem.IsUsingStrokes)
            {
                return null;
            }

            var strokes = objectsChangedHistoryItem.StrokesAdded;
            var isAddedStroke = true;
            if (!strokes.Any())
            {
                isAddedStroke = false;
                strokes = objectsChangedHistoryItem.StrokesRemoved;
            }

            if (strokes.Count != 1)
            {
                // TODO: Throw error?
                return null;
            }

            var stroke = strokes.First();

            var historyIndex = objectsChangedHistoryItem.HistoryIndex;
            var arraysOnPage =
                ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, historyIndex)
                                  .OfType<CLPArray>()
                                  .Where(a => a.ArrayType == ArrayTypes.Array && a.IsGridOn)
                                  .ToList();

            if (!arraysOnPage.Any())
            {
                return null;
            }

            var array = InkCodedActions.FindMostOverlappedPageObjectAtHistoryIndex(page, arraysOnPage.Cast<IPageObject>().ToList(), stroke, historyIndex) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var codedDescription = isAddedStroke ? Codings.ACTION_ARRAY_DIVIDE_INK : Codings.ACTION_ARRAY_DIVIDE_INK_ERASE;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID); // TODO: Confirm increments correctly

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
                    return null;
                }

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
                inkDivideAction.MetaData.Add("REFERENCE_PAGE_OBJECT_ID", array.ID);

                var cluster = InkCodedActions.GetContainingCluster(stroke);
                if (cluster != null)
                {
                    cluster.Strokes.Remove(stroke);
                    if (cluster.StrokesOnPage.Contains(stroke))
                    {
                        cluster.StrokesOnPage.Remove(stroke);
                    }
                    if (cluster.StrokesErased.Contains(stroke))
                    {
                        cluster.StrokesErased.Remove(stroke);
                    }
                }

                return inkDivideAction;
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
                    return null;
                }

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
                inkDivideAction.MetaData.Add("REFERENCE_PAGE_OBJECT_ID", array.ID);

                var cluster = InkCodedActions.GetContainingCluster(stroke);
                if (cluster != null)
                {
                    cluster.Strokes.Remove(stroke);
                    if (cluster.StrokesOnPage.Contains(stroke))
                    {
                        cluster.StrokesOnPage.Remove(stroke);
                    }
                    if (cluster.StrokesErased.Contains(stroke))
                    {
                        cluster.StrokesErased.Remove(stroke);
                    }
                }

                return inkDivideAction;
            }

            #endregion // Ink Divide Interpretation

            return null;
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

            var isSkipAdd = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD;

            var strokes = isSkipAdd
                              ? inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesRemoved).ToList();

            #region Skip Counting Interpretation

            // Initialize StrokeCollection for each row
            var strokeGroupPerRow = new Dictionary<int, StrokeCollection>();
            for (var i = 1; i <= array.Rows; i++)
            {
                strokeGroupPerRow.Add(i, new StrokeCollection());
            }

            // Row boundaries
            var rowBoundaryX = strokes.Select(s => s.GetBounds().Left).Min() - 5;
            var rowBoundaryWidth = strokes.Select(s => s.GetBounds().Right).Max() - rowBoundaryX + 10;
            var rowBoundaryHeight = array.GridSquareSize * 2.0;

            // Determine strokes to ignore or group later.
            var notSkipCountStrokes = strokes.Where(s => s.GetBounds().Height >= array.GridSquareSize * 2.0).ToList();
            if (notSkipCountStrokes.Any())
            {
                Console.WriteLine("*****NO SKIP COUNT STROKES TO IGNORE*****");
                // TODO: establish other exclusion factors and re-cluster to ignore these strokes.
            }

            var cuttoffHeightByAverageStrokeHeight = strokes.Select(s => s.GetBounds().Height).Average() * 0.5;
            var cuttoffHeightByGridSquareSize = array.GridSquareSize * 0.33;
            var strokeCutOffHeight = Math.Max(cuttoffHeightByAverageStrokeHeight, cuttoffHeightByGridSquareSize);
            var ungroupedStrokes = strokes.Where(s => s.GetBounds().Height < strokeCutOffHeight).ToList();
            var skipCountStrokes = strokes.Where(s => s.GetBounds().Height >= strokeCutOffHeight).ToList();

            // Place strokes in most likely row groupings
            foreach (var stroke in skipCountStrokes)
            {
                var strokeBounds = stroke.GetBounds();

                var highestIntersectPercentage = 0.0;
                var mostLikelyRow = 0;
                for (var row = 1; row <= array.Rows; row++)
                {
                    var rowBoundary = new Rect
                    {
                        X = rowBoundaryX,
                        Y = array.YPosition + array.LabelLength + ((row - 1) * array.GridSquareSize) - (0.5 * array.GridSquareSize),
                        Width = rowBoundaryWidth,
                        Height = rowBoundaryHeight
                    };

                    var intersect = Rect.Intersect(strokeBounds, rowBoundary);
                    if (intersect.IsEmpty)
                    {
                        continue;
                    }
                    var intersectPercentage = intersect.Area() / strokeBounds.Area();
                    if (intersectPercentage > 0.9 &&
                        highestIntersectPercentage > 0.9)
                    {
                        // TODO: Log how often this happens. Should only happen whe stroke is 90% intersected by 2 rows.
                        var distanceToRowMidPoint = Math.Abs(strokeBounds.Bottom - rowBoundary.Center().Y);
                        var distanceToPreviousRowMidPoint = Math.Abs(strokeBounds.Bottom - (rowBoundary.Center().Y - array.GridSquareSize));
                        mostLikelyRow = distanceToRowMidPoint < distanceToPreviousRowMidPoint ? row : row - 1;
                        break;
                    }
                    if (intersectPercentage > highestIntersectPercentage)
                    {
                        highestIntersectPercentage = intersectPercentage;
                        mostLikelyRow = row;
                    }
                }

                if (mostLikelyRow == 0)
                {
                    notSkipCountStrokes.Add(stroke);
                    //Console.WriteLine("*****NO SKIP COUNT STROKES TO IGNORE*****");
                    // TODO: re-cluster to ignore these strokes.
                    continue;
                }

                strokeGroupPerRow[mostLikelyRow].Add(stroke);
            }

            foreach (var stroke in ungroupedStrokes)
            {
                var closestStroke = stroke.FindClosestStroke(skipCountStrokes);
                for (var row = 1; row <= array.Rows; row++)
                {
                    if (strokeGroupPerRow[row].Contains(closestStroke))
                    {
                        strokeGroupPerRow[row].Add(stroke);
                        break;
                    }
                }
            }

            var strokesGroupedCount = strokeGroupPerRow.Values.SelectMany(s => s).Count();
            if (strokesGroupedCount < 3)
            {
                // Not enough to be skip counting.
                return null;
            }

            // Interpret handwriting of each row's grouping of strokes.
            var interpretedRowValues = new List<string>();
            for (var row = 1; row <= array.Rows; row++)
            {
                var expectedRowValue = row * array.Columns;
                var strokesInRow = strokeGroupPerRow[row];

                List<string> interpretations;
                if (!isSkipAdd)
                {
                    var orderedStrokes = InkCodedActions.GetOrderStrokesWhereAddedToPage(page, strokesInRow.ToList());
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                }
                else
                {
                    interpretations = InkInterpreter.StrokesToAllGuessesText(strokesInRow);
                }

                if (!interpretations.Any())
                {
                    interpretedRowValues.Add(string.Empty);
                    continue;
                }

                var actualMatch = InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedRowValue);
                if (!string.IsNullOrEmpty(actualMatch))
                {
                    interpretedRowValues.Add(actualMatch);
                    continue;
                }

                var bestGuess = InkInterpreter.InterpretationClosestToANumber(interpretations);
                interpretedRowValues.Add(bestGuess);
            }

            var formattedSkips = string.Format("\"{0}\"", string.Join("\" \"", interpretedRowValues));

            #endregion // Skip Counting Interpretation

            var firstStroke = strokes.First();
            var cluster = InkCodedActions.GetContainingCluster(firstStroke);
            if (cluster.ClusterType == InkCluster.ClusterTypes.PossibleARRskip)
            {
                if (!strokeGroupPerRow.Keys.Any() ||
                    !isSkipAdd)
                {
                    return null;
                }

                foreach (var stroke in strokes)
                {
                    cluster.StrokesOnPage.Add(stroke);
                }

                cluster.ClusterType = InkCluster.ClusterTypes.ARRskip;

                var historyIndex = inkAction.HistoryItems.First().HistoryIndex;
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
                var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID);
                var location = inkAction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) ? "right" : "left";

                var codedActionID = string.Format("{0}, {1}", formattedSkips, location);

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = isSkipAdd ? Codings.ACTION_ARRAY_SKIP : Codings.ACTION_ARRAY_SKIP_ERASE,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            if (cluster.ClusterType == InkCluster.ClusterTypes.ARRskip)
            {
                if (!isSkipAdd)
                {
                    foreach (var stroke in strokes)
                    {
                        cluster.StrokesOnPage.Remove(stroke);
                        cluster.StrokesErased.Add(stroke);
                    }
                }
                else
                {
                    foreach (var stroke in strokes)
                    {
                        cluster.StrokesOnPage.Add(stroke);
                    }
                }

                //var onPageInterpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(cluster.StrokesOnPage)) ?? string.Empty;
                //onPageInterpretation = string.Format("\"{0}\"", onPageInterpretation);
                //var formattedInterpretation = string.Format("{0}; {1}", changedInterpretation, onPageInterpretation);

                var historyIndex = inkAction.HistoryItems.First().HistoryIndex;
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
                var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID);
                var location = inkAction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) ? "right" : "left";

                var codedActionID = string.Format("{0}, {1}", formattedSkips, location);

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = isSkipAdd ? Codings.ACTION_ARRAY_SKIP : Codings.ACTION_ARRAY_SKIP_ERASE,
                    CodedObjectID = codedID,
                    CodedObjectIDIncrement = incrementID,
                    CodedObjectActionID = codedActionID
                };

                return historyAction;
            }

            return null;
        }

        public static IHistoryAction ArrayEquation(CLPPage page, IHistoryAction inkAction)
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

            var objectID = array.GetCodedIDAtHistoryIndex(inkAction.HistoryItems.First().HistoryIndex);
            var isEqnAdd = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD;

            var strokes = isEqnAdd
                              ? inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesRemoved).ToList();
            
            var firstStroke = strokes.First();
            var cluster = InkCodedActions.GetContainingCluster(firstStroke);
            if (cluster.ClusterType == InkCluster.ClusterTypes.PossibleARReqn)
            {
                var interpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(strokes));
                if (interpretation == null ||
                    !isEqnAdd)
                {
                    return null;
                }

                foreach (var stroke in strokes)
                {
                    cluster.StrokesOnPage.Add(stroke);
                }

                cluster.ClusterType = InkCluster.ClusterTypes.ARReqn;

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = Codings.OBJECT_ARRAY,
                    CodedObjectAction = isEqnAdd ? Codings.ACTION_ARRAY_EQN : Codings.ACTION_ARRAY_EQN_ERASE,
                    CodedObjectID = objectID,
                    CodedObjectActionID = string.Format("\"{0}\"", interpretation)
                };

                return historyAction;
            }

            if (cluster.ClusterType == InkCluster.ClusterTypes.ARReqn)
            {
                List<string> interpretations;
                if (!isEqnAdd)
                {
                    var orderedStrokes = InkCodedActions.GetOrderStrokesWhereAddedToPage(page, strokes);
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                }
                else
                {
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokes));
                }
                
                var interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                var changedInterpretation = string.Format("\"{0}\"", interpretation);
                
                if (!isEqnAdd)
                {
                    foreach (var stroke in strokes)
                    {
                        cluster.StrokesOnPage.Remove(stroke);
                        cluster.StrokesErased.Add(stroke);
                    }
                }
                else
                {
                    foreach (var stroke in strokes)
                    {
                        cluster.StrokesOnPage.Add(stroke);
                    }
                }

                var onPageInterpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(cluster.StrokesOnPage)) ?? string.Empty;
                onPageInterpretation = string.Format("\"{0}\"", onPageInterpretation);
                var formattedInterpretation = string.Format("{0}; {1}", changedInterpretation, onPageInterpretation);

                var historyAction = new HistoryAction(page, inkAction)
                {
                    CodedObject = Codings.OBJECT_ARRAY,
                    CodedObjectAction = isEqnAdd ? Codings.ACTION_ARRAY_EQN : Codings.ACTION_ARRAY_EQN_ERASE,
                    CodedObjectID = objectID,
                    CodedObjectActionID = formattedInterpretation
                };

                return historyAction;
            }

            return null;
        }

        #endregion // Static Methods

        #region Utility Methods

        public static string StaticSkipCountAnalysis(CLPPage page, CLPArray array, bool isDebugging = false)
        {
            var historyIndex = 0;
            var lastHistoryItem = page.History.CompleteOrderedHistoryItems.LastOrDefault();
            if (lastHistoryItem != null)
            {
                historyIndex = lastHistoryItem.HistoryIndex;
            }

            var strokes = page.InkStrokes.ToList();

            var strokeGroupPerRow = GroupPossibleSkipCountStrokes(page, array, strokes, historyIndex, isDebugging);
            var interpretedRowValues = InterpretSkipCountGroups(page, array, strokeGroupPerRow, historyIndex);
            var isSkipCounting = IsSkipCounting(interpretedRowValues);

            return isSkipCounting ? FormatInterpretedSkipCountGroups(interpretedRowValues) : string.Empty;
        }

        // Dictionary key "-1" contains all rejected strokes.
        public static Dictionary<int, StrokeCollection> GroupPossibleSkipCountStrokes(CLPPage page, CLPArray array, List<Stroke> strokes, int historyIndex, bool isDebugging = false)
        {
            const double RIGHT_OF_VISUAL_RIGHT_THRESHOLD = 80.0;
            const double LEFT_OF_VISUAL_RIGHT_THRESHOLD = 41.5;

            var arrayPosition = array.GetPositionAtHistoryIndex(historyIndex);
            var arrayDimensions = array.GetDimensionsAtHistoryIndex(historyIndex);
            var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
            var arrayVisualRight = arrayPosition.X + arrayDimensions.X - array.LabelLength;
            var arrayVisualTop = arrayPosition.Y + array.LabelLength;
            var halfGridSquareSize = array.GridSquareSize * 0.5;

            // Initialize StrokeCollection for each row
            var strokeGroupPerRow = new Dictionary<int, StrokeCollection>();
            for (var i = 1; i <= arrayColumnsAndRows.Y; i++)
            {
                strokeGroupPerRow.Add(i, new StrokeCollection());
            }

            // Separate strokes into rejected, skip count, or ungrouped
            // Ungrouped will later be grouped via closeness because they are likely to be 100% intersected with 2 rows
            var rejectedStrokes = new List<Stroke>();
            var acceptedStrokes = new List<Stroke>();
            var skipCountStrokes = new List<Stroke>();
            var ungroupedStrokes = new List<Stroke>();
            var acceptedBoundary = new Rect(arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD,
                                            arrayVisualTop - halfGridSquareSize,
                                            LEFT_OF_VISUAL_RIGHT_THRESHOLD + RIGHT_OF_VISUAL_RIGHT_THRESHOLD,
                                            array.GridSquareSize * (arrayColumnsAndRows.Y + 1));
            
            foreach (var stroke in strokes)
            {
                var strokeBounds = stroke.GetBounds();

                // Rejected for being outside the accepted skip counting bounds
                var intersect = Rect.Intersect(strokeBounds, acceptedBoundary);
                if (intersect.IsEmpty)
                {
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                var intersectPercentage = intersect.Area() / strokeBounds.Area();
                if (intersectPercentage <= 0.90)
                {
                    var weightedCenterX = stroke.WeightedCenter().X;
                    if (weightedCenterX < arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD ||
                        weightedCenterX > arrayVisualRight + RIGHT_OF_VISUAL_RIGHT_THRESHOLD)
                    {
                        rejectedStrokes.Add(stroke);
                        continue;
                    }
                }

                // Rejected for being a dot
                if (stroke.IsInvisiblySmall())
                {
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                acceptedStrokes.Add(stroke);
            }

            // No strokes at all inside acceptable boundary
            if (!acceptedStrokes.Any())
            {
                strokeGroupPerRow.Add(-1, new StrokeCollection(rejectedStrokes));
                return strokeGroupPerRow;
            }

            var averageStrokeHeight = acceptedStrokes.Select(s => s.GetBounds().Height).Average();
            var ungroupedCutOffHeight = Math.Max(averageStrokeHeight * 0.50, array.GridSquareSize * 0.33);
            foreach (var stroke in acceptedStrokes)
            {
                var strokeBounds = stroke.GetBounds();

                // Rejected for being too tall
                if (strokeBounds.Height >= array.GridSquareSize * 2.0 ||
                    strokeBounds.Height > averageStrokeHeight * 2.16)
                {
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                // Ungrouped for being too small
                if (strokeBounds.Height < ungroupedCutOffHeight)
                {
                    ungroupedStrokes.Add(stroke);
                    continue;
                }

                skipCountStrokes.Add(stroke);
            }

            #region Debug

            if (isDebugging)
            {
                var sleepTime = 3000;
                page.ClearBoundaries();
                var tempBoundary = new TemporaryBoundary(page, acceptedBoundary.X, acceptedBoundary.Y, acceptedBoundary.Height, acceptedBoundary.Width)
                {
                    RegionText = "Accepted Boundary"
                };
                page.PageObjects.Add(tempBoundary);
                PageHistory.UISleep(800);
                var heightWidths = new Dictionary<Stroke, Point>();
                foreach (var stroke in acceptedStrokes)
                {
                    var width = stroke.DrawingAttributes.Width;
                    var height = stroke.DrawingAttributes.Height;
                    heightWidths.Add(stroke, new Point(width, height));

                    stroke.DrawingAttributes.Width = 8;
                    stroke.DrawingAttributes.Height = 8;
                }
                PageHistory.UISleep(sleepTime);
                foreach (var stroke in acceptedStrokes)
                {
                    var width = heightWidths[stroke].X;
                    var height = heightWidths[stroke].Y;
                    stroke.DrawingAttributes.Width = width;
                    stroke.DrawingAttributes.Height = height;
                }
                tempBoundary.RegionText = "Skip Strokes";
                heightWidths.Clear();
                foreach (var stroke in skipCountStrokes)
                {
                    var width = stroke.DrawingAttributes.Width;
                    var height = stroke.DrawingAttributes.Height;
                    heightWidths.Add(stroke, new Point(width, height));

                    stroke.DrawingAttributes.Width = 8;
                    stroke.DrawingAttributes.Height = 8;
                }
                PageHistory.UISleep(sleepTime);
                foreach (var stroke in skipCountStrokes)
                {
                    var width = heightWidths[stroke].X;
                    var height = heightWidths[stroke].Y;
                    stroke.DrawingAttributes.Width = width;
                    stroke.DrawingAttributes.Height = height;
                }
                tempBoundary.RegionText = "Ungrouped Strokes";
                heightWidths.Clear();
                foreach (var stroke in ungroupedStrokes)
                {
                    var width = stroke.DrawingAttributes.Width;
                    var height = stroke.DrawingAttributes.Height;
                    heightWidths.Add(stroke, new Point(width, height));

                    stroke.DrawingAttributes.Width = 8;
                    stroke.DrawingAttributes.Height = 8;
                }
                PageHistory.UISleep(sleepTime);
                foreach (var stroke in ungroupedStrokes)
                {
                    var width = heightWidths[stroke].X;
                    var height = heightWidths[stroke].Y;
                    stroke.DrawingAttributes.Width = width;
                    stroke.DrawingAttributes.Height = height;
                }
                page.ClearBoundaries();
            }

            #endregion // Debug

            // Row boundaries
            var rowBoundaryX = strokes.Select(s => s.GetBounds().Left).Min() - 5;
            var rowBoundaryWidth = strokes.Select(s => s.GetBounds().Right).Max() - rowBoundaryX + 10;
            var rowBoundaryHeight = array.GridSquareSize * 2.0;

            // Place strokes in most likely row groupings
            foreach (var stroke in skipCountStrokes)
            {
                var strokeBounds = stroke.GetBounds();

                var highestIntersectPercentage = 0.0;
                var mostLikelyRow = 0;
                for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
                {
                    var rowBoundary = new Rect
                                      {
                                          X = rowBoundaryX,
                                          Y = arrayVisualTop + ((row - 1) * array.GridSquareSize) - halfGridSquareSize,
                                          Width = rowBoundaryWidth,
                                          Height = rowBoundaryHeight
                                      };

                    var intersect = Rect.Intersect(strokeBounds, rowBoundary);
                    if (intersect.IsEmpty)
                    {
                        continue;
                    }
                    var intersectPercentage = intersect.Area() / strokeBounds.Area();
                    if (intersectPercentage > 0.9 &&
                        highestIntersectPercentage > 0.9)
                    {
                        // TODO: Log how often this happens. Should only happen whe stroke is 90% intersected by 2 rows.
                        var distanceToRowMidPoint = Math.Abs(strokeBounds.Bottom - rowBoundary.Center().Y);
                        var distanceToPreviousRowMidPoint = Math.Abs(strokeBounds.Bottom - (rowBoundary.Center().Y - array.GridSquareSize));
                        mostLikelyRow = distanceToRowMidPoint < distanceToPreviousRowMidPoint ? row : row - 1;
                        break;
                    }
                    if (intersectPercentage > highestIntersectPercentage)
                    {
                        highestIntersectPercentage = intersectPercentage;
                        mostLikelyRow = row;
                    }
                }

                if (mostLikelyRow == 0)
                {
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                strokeGroupPerRow[mostLikelyRow].Add(stroke);
            }

            foreach (var stroke in ungroupedStrokes)
            {
                var closestStroke = stroke.FindClosestStroke(skipCountStrokes);
                for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
                {
                    if (strokeGroupPerRow[row].Contains(closestStroke))
                    {
                        strokeGroupPerRow[row].Add(stroke);
                        break;
                    }
                }
            }

            strokeGroupPerRow.Add(-1, new StrokeCollection(rejectedStrokes));

            #region Debug

            if (isDebugging)
            {
                page.ClearBoundaries();

                foreach (var strokeGroupKey in strokeGroupPerRow.Keys)
                {
                    if (strokeGroupKey == -1)
                    {
                        continue;
                    }

                    var strokeGroup = strokeGroupPerRow[strokeGroupKey];

                    var heightWidths = new Dictionary<Stroke, Point>();
                    foreach (var stroke in strokeGroup)
                    {
                        var width = stroke.DrawingAttributes.Width;
                        var height = stroke.DrawingAttributes.Height;
                        heightWidths.Add(stroke, new Point(width, height));

                        stroke.DrawingAttributes.Width = 8;
                        stroke.DrawingAttributes.Height = 8;
                    }
                    PageHistory.UISleep(1000);
                    foreach (var stroke in strokeGroup)
                    {
                        var width = heightWidths[stroke].X;
                        var height = heightWidths[stroke].Y;
                        stroke.DrawingAttributes.Width = width;
                        stroke.DrawingAttributes.Height = height;
                    }
                }
            }

            #endregion // Debug

            return strokeGroupPerRow;
        }

        // Interpret handwriting of each row's grouping of strokes.
        public static List<string> InterpretSkipCountGroups(CLPPage page, CLPArray array, Dictionary<int, StrokeCollection> strokeGroupPerRow, int historyIndex, bool isSkipAdd = true)
        {
            var interpretedRowValues = new List<string>();
            var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
            for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
            {
                var expectedRowValue = row * (int)arrayColumnsAndRows.X;
                var strokesInRow = strokeGroupPerRow[row];

                List<string> interpretations;
                if (!isSkipAdd)
                {
                    var orderedStrokes = InkCodedActions.GetOrderStrokesWhereAddedToPage(page, strokesInRow.ToList());
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                }
                else
                {
                    interpretations = InkInterpreter.StrokesToAllGuessesText(strokesInRow);
                }

                if (!interpretations.Any())
                {
                    interpretedRowValues.Add(string.Empty);
                    continue;
                }

                var actualMatch = InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedRowValue);
                if (!string.IsNullOrEmpty(actualMatch))
                {
                    interpretedRowValues.Add(actualMatch);
                    continue;
                }

                var bestGuess = InkInterpreter.InterpretationClosestToANumber(interpretations);
                interpretedRowValues.Add(bestGuess);
            }

            return interpretedRowValues;
        }

        public static string FormatInterpretedSkipCountGroups(List<string> interpretedRowValues)
        {
            var formattedSkips = string.Format("\"{0}\"", string.Join("\" \"", interpretedRowValues));
            return formattedSkips;
        }

        public static bool IsSkipCounting(List<string> interpretedRowValues)
        {
            // Not enough rows for skip counting (AKA only 1 row)
            if (interpretedRowValues == null || 
                interpretedRowValues.Count < 2)
            {
                return false;
            }

            // Not enough numeric interpretations
            var nonEmptyInterpretationsCount = interpretedRowValues.Count(s => !string.IsNullOrEmpty(s));
            var numericInterpreationsCount = interpretedRowValues.Count(s => s.All(char.IsDigit));
            if (numericInterpreationsCount == 0 ||
                numericInterpreationsCount / (nonEmptyInterpretationsCount * 1.0) < 0.34)
            {
                return false;
            }

            // Larger than 1 gap between interpreted values
            // Includes case where no strokes in first 2 rows
            var gapCount = 0;
            foreach (var interpretedRowValue in interpretedRowValues)
            {
                if (string.IsNullOrEmpty(interpretedRowValue))
                {
                    gapCount++;
                }
                else
                {
                    gapCount = 0;
                }

                if (gapCount > 1)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion // Utility Methods
    }
}