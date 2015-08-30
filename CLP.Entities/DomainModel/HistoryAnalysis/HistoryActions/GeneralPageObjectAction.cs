using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    public static class ObjectCodedActions
    {
        #region Static Methods

        public static HistoryAction Add(CLPPage page, ObjectsOnPageChangedHistoryItem objectsOnPageChangedHistoryItem)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItem == null ||
                !objectsOnPageChangedHistoryItem.PageObjectIDsAdded.Any() ||
                objectsOnPageChangedHistoryItem.PageObjectIDsRemoved.Any())
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
                                        CodedObjectIDIncrement = HistoryAction.IncrementAndGetIncrementID(pageObject.ID, codedObject, codedObjectID)
                                    };

                return historyAction;
            }

            // TODO: deal with multiple pageObjects added at once (create multiple arrays at the same time)
            // special case for Bins
            return null;
        }

        public static HistoryAction Delete(CLPPage page, ObjectsOnPageChangedHistoryItem objectsOnPageChangedHistoryItem)
        {
            if (page == null ||
                objectsOnPageChangedHistoryItem == null ||
                !objectsOnPageChangedHistoryItem.PageObjectIDsRemoved.Any() ||
                objectsOnPageChangedHistoryItem.PageObjectIDsAdded.Any())
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
                                        CodedObjectIDIncrement = HistoryAction.GetIncrementID(pageObject.ID, codedObject, codedObjectID)
                                    };

                return historyAction;
            }

            // TODO: deal with multiple pageObjects deleted at once (lasso?)
            // special case for Bins
            return null;
        }

        public static List<IPageObject> GetMovedPageObjects(CLPPage page, List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<ObjectsMovedBatchHistoryItem>().SelectMany(h => h.PageObjectIDs.Keys.Select(page.GetPageObjectByIDOnPageOrInHistory)).ToList();
        }

        public static List<IPageObject> GetResizedPageObjects(CLPPage page, List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<PageObjectResizeBatchHistoryItem>().Select(h => page.GetPageObjectByIDOnPageOrInHistory(h.PageObjectID)).ToList();
        }

        #endregion // Static Methods
    }
}