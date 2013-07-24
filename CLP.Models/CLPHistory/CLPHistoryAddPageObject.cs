using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryAddPageObject : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryAddPageObject(ICLPPage parentPage, string pageObjectUniqueID)
            : base(parentPage)
        {
            PageObjectUniqueID = pageObjectUniqueID;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryAddPageObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

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

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            PageObject = ParentPage.GetPageObjectByUniqueID(PageObjectUniqueID);
            try
            {
                ParentPage.PageObjects.Remove(PageObject);
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteErrorToLog("Undo AddPageObject Error.", ex);
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(PageObject != null)
            {
                ParentPage.PageObjects.Add(PageObject);
                PageObject = null; //on Page again, don't want to reserialize.
            }
            else
            {
                Logger.Instance.WriteToLog("AddPageObject Redo Failure: No object to add.");
            }
        }

        #endregion //Methods
    }
}
