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
            var movedPageObjects = historyItems.OfType<ObjectsMovedBatchHistoryItem>().SelectMany(h => h.PageObjectIDs.Select(ParentPage.GetPageObjectByIDOnPageOrInHistory)).ToList();
            var resizedPageObjects = historyItems.OfType<PageObjectResizeBatchHistoryItem>().Select(h => ParentPage.GetPageObjectByIDOnPageOrInHistory(h.PageObjectID)).ToList();
            var addedPageObjects = historyItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsAdded.Select(ParentPage.GetPageObjectByIDOnPageOrInHistory)).ToList();
            var removedPageObjects = historyItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsRemoved.Select(ParentPage.GetPageObjectByIDOnPageOrInHistory)).ToList();

            if (movedPageObjects.Count + resizedPageObjects.Count +
                addedPageObjects.Count + removedPageObjects.Count != 1)
            {
                //throw error
            }

            var pageObjectType = "";
            if (movedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Move;
                pageObjectType = movedPageObjects.First().GetType().ToString();               
            }
            else if (resizedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Resize;
                pageObjectType = resizedPageObjects.First().GetType().ToString(); 
            }
            else if (addedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Add;
                pageObjectType = addedPageObjects.First().GetType().ToString(); 
            }
            else if (removedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Delete;
                pageObjectType = removedPageObjects.First().GetType().ToString(); 
            }
            PageObjectType = pageObjectType;
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
                var historyItems = HistoryItems;
                var objectCode = "";
                var objectDescriptor = "";
                if (PageObjectType == "CLPArray")
                {
                    var arrayHistoryItem = historyItems.First() as CLPArray;
                    objectCode = "ARR";
                    objectDescriptor = string.Format(" [{0}x{1}]", arrayHistoryItem.Rows, arrayHistoryItem.Columns);
                }
                else if (PageObjectType == "NumberLine")
                {
                    var numberLineHistoryItem = historyItems.First() as NumberLine;
                    objectCode = "NL";
                    objectDescriptor = string.Format(" [{0}]", numberLineHistoryItem.NumberLineSize);
                }
                else if (PageObjectType == "StampedObject")
                {
                    var stampHistoryItem = historyItems.First() as StampedObject;
                    objectCode = "STAMP";
                    objectDescriptor = "[]"; //pictorial, parts?
                }
                
                if (GeneralAction == GeneralActions.Add)
                {  
                    return string.Format("{0}{1}.", PageObjectType, objectDescriptor);

                }

                var codedActionType = GeneralAction.ToString().ToLower();       
                return string.Format("{0} {1}{2}.", objectDescriptor, codedActionType, objectDescriptor);
            }
        }

        #endregion //Properties
    }
}