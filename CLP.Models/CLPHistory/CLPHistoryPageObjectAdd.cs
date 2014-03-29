using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPageObjectAdd : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryPageObjectAdd(ICLPPage parentPage, string pageObjectUniqueID, int index)
            : base(parentPage)
        {
            PageObjectUniqueID = pageObjectUniqueID;
            Index = index;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryPageObjectAdd(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 400;
            }
        }

        /// <summary>
        /// UniqueID of the PageObject added to the page.
        /// </summary>
        public string PageObjectUniqueID
        {
            get { return GetValue<string>(PageObjectUniqueIDProperty); }
            set { SetValue(PageObjectUniqueIDProperty, value); }
        }

        public static readonly PropertyData PageObjectUniqueIDProperty = RegisterProperty("PageObjectUniqueID", typeof(string));

        /// <summary>
        /// Actual PageObject. NULL unless the pageObject no longer exists on the page, due to an Undo Action.
        /// </summary>
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof(ICLPPageObject));

        /// <summary>
        /// Index of the pageObject when removed from the page.
        /// </summary>
        public int Index
        {
            get { return GetValue<int>(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly PropertyData IndexProperty = RegisterProperty("Index", typeof(int));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            PageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);
            Index = ParentPage.PageObjects.IndexOf(PageObject);
            try
            {
                ParentPage.PageObjects.Remove(PageObject);
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteErrorToLog("Undo AddPageObject Error.", ex);
            }

            //If page object was array and FFC with remainder on page, update
            //TODO: This shouldn't be here, find more appropriate place.
            if(PageObject.PageObjectType == "CLPArray")
            {
                foreach(var pageObject in ParentPage.PageObjects)
                {
                    if(pageObject is CLPFuzzyFactorCard)
                    {
                        if((pageObject as CLPFuzzyFactorCard).IsRemainderRegionDisplayed)
                        {
                            (pageObject as CLPFuzzyFactorCard).UpdateRemainderRegion();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(PageObject == null)
            {
                Logger.Instance.WriteToLog("AddPageObject Redo Failure: No object to add.");
                return;
            }
            PageObject.ParentPage = ParentPage;

            //restore proper z-order if possible
            if(Index >= ParentPage.PageObjects.Count)
            {
                ParentPage.PageObjects.Add(PageObject);
            }
            else
            {
                ParentPage.PageObjects.Insert(Index, PageObject);
            }
            
            //If page object was array and FFC with remainder on page, update
            //TODO: This shouldn't be here, find more appropriate place.
            if(PageObject.PageObjectType == "CLPArray")
            {
                foreach(var pageObject in ParentPage.PageObjects)
                {
                    if(pageObject is CLPFuzzyFactorCard)
                    {
                        if((pageObject as CLPFuzzyFactorCard).IsRemainderRegionDisplayed)
                        {
                            (pageObject as CLPFuzzyFactorCard).UpdateRemainderRegion();
                            break;
                        }
                    }
                }
            }

            PageObject = null; //no sense storing the actual pageObject for serialization if it's on the page.
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryPageObjectAdd;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);
            if(pageObject == null)
            {
                Logger.Instance.WriteToLog("Failed to get pageObject by ID during UndoRedoComplete in HistoryPageObjectAdd.");
                return null;
            }
            clonedHistoryItem.PageObject = pageObject;
            clonedHistoryItem.Index = ParentPage.PageObjects.IndexOf(pageObject);


            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
