using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel;
using CLP.InkInterpretation;

namespace CLP.Entities
{
    public static class ArraySemanticEvents
    {
        #region Initialization

        public static ISemanticEvent Rotate(CLPPage page, CLPArrayRotateHistoryAction rotateHistoryAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(rotateHistoryAction), rotateHistoryAction);

            var arrayID = rotateHistoryAction.ArrayID;
            var array = page.GetPageObjectByIDOnPageOrInHistory(arrayID) as CLPArray;
            if (array == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, rotateHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Rotate, Array NULL");
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = Codings.EVENT_ARRAY_ROTATE;
            var codedID = $"{rotateHistoryAction.OldRows}x{rotateHistoryAction.OldColumns}";
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(array.ID, codedObject, codedID);

            var eventInfo = $"{rotateHistoryAction.OldColumns}x{rotateHistoryAction.OldRows}";
            var eventInfoIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject(array.ID, codedObject, eventInfo);
            if (!string.IsNullOrWhiteSpace(eventInfoIncrementID))
            {
                eventInfo += " " + eventInfoIncrementID;
            }

            var semanticEvent = new SemanticEvent(page, rotateHistoryAction)
            {
                CodedObject = codedObject,
                EventType = eventType,
                CodedObjectID = codedID,
                CodedObjectIDIncrement = incrementID,
                EventInformation = eventInfo,
                ReferencePageObjectID = arrayID
            };

            return semanticEvent;
        }

        public static ISemanticEvent Cut(CLPPage page, PageObjectCutHistoryAction cutHistoryAction)
        {
            // TODO: Refactor and handle cases when something other than Array is cut.
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(cutHistoryAction), cutHistoryAction);

            var cutPageObjectID = cutHistoryAction.CutPageObjectID;
            if (string.IsNullOrWhiteSpace(cutPageObjectID))
            {
                var nothingCutSemanticEvent = new SemanticEvent(page, cutHistoryAction)
                {
                    CodedObject = Codings.OBJECT_NOTHING,
                    EventType = Codings.EVENT_CUT
                };

                return nothingCutSemanticEvent;
            }

