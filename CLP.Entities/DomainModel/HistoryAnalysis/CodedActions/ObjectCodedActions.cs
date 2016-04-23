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

            if (addedPageObjects.Count == 1)
            {
                var historyIndex = objectsOnPageChangedHistoryItem.HistoryIndex;
                var pageObject = addedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItem)
                                    {
                                        CodedObject = codedObject,
                                        CodedObjectAction = Codings.ACTION_OBJECT_ADD,
                                        IsObjectActionVisible = false,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = SetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                        ReferencePageObjectID = pageObject.ID
                                    };

                return historyAction;
            }
            else
            {
                // HACK
                var historyIndex = objectsOnPageChangedHistoryItem.HistoryIndex;
                var pageObject = addedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItem)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_OBJECT_ADD,
                    IsObjectActionVisible = false,
                    CodedObjectID = codedObjectID,
                    CodedObjectIDIncrement = SetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID)
                };

                return historyAction;
            }

            // TODO: deal with multiple pageObjects added at once (create multiple arrays at the same time)
            // special case for Bins
            return null;
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
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItem)
                                    {
                                        CodedObject = codedObject,
                                        CodedObjectAction = Codings.ACTION_OBJECT_DELETE,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                        ReferencePageObjectID = pageObject.ID
                };

                return historyAction;
            }
            else
            {
                // HACK
                var historyIndex = objectsOnPageChangedHistoryItem.HistoryIndex;
                var pageObject = removedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var historyAction = new HistoryAction(page, objectsOnPageChangedHistoryItem)
                {
                    CodedObject = codedObject,
                    CodedObjectAction = Codings.ACTION_OBJECT_DELETE,
                    CodedObjectID = codedObjectID,
                    CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID)
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

            if (movedPageObjects.Count != 1)
            {
                return null;
            }

            var historyIndex = objectsMovedHistoryItems.First().HistoryIndex;
            var pageObject = movedPageObjects.First();
            var codedObject = pageObject.CodedName;
            var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
            var historyAction = new HistoryAction(page, objectsMovedHistoryItems.Cast<IHistoryItem>().ToList())
                                {
                                    CodedObject = codedObject,
                                    CodedObjectAction = Codings.ACTION_OBJECT_MOVE,
                                    CodedObjectID = codedObjectID,
                                    CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                    CodedObjectActionID =
                                        string.Format("({0}, {1}) to ({2}, {3})",
                                                      Math.Round(objectsMovedHistoryItems.First().TravelledPositions.First().X),
                                                      Math.Round(objectsMovedHistoryItems.First().TravelledPositions.First().Y),
                                                      Math.Round(objectsMovedHistoryItems.Last().TravelledPositions.Last().X),
                                                      Math.Round(objectsMovedHistoryItems.Last().TravelledPositions.Last().Y)),
                                    ReferencePageObjectID = pageObject.ID
                                    // TODO: make coded action to describe distance travelled during move.
                                    // note that there may be more than one objectsMovedHistoryItem in a row where
                                    // a student moved the same pageObject several consecutive times.
                                };

            return historyAction;
        }

        public static IHistoryAction Resize(CLPPage page, List<PageObjectResizeBatchHistoryItem> objectsResizedHistoryItems)
        {
            if (page == null ||
                objectsResizedHistoryItems == null ||
                !objectsResizedHistoryItems.Any())
            {
                return null;
            }

            var pageObjectID = objectsResizedHistoryItems.First().PageObjectID;
            var pageObject = page.GetPageObjectByIDOnPageOrInHistory(pageObjectID);
            if (pageObject == null)
            {
                return null;
            }

            var historyIndex = objectsResizedHistoryItems.First().HistoryIndex;
            var codedObject = pageObject.CodedName;
            var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
            var historyAction = new HistoryAction(page, objectsResizedHistoryItems.Cast<IHistoryItem>().ToList())
            {
                CodedObject = codedObject,
                CodedObjectAction = Codings.ACTION_OBJECT_RESIZE,
                CodedObjectID = codedObjectID,
                CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                CodedObjectActionID =string.Format("({0}, {1}) to ({2}, {3})",
                                                      Math.Round(objectsResizedHistoryItems.First().StretchedDimensions.First().X),
                                                      Math.Round(objectsResizedHistoryItems.First().StretchedDimensions.First().Y),
                                                      Math.Round(objectsResizedHistoryItems.Last().StretchedDimensions.Last().X),
                                                      Math.Round(objectsResizedHistoryItems.Last().StretchedDimensions.Last().Y)),
                ReferencePageObjectID = pageObjectID
            };

            return historyAction;
        }

        #endregion // Verify And Generate Methods

        #region Utility Methods

        private static readonly Dictionary<string, int> CurrentHighestIncrementIDsForCodedObjectAndID = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> CurrentIncrementIDForPageObject = new Dictionary<string, int>();

        public static void InitializeIncrementIDs()
        {
            CurrentHighestIncrementIDsForCodedObjectAndID.Clear();
            CurrentIncrementIDForPageObject.Clear();
        }

        public static string GetCurrentIncrementIDForPageObject(string pageObjectID, string codedObject, string codedID)
        {
            var compoundID = string.Format("{0};{1};{2}", pageObjectID, codedObject, codedID);

            return CurrentIncrementIDForPageObject[compoundID].ToLetter();
        }

        public static string SetCurrentIncrementIDForPageObject(string pageObjectID, string codedObject, string codedID, bool isEraseEvent = false)
        {
            var objectAndID = string.Format("{0};{1}", codedObject, codedID);
            var compoundID = string.Format("{0};{1};{2}", pageObjectID, codedObject, codedID);
            if (!CurrentHighestIncrementIDsForCodedObjectAndID.ContainsKey(objectAndID))
            {
                CurrentHighestIncrementIDsForCodedObjectAndID.Add(objectAndID, 0);
                CurrentIncrementIDForPageObject[compoundID] = CurrentHighestIncrementIDsForCodedObjectAndID[objectAndID];
            }
            else if (!isEraseEvent)
            {
                CurrentHighestIncrementIDsForCodedObjectAndID[objectAndID]++;
                CurrentIncrementIDForPageObject[compoundID] = CurrentHighestIncrementIDsForCodedObjectAndID[objectAndID];
            }

            return CurrentIncrementIDForPageObject[compoundID].ToLetter();
        }

        public static string GetCurrentIncrementIDForPageObject_Sub(string pageObjectID, string codedObject, string codedID, int subPosition, string subID)
        {
            var compoundID = string.Format("{0};{1};{2};{3};{4}", pageObjectID, codedObject, codedID, subPosition, subID);

            return CurrentIncrementIDForPageObject[compoundID].ToLetter();
        }

        public static string SetCurrentIncrementIDForPageObject_Sub(string pageObjectID, string codedObject, string codedID, int subPosition, string subID, bool isEraseEvent = false)
        {
            var objectAndID = string.Format("{0};{1}", codedObject, subID);
            var compoundID = string.Format("{0};{1};{2};{3};{4}", pageObjectID, codedObject, codedID, subPosition, subID);
            if (!CurrentHighestIncrementIDsForCodedObjectAndID.ContainsKey(objectAndID))
            {
                CurrentHighestIncrementIDsForCodedObjectAndID.Add(objectAndID, 0);
                CurrentIncrementIDForPageObject[compoundID] = CurrentHighestIncrementIDsForCodedObjectAndID[objectAndID];
            }
            else if (!isEraseEvent)
            {
                CurrentHighestIncrementIDsForCodedObjectAndID[objectAndID]++;
                CurrentIncrementIDForPageObject[compoundID] = CurrentHighestIncrementIDsForCodedObjectAndID[objectAndID];
            }

            return CurrentIncrementIDForPageObject[compoundID].ToLetter();
        }

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

        #endregion // Utility Methods
    }
}