using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPageObjectRemove : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryPageObjectRemove(ICLPPage parentPage, ICLPPageObject pageObject, int index)
            : base(parentPage)
        {
            PageObjectUniqueID = pageObject.UniqueID;
            PageObject = pageObject;
            Index = index;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryPageObjectRemove(SerializationInfo info, StreamingContext context)
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
        /// UniqueID of the PageObject removed from the page.
        /// </summary>
        public string PageObjectUniqueID
        {
            get { return GetValue<string>(PageObjectUniqueIDProperty); }
            set { SetValue(PageObjectUniqueIDProperty, value); }
        }

        public static readonly PropertyData PageObjectUniqueIDProperty = RegisterProperty("PageObjectUniqueID", typeof(string));

        /// <summary>
        /// Actual PageObject. Will be restored to page on Undo and become NULL.
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
            if(PageObject == null)
            {
                Logger.Instance.WriteToLog("AddPageObject Redo Failure: No object to add.");
                return;
            }

            //restore proper z-order if possible
            if(Index >= ParentPage.PageObjects.Count)
            {
                ParentPage.PageObjects.Add(PageObject);
            }
            else
            {
                ParentPage.PageObjects.Insert(Index, PageObject);
            }
            PageObject = null; //no sense storing the actual pageObject for serialization if it's on the page.
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
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
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryPageObjectRemove;
            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