            var cutArray = page.GetPageObjectByIDOnPageOrInHistory(cutPageObjectID) as CLPArray;
            if (cutArray == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, cutHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Cut, Cut Array NULL");
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = Codings.EVENT_CUT;
            var codedID = cutArray.GetCodedIDAtHistoryIndex(cutHistoryAction.HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(cutArray.ID, codedObject, codedID);

            var eventInfoSegments = new List<string>();
            foreach (var halvedPageObjectID in cutHistoryAction.HalvedPageObjectIDs)
            {
                var array = page.GetPageObjectByIDOnPageOrInHistory(halvedPageObjectID) as CLPArray;
                if (array == null)
                {
                    return SemanticEvent.GetErrorSemanticEvent(page, cutHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Cut, Halved Array NULL");
                }

                var arrayCodedID = array.GetCodedIDAtHistoryIndex(cutHistoryAction.HistoryActionIndex + 1);
                var arrayIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject(array.ID, codedObject, arrayCodedID);
                var eventInfoSegment = string.IsNullOrWhiteSpace(arrayIncrementID) ? arrayCodedID : $"{arrayCodedID} {arrayIncrementID}";
                eventInfoSegments.Add(eventInfoSegment);
            }

            var cuttingStroke = page.GetStrokeByIDOnPageOrInHistory(cutHistoryAction.CuttingStrokeID);
            if (cuttingStroke == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, cutHistoryAction, Codings.ERROR_TYPE_NULL_STROKE, "Cut, Cutting Stroke NULL");
            }

            var isVerticalStrokeCut = cuttingStroke.IsVerticalStroke();
            if (isVerticalStrokeCut)
            {
                eventInfoSegments.Add(Codings.EVENT_INFO_ARRAY_CUT_VERTICAL);
            }

            var eventInfo = string.Join(", ", eventInfoSegments);

            var semanticEvent = new SemanticEvent(page, cutHistoryAction)
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = cutPageObjectID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent Snap(CLPPage page, CLPArraySnapHistoryAction snapHistoryAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(snapHistoryAction), snapHistoryAction);

            var persistingArray = page.GetPageObjectByIDOnPageOrInHistory(snapHistoryAction.PersistingArrayID) as CLPArray;
            if (persistingArray == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, snapHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Snap, Persisting Array NULL");
            }

            var snappedArray = page.GetPageObjectByIDOnPageOrInHistory(snapHistoryAction.SnappedArrayID) as CLPArray;
            if (snappedArray == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, snapHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Snap, Snapped Array NULL");
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = Codings.EVENT_ARRAY_SNAP;

            // For consistency's sake, the persistingArray is always the CodedID.
            // It is the array that remains on the page, therefore it is the one
            // whose dimensions change. The SubID is the array that is snapped onto
            // the persistingArray then disappears.
            var codedID = persistingArray.GetCodedIDAtHistoryIndex(snapHistoryAction.HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(persistingArray.ID, codedObject, codedID);
            var codedSubID = snappedArray.GetCodedIDAtHistoryIndex(snapHistoryAction.HistoryActionIndex);
            var incrementSubID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(snappedArray.ID, codedObject, codedSubID);

            var eventInfo = persistingArray.GetCodedIDAtHistoryIndex(snapHistoryAction.HistoryActionIndex + 1);
            var eventInfoIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject(persistingArray.ID, codedObject, eventInfo);
            if (!string.IsNullOrWhiteSpace(eventInfoIncrementID))
            {
                eventInfo += " " + eventInfoIncrementID;
            }

            var semanticEvent = new SemanticEvent(page, snapHistoryAction)
            {
                CodedObject = codedObject,
                EventType = eventType,
                CodedObjectID = codedID,
                CodedObjectIDIncrement = incrementID,
                CodedObjectSubID = codedSubID,
                CodedObjectSubIDIncrement = incrementSubID,
                EventInformation = eventInfo
            };

            return semanticEvent;
        }

        public static ISemanticEvent Divide(CLPPage page, CLPArrayDivisionsChangedHistoryAction divideHistoryAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(divideHistoryAction), divideHistoryAction);

            var dividedArrayID = divideHistoryAction.ArrayID;
            var dividedArray = page.GetPageObjectByIDOnPageOrInHistory(dividedArrayID) as CLPArray;
            if (dividedArray == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, divideHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Divide, Divided Array NULL");
            }

            if (divideHistoryAction.IsColumnRegionsChange == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, divideHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Divide, Action Missing Old and New Regions");
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = Codings.EVENT_ARRAY_DIVIDE;
            var codedID = dividedArray.GetCodedIDAtHistoryIndex(divideHistoryAction.HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(dividedArray.ID, codedObject, codedID);

            // TODO: Right now, listing all regions after divide. Alternatively, can list just new regions and have SubID for the replaced region
            var index = 0;
            var eventInfoSegments = new List<string>();
            foreach (var region in divideHistoryAction.NewRegions.OrderBy(r => r.Position))
            {
                int regionRow;
                int regionColumn;
                if (divideHistoryAction.IsColumnRegionsChange.Value)
                {
                    regionColumn = (int)dividedArray.GetColumnsAndRowsAtHistoryIndex(divideHistoryAction.HistoryActionIndex).X;
                    regionRow = region.Value;
                }
                else
                {
                    regionColumn = region.Value;
                    regionRow = (int)dividedArray.GetColumnsAndRowsAtHistoryIndex(divideHistoryAction.HistoryActionIndex).Y;
                }

                var segmentID = $"{regionColumn}x{regionRow}";
                var segmentIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject_Sub(dividedArray.ID, codedObject, codedID, index, segmentID);
                if (!string.IsNullOrWhiteSpace(segmentIncrementID))
                {
                    segmentID += " " + segmentIncrementID;
                }

                eventInfoSegments.Add(segmentID);
                index++;
            }

            if (divideHistoryAction.IsColumnRegionsChange.Value)
            {
                eventInfoSegments.Add(Codings.EVENT_INFO_ARRAY_DIVIDER_VERTICAL);
            }

            var eventInfo = string.Join(", ", eventInfoSegments);

            var semanticEvent = new SemanticEvent(page, divideHistoryAction)
            {
                CodedObject = codedObject,
                EventType = eventType,
                CodedObjectID = codedID,
                CodedObjectIDIncrement = incrementID,
                EventInformation = eventInfo,
                ReferencePageObjectID = dividedArrayID
            };

            return semanticEvent;
        }

        #endregion // Initialization

        #region Clustering

        public static ISemanticEvent CountingLine(CLPPage page, IHistoryAction historyAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(historyAction), historyAction);

            var objectsChangedHistoryAction = historyAction as ObjectsOnPageChangedHistoryAction;
            if (objectsChangedHistoryAction == null ||
                objectsChangedHistoryAction.IsUsingPageObjects ||
                !objectsChangedHistoryAction.IsUsingStrokes)
            {
                return null;
            }

            var strokes = objectsChangedHistoryAction.StrokesAdded;
            var isAddedStroke = true;
            if (!strokes.Any())
            {
                isAddedStroke = false;
                strokes = objectsChangedHistoryAction.StrokesRemoved;
            }

            if (strokes.Count != 1)
            {
                return null;
            }

            var stroke = strokes.First();

            var historyIndex = objectsChangedHistoryAction.HistoryActionIndex;
            var arraysOnPage = page.GetPageObjectsOnPageAtHistoryIndex(historyIndex).OfType<CLPArray>().Where(a => a.ArrayType == ArrayTypes.Array && a.IsGridOn).ToList();

            if (!arraysOnPage.Any())
            {
                return null;
            }

            var array = InkSemanticEvents.FindMostOverlappedPageObjectAtHistoryIndex(page, arraysOnPage.Cast<IPageObject>().ToList(), stroke, historyIndex) as CLPArray;
            if (array == null)
            {
                return null;
            }

            #region Counting Line Interpretation

            var arrayDimensions = array.GetDimensionsAtHistoryIndex(historyIndex);
            var arrayWidth = arrayDimensions.X - (2 * array.LabelLength);

            var referenceStrokeCopy = stroke.GetStrokeCopyAtHistoryIndex(page, historyIndex);
            var isHorizontalLine = referenceStrokeCopy.IsHorizontalLine();
            var strokeBoundsWidth = referenceStrokeCopy.BoundsWidth();

            var isCountingLine = isHorizontalLine && strokeBoundsWidth > arrayWidth / 2;

            #endregion // Counting Line Interpretation

            if (!isCountingLine)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = isAddedStroke ? Codings.EVENT_ARRAY_COUNT_LINE : Codings.EVENT_ARRAY_COUNT_LINE_ERASE;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(array.ID, codedObject, codedID);

            var countingLineEvent = new SemanticEvent(page, historyAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = eventType,
                                        CodedObjectID = codedID,
                                        CodedObjectIDIncrement = incrementID,
                                        ReferencePageObjectID = array.ID
                                    };

            return countingLineEvent;
        }

        public static ISemanticEvent InkDivide(CLPPage page, IHistoryAction historyAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(historyAction), historyAction);

            var objectsChangedHistoryAction = historyAction as ObjectsOnPageChangedHistoryAction;
            if (objectsChangedHistoryAction == null ||
                objectsChangedHistoryAction.IsUsingPageObjects ||
                !objectsChangedHistoryAction.IsUsingStrokes)
            {
                return null;
            }

            var strokes = objectsChangedHistoryAction.StrokesAdded;
            var isAddedStroke = true;
            if (!strokes.Any())
            {
                isAddedStroke = false;
                strokes = objectsChangedHistoryAction.StrokesRemoved;
            }

            if (strokes.Count != 1)
            {
                return null;
            }

            var stroke = strokes.First();

            var historyIndex = objectsChangedHistoryAction.HistoryActionIndex;
            var arraysOnPage = page.GetPageObjectsOnPageAtHistoryIndex(historyIndex).OfType<CLPArray>().Where(a => a.ArrayType == ArrayTypes.Array && a.IsGridOn).ToList();

            if (!arraysOnPage.Any())
            {
                return null;
            }

            var array = InkSemanticEvents.FindMostOverlappedPageObjectAtHistoryIndex(page, arraysOnPage.Cast<IPageObject>().ToList(), stroke, historyIndex) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var eventInfo = string.Empty;

            #region Ink Divide Interpretation

            var isInkDivide = false;

            var verticalDividers = new List<int>
                                   {
                                       0
                                   };
            var horizontalDividers = new List<int>
                                     {
                                         0
                                     };

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
                (strokeTop - cuttableTop <= SMALL_THRESHOLD || cuttableBottom - strokeBottom <= SMALL_THRESHOLD) &&
                (strokeTop - cuttableTop <= LARGE_THRESHOLD && cuttableBottom - strokeBottom <= LARGE_THRESHOLD) &&
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

                verticalDividers.Add(dividerValue);
                verticalDividers.Add(array.Columns); // TODO: Fix for if multiple ink dividers are made in a row
                verticalDividers = verticalDividers.Distinct().OrderBy(x => x).ToList();
                var verticalDivisions = verticalDividers.Zip(verticalDividers.Skip(1), (x, y) => y - x).Select(x => $"{arrayColumnsAndRows.Y}x{x}");

                var position = 0;
                var subIDsWithIncrementIDs = new List<string>();
                foreach (var verticalDivision in verticalDivisions)
                {
                    var subIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject_Sub(array.ID,
                                                                                                     Codings.OBJECT_ARRAY,
                                                                                                     array.GetCodedIDAtHistoryIndex(historyIndex),
                                                                                                     position,
                                                                                                     verticalDivision,
                                                                                                     !isAddedStroke);
                    var subIDWithIncrementID = $"{verticalDivision}{(!string.IsNullOrEmpty(subIncrementID) ? " " + subIncrementID : string.Empty)}";
                    subIDsWithIncrementIDs.Add(subIDWithIncrementID);
                    position++;
                }

                eventInfo = string.Join(", ", subIDsWithIncrementIDs);
                isInkDivide = true;
            }

            if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                strokeBottom <= cuttableBottom &&
                strokeTop >= cuttableTop &&
                (cuttableRight - strokeRight <= SMALL_THRESHOLD || strokeLeft - cuttableLeft <= SMALL_THRESHOLD) &&
                (cuttableRight - strokeRight <= LARGE_THRESHOLD && strokeLeft - cuttableLeft <= LARGE_THRESHOLD) &&
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

                horizontalDividers.Add(dividerValue);
                horizontalDividers.Add(array.Rows);
                horizontalDividers = horizontalDividers.Distinct().OrderBy(x => x).ToList();
                var horizontalDivisions = horizontalDividers.Zip(horizontalDividers.Skip(1), (x, y) => y - x).Select(x => $"{x}x{arrayColumnsAndRows.X}");

                var position = 0;
                var subIDsWithIncrementIDs = new List<string>();
                foreach (var horizontalDivision in horizontalDivisions)
                {
                    var subIncrementID = ObjectSemanticEvents.SetCurrentIncrementIDForPageObject_Sub(array.ID,
                                                                                                     Codings.OBJECT_ARRAY,
                                                                                                     array.GetCodedIDAtHistoryIndex(historyIndex),
                                                                                                     position,
                                                                                                     horizontalDivision,
                                                                                                     !isAddedStroke);
                    var subIDWithIncrementID = $"{horizontalDivision}{(!string.IsNullOrEmpty(subIncrementID) ? " " + subIncrementID : string.Empty)}";
                    subIDsWithIncrementIDs.Add(subIDWithIncrementID);
                    position++;
                }

                eventInfo = string.Join(", ", subIDsWithIncrementIDs);
                isInkDivide = true;
            }

            #endregion // Ink Divide Interpretation

            if (!isInkDivide)
            {
                return null;
            }

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = isAddedStroke ? Codings.EVENT_ARRAY_DIVIDE_INK : Codings.EVENT_ARRAY_DIVIDE_INK_ERASE;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(array.ID, codedObject, codedID);

            var inkDivideEvent = new SemanticEvent(page, historyAction)
                                 {
                                     CodedObject = codedObject,
                                     EventType = eventType,
                                     CodedObjectID = codedID,
                                     CodedObjectIDIncrement = incrementID,
                                     EventInformation = eventInfo,
                                     ReferencePageObjectID = array.ID
                                 };

            return inkDivideEvent;
        }

        #endregion // Clustering

        #region Interpretation

        public static ISemanticEvent SkipCounting(CLPPage page, ISemanticEvent inkEvent)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(inkEvent), inkEvent);

            if (inkEvent.CodedObject != Codings.OBJECT_INK ||
                !(inkEvent.EventType == Codings.EVENT_INK_ADD || 
                  inkEvent.EventType == Codings.EVENT_INK_ERASE))
            {
                return null;
            }

            var referenceArrayID = inkEvent.ReferencePageObjectID;
            if (referenceArrayID == null)
            {
                return null;
            }

            // BUG: Potential bug, if the array was deleted, but all the ink of the skip count wasn't deleted when the array was deleted
            // because it wasn't fully captured, this inkEvent shouldn't continue quite the way it does here. Something like the following:
            // INK strokes erase [A] formerly skip strokes for ARR [8x8]
            var array = page.GetPageObjectByIDOnPageOrInHistory(referenceArrayID) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var isSkipAdd = inkEvent.EventType == Codings.EVENT_INK_ADD;

