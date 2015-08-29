using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class GeneralPageObjectAction : AHistoryActionBase
    {
        #region CodedActions

        public const string ACTION_ADD = "add";
        public const string ACTION_DELETE = "delete";
        public const string ACTION_MOVE = "move";
        public const string ACTION_RESIZE = "resize";

        #endregion // CodedActions

        #region Constructors

        /// <summary>Initializes <see cref="GeneralPageObjectAction" /> using <see cref="CLPPage" />.</summary>
        public GeneralPageObjectAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            : base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="GeneralPageObjectAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public GeneralPageObjectAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Static Methods

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

        public static bool IsHistoryItemsOnlyGeneralPageObjectActions(CLPPage page, List<IHistoryItem> historyItems)
        {
            return historyItems.All(h => h is ObjectsOnPageChangedHistoryItem || h is PageObjectResizeBatchHistoryItem || h is ObjectsMovedBatchHistoryItem);
        } 

        public static GeneralPageObjectAction VerifyAndGenerate(CLPPage parentPage, List<IHistoryItem> historyItems)
        {
            if (!IsHistoryItemsOnlyGeneralPageObjectActions(parentPage, historyItems))
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
                    var pageObject = addedPageObjects.First();
                    var historyAction = new GeneralPageObjectAction(parentPage, historyItems)
                                        {
                                            CodedObject = pageObject.CodedName,
                                            CodedObjectAction = ACTION_ADD,
                                            IsObjectActionVisible = false,
                                            CodedObjectID = pageObject.CodedID
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
                    var pageObject = addedPageObjects.First();
                    var historyAction = new GeneralPageObjectAction(parentPage, historyItems)
                                        {
                                            CodedObject = pageObject.CodedName,
                                            CodedObjectAction = ACTION_DELETE,
                                            CodedObjectID = pageObject.CodedID
                                        };

                    return historyAction;
                }
            }

            return null;
        }

        #endregion // Static Methods
    }
}