using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryRemoveObject : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryRemoveObject(CLPPage page, ICLPPageObject item) : base(HistoryItemType.RemoveObject, page)
        {
            PageObject = item;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryRemoveObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Page object removed in this historical event
        /// </summary>
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = 
            RegisterProperty("PageObject", typeof(ICLPPageObject), null);

        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint()
        {
            return new CLPHistoryAddObject(Page, PageObject);
        }

        public override CLPHistoryItem GetRedoFingerprint()
        {
            return new CLPHistoryRemoveObject(Page, PageObject);
        }

        override public void Undo()
        {
            Page.PageObjects.Add(PageObject);
        }

        override public void Redo()
        {
            Page.PageObjects.Remove(PageObject);
        }

        override public void ReplaceHistoricalRecords(ICLPPageObject oldObject, ICLPPageObject newObject)
        {
            if(PageObject.UniqueID == oldObject.UniqueID)
            {
                PageObject = newObject;
            }
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryRemoveObject) ||
                (obj as CLPHistoryRemoveObject).PageObject.UniqueID != PageObject.UniqueID)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}