            var strokes = isSkipAdd
                              ? inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesRemoved).ToList();

            var firstStroke = strokes.First();
            var cluster = InkSemanticEvents.GetContainingCluster(firstStroke);
            if (cluster.ClusterType != InkCluster.ClusterTypes.ArraySkipCounting)
            {
                return null;
            }

            var historyIndex = inkEvent.LastHistoryAction.HistoryActionIndex;
            var strokesOnPage = cluster.GetClusterStrokesOnPageAtHistoryIndex(page, historyIndex);

            var strokeGroupPerRow = GroupPossibleSkipCountStrokes(page, array, strokes, historyIndex);
            var strokeGroupPerRowOnPage = GroupPossibleSkipCountStrokes(page, array, strokesOnPage, historyIndex);
            var interpretedRowValues = InterpretSkipCountGroups(page, array, strokeGroupPerRow, historyIndex);
            var interpretedRowValuesOnPage = InterpretSkipCountGroups(page, array, strokeGroupPerRowOnPage, historyIndex);
            var formattedSkips = FormatInterpretedSkipCountGroups(interpretedRowValues);
            var formattedSkipsOnPage = FormatInterpretedSkipCountGroups(interpretedRowValuesOnPage);

            var formattedInterpretation = $"{formattedSkips}; {formattedSkipsOnPage}";

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = isSkipAdd ? Codings.EVENT_ARRAY_SKIP : Codings.EVENT_ARRAY_SKIP_ERASE;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(array.ID, codedObject, codedID);
            var location = inkEvent.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_RIGHT_SKIP) ? "right" : "left";

            var eventInfo = $"{formattedInterpretation}, {location}";

            var semanticEvent = new SemanticEvent(page, inkEvent)
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = referenceArrayID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent BottomSkipCounting(CLPPage page, ISemanticEvent inkEvent)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(inkEvent), inkEvent);

            if (inkEvent.CodedObject != Codings.OBJECT_INK ||
                !(inkEvent.EventType == Codings.EVENT_INK_ADD ||
                  inkEvent.EventType == Codings.EVENT_INK_ERASE))
            {
                return null;
            }

            var referenceArrayID = inkEvent.ReferencePageObjectID;
            if (referenceArrayID == null)
            {
                return null;
            }

            // BUG: Potential bug, if the array was deleted, but all the ink of the skip count wasn't deleted when the array was deleted
            // because it wasn't fully captured, this inkEvent shouldn't continue quite the way it does here. Something like the following:
            // INK strokes erase [A] formerly skip strokes for ARR [8x8]
            var array = page.GetPageObjectByIDOnPageOrInHistory(referenceArrayID) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var isSkipAdd = inkEvent.EventType == Codings.EVENT_INK_ADD;

            var strokes = isSkipAdd
                              ? inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesRemoved).ToList();

            var firstStroke = strokes.First();
            var cluster = InkSemanticEvents.InkClusters.FirstOrDefault(c => c.Strokes.Contains(firstStroke) && c.ClusterType == InkCluster.ClusterTypes.ArrayBottomSkipCounting);
            if (cluster == null)
            {
                return null;
            }

            var historyIndex = inkEvent.LastHistoryAction.HistoryActionIndex;
            var strokesOnPage = cluster.GetClusterStrokesOnPageAtHistoryIndex(page, historyIndex);

            var formattedSkips = InterpretBottomSkipCountStrokes(strokes);
            var formattedSkipsOnPage = InterpretBottomSkipCountStrokes(strokesOnPage);

            var formattedInterpretation = $"{formattedSkips}; {formattedSkipsOnPage}";

            var codedObject = Codings.OBJECT_ARRAY;
            var eventType = isSkipAdd ? Codings.EVENT_ARRAY_SKIP : Codings.EVENT_ARRAY_SKIP_ERASE;
            var codedID = array.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(array.ID, codedObject, codedID);
            var location = "bottom";

            var eventInfo = $"{formattedInterpretation}, {location}";

            var semanticEvent = new SemanticEvent(page, inkEvent)
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = referenceArrayID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent ArrayEquation(CLPPage page, ISemanticEvent inkEvent)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(inkEvent), inkEvent);

            if (inkEvent.CodedObject != Codings.OBJECT_INK ||
                !(inkEvent.EventType == Codings.EVENT_INK_ADD || 
                  inkEvent.EventType == Codings.EVENT_INK_ERASE))
            {
                return null;
            }

            var referenceArrayID = inkEvent.ReferencePageObjectID;
            if (referenceArrayID == null)
            {
                return null;
            }

            var array = page.GetPageObjectByIDOnPageOrInHistory(referenceArrayID) as CLPArray;
            if (array == null)
            {
                return null;
            }

            var codedID = array.GetCodedIDAtHistoryIndex(inkEvent.HistoryActions.First().HistoryActionIndex);
            var isEqnAdd = inkEvent.EventType == Codings.EVENT_INK_ADD;

            var strokes = isEqnAdd
                              ? inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesAdded).ToList()
                              : inkEvent.HistoryActions.Cast<ObjectsOnPageChangedHistoryAction>().SelectMany(h => h.StrokesRemoved).ToList();

            var firstStroke = strokes.First();
            var cluster = InkSemanticEvents.GetContainingCluster(firstStroke);
            switch (cluster.ClusterType)
            {
                case InkCluster.ClusterTypes.Unknown:
                {
                    if (!isEqnAdd)
                    {
                        return null;
                    }

                    var orderedStrokes = InkSemanticEvents.GetOrderStrokesWereAddedToPage(page, strokes);
                    var interpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(orderedStrokes));
                    if (interpretation == null)
                    {
                        return null;
                    }

                    cluster.ClusterType = InkCluster.ClusterTypes.ArrayEquation;

                    var semanticEvent = new SemanticEvent(page, inkEvent)
                                        {
                                            CodedObject = Codings.OBJECT_ARRAY,
                                            EventType = Codings.EVENT_ARRAY_EQN,
                                            CodedObjectID = codedID,
                                            EventInformation = $"\"{interpretation}\"",
                                            ReferencePageObjectID = referenceArrayID
                                        };

                    return semanticEvent;
                }
                case InkCluster.ClusterTypes.ArrayEquation:
                {
                    var orderedStrokes = InkSemanticEvents.GetOrderStrokesWereAddedToPage(page, strokes);
                    var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));
                    var interpretation = InkInterpreter.InterpretationClosestToANumber(interpretations);
                    var changedInterpretation = $"\"{interpretation}\"";

                    var strokesOnPage = cluster.GetClusterStrokesOnPageAtHistoryIndex(page, inkEvent.HistoryActions.Last().HistoryActionIndex);
                    var orderedStrokesOnPage = InkSemanticEvents.GetOrderStrokesWereAddedToPage(page, strokesOnPage);
                    var onPageInterpretation = InkInterpreter.StrokesToArithmetic(new StrokeCollection(orderedStrokesOnPage)) ?? string.Empty;
                    onPageInterpretation = $"\"{onPageInterpretation}\"";

                    var formattedInterpretation = $"{changedInterpretation}; {onPageInterpretation}";

                    var semanticEvent = new SemanticEvent(page, inkEvent)
                                        {
                                            CodedObject = Codings.OBJECT_ARRAY,
                                            EventType = isEqnAdd ? Codings.EVENT_ARRAY_EQN : Codings.EVENT_ARRAY_EQN_ERASE,
                                            CodedObjectID = codedID,
                                            EventInformation = formattedInterpretation,
                                            ReferencePageObjectID = referenceArrayID
                                        };

                    return semanticEvent;
                }
            }

            return null;
        }

        #endregion // Interpretation

        #region Utility

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
                // Rule 1: Rejected for being invisibly small.
                if (stroke.IsInvisiblySmall())
                {
                    Rule1Count++;
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                var strokeBounds = stroke.GetBounds();

                // Rule 2: Rejected for being too tall.
                if (strokeBounds.Height >= array.GridSquareSize * 2.0)
                {
                    Rule2Count++;
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                // Rule 3: Rejected for being outside the accepted skip counting bounds
                var intersect = Rect.Intersect(strokeBounds, acceptedBoundary);
                if (intersect.IsEmpty)
                {
                    Rule3bCount++;
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                var intersectPercentage = intersect.Area() / strokeBounds.Area();
                if (intersectPercentage <= 0.50)
                {
                    Rule3bCount++;
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                if (intersectPercentage <= 0.90)
                {
                    var weightedCenterX = stroke.WeightedCenter().X;
                    if (weightedCenterX < arrayVisualRight - LEFT_OF_VISUAL_RIGHT_THRESHOLD ||
                        weightedCenterX > arrayVisualRight + RIGHT_OF_VISUAL_RIGHT_THRESHOLD)
                    {
                        Rule3cCount++;
                        rejectedStrokes.Add(stroke);
                        continue;
                    }
                }

                Rule3aCount++;
                acceptedStrokes.Add(stroke);
            }

            // No strokes at all inside acceptable boundary
            if (!acceptedStrokes.Any())
            {
                AllStrokesAreOutsideOfAcceptableBoundary++;
                strokeGroupPerRow.Add(-1, new StrokeCollection(rejectedStrokes));
                RejectedStrokesCount += rejectedStrokes.Distinct().Count();
                return strokeGroupPerRow;
            }

            var averageStrokeHeight = acceptedStrokes.Select(s => s.GetBounds().Height).Average();
            var ungroupedCutOffHeight = Math.Max(averageStrokeHeight * 0.50, array.GridSquareSize * 0.33);
            var probableSkipCountStrokes = new List<Stroke>();
            foreach (var stroke in acceptedStrokes)
            {
                var strokeBounds = stroke.GetBounds();

                // Rejected that deviate too much from average height
                if (strokeBounds.Height > averageStrokeHeight * 2.16)
                {
                    Rule4Count++;
                    rejectedStrokes.Add(stroke);
                    continue;
                }

                probableSkipCountStrokes.Add(stroke);
            }

            // No probably skip strokes, shouldn't ever happen
            if (!probableSkipCountStrokes.Any())
            {
                strokeGroupPerRow.Add(-1, new StrokeCollection(rejectedStrokes));
                RejectedStrokesCount += rejectedStrokes.Distinct().Count();
                return strokeGroupPerRow;
            }

            if (probableSkipCountStrokes.Count == 1)
            {
                skipCountStrokes.Add(probableSkipCountStrokes.First());
            }
            else
            {
                var averageClosestDistance = probableSkipCountStrokes.Select(s => Math.Sqrt(s.DistanceSquaredByClosestPoint(s.FindClosestStroke(probableSkipCountStrokes)))).Average();
                foreach (var stroke in probableSkipCountStrokes)
                {
                    var strokeBounds = stroke.GetBounds();

                    // Rejected for being too far away from other probable strokes
                    var closestStroke = stroke.FindClosestStroke(probableSkipCountStrokes);
                    var distance = Math.Sqrt(stroke.DistanceSquaredByClosestPoint(closestStroke));
                    if (distance > averageClosestDistance * 4.0)
                    {
                        Rule6Count++;
                        rejectedStrokes.Add(stroke);
                        continue;
                    }

                    // Ungrouped for being too small
                    if (strokeBounds.Height < ungroupedCutOffHeight)
                    {
                        Rule5Count++;
                        ungroupedStrokes.Add(stroke);
                        continue;
                    }

                    skipCountStrokes.Add(stroke);
                }
            }

            #region Debug

            if (isDebugging)
            {
                var sleepTime = 2500;
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

            var overlapStrokes = new List<Stroke>();
            // Place strokes in most likely row groupings
            foreach (var stroke in skipCountStrokes)
            {
                var strokeBounds = stroke.GetBounds();

                var highestIntersectPercentage = 0.0;
                var mostLikelyRow = 0;
                for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
                {
                    var rowExtendedBoundary = new Rect
                                              {
                                                  X = rowBoundaryX,
                                                  Y = arrayVisualTop + ((row - 1) * array.GridSquareSize) - halfGridSquareSize,
                                                  Width = rowBoundaryWidth,
                                                  Height = rowBoundaryHeight
                                              };

                    var rowBoundary = new Rect
                                      {
                                          X = rowBoundaryX,
                                          Y = arrayVisualTop + ((row - 1) * array.GridSquareSize),
                                          Width = rowBoundaryWidth,
                                          Height = array.GridSquareSize
                                      };

                    var intersect = Rect.Intersect(strokeBounds, rowExtendedBoundary);
                    if (intersect.IsEmpty)
                    {
                        continue;
                    }
                    var intersectPercentage = intersect.Area() / strokeBounds.Area();

                    // Should only happen whe stroke is 90% intersected by 2 rows
                    if (intersectPercentage > 0.9 &&
                        highestIntersectPercentage > 0.9)
                    {
                        var smallIntersect = Rect.Intersect(strokeBounds, rowBoundary);
                        if (!intersect.IsEmpty)
                        {
                            var smallIntersectPercentage = smallIntersect.Area() / strokeBounds.Area();
                            if (smallIntersectPercentage > 0.80)
                            {
                                mostLikelyRow = row;
                                break;
                            }
                        }

                        Rule7cCount++;

                        overlapStrokes.Add(stroke);
                        strokeGroupPerRow[row].Add(stroke);
                        strokeGroupPerRow[row - 1].Add(stroke);
                        mostLikelyRow = -1;
                        //var distanceToRowMidPoint = Math.Abs(stroke.WeightedCenter().Y - rowBoundary.Center().Y);
                        //var distanceToPreviousRowMidPoint = Math.Abs(stroke.WeightedCenter().Y - (rowBoundary.Center().Y - array.GridSquareSize));
                        //mostLikelyRow = distanceToRowMidPoint < distanceToPreviousRowMidPoint ? row : row - 1;
                        break;
                    }
                    if (intersectPercentage > highestIntersectPercentage)
                    {
                        highestIntersectPercentage = intersectPercentage;
                        mostLikelyRow = row;
                    }
                }

                switch (mostLikelyRow)
                {
                    case -1:
                        continue;
                    case 0:
                        Rule7dCount++;
                        rejectedStrokes.Add(stroke);
                        continue;
                }

                strokeGroupPerRow[mostLikelyRow].Add(stroke);
            }

            // Match overlapping strokes
            var allGroupedStrokes = strokeGroupPerRow.Values.SelectMany(s => s).ToList();
            var finalOverlapStrokes = new List<Stroke>();
            foreach (var stroke in overlapStrokes)
            {
                var strokeBounds = stroke.GetBounds();
                var strokeRect = new Rect(0, strokeBounds.Top, 10, strokeBounds.Height);

                var highestIntersectPercentage = 0.0;
                //var minAbsoluteAngle = double.MaxValue;

                Stroke mostLikelyStroke = null;
                foreach (var groupedStroke in allGroupedStrokes.Where(s => !overlapStrokes.Contains(s)))
                {
                    var groupedStrokeBounds = groupedStroke.GetBounds();
                    var groupedStrokeRect = new Rect(0, groupedStrokeBounds.Top, 10, groupedStrokeBounds.Height);
                    var intersect = Rect.Intersect(strokeRect, groupedStrokeRect);
                    if (intersect.IsEmpty)
                    {
                        continue;
                    }
                    var intersectPercentage = intersect.Area() / strokeRect.Area();
                    if (intersectPercentage > highestIntersectPercentage)
                    {
                        highestIntersectPercentage = intersectPercentage;
                        mostLikelyStroke = groupedStroke;
                    }

                    //var absoluteAngle = stroke.AbsoluteAngleBetweenStroke(groupedStroke);
                    //if (absoluteAngle < minAbsoluteAngle &&
                    //    absoluteAngle < 25.0)
                    //{
                    //    minAbsoluteAngle = absoluteAngle;
                    //    mostLikelyStroke = groupedStroke;
                    //}
                }

                if (mostLikelyStroke == null)
                {
                    var highestRowIntersectPercentage = 0.0;
                    var mostLikelyRow = 0;
                    for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
                    {
                        var rowBoundary = new Rect
                                          {
                                              X = 0,
                                              Y = arrayVisualTop + ((row - 1) * array.GridSquareSize),
                                              Width = 10,
                                              Height = array.GridSquareSize
                                          };

                        var intersect = Rect.Intersect(strokeRect, rowBoundary);
                        if (intersect.IsEmpty)
                        {
                            continue;
                        }
                        var intersectPercentage = intersect.Area() / strokeRect.Area();

                        if (intersectPercentage > 0.75 &&
                            intersectPercentage > highestIntersectPercentage)
                        {
                            highestIntersectPercentage = intersectPercentage;
                            mostLikelyRow = row;
                        }
                    }

                    if (mostLikelyRow == 0)
                    {
                        finalOverlapStrokes.Add(stroke);
                        Rule8cCount++;
                        continue;
                    }

                    Rule8bCount++;
                    foreach (var strokesInRow in strokeGroupPerRow.Values)
                    {
                        if (strokesInRow.Contains(stroke))
                        {
                            strokesInRow.Remove(stroke);
                        }
                    }

                    strokeGroupPerRow[mostLikelyRow].Add(stroke);
                }
                else
                {
                    Rule8aCount++;
                    foreach (var strokesInRow in strokeGroupPerRow.Values)
                    {
                        if (strokesInRow.Contains(stroke) &&
                            !strokesInRow.Contains(mostLikelyStroke))
                        {
                            strokesInRow.Remove(stroke);
                        }
                    }
                }
            }

            // Match ungrouped strokes to row with closest stroke
            allGroupedStrokes = strokeGroupPerRow.Values.SelectMany(s => s).ToList();
            foreach (var stroke in ungroupedStrokes)
            {
                var closestStroke = stroke.FindClosestStroke(allGroupedStrokes);
                for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
                {
                    if (strokeGroupPerRow[row].Contains(closestStroke))
                    {
                        Rule9Count++;
                        strokeGroupPerRow[row].Add(stroke);
                    }
                }
            }

            allGroupedStrokes = strokeGroupPerRow.Values.SelectMany(s => s).ToList();
            var numberOfStrokesOverArray = allGroupedStrokes.Count(s => s.WeightedCenter().X < arrayVisualRight);
            var numberOfStrokesRightOfArray = allGroupedStrokes.Count(s => s.WeightedCenter().X >= arrayVisualRight);
            if (numberOfStrokesRightOfArray > numberOfStrokesOverArray)
            {
                var strokesToReject = allGroupedStrokes.Where(s => s.WeightedCenter().X < arrayVisualRight - 5.0).ToList();
                if (strokesToReject.Any())
                {
                    Rule10Count++;
                }
                foreach (var strokesInRow in strokeGroupPerRow.Values)
                {
                    foreach (var stroke in strokesToReject.Where(stroke => strokesInRow.Contains(stroke)))
                    {
                        Rule10RejectedStrokesCount++;
                        strokesInRow.Remove(stroke);
                        rejectedStrokes.Add(stroke);
                    }
                }
            }

            UngroupedStrokesCount += ungroupedStrokes.Distinct().Count();
            RejectedStrokesCount += rejectedStrokes.Distinct().Count();
            SkipStrokesCount += strokeGroupPerRow.Values.SelectMany(s => s).Distinct().Count();
            OverlappingStrokesCount += finalOverlapStrokes.Distinct().Count();

            strokeGroupPerRow.Add(-1, new StrokeCollection(rejectedStrokes.Distinct()));

            #region Debug

            if (isDebugging)
            {
                var sleepTime = 2500;
                page.ClearBoundaries();
                var tempBoundary = new TemporaryBoundary(page, acceptedBoundary.X, acceptedBoundary.Y, acceptedBoundary.Height, acceptedBoundary.Width)
                                   {
                                       RegionText = "Overlapping Strokes"
                                   };
                page.PageObjects.Add(tempBoundary);
                var heightWidths = new Dictionary<Stroke, Point>();
                foreach (var stroke in overlapStrokes)
                {
                    var width = stroke.DrawingAttributes.Width;
                    var height = stroke.DrawingAttributes.Height;
                    heightWidths.Add(stroke, new Point(width, height));

                    stroke.DrawingAttributes.Width = 8;
                    stroke.DrawingAttributes.Height = 8;
                }
                PageHistory.UISleep(sleepTime);
                foreach (var stroke in overlapStrokes)
                {
                    var width = heightWidths[stroke].X;
                    var height = heightWidths[stroke].Y;
                    stroke.DrawingAttributes.Width = width;
                    stroke.DrawingAttributes.Height = height;
                }

                heightWidths.Clear();
                page.ClearBoundaries();

                foreach (var strokeGroupKey in strokeGroupPerRow.Keys)
                {
                    if (strokeGroupKey == -1)
                    {
                        continue;
                    }

                    var strokeGroup = strokeGroupPerRow[strokeGroupKey];

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
        public static List<string> InterpretSkipCountGroups(CLPPage page,
                                                            CLPArray array,
                                                            Dictionary<int, StrokeCollection> strokeGroupPerRow,
                                                            int historyIndex,
                                                            bool isIgnoringInterpretationImprovements = false)
        {
            var interpretedRowValues = new List<string>();
            var allGroupedStrokes = strokeGroupPerRow.Where(kv => kv.Key != -1).SelectMany(kv => kv.Value).ToList();
            var overlappingStrokes = allGroupedStrokes.GroupBy(i => i).Where(i => i.Count() > 1).Select(i => i.Key).ToList();
            if (allGroupedStrokes.Distinct().ToList().Count == overlappingStrokes.Count)
            {
                return interpretedRowValues;
            }

            var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
            for (var row = 1; row <= arrayColumnsAndRows.Y; row++)
            {
                var expectedRowValue = row * (int)arrayColumnsAndRows.X;

                // Generate all subsets of strokes in the current row.
                var strokesInRow = strokeGroupPerRow[row];
                var overlapStrokesInRow = overlappingStrokes.Where(s => strokesInRow.Contains(s)).ToList();
                var nonOverlapStrokesInRow = strokesInRow.Where(s => !overlapStrokesInRow.Contains(s)).ToList();
                var strokeCombinations = new List<List<Stroke>>();
                var subsets = overlapStrokesInRow.SubSetsOf();
                strokeCombinations.AddRange(subsets.Select(subset => nonOverlapStrokesInRow.Concat(subset).ToList()));

                // Get best interpretation from subsets.
                var bestGuess = string.Empty;
                var highestPercentageOfCorrectDigits = 0.0;
                var minLengthDistance = int.MaxValue;
                var closestLengthMatch = string.Empty;
                foreach (var strokeCombination in strokeCombinations)
                {
                    List<string> interpretations;

                    var orderedStrokes = InkSemanticEvents.GetOrderStrokesWereAddedToPage(page, strokeCombination.ToList());
                    interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(orderedStrokes));

                    if (!interpretations.Any())
                    {
                        continue;
                    }

                    if (!isIgnoringInterpretationImprovements)
                    {
                        var actualMatch = InkInterpreter.MatchInterpretationToExpectedInt(interpretations, expectedRowValue);
                        if (!string.IsNullOrEmpty(actualMatch))
                        {
                            bestGuess = actualMatch;
                            break;
                        }
                    }

                    #region Debugging

                    //if (row == 3)
                    //{
                    //    var heightWidths = new Dictionary<Stroke, Point>();
                    //    foreach (var stroke in strokeCombination)
                    //    {
                    //        var width = stroke.DrawingAttributes.Width;
                    //        var height = stroke.DrawingAttributes.Height;
                    //        heightWidths.Add(stroke, new Point(width, height));

                    //        stroke.DrawingAttributes.Width = 8;
                    //        stroke.DrawingAttributes.Height = 8;
                    //    }
                    //    PageHistory.UISleep(2000);
                    //    foreach (var stroke in strokeCombination)
                    //    {
                    //        var width = heightWidths[stroke].X;
                    //        var height = heightWidths[stroke].Y;
                    //        stroke.DrawingAttributes.Width = width;
                    //        stroke.DrawingAttributes.Height = height;
                    //    }
                    //}

                    #endregion // Debugging

                    var guess = InkInterpreter.InterpretationClosestToANumber(interpretations);
                    if (string.IsNullOrEmpty(bestGuess))
                    {
                        bestGuess = guess;
                    }

                    var comparableGuess = guess;
                    var numberOfDigits = guess.Count(char.IsDigit);
                    var numberOfCorrectDigits = 0;
                    foreach (var c in expectedRowValue.ToString())
                    {
                        if (comparableGuess.Contains(c))
                        {
                            numberOfCorrectDigits++;
                            var index = comparableGuess.IndexOf(c);
                            var clippedComparableGuess = comparableGuess.Skip(index + 1).ToList();
                            comparableGuess = !clippedComparableGuess.Any() ? string.Empty : new string(clippedComparableGuess.ToArray());
                        }
                    }

                    var percentageOfCorrectDigits = numberOfCorrectDigits / (numberOfDigits * 1.0);
                    if (percentageOfCorrectDigits > highestPercentageOfCorrectDigits)
                    {
                        highestPercentageOfCorrectDigits = percentageOfCorrectDigits;
                        bestGuess = guess;
                    }

                    var lengthDifference = Math.Abs(guess.Length - expectedRowValue.ToString().Length);
                    if (lengthDifference < minLengthDistance)
                    {
                        minLengthDistance = lengthDifference;
                        closestLengthMatch = guess;
                    }
                }

                if (highestPercentageOfCorrectDigits < 0.1 &&
                    minLengthDistance < int.MaxValue)
                {
                    bestGuess = closestLengthMatch;
                }

                interpretedRowValues.Add(bestGuess);
            }

            return interpretedRowValues;
        }

        public static string FormatInterpretedSkipCountGroups(List<string> interpretedRowValues)
        {
            var formattedSkips = $"\"{string.Join("\" \"", interpretedRowValues)}\"";
            return formattedSkips;
        }

        public static bool IsSkipCounting(List<string> interpretedRowValues)
        {
            var isSkipCounting = true;

            // Rule 0: Passed null value.
            if (interpretedRowValues == null)
            {
                ArraysRule0++;
                return false;
            }

            if (!interpretedRowValues.Any())
            {
                ArraysRule2++;
                return false;
            }

            // Rule 1: Only 1 row in the array.
            if (interpretedRowValues.Count < 2)
            {
                ArraysRule1++;
                return false;
            }

            var nonEmptyInterpretationsCount = interpretedRowValues.Count(s => !string.IsNullOrEmpty(s));
            var numericInterpreationsCount = interpretedRowValues.Count(s => s.All(char.IsDigit) && !string.IsNullOrEmpty(s));

            // Rule 2: Fewer than 2 rows have an interpreted value.
            if (nonEmptyInterpretationsCount < 2)
            {
                ArraysRule2++;
                return false;
            }

            // Rule 3: No rows have an interpreted value that is a number.
            if (numericInterpreationsCount == 0)
            {
                ArraysRule3++;
                //return false;
                isSkipCounting = false;
            }

            // Rule 4: Of the rows with interpreted values, the percentage of those interpreted values with numeric results is less than 34%.
            if (numericInterpreationsCount / (nonEmptyInterpretationsCount * 1.0) < 0.34)
            {
                ArraysRule4++;
                //return false;
                isSkipCounting = false;
            }

            // Rule 5: The first row does not have an interpreted value and only 50% or less of the rows have an interpreted value.
            if (string.IsNullOrEmpty(interpretedRowValues.First()) &&
                nonEmptyInterpretationsCount / (interpretedRowValues.Count * 1.0) <= .5)
            {
                ArraysRule5++;
                //return false;
                isSkipCounting = false;
            }

            // Rule 6: The first 2 rows do not have interpreted values.
            if (string.IsNullOrEmpty(interpretedRowValues[0]) &&
                string.IsNullOrEmpty(interpretedRowValues[1]))
            {
                ArraysRule6++;
                //return false;
                isSkipCounting = false;
            }

            var numberOfSingleGaps = 0;
            for (int i = 0; i < interpretedRowValues.Count; i++)
            {
                if (i == 0 ||
                    i == interpretedRowValues.Count - 1)
                {
                    continue;
                }

                var isGap = !string.IsNullOrEmpty(interpretedRowValues[i - 1]) && string.IsNullOrEmpty(interpretedRowValues[i]) && !string.IsNullOrEmpty(interpretedRowValues[i + 1]);

                if (isGap)
                {
                    numberOfSingleGaps++;
                }
            }

            // Rule 7: This is more than 1 gap of 1 row between interpreted values.
            if (numberOfSingleGaps > 1)
            {
                ArraysRule7++;
                //return false;
                isSkipCounting = false;
            }

            var numberOfDoubleGaps = 0;
            for (int i = 0; i < interpretedRowValues.Count; i++)
            {
                if (i == 0 ||
                    i >= interpretedRowValues.Count - 2)
                {
                    continue;
                }

                var isDoubleGap = !string.IsNullOrEmpty(interpretedRowValues[i - 1]) && string.IsNullOrEmpty(interpretedRowValues[i]) && string.IsNullOrEmpty(interpretedRowValues[i + 1]) &&
                                  !string.IsNullOrEmpty(interpretedRowValues[i + 2]);

                if (isDoubleGap)
                {
                    numberOfDoubleGaps++;
                }
            }

            // Rule 8: There is a gap of more than 1 row between interpreted values.
            if (numberOfDoubleGaps > 0)
            {
                ArraysRule8++;
                //return false;
                isSkipCounting = false;
            }

            // Rule 9: More than 2 rows share the same interpreted value.
            var maxDuplicateCount = interpretedRowValues.Where(s => !string.IsNullOrEmpty(s)).GroupBy(i => i).Select(i => i.Count()).Max();
            if (maxDuplicateCount > 2)
            {
                ArraysRule9++;
                //return false;
                isSkipCounting = false;
            }

            return isSkipCounting;

            //return true;
        }

        private class HeuristicValue
        {
            public enum HeuristicDesignation
            {
                Empty,
                Expected,
                WrongDimension,
                UnknownNumeric,
                PartialNumeric,
                FullText
            }

            public object Value;
            public int Row;
            public HeuristicDesignation Designation;
            public HeuristicDesignation CorrectedDesignation;
            public bool IsFinal;

            public bool IsNumeric
            {
                get { return Designation != HeuristicDesignation.FullText && Designation != HeuristicDesignation.PartialNumeric && Designation != HeuristicDesignation.Empty; }
            }

            public bool IsCorrectedNumeric
            {
                get
                {
                    return CorrectedDesignation != HeuristicDesignation.FullText && CorrectedDesignation != HeuristicDesignation.PartialNumeric && CorrectedDesignation != HeuristicDesignation.Empty;
                }
            }
        }

        public static string Heuristics(List<string> interpretedRowValues, int rows, int columns)
        {
            var heuristicValues = new List<HeuristicValue>();

            // Initial Classification of each interpreted row.
            for (var i = 0; i < interpretedRowValues.Count; i++)
            {
                var expectedValue = columns * (i + 1);
                var wrongDimensionExpectedValue = rows * (i + 1);
                var interpretedValue = interpretedRowValues[i];

                if (string.IsNullOrEmpty(interpretedValue))
                {
                    heuristicValues.Add(new HeuristicValue
                                        {
                                            Value = string.Empty,
                                            Row = i + 1,
                                            Designation = HeuristicValue.HeuristicDesignation.Empty,
                                            CorrectedDesignation = HeuristicValue.HeuristicDesignation.Empty,
                                            IsFinal = true
                                        });

                    continue;
                }

                int numericValue;
                var isNumeric = int.TryParse(interpretedValue, out numericValue);
                if (isNumeric)
                {
                    if (numericValue == expectedValue)
                    {
                        heuristicValues.Add(new HeuristicValue
                                            {
                                                Value = numericValue,
                                                Row = i + 1,
                                                Designation = HeuristicValue.HeuristicDesignation.Expected,
                                                CorrectedDesignation = HeuristicValue.HeuristicDesignation.Expected,
                                                IsFinal = true
                                            });
                    }
                    else if (numericValue == wrongDimensionExpectedValue)
                    {
                        heuristicValues.Add(new HeuristicValue
                                            {
                                                Value = numericValue,
                                                Row = i + 1,
                                                Designation = HeuristicValue.HeuristicDesignation.WrongDimension,
                                                CorrectedDesignation = HeuristicValue.HeuristicDesignation.WrongDimension,
                                                IsFinal = false
                                            });
                    }
                    else
                    {
                        heuristicValues.Add(new HeuristicValue
                                            {
                                                Value = numericValue,
                                                Row = i + 1,
                                                Designation = HeuristicValue.HeuristicDesignation.UnknownNumeric,
                                                CorrectedDesignation = HeuristicValue.HeuristicDesignation.UnknownNumeric,
                                                IsFinal = false
                                            });
                    }
                }
                else
                {
                    if (interpretedValue.Any(char.IsNumber))
                    {
                        heuristicValues.Add(new HeuristicValue
                                            {
                                                Value = interpretedValue,
                                                Row = i + 1,
                                                Designation = HeuristicValue.HeuristicDesignation.PartialNumeric,
                                                CorrectedDesignation = HeuristicValue.HeuristicDesignation.PartialNumeric,
                                                IsFinal = false
                                            });
                    }
                    else
                    {
                        heuristicValues.Add(new HeuristicValue
                                            {
                                                Value = interpretedValue,
                                                Row = i + 1,
                                                Designation = HeuristicValue.HeuristicDesignation.FullText,
                                                CorrectedDesignation = HeuristicValue.HeuristicDesignation.FullText,
                                                IsFinal = false
                                            });
                    }
                }
            }

            // Verify Wrong Dimension Finality
            var wrongDimensionValues = heuristicValues.Where(h => h.Designation == HeuristicValue.HeuristicDesignation.WrongDimension && !h.IsFinal).ToList();
            var percentWrongDimension = wrongDimensionValues.Count / (heuristicValues.Count * 1.0);
            if (percentWrongDimension >= 0.8)
            {
                foreach (var heuristicValue in wrongDimensionValues)
                {
                    heuristicValue.IsFinal = true;
                }
            }

            // Resolve PartialNumeric
            var partialNumericValues = heuristicValues.Where(h => h.Designation == HeuristicValue.HeuristicDesignation.PartialNumeric && !h.IsFinal).ToList();
            foreach (var heuristicValue in partialNumericValues)
            {
                var expectedValue = columns * heuristicValue.Row;
                var wrongDimensionExpectedValue = rows * heuristicValue.Row;
                var isContained = false;
                foreach (var digit in expectedValue.ToString())
                {
                    var val = (string)heuristicValue.Value;
                    if (!val.Contains(digit))
                    {
                        continue;
                    }

                    heuristicValue.Value = expectedValue;
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.Expected;
                    heuristicValue.IsFinal = true;
                    isContained = true;
                    break;
                }

                if (isContained)
                {
                    continue;
                }

                foreach (var digit in wrongDimensionExpectedValue.ToString())
                {
                    var val = (string)heuristicValue.Value;
                    if (!val.Contains(digit))
                    {
                        continue;
                    }

                    heuristicValue.Value = wrongDimensionExpectedValue;
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.WrongDimension;
                    heuristicValue.IsFinal = true;
                    isContained = true;
                    break;
                }
            }

            // Resolve values surrounded by Empties
            // HACK: Shouldn't be necessary if I can get skip count tied to sub arrays
            var orderedHeuristicValues = heuristicValues.OrderBy(h => h.Row).ToList();
            for (var i = 1; i < orderedHeuristicValues.Count - 1; i++)
            {
                var prev = orderedHeuristicValues[i - 1];
                var next = orderedHeuristicValues[i + 1];
                if (prev.Designation == HeuristicValue.HeuristicDesignation.Empty &&
                    next.Designation == HeuristicValue.HeuristicDesignation.Empty)
                {
                    var heuristicValue = orderedHeuristicValues[i];
                    heuristicValue.Value = string.Empty;
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.Empty;
                    heuristicValue.IsFinal = true;
                }
            }

            // Generate Skip Sizes
            var skipSizes = new Dictionary<int, int>();
            for (var i = 1; i < orderedHeuristicValues.Count; i++)
            {
                var prev = orderedHeuristicValues[i - 1];
                var heuristicValue = orderedHeuristicValues[i];

                if (!prev.IsCorrectedNumeric ||
                    !heuristicValue.IsCorrectedNumeric)
                {
                    continue;
                }

                var skipSize = (int)heuristicValue.Value - (int)prev.Value;
                if (!skipSizes.ContainsKey(skipSize))
                {
                    skipSizes.Add(skipSize, 0);
                }
                skipSizes[skipSize]++;
            }

            var dominantSkipSize = skipSizes.OrderByDescending(d => d.Value).First().Key;

            // Convert non-numerics
            // TODO: Clean up, this will probably have to be run twice if there are instances of 3 non-numerics in a row
            for (var i = 1; i < orderedHeuristicValues.Count; i++)
            {
                var heuristicValue = orderedHeuristicValues[i];
                if (heuristicValue.IsCorrectedNumeric ||
                    heuristicValue.IsFinal)
                {
                    continue;
                }

                var prev = orderedHeuristicValues[i - 1];
                if (!prev.IsCorrectedNumeric)
                {
                    // TODO: Shouldn't hit this unless run into necessary above clean up.
                    continue;
                }

                var correctedValue = (int)prev.Value + dominantSkipSize;
                heuristicValue.Value = correctedValue;
                heuristicValue.IsFinal = true;

                var expectedValue = columns * heuristicValue.Row;
                if (correctedValue == expectedValue)
                {
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.Expected;
                    continue;
                }

                var wrongDimensionExpectedValue = rows * heuristicValue.Row;
                if (correctedValue == wrongDimensionExpectedValue)
                {
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.WrongDimension;
                    continue;
                }

                heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.UnknownNumeric;
            }

            // Resolve Unknown Middles
            for (var i = 1; i < orderedHeuristicValues.Count - 1; i++)
            {
                var heuristicValue = orderedHeuristicValues[i];
                if (heuristicValue.IsFinal)
                {
                    continue;
                }

                var prev = orderedHeuristicValues[i - 1];
                var next = orderedHeuristicValues[i + 1];

                var prevExpected = columns * (heuristicValue.Row - 1);
                var nextExpected = columns * (heuristicValue.Row + 1);
                if ((int)prev.Value == prevExpected &&
                    (int)next.Value == nextExpected)
                {
                    var expectedValue = columns * heuristicValue.Row;
                    heuristicValue.Value = expectedValue;
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.Expected;
                    heuristicValue.IsFinal = true;
                    continue;
                }

                var prevWrongDimensionExpected = rows * (heuristicValue.Row - 1);
                var nextWrongDimensionExpected = rows * (heuristicValue.Row + 1);
                if ((int)prev.Value == prevWrongDimensionExpected &&
                    (int)next.Value == nextWrongDimensionExpected)
                {
                    var expectedValue = rows * heuristicValue.Row;
                    heuristicValue.Value = expectedValue;
                    heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.WrongDimension;
                    heuristicValue.IsFinal = true;
                    continue;
                }

                heuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.UnknownNumeric;
                heuristicValue.IsFinal = true;
            }

            // Resolve Firsts
            var firstHeuristicValue = orderedHeuristicValues[0];
            if (!firstHeuristicValue.IsFinal)
            {
                var nextHeuristicValue = orderedHeuristicValues[1];
                var correctedValue = (int)nextHeuristicValue.Value - dominantSkipSize;
                var expectedValue = columns;
                var expectedWrongDimensionValue = rows;
                if (correctedValue == expectedValue)
                {
                    firstHeuristicValue.Value = correctedValue;
                    firstHeuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.Expected;
                    firstHeuristicValue.IsFinal = true;
                }
                else if (correctedValue == expectedWrongDimensionValue)
                {
                    firstHeuristicValue.Value = correctedValue;
                    firstHeuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.WrongDimension;
                    firstHeuristicValue.IsFinal = true;
                }
                else
                {
                    firstHeuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.UnknownNumeric;
                    firstHeuristicValue.IsFinal = true;
                }
            }

            // Resolve Lasts
            var lastHeuristicValue = orderedHeuristicValues[orderedHeuristicValues.Count - 1];
            if (!lastHeuristicValue.IsFinal)
            {
                var prevHeuristicValue = orderedHeuristicValues[orderedHeuristicValues.Count - 2];
                var correctedValue = (int)prevHeuristicValue.Value + dominantSkipSize;
                var expectedValue = columns * rows;
                var expectedWrongDimensionValue = rows * rows;
                if (correctedValue == expectedValue)
                {
                    lastHeuristicValue.Value = correctedValue;
                    lastHeuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.Expected;
                    lastHeuristicValue.IsFinal = true;
                }
                else if (correctedValue == expectedWrongDimensionValue)
                {
                    lastHeuristicValue.Value = correctedValue;
                    lastHeuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.WrongDimension;
                    lastHeuristicValue.IsFinal = true;
                }
                else
                {
                    lastHeuristicValue.CorrectedDesignation = HeuristicValue.HeuristicDesignation.UnknownNumeric;
                    lastHeuristicValue.IsFinal = true;
                }
            }

            // Convert to English Values
            var skips = orderedHeuristicValues.Where(h => h.IsCorrectedNumeric).Select(h => (int)h.Value).ToList();
            var jumpRanges = new List<string>
                             {
                                 string.Join(", ", skips)
                             };
            var currentFirst = -1;
            var currentLast = -1;
            var currentJumpSize = -1;

            for (var i = 1; i < skips.Count; i++)
            {
                var current = skips[i];
                var previous = skips[i - 1];
                var jump = current - previous;

                if (i == 1)
                {
                    currentFirst = previous;
                    currentJumpSize = jump;
                    currentLast = current;
                    continue;
                }

                if (currentJumpSize == jump)
                {
                    currentLast = current;
                }
                else
                {
                    var jumpRange = string.Format("By {0}, from {1} to {2}", currentJumpSize, currentFirst, currentLast);
                    jumpRanges.Add(jumpRange);

                    currentFirst = previous;
                    currentLast = current;
                    currentJumpSize = jump;
                }

                if (i == skips.Count - 1)
                {
                    var jumpRange = string.Format("By {0}, from {1} to {2}", currentJumpSize, currentFirst, currentLast);
                    jumpRanges.Add(jumpRange);
                }
            }

            // Refresh Dominant Skip Size.
            skipSizes.Clear();
            for (var i = 1; i < orderedHeuristicValues.Count; i++)
            {
                var prev = orderedHeuristicValues[i - 1];
                var heuristicValue = orderedHeuristicValues[i];

                if (!prev.IsCorrectedNumeric ||
                    !heuristicValue.IsCorrectedNumeric)
                {
                    continue;
                }

                var skipSize = (int)heuristicValue.Value - (int)prev.Value;
                if (!skipSizes.ContainsKey(skipSize))
                {
                    skipSizes.Add(skipSize, 0);
                }
                skipSizes[skipSize]++;
            }

            dominantSkipSize = skipSizes.OrderByDescending(d => d.Value).First().Key;
            if (dominantSkipSize == rows &&
                rows != columns)
            {
                jumpRanges.Add("\t- Skip Counted by Wrong Dimension");
            }

            if (skipSizes.Count > 1 &&
                skipSizes.ContainsKey(columns))
            {
                jumpRanges.Add("\t- Likely arithmetic error");
            }

            var result = string.Join("\n", jumpRanges);
            return result;
        }

        #region BottomSkipCounting

        public static List<Stroke> GroupPossibleBottomSkipCountStrokes(CLPArray array, List<Stroke> strokes, int historyIndex)
        {
            const double TOP_OF_VISUAL_BOTTOM_THRESHOLD = 45.0;
            const double BOTTOM_OF_VISUAL_BOTTOM_THRESHOLD = 51.5;

            var arrayPosition = array.GetPositionAtHistoryIndex(historyIndex);
            var arrayDimensions = array.GetDimensionsAtHistoryIndex(historyIndex);
            var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
            var arrayVisualBottom = arrayPosition.Y + arrayDimensions.Y - array.LabelLength;
            var arrayVisualLeft = arrayPosition.X + array.LabelLength;
            var halfGridSquareSize = array.GridSquareSize * 0.5;

            var skipCountStrokes = new List<Stroke>();
            var acceptedBoundary = new Rect(arrayVisualLeft - halfGridSquareSize,
                                            arrayVisualBottom - TOP_OF_VISUAL_BOTTOM_THRESHOLD,
                                            array.GridSquareSize * (arrayColumnsAndRows.X + 1),
                                            BOTTOM_OF_VISUAL_BOTTOM_THRESHOLD + TOP_OF_VISUAL_BOTTOM_THRESHOLD);

            foreach (var stroke in strokes)
            {
                // Rule 1: Rejected for being invisibly small.
                if (stroke.IsInvisiblySmall())
                {
                    continue;
                }

                var strokeBounds = stroke.GetBounds();

                // Rule 3: Rejected for being outside the accepted skip counting bounds
                var intersect = Rect.Intersect(strokeBounds, acceptedBoundary);
                if (intersect.IsEmpty)
                {
                    continue;
                }

                var intersectPercentage = intersect.Area() / strokeBounds.Area();
                if (intersectPercentage <= 0.50)
                {
                    continue;
                }

                if (intersectPercentage <= 0.90)
                {
                    var weightedCenterY = stroke.WeightedCenter().Y;
                    if (weightedCenterY < arrayVisualBottom - TOP_OF_VISUAL_BOTTOM_THRESHOLD ||
                        weightedCenterY > arrayVisualBottom + BOTTOM_OF_VISUAL_BOTTOM_THRESHOLD)
                    {
                        continue;
                    }
                }

                skipCountStrokes.Add(stroke);
            }

            return skipCountStrokes;
        }

        public static string InterpretBottomSkipCountStrokes(List<Stroke> strokes)
        {
            // No strokes at all inside acceptable boundary
            if (!strokes.Any())
            {
                return string.Empty;
            }

            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(strokes));
            var guess = InkInterpreter.InterpretationClosestToANumber(interpretations);

            return guess;
        }

        public static bool IsBottomSkipCounting(CLPArray array, string interpretation)
        {
            if (string.IsNullOrEmpty(interpretation))
            {
                return false;
            }


            return IsBottomSkipCountingByCorrectDimension(array, interpretation) || IsBottomSkipCountingByWrongDimension(array, interpretation);
        }

        public static bool IsBottomSkipCountingByCorrectDimension(CLPArray array, string interpretation)
        {
            if (string.IsNullOrEmpty(interpretation))
            {
                return false;
            }

            var expectedValue = string.Empty;
            for (var i = 1; i <= array.Columns; i++)
            {
                expectedValue += i * array.Rows;
            }

            var editDistance = EditDistance.Compute(expectedValue, interpretation);

            return editDistance <= 4;
        }

        public static bool IsBottomSkipCountingByWrongDimension(CLPArray array, string interpretation)
        {
            if (string.IsNullOrEmpty(interpretation))
            {
                return false;
            }

            var expectedValueForWrongDimension = string.Empty;
            for (var i = 1; i <= array.Columns; i++)
            {
                expectedValueForWrongDimension += i * array.Columns;
            }

            var wrongDimensionEditDistance = EditDistance.Compute(expectedValueForWrongDimension, interpretation);

            return wrongDimensionEditDistance <= 4;
        }

        #endregion // BottomSkipCounting

        #endregion // Utility

        #region Static Analysis

        public static string StaticSkipCountAnalysis(CLPPage page, CLPArray array, bool isDebugging = false)
        {
            var historyIndex = 0;
            var lastHistoryAction = page.History.CompleteOrderedHistoryActions.LastOrDefault();
            if (lastHistoryAction != null)
            {
                historyIndex = lastHistoryAction.HistoryActionIndex;
            }

            var strokes = page.InkStrokes.ToList();

            var strokeGroupPerRow = GroupPossibleSkipCountStrokes(page, array, strokes, historyIndex, isDebugging);
            var interpretedRowValues = InterpretSkipCountGroups(page, array, strokeGroupPerRow, historyIndex);
            var isSkipCounting = IsSkipCounting(interpretedRowValues);

            return isSkipCounting ? FormatInterpretedSkipCountGroups(interpretedRowValues) : string.Empty;
        }

        public static string StaticBottomSkipCountAnalysis(CLPPage page, CLPArray array, bool isDebugging = false)
        {
            var historyIndex = 0;
            var lastHistoryAction = page.History.CompleteOrderedHistoryActions.LastOrDefault();
            if (lastHistoryAction != null)
            {
                historyIndex = lastHistoryAction.HistoryActionIndex;
            }

            var strokes = page.InkStrokes.ToList();

            const double TOP_OF_VISUAL_BOTTOM_THRESHOLD = 45.0;
            const double BOTTOM_OF_VISUAL_BOTTOM_THRESHOLD = 51.5;

            var arrayPosition = array.GetPositionAtHistoryIndex(historyIndex);
            var arrayDimensions = array.GetDimensionsAtHistoryIndex(historyIndex);
            var arrayColumnsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
            var arrayVisualBottom = arrayPosition.Y + arrayDimensions.Y - array.LabelLength;
            var arrayVisualLeft = arrayPosition.X + array.LabelLength;
            var halfGridSquareSize = array.GridSquareSize * 0.5;

            var skipCountStrokes = new List<Stroke>();
            var acceptedBoundary = new Rect(arrayVisualLeft - halfGridSquareSize,
                                            arrayVisualBottom - TOP_OF_VISUAL_BOTTOM_THRESHOLD,
                                            array.GridSquareSize * (arrayColumnsAndRows.X + 1),
                                            BOTTOM_OF_VISUAL_BOTTOM_THRESHOLD + TOP_OF_VISUAL_BOTTOM_THRESHOLD);

            foreach (var stroke in strokes)
            {
                // Rule 1: Rejected for being invisibly small.
                if (stroke.IsInvisiblySmall())
                {
                    continue;
                }

                var strokeBounds = stroke.GetBounds();

                // Rule 3: Rejected for being outside the accepted skip counting bounds
                var intersect = Rect.Intersect(strokeBounds, acceptedBoundary);
                if (intersect.IsEmpty)
                {
                    continue;
                }

                var intersectPercentage = intersect.Area() / strokeBounds.Area();
                if (intersectPercentage <= 0.50)
                {
                    continue;
                }

                if (intersectPercentage <= 0.90)
                {
                    var weightedCenterY = stroke.WeightedCenter().Y;
                    if (weightedCenterY < arrayVisualBottom - TOP_OF_VISUAL_BOTTOM_THRESHOLD ||
                        weightedCenterY > arrayVisualBottom + BOTTOM_OF_VISUAL_BOTTOM_THRESHOLD)
                    {
                        continue;
                    }
                }

                skipCountStrokes.Add(stroke);
            }

            // No strokes at all inside acceptable boundary
            if (!skipCountStrokes.Any())
            {
                return string.Empty;
            }

            var interpretations = InkInterpreter.StrokesToAllGuessesText(new StrokeCollection(skipCountStrokes));
            var guess = InkInterpreter.InterpretationClosestToANumber(interpretations);

            return guess;
        }

        #endregion // Static Analysis

        #region Logging

        public static int Rule1Count = 0;
        public static int Rule2Count = 0;
        public static int Rule3aCount = 0;
        public static int Rule3bCount = 0;
        public static int Rule3cCount = 0;
        public static int Rule4Count = 0;
        public static int Rule5Count = 0;
        public static int Rule6Count = 0;
        public static int Rule7cCount = 0;
        public static int Rule7dCount = 0;
        public static int Rule8aCount = 0;
        public static int Rule8bCount = 0;
        public static int Rule8cCount = 0;
        public static int Rule9Count = 0;
        public static int Rule10Count = 0;
        public static int Rule10RejectedStrokesCount = 0;
        public static int RejectedStrokesCount = 0;
        public static int SkipStrokesCount = 0;
        public static int UngroupedStrokesCount = 0;
        public static int OverlappingStrokesCount = 0;
        public static int AllStrokesAreOutsideOfAcceptableBoundary = 0;

        public static int ArraysRule0 = 0;
        public static int ArraysRule1 = 0;
        public static int ArraysRule2 = 0;
        public static int ArraysRule3 = 0;
        public static int ArraysRule4 = 0;
        public static int ArraysRule5 = 0;
        public static int ArraysRule6 = 0;
        public static int ArraysRule7 = 0;
        public static int ArraysRule8 = 0;
        public static int ArraysRule9 = 0;

        #endregion // Logging
    }
}