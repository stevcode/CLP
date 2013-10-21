using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPageObjectsMassAdd : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryPageObjectsMassAdd(ICLPPage parentPage, IEnumerable<string> pageObjectIDs)
            : base(parentPage)
        {
            PageObjectIDs = pageObjectIDs.ToList();
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryPageObjectsMassAdd(SerializationInfo info, StreamingContext context)
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
        /// List of all the UniqueIDs of the pageObjects added.
        /// </summary>
        public List<string> PageObjectIDs
        {
            get { return GetValue<List<string>>(PageObjectIDsProperty); }
            set { SetValue(PageObjectIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectIDsProperty = RegisterProperty("PageObjectIDs", typeof(List<string>));

        /// <summary>
        /// List of the pageObjects that were removed from the page as a result of the UndoAction(). Cleared on Redo().
        /// </summary>
        public List<ICLPPageObject> PageObjects
        {
            get { return GetValue<List<ICLPPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(List<ICLPPageObject>), () => new List<ICLPPageObject>());

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            foreach(var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByUniqueID(pageObjectID))) 
            {
                try
                {
                    ParentPage.PageObjects.Remove(pageObject);
                    PageObjects.Add(pageObject);
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteErrorToLog("Undo PageObjectsMassAdd Error.", ex);
                }
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(!PageObjects.Any())
            {
                Logger.Instance.WriteToLog("PageObjectsMassAdd Redo Failure: No objects to add.");
                return;
            }

            foreach(var pageObject in PageObjects)
            {
                ParentPage.PageObjects.Add(pageObject);
            }

            PageObjects.Clear(); //no sense storing the actual pageObjects for serialization if it's on the page.
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryPageObjectsMassAdd;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            clonedHistoryItem.PageObjects.Clear();
            foreach(var pageObject in PageObjectIDs.Select(pageObjectID => ParentPage.GetPageObjectByUniqueID(pageObjectID)))
            {
                clonedHistoryItem.PageObjects.Add(pageObject);
            }

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
