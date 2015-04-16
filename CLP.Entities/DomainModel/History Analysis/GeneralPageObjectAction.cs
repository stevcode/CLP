using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class GeneralPageObjectAction : AHistoryActionBase
    {
        public enum GeneralActions
        {
            Move,
            Resize,
            Add,
            Delete
        }

        #region Constructors

        /// <summary>Initializes <see cref="GeneralPageObjectAction" /> using <see cref="CLPPage" />.</summary>
        public GeneralPageObjectAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            : base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            var movedPageObjects = historyItems.OfType<ObjectsMovedBatchHistoryItem>().SelectMany(h => h.PageObjectIDs.Select(x => ParentPage.GetPageObjectByID(x.Key))).ToList();
            var resizedPageObjects = historyItems.OfType<PageObjectResizeBatchHistoryItem>().Select(h => ParentPage.GetPageObjectByID(h.PageObjectID)).ToList();
            var addedPageObjects = historyItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsAdded.Select(ParentPage.GetPageObjectByID)).ToList();
            var removedPageObjects = historyItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsRemoved.Select(ParentPage.GetPageObjectByID)).ToList();

            if (movedPageObjects.Count == 1 &&
                !resizedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Move;
                var pageObjectType = movedPageObjects.First().GetType().ToString();
                PageObjectType = pageObjectType;
            }
            else if (resizedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Resize;
            }
            else if (addedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Add;
            }
            else if (removedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Delete;
            }
            else
            {
                //throw error here
            }
        }

        /// <summary>Initializes <see cref="GeneralPageObjectAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public GeneralPageObjectAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The type of General Action this HistoryAction represents.
        /// </summary>
        public GeneralActions GeneralAction
        {
            get { return GetValue<GeneralActions>(GeneralActionProperty); }
            set { SetValue(GeneralActionProperty, value); }
        }

        public static readonly PropertyData GeneralActionProperty = RegisterProperty("GeneralAction", typeof (GeneralActions));

        /// <summary>
        /// SUMMARY
        /// </summary>
        public string PageObjectType
        {
            get { return GetValue<string>(PageObjectTypeProperty); }
            set { SetValue(PageObjectTypeProperty, value); }
        }

        public static readonly PropertyData PageObjectTypeProperty = RegisterProperty("PageObjectType", typeof (string));

        public override string CodedValue
        {
            get
            {
                var codedActionType = GeneralAction.ToString().ToLower();
                return string.Format("{0} {1} {2}", PageObjectType, codedActionType, "[8x8]");
            }
        }

        #endregion //Properties
    }
}