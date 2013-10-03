using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryStampPlace : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPHistoryStampPlace(ICLPPage parentPage, string stampCopyId)
            : base(parentPage)
        {
            StampCopyId = stampCopyId;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryStampPlace(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Unique ID of the stamp copy created in this history event.
        /// </summary>
        public string StampCopyId
        {
            get { return GetValue<string>(StampCopyIdProperty); }
            set { SetValue(StampCopyIdProperty, value); }
        }

        public static readonly PropertyData StampCopyIdProperty = RegisterProperty("StampCopyId", typeof(string));

        /// <summary>
        /// Stamp copy created in this history event; null unless undone
        /// </summary>
        public ICLPPageObject StampCopy
        {
            get { return GetValue<ICLPPageObject>(StampCopyProperty); }
            set { SetValue(StampCopyProperty, value); }
        }

        public static readonly PropertyData StampCopyProperty = RegisterProperty("StampCopy", typeof(ICLPPageObject));

        /// <summary>
        /// Page objects contained by the stamp copy created in this history event; null unless undone
        /// </summary>
        public ObservableCollection<ICLPPageObject> PageObjectsOverStampCopy
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectsOverStampCopyProperty); }
            set { SetValue(PageObjectsOverStampCopyProperty, value); }
        }

        public static readonly PropertyData PageObjectsOverStampCopyProperty = RegisterProperty("PageObjectsOverStampCopy", typeof(ObservableCollection<ICLPPageObject>));


        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            StampCopy = ParentPage.GetPageObjectByUniqueID(StampCopyId) as ICLPPageObject;
            if(StampCopy != null && ParentPage.PageObjects.Contains(StampCopy))
            {
                PageObjectsOverStampCopy = StampCopy.GetPageObjectsOverPageObject();
                foreach(ICLPPageObject pageObject in PageObjectsOverStampCopy)
                {
                    ParentPage.PageObjects.Remove(pageObject);
                }
                ParentPage.PageObjects.Remove(StampCopy);
            }
            else
            {
                Logger.Instance.WriteToLog("StampPlace Undo: Could not find StampCopy");
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            if(StampCopy != null)
            {
                ParentPage.PageObjects.Add(StampCopy);
                foreach (ICLPPageObject pageObject in PageObjectsOverStampCopy) {
                    pageObject.IsInternalPageObject = true;
                    ParentPage.PageObjects.Add(pageObject);
                }
            }
            StampCopy = null;
            PageObjectsOverStampCopy = null;
        }

        #endregion //Methods
    }
}