using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;

namespace CLP.Entities
{
    public static class ObjectSemanticEvents
    {
        #region Verify And Generate Methods

        public static ISemanticEvent Add(CLPPage page, ObjectsOnPageChangedHistoryAction objectsOnPageChangedHistoryAction)
        {
            if (page == null ||
                objectsOnPageChangedHistoryAction == null ||
                !objectsOnPageChangedHistoryAction.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryAction.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryAction.IsUsingStrokes)
            {
                return null;
            }

            var addedPageObjects = objectsOnPageChangedHistoryAction.PageObjectsAdded;

            if (addedPageObjects.Count == 1)
            {
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
                var pageObject = addedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = Codings.EVENT_OBJECT_ADD,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = SetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                        ReferencePageObjectID = pageObject.ID
                                    };

                return semanticEvent;
            }
            else
            {
                // HACK
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
                var pageObject = addedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = Codings.EVENT_OBJECT_ADD,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = SetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID)
                                    };

                return semanticEvent;
            }

            // TODO: deal with multiple pageObjects added at once (create multiple arrays at the same time)
            // special case for Bins
            //return null;
        }

        public static ISemanticEvent Delete(CLPPage page, ObjectsOnPageChangedHistoryAction objectsOnPageChangedHistoryAction)
        {
            if (page == null ||
                objectsOnPageChangedHistoryAction == null ||
                !objectsOnPageChangedHistoryAction.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryAction.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryAction.IsUsingStrokes)
            {
                return null;
            }

            var removedPageObjects = objectsOnPageChangedHistoryAction.PageObjectsRemoved;
            if (removedPageObjects.Count == 1)
            {
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
                var pageObject = removedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = Codings.EVENT_OBJECT_DELETE,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                        ReferencePageObjectID = pageObject.ID
                                    };

                return semanticEvent;
            }
            else
            {
                // HACK
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
                var pageObject = removedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = Codings.EVENT_OBJECT_DELETE,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID)
                                    };

                return semanticEvent;
            }

            // TODO: deal with multiple pageObjects deleted at once (lasso?)
            // special case for Bins
            //return null;
        }

        public static ISemanticEvent Move(CLPPage page, List<ObjectsMovedBatchHistoryAction> objectsMovedHistoryActions)
        {
            if (page == null ||
                objectsMovedHistoryActions == null ||
                !objectsMovedHistoryActions.Any())
            {
                return null;
            }

            var movedPageObjects = GetMovedPageObjects(page, objectsMovedHistoryActions);
            var movedStrokes = GetMovedStrokes(page, objectsMovedHistoryActions);

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

            var historyIndex = objectsMovedHistoryActions.First().HistoryActionIndex;
            var pageObject = movedPageObjects.First();
            var codedObject = pageObject.CodedName;
            var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
            var semanticEvent = new SemanticEvent(page, objectsMovedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = Codings.EVENT_OBJECT_MOVE,
                                    CodedObjectID = codedObjectID,
                                    CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                    EventInformation =
                                        string.Format("({0}, {1}) to ({2}, {3})",
                                                      Math.Round(objectsMovedHistoryActions.First().TravelledPositions.First().X),
                                                      Math.Round(objectsMovedHistoryActions.First().TravelledPositions.First().Y),
                                                      Math.Round(objectsMovedHistoryActions.Last().TravelledPositions.Last().X),
                                                      Math.Round(objectsMovedHistoryActions.Last().TravelledPositions.Last().Y)),
                                    ReferencePageObjectID = pageObject.ID
                                    // TODO: make eventInfo to describe distance travelled during move.
                                    // note that there may be more than one objectsMovedHistoryAction in a row where
                                    // a student moved the same pageObject several consecutive times.
                                };

            return semanticEvent;
        }

        public static ISemanticEvent Resize(CLPPage page, List<PageObjectResizeBatchHistoryAction> objectsResizedHistoryActions)
        {
            if (page == null ||
                objectsResizedHistoryActions == null ||
                !objectsResizedHistoryActions.Any())
            {
                return null;
            }

            var pageObjectID = objectsResizedHistoryActions.First().PageObjectID;
            var pageObject = page.GetPageObjectByIDOnPageOrInHistory(pageObjectID);
            if (pageObject == null)
            {
                return null;
            }

            var historyIndex = objectsResizedHistoryActions.First().HistoryActionIndex;
            var codedObject = pageObject.CodedName;
            var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
            var semanticEvent = new SemanticEvent(page, objectsResizedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = Codings.EVENT_OBJECT_RESIZE,
                                    CodedObjectID = codedObjectID,
                                    CodedObjectIDIncrement = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID),
                                    EventInformation =
                                        string.Format("({0}, {1}) to ({2}, {3})",
                                                      Math.Round(objectsResizedHistoryActions.First().StretchedDimensions.First().X),
                                                      Math.Round(objectsResizedHistoryActions.First().StretchedDimensions.First().Y),
                                                      Math.Round(objectsResizedHistoryActions.Last().StretchedDimensions.Last().X),
                                                      Math.Round(objectsResizedHistoryActions.Last().StretchedDimensions.Last().Y)),
                                    ReferencePageObjectID = pageObjectID
                                };

            return semanticEvent;
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

            if (!CurrentIncrementIDForPageObject.ContainsKey(compoundID))
            {
                SetCurrentIncrementIDForPageObject(pageObjectID, codedObject, codedID);
            }

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

        public static List<IPageObject> GetMovedPageObjects(CLPPage page, List<ObjectsMovedBatchHistoryAction> historyActions)
        {
            return historyActions.SelectMany(h => h.PageObjectIDs.Keys.Select(page.GetPageObjectByIDOnPageOrInHistory)).Distinct().ToList();
        }

        public static List<Stroke> GetMovedStrokes(CLPPage page, List<ObjectsMovedBatchHistoryAction> historyActions)
        {
            return historyActions.SelectMany(h => h.StrokeIDs.Keys.Select(page.GetStrokeByIDOnPageOrInHistory)).Distinct().ToList();
        }

        public static List<IPageObject> GetResizedPageObjects(CLPPage page, List<IHistoryAction> historyActions)
        {
            return historyActions.OfType<PageObjectResizeBatchHistoryAction>().Select(h => page.GetPageObjectByIDOnPageOrInHistory(h.PageObjectID)).ToList();
        }

        #endregion // Utility Methods
    }
}