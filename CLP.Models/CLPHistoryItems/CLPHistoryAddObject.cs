using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryAddObject : CLPHistoryItem
    {

        #region Constructor

        public CLPHistoryAddObject(String objectId) : base(HistoryItemType.AddObject)
        {
            ObjectId = objectId;
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
        /// Unique ID of page object added in this historical event
        /// </summary>
        public String ObjectId
        {
            get { return GetValue<String>(ObjectIdProperty); }
            set { SetValue(ObjectIdProperty, value); }
        }

        public static readonly PropertyData ObjectIdProperty = 
            RegisterProperty("ObjectId", typeof(String), null);

        /// <summary>
        /// The page object itself; null unless this event is in the Future
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

        public override void Undo(CLPPage page)
        {
            PageObject = GetPageObjectByUniqueID(page, ObjectId); // store it in case we need to put it back later
            if(PageObject == null)
            {
                Console.WriteLine("AddObject Undo Failure: No object to remove.");
                return;
            }
            page.PageObjects.Remove(PageObject);
        }

        override public void Redo(CLPPage page)
        {
            if(PageObject == null)
            {
                Console.WriteLine("AddObject Redo Failure: No object to add.");
                return;
            }
            page.PageObjects.Add(PageObject);
            PageObject = null; // don't need to put it back later anymore
        }

        override public CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return new CLPHistoryRemoveObject(GetPageObjectByUniqueID(page, ObjectId));
        }

        override public CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return new CLPHistoryAddObject(ObjectId);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryAddObject) ||
                (obj as CLPHistoryAddObject).ObjectId != ObjectId)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}