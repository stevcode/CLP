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
            var movedPageObjects = MovedPageObjects;
            var resizedPageObjects = ResizedPageObjects;
            var addedPageObjects = AddedPageObjects;
            var removedPageObjects = RemovedPageObjects;

            if (movedPageObjects.Count + resizedPageObjects.Count + removedPageObjects.Count == 0)
            {
                if (addedPageObjects.Count < 1)
                {
                    //throw error, no objects
                }

                var addedObject = addedPageObjects.First().GetType().ToString();
                if (addedPageObjects.Count > 1 &&
                    addedObject != "StampObject") 
                {
                    //throw error, more than one StampObject added
                }
            }
            else if (movedPageObjects.Count + resizedPageObjects.Count + removedPageObjects.Count == 1)
            {
                if (addedPageObjects.Count > 0)
                {
                    //throw error
                }
            }
            else {
                //throw error
            }

            if (movedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Move;
                PageObjectType = movedPageObjects.First().GetType().ToString();               
            }
            else if (resizedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Resize;
                PageObjectType = resizedPageObjects.First().GetType().ToString(); 
            }
            else if (addedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Add;
                PageObjectType = addedPageObjects.First().GetType().ToString(); 
            }
            else if (removedPageObjects.Any())
            {
                GeneralAction = GeneralActions.Delete;
                PageObjectType = removedPageObjects.First().GetType().ToString(); 
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
                var historyItems = HistoryItems;
                var parentPage = historyItems.First().ParentPage;
                var firstObjectID = "";
                var objectIDsList = new List<string>();

                if (GeneralAction == GeneralActions.Add)
                {  
                    var historyItem = historyItems.First() as ObjectsOnPageChangedHistoryItem;
                    firstObjectID = historyItem.PageObjectIDsAdded.First();
                    objectIDsList = historyItem.PageObjectIDsAdded;
                }
                else if (GeneralAction == GeneralActions.Delete)
                {
                    var historyItem = historyItems.First() as ObjectsOnPageChangedHistoryItem;
                    firstObjectID = historyItem.PageObjectIDsRemoved.First();
                }
                else if (GeneralAction == GeneralActions.Move)
                {
                    var historyItem = historyItems.First() as ObjectsMovedBatchHistoryItem;
                    firstObjectID = historyItem.PageObjectIDs.First();
                }
                else //resize
                {
                   var historyItem = historyItems.First() as PageObjectResizeBatchHistoryItem;
                    firstObjectID = historyItem.PageObjectID;
                }

                var objectCode = "";
                var objectDescriptor = "";

                if (PageObjectType == "CLPArray")
                {
                    var arrayHistoryItem = parentPage.GetPageObjectByIDOnPageOrInHistory(firstObjectID) as CLPArray;
                    objectCode = "ARR";
                    objectDescriptor = string.Format(" [{0}x{1}]", arrayHistoryItem.Rows, arrayHistoryItem.Columns);
                }
                else if (PageObjectType == "NumberLine")
                {
                    var numberLineHistoryItem = parentPage.GetPageObjectByIDOnPageOrInHistory(firstObjectID) as NumberLine;
                    objectCode = "NL";
                    objectDescriptor = string.Format(" [{0}]", numberLineHistoryItem.NumberLineSize);
                }
                else if (PageObjectType == "Stamp")
                {
                    var stampHistoryItem = parentPage.GetPageObjectByIDOnPageOrInHistory(firstObjectID) as Stamp;
                    objectCode = "STAMP";//pictorial, parts?
                }
                else if (PageObjectType == "StampedObject")
                {
                    var stampHistoryItem = historyItems.First() as StampedObject;
                    objectCode = "STAMP IMAGE";
                    if (GeneralAction == GeneralActions.Add)
                    {
                        objectDescriptor = string.Format("[{0}x]", objectIDsList.Count); //pictorial, parts?
                    }
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

        #region Calculated Properties

        public List<IPageObject> MovedPageObjects
        {
            get { return HistoryItems.OfType<ObjectsMovedBatchHistoryItem>().SelectMany(h => h.PageObjectIDs.Select(ParentPage.GetPageObjectByIDOnPageOrInHistory)).ToList();}
        }

        public List<IPageObject> ResizedPageObjects
        {
            get { return HistoryItems.OfType<PageObjectResizeBatchHistoryItem>().Select(h => ParentPage.GetPageObjectByIDOnPageOrInHistory(h.PageObjectID)).ToList(); }
           
        }

        public List<IPageObject> AddedPageObjects
        {
            get { return HistoryItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsAdded.Select(ParentPage.GetPageObjectByIDOnPageOrInHistory)).ToList(); }
        }

        public List<IPageObject> RemovedPageObjects
        {
            get { return HistoryItems.OfType<ObjectsOnPageChangedHistoryItem>().SelectMany(h => h.PageObjectIDsRemoved.Select(ParentPage.GetPageObjectByIDOnPageOrInHistory)).ToList();};}
        }

        #endregion //Calculated Properties

    }
}