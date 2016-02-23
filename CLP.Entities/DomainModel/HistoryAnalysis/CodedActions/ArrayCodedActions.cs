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

            // BUG: Doesn't deal with skip erase!
            var strokes = inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList();
            if (strokes.Count < 2)
            {
                return null;
            }

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
                //Console.WriteLine("*****NO SKIP COUNT STROKES TO IGNORE*****");
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
                var interpretations = InkInterpreter.StrokesToAllGuessesText(strokesInRow);
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

            if (strokeGroupPerRow.Keys.Any())
            {
                var historyIndex = inkAction.HistoryItems.First().HistoryIndex;
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
                var incrementID = HistoryAction.GetIncrementID(array.ID, codedObject, codedID);
                var location = inkAction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) ? "right" : "left";

                var codedActionID = string.Format("{0}, {1}", formattedSkips, location);

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

        public static IHistoryAction AttemptInkDivide(CLPPage page, IHistoryItem historyItem)
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

                return inkDivideAction;
            }

            #endregion // Ink Divide Interpretation

            return null;
        }

        public static IHistoryAction ArrayEquation(CLPPage page, IHistoryAction inkAction)
        {
            const double INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD = 5.0;
            const string MULTIPLICATION_SYMBOL = "×";
            const string ADDITION_SYMBOL = "+";
            const string EQUALS_SYMBOL = "=";

            if (page == null ||
                inkAction == null ||
                inkAction.CodedObject != Codings.OBJECT_INK ||
                !(inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ||
                  inkAction.CodedObjectAction == Codings.ACTION_INK_ERASE))
            {
                return null;
            }

            var strokes = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD
                              ? inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.StrokesRemoved).ToList();

            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokes));
            var interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);

            var definitelyInArith = new List<string> { MULTIPLICATION_SYMBOL, ADDITION_SYMBOL, EQUALS_SYMBOL };
            var percentageOfDigits = InkCodedActions.GetPercentageOfDigits(interpretation);
            var isDefinitelyArith = definitelyInArith.Any(s => interpretation.Contains(s));

            if (percentageOfDigits < INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD &&
                !isDefinitelyArith)
            {
                return null;
            }

            var historyAction = new HistoryAction(page, inkAction)
            {
                CodedObject = Codings.OBJECT_ARRAY,
                CodedObjectAction = inkAction.CodedObjectAction == Codings.ACTION_INK_ADD ? Codings.ACTION_ARRAY_EQN : Codings.ACTION_ARRAY_EQN_ERASE,
                CodedObjectID = inkAction.CodedObjectID,
                CodedObjectActionID = string.Format("\"{0}\"", interpretation)
            };

            return historyAction;
        }

        #endregion // Static Methods
    }
}