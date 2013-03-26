using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models.CLPHistoryItems
{
    [Serializable]
    public class CLPHistoryAddObject : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryAddObject(CLPPage page, ICLPPageObject item) : base(HistoryItemType.AddObject, page)
        {
            PageObject = item;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryAddObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Page object added in this historical event
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

        public override void Undo()
        {
            Page.PageObjects.Remove(PageObject);
        }

        override public void Redo()
        {
            Page.PageObjects.Add(PageObject);
        }

        override public CLPHistoryItem GetUndoFingerprint()
        {
            return new CLPHistoryRemoveObject(Page, PageObject);
        }

        override public CLPHistoryItem GetRedoFingerprint()
        {
            return new CLPHistoryAddObject(Page, PageObject);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryAddObject) ||
                (obj as CLPHistoryAddObject).PageObject.UniqueID != PageObject.UniqueID)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}