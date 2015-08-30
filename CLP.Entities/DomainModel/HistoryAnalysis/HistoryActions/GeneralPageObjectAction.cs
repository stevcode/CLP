using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public static class ObjectCodedActions
    {
        #region Static Methods

        public static HistoryAction VerifyAndGenerate(CLPPage parentPage, List<IHistoryItem> historyItems)
        {
            if (!historyItems.All(h => h is ObjectsOnPageChangedHistoryItem || 
                                       h is PageObjectResizeBatchHistoryItem || 
                                       h is ObjectsMovedBatchHistoryItem) ||
                !historyItems.Any())
            {
                return null;
            }

            var movedPageObjects = GetMovedPageObjects(parentPage, historyItems);
            var resizedPageObjects = GetResizedPageObjects(parentPage, historyItems);
            var addedPageObjects = GetAddedPageObjects(parentPage, historyItems);
            var removedPageObjects = GetRemovedPageObjects(parentPage, historyItems);

            if (movedPageObjects.Any() &&
                !resizedPageObjects.Any() &&
                !addedPageObjects.Any() &&
                !removedPageObjects.Any())
            {
                return null;
            }

            if (resizedPageObjects.Any() &&
                !movedPageObjects.Any() &&
                !addedPageObjects.Any() &&
                !removedPageObjects.Any())
            {
                return null;
            }

            if (addedPageObjects.Any() &&
                !resizedPageObjects.Any() &&
                !movedPageObjects.Any() &&
                !removedPageObjects.Any())
            {
                if (addedPageObjects.Count == 1)
                {
                    var historyIndex = historyItems.First().HistoryIndex;
                    var pageObject = addedPageObjects.First();
                    var codedObject = pageObject.CodedName;
                    var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex + 1);
                    var historyAction = new HistoryAction(parentPage, historyItems)
                    {
                        CodedObject = codedObject,
                        CodedObjectAction = Codings.ACTION_OBJECT_ADD,
                        IsObjectActionVisible = false,
                        CodedObjectID = codedObjectID,
                        CodedObjectIDIncrement = IncrementAndGetIncrementID(codedObject, codedObjectID)
                    };

                    return historyAction;
                }

            }

            if (removedPageObjects.Any() &&
                !resizedPageObjects.Any() &&
                !addedPageObjects.Any() &&
                !movedPageObjects.Any())
            {
                if (removedPageObjects.Count == 1)
                {
                    var historyIndex = historyItems.First().HistoryIndex;
                    var pageObject = addedPageObjects.First();
                    var codedObject = pageObject.CodedName;
                    var codedObjectID = pageObject.GetCodedIDAtHistoryIndex(historyIndex);
                    var historyAction = new HistoryAction(parentPage, historyItems)
                    {
                        CodedObject = codedObject,
                        CodedObjectAction = Codings.ACTION_OBJECT_DELETE,
                        CodedObjectID = codedObjectID,
                        CodedObjectIDIncrement = GetIncrementID(codedObject, codedObjectID)
                    };

                    return historyAction;
                }
            }

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

        public static List<IPageObject> GetAddedPageObjects(CLPPage page, List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsAdded.Select(page.GetPageObjectByIDOnPageOrInHistory)).ToList();
        }

        public static List<IPageObject> GetRemovedPageObjects(CLPPage page, List<IHistoryItem> historyItems)
        {
            return historyItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsRemoved.Select(page.GetPageObjectByIDOnPageOrInHistory)).ToList();
        }

        #endregion // Static Methods
    }
}