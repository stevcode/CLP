using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace CLP.Entities
{
    public static class ObjectCodedActions
    {
        #region Verify And Generate Methods

        public static IHistoryAction Add(CLPPage page, ObjectsOnPageChangedHistoryItem objectsOnPageChangedHistoryItem)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItem == null ||
                !objectsOnPageChangedHistoryItem.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryItem.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryItem.IsUsingStrokes)
            {
                return null;
            }

            var addedPageObjects = objectsOnPageChangedHistoryItem.PageObjectsAdded;
            var historyIndex = objectsOnPageChangedHistoryItem.HistoryIndex;

            if (addedPageObjects.Count == 1)
            {
                var pageObject = addedPageObjects.First();
                if (pageObject is Bin || pageObject is Mark)
                {
                    return null;
                }
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItem)
                                    {
                                        CodedObject = codedObject,
                                        CodedObjectAction = Codings.ACTION_OBJECT_ADD,
                                        IsObjectActionVisible = false,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = HistoryAction.IncrementAndGetIncrementID(pageObject.ID, codedObject, codedObjectID)
                                    };

                return historyAction;
            }
            return null;
        }

        public static IHistoryAction DeleteBins(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItems == null ||
                objectsOnPageChangedHistoryItems.Any(h => h.IsUsingStrokes && !h.PageObjectIDsRemoved.Any()))
            {
                return null;
            }
            var removedPageObjects = objectsOnPageChangedHistoryItems.First().PageObjectsRemoved;
            var pageObject = (removedPageObjects.First() is Bin) ? removedPageObjects.First() : removedPageObjects.Last();
            var codedObjectID = objectsOnPageChangedHistoryItems.Count.ToString();
            var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItems.Cast<IHistoryItem>().ToList()) //use first object
            {
                CodedObject = Codings.OBJECT_BINS,
                CodedObjectAction = Codings.ACTION_OBJECT_DELETE,
                CodedObjectID = codedObjectID,
                CodedObjectIDIncrement = HistoryAction.IncrementAndGetIncrementID(pageObject.ID, Codings.OBJECT_BINS, codedObjectID)
            };
            return historyAction;
        }

        public static IHistoryAction AddBins(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItems == null ||
                objectsOnPageChangedHistoryItems.Any(h => h.IsUsingStrokes && !h.PageObjectIDsAdded.Any()))
            {
                return null;
            }
            var addedPageObjects = objectsOnPageChangedHistoryItems.First().PageObjectsAdded;
            var pageObject = (addedPageObjects.First() is Bin) ? addedPageObjects.First() : addedPageObjects.Last();
            var codedObjectID = objectsOnPageChangedHistoryItems.Count.ToString();
            var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItems.Cast<IHistoryItem>().ToList()) //use first object
            {
                CodedObject = Codings.OBJECT_BINS,
                CodedObjectAction = Codings.ACTION_OBJECT_ADD,
                IsObjectActionVisible = false,
                CodedObjectID = codedObjectID,
                CodedObjectIDIncrement = HistoryAction.IncrementAndGetIncrementID(pageObject.ID, Codings.OBJECT_BINS, codedObjectID)
            };
            return historyAction;
        }

        public static IHistoryAction AddOrDeleteMarks(CLPPage page, List<ObjectsOnPageChangedHistoryItem> objectsOnPageChangedHistoryItems)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItems == null ||
                objectsOnPageChangedHistoryItems.Any(h => h.IsUsingStrokes && !h.PageObjectIDsAdded.Any()))
            {
                return null;
            }
            var pageObjects = objectsOnPageChangedHistoryItems.First().PageObjectsAdded.Any() ? objectsOnPageChangedHistoryItems.First().PageObjectsAdded :
                objectsOnPageChangedHistoryItems.First().PageObjectsRemoved;
            var pageObject = pageObjects.First();
            var addedMark = pageObject as Mark;
            var whichBin = Mark.IsInWhichBin(page, objectsOnPageChangedHistoryItems.First().HistoryIndex, addedMark);
            if (!(whichBin == "OUT"))
            {
                whichBin = "Bin " + whichBin;
            }
            var whichBinWithHeader = "";
            var colorAsString = addedMark.MarkColor;
            var codedObjectID = objectsOnPageChangedHistoryItems.Count.ToString();
            var codedObjectActionID = whichBin + ", " +
                colorAsString + ", " + addedMark.MarkShape.ToString();
            var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItems.Cast<IHistoryItem>().ToList()) //use first object
            {
                CodedObject = Codings.OBJECT_MARK,
                CodedObjectAction = objectsOnPageChangedHistoryItems.First().PageObjectsAdded.Any() ? Codings.ACTION_OBJECT_ADD : Codings.ACTION_OBJECT_DELETE,
                IsObjectActionVisible = !objectsOnPageChangedHistoryItems.First().PageObjectsAdded.Any(),
                CodedObjectID = codedObjectID,
                CodedObjectActionID = codedObjectActionID
            };
            return historyAction;
        }

        public static IHistoryAction Delete(CLPPage page, ObjectsOnPageChangedHistoryItem objectsOnPageChangedHistoryItem)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItem == null ||
                !objectsOnPageChangedHistoryItem.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryItem.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryItem.IsUsingStrokes)
            {
                return null;
            }

            var removedPageObjects = objectsOnPageChangedHistoryItem.PageObjectsRemoved;
            if (removedPageObjects.Count == 1)
            {
                var historyIndex = objectsOnPageChangedHistoryItem.HistoryIndex;
                var pageObject = removedPageObjects.First();
                if (pageObject is Bin || pageObject is Mark)
                {
                    return null;
                }
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItem)
                                    {
                                        CodedObject = codedObject,
                                        CodedObjectAction = Codings.ACTION_OBJECT_DELETE,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = HistoryAction.GetIncrementID(pageObject.ID, codedObject, codedObjectID)
                                    };

                return historyAction;
            }

            // TODO: deal with multiple pageObjects deleted at once (lasso?)
            // special case for Bins
            return null;
        }

        public static IHistoryAction Move(CLPPage page, List<ObjectsMovedBatchHistoryItem> objectsMovedHistoryItems)
        {
            if (page == null ||
                objectsMovedHistoryItems == null ||
                !objectsMovedHistoryItems.Any())
            {
                return null;
            }

            var movedPageObjects = GetMovedPageObjects(page, objectsMovedHistoryItems);
            var movedStrokes = GetMovedStrokes(page, objectsMovedHistoryItems);

            if (movedPageObjects.Count > 1 ||
                movedStrokes.Any()) // Lasso move
            {
                // TODO
                return null;
            }

            if (movedPageObjects.Count == 1)
            {
                var historyIndex = objectsMovedHistoryItems.First().HistoryIndex;
                var pageObject = movedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var historyAction = new HistoryAction(page, objectsMovedHistoryItems.Cast<IHistoryItem>().ToList())
                                    {
                                        CodedObject = codedObject,
                                        CodedObjectAction = Codings.ACTION_OBJECT_MOVE,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = HistoryAction.GetIncrementID(pageObject.ID, codedObject, codedObjectID),
                                        CodedObjectActionID =
                                            string.Format("({0}, {1}) to ({2}, {3})",
                                                          Math.Round(objectsMovedHistoryItems.First().TravelledPositions.First().X),
                                                          Math.Round(objectsMovedHistoryItems.First().TravelledPositions.First().Y),
                                                          Math.Round(objectsMovedHistoryItems.Last().TravelledPositions.Last().X),
                                                          Math.Round(objectsMovedHistoryItems.Last().TravelledPositions.Last().Y))
                                        // TODO: make coded action to describe distance travelled during move.
                                        // note that there may be more than one objectsMovedHistoryItem in a row where
                                        // a student moved the same pageObject several consecutive times.
                                    };

                return historyAction;
            }

            return null;
        }

        #endregion // Verify And Generate Methods

        #region Utility Methods

        public static List<IPageObject> GetMovedPageObjects(CLPPage page, List<ObjectsMovedBatchHistoryItem> historyItems)
        {
            return historyItems.SelectMany(h => h.PageObjectIDs.Keys.Select(page.GetPageObjectByIDOnPageOrInHistory)).Distinct().ToList();
        }

        public static List<Stroke> GetMovedStrokes(CLPPage page, List<ObjectsMovedBatchHistoryItem> historyItems)
        {
            return historyItems.SelectMany(h => h.StrokeIDs.Keys.Select(page.GetStrokeByIDOnPageOrInHistory)).Distinct().ToList();
        }

        public static List<IPageObject> GetResizedPageObjects(CLPPage page, List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<PageObjectResizeBatchHistoryItem>().Select(h => page.GetPageObjectByIDOnPageOrInHistory(h.PageObjectID)).ToList();
        }

        public static List<IPageObject> GetPageObjectsOnPageAtHistoryIndex(CLPPage page, int historyIndex, bool isIncludingMC = false)
        {
            if (isIncludingMC)
            {
                return page.PageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex)).ToList();
            }

            return page.PageObjects.Where(p => p.IsOnPageAtHistoryIndex(historyIndex) && !(p is MultipleChoiceBox)).ToList();
        }

        public static Rect GetPageObjectBoundsAtHistoryIndex(CLPPage page, IPageObject pageObject, int historyIndex)
        {
            var position = pageObject.GetPositionAtHistoryIndex(historyIndex);
            var dimensions = pageObject.GetDimensionsAtHistoryIndex(historyIndex);
            return new Rect(position.X, position.Y, dimensions.X, dimensions.Y);
        }

        #endregion // Utility Methods
    }
}