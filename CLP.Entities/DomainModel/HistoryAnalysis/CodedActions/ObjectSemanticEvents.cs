using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using Catel;

namespace CLP.Entities
{
    public static class ObjectSemanticEvents
    {
        #region Verify And Generate Methods

        public static ISemanticEvent Add(CLPPage page, ObjectsOnPageChangedHistoryAction objectsOnPageChangedHistoryAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(objectsOnPageChangedHistoryAction), objectsOnPageChangedHistoryAction);

            if (!objectsOnPageChangedHistoryAction.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryAction.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryAction.IsUsingStrokes)
            {
                return null;
            }

            var addedPageObjects = objectsOnPageChangedHistoryAction.PageObjectsAdded;

            if (addedPageObjects.Count == 1)
            {
                var pageObject = addedPageObjects.First();

                var codedObject = pageObject.CodedName;
                var eventType = Codings.EVENT_OBJECT_ADD;
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                var incrementID = SetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID);

                var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = eventType,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = incrementID,
                                        ReferencePageObjectID = pageObject.ID
                                    };

                return semanticEvent;
            }
            else
            {
                // HACK - To fix, create multiple SemEvents, eventType = "multi add", then in a later pass, combine all multi-adds into a single multi-add Event
                // that is composed of these sequential multi-adds. This keeps ReferencePageObjectIDs intact.
                var pageObject = addedPageObjects.First();
                var codedObject = pageObject.CodedName;
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
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
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(objectsOnPageChangedHistoryAction), objectsOnPageChangedHistoryAction);

            if (!objectsOnPageChangedHistoryAction.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryAction.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryAction.IsUsingStrokes)
            {
                return null;
            }

            var removedPageObjects = objectsOnPageChangedHistoryAction.PageObjectsRemoved;
            if (removedPageObjects.Count == 1)
            {
                var pageObject = removedPageObjects.First();

                var codedObject = pageObject.CodedName;
                var eventType = Codings.EVENT_OBJECT_DELETE;
                var historyIndex = objectsOnPageChangedHistoryAction.HistoryActionIndex;
                var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                var incrementID = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID);

                var semanticEvent = new SemanticEvent(page, objectsOnPageChangedHistoryAction)
                                    {
                                        CodedObject = codedObject,
                                        EventType = eventType,
                                        CodedObjectID = codedObjectID,
                                        CodedObjectIDIncrement = incrementID,
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
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(objectsMovedHistoryActions), objectsMovedHistoryActions);

            if (!objectsMovedHistoryActions.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, objectsMovedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_EMPTY_LIST, "Move, No Actions");
            }

            var movedPageObjects = GetMovedPageObjects(page, objectsMovedHistoryActions);
            var movedStrokes = GetMovedStrokes(page, objectsMovedHistoryActions);

            if (!movedPageObjects.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, objectsMovedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Move, No PageObject Moved");
            }

            if (movedPageObjects.Count > 1) // Lasso move
            {
                // TODO
                return SemanticEvent.GetErrorSemanticEvent(page, objectsMovedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_MIXED_LIST, "Move, PageObjects Moved By Lasso");
            }

            if (movedStrokes.Any()) // Strokes moved by Lasso
            {
                // TODO
                return SemanticEvent.GetErrorSemanticEvent(page, objectsMovedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_MIXED_LIST, "Move, Strokes Moved By Lasso");
            }
            
            var pageObject = movedPageObjects.First();
            var codedObject = pageObject.CodedName;
            var eventType = Codings.EVENT_OBJECT_MOVE;
            var historyIndex = objectsMovedHistoryActions.First().HistoryActionIndex;
            var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID);

            // TODO: make eventInfo to describe distance travelled during move.
            var startX = Math.Round(objectsMovedHistoryActions.First().TravelledPositions.First().X);
            var startY = Math.Round(objectsMovedHistoryActions.First().TravelledPositions.First().Y);
            var endX = Math.Round(objectsMovedHistoryActions.Last().TravelledPositions.Last().X);
            var endY = Math.Round(objectsMovedHistoryActions.Last().TravelledPositions.Last().Y);
            var eventInfo = $"({startX}, {startY}) to ({endX}, {endY})";
            if (objectsMovedHistoryActions.Count > 1)
            {
                eventInfo += ", multiple";
            }

            var semanticEvent = new SemanticEvent(page, objectsMovedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedObjectID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = pageObject.ID
                                };

            return semanticEvent;
        }

        public static ISemanticEvent Resize(CLPPage page, List<PageObjectResizeBatchHistoryAction> objectsResizedHistoryActions)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(objectsResizedHistoryActions), objectsResizedHistoryActions);

            if (!objectsResizedHistoryActions.Any())
            {
                return SemanticEvent.GetErrorSemanticEvent(page, objectsResizedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_EMPTY_LIST, "Resize, No Actions");
            }

            var pageObjectID = objectsResizedHistoryActions.First().PageObjectID;
            var pageObject = page.GetPageObjectByIDOnPageOrInHistory(pageObjectID);
            if (pageObject == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, objectsResizedHistoryActions.Cast<IHistoryAction>().ToList(), Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "Resize, PageObject NULL");
            }

            var codedObject = pageObject.CodedName;
            var eventType = Codings.EVENT_OBJECT_RESIZE;
            var historyIndex = objectsResizedHistoryActions.First().HistoryActionIndex;
            var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
            var incrementID = GetCurrentIncrementIDForPageObject(pageObject.ID, codedObject, codedObjectID);

            var startWidth = Math.Round(objectsResizedHistoryActions.First().StretchedDimensions.First().X);
            var startHeight = Math.Round(objectsResizedHistoryActions.First().StretchedDimensions.First().Y);
            var endWidth = Math.Round(objectsResizedHistoryActions.Last().StretchedDimensions.Last().X);
            var endHeight = Math.Round(objectsResizedHistoryActions.Last().StretchedDimensions.Last().Y);
            var eventInfo = $"({startWidth}, {startHeight}) to ({endWidth}, {endHeight})";

            var semanticEvent = new SemanticEvent(page, objectsResizedHistoryActions.Cast<IHistoryAction>().ToList())
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedObjectID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
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
            var compoundID = $"{pageObjectID};{codedObject};{codedID}";

            if (!CurrentIncrementIDForPageObject.ContainsKey(compoundID))
            {
                SetCurrentIncrementIDForPageObject(pageObjectID, codedObject, codedID);
            }

            return CurrentIncrementIDForPageObject[compoundID].ToLetter();
        }

        public static string SetCurrentIncrementIDForPageObject(string pageObjectID, string codedObject, string codedID, bool isEraseEvent = false)
        {
            var objectAndID = $"{codedObject};{codedID}";
            var compoundID = $"{pageObjectID};{codedObject};{codedID}";
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
            var compoundID = $"{pageObjectID};{codedObject};{codedID};{subPosition};{subID}";

            return CurrentIncrementIDForPageObject[compoundID].ToLetter();
        }

        public static string SetCurrentIncrementIDForPageObject_Sub(string pageObjectID, string codedObject, string codedID, int subPosition, string subID, bool isEraseEvent = false)
        {
            var objectAndID = $"{codedObject};{subID}";
            var compoundID = $"{pageObjectID};{codedObject};{codedID};{subPosition};{subID}";
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