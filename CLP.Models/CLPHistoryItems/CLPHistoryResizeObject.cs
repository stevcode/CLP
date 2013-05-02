using System;
using System.Collections;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryResizeObject : CLPHistoryItem
    {
        #region Constructor

        public CLPHistoryResizeObject(String objectId, double oldHeight, 
                double oldWidth, double newHeight, double newWidth) : base(HistoryItemType.ResizeObject)
        {
            ObjectId = objectId;
            OldHeight = oldHeight;
            OldWidth = oldWidth;
            NewHeight = newHeight;
            NewWidth = newWidth;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryResizeObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Page object (by unique ID) resized in this historical event
        /// </summary>
        public String ObjectId
        {
            get { return GetValue<String>(ObjectIdProperty); }
            set { SetValue(ObjectIdProperty, value); }
        }

        public static readonly PropertyData ObjectIdProperty = RegisterProperty("ObjectId", typeof(String), null);

        /// <summary>
        /// The page object's height before the resizing
        /// </summary>
        public double OldHeight
        {
            get { return GetValue<double>(OldHeightProperty); }
            set { SetValue(OldHeightProperty, value); }
        }

        public static readonly PropertyData OldHeightProperty = RegisterProperty("OldHeight", typeof(double), null);

        /// <summary>
        /// The page object's width before the resizing
        /// </summary>
        public double OldWidth
        {
            get { return GetValue<double>(OldWidthProperty); }
            set { SetValue(OldWidthProperty, value); }
        }

        public static readonly PropertyData OldWidthProperty = RegisterProperty("OldWidth", typeof(double), null);

        /// <summary>
        /// The page object's height after the resizing
        /// </summary>
        public double NewHeight
        {
            get { return GetValue<double>(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        public static readonly PropertyData NewHeightProperty = RegisterProperty("NewHeight", typeof(double), null);

        /// <summary>
        /// The page object's width after the resizing
        /// </summary>
        public double NewWidth
        {
            get { return GetValue<double>(NewWidthProperty); }
            set { SetValue(NewWidthProperty, value); }
        }

        public static readonly PropertyData NewWidthProperty = RegisterProperty("NewWidth", typeof(double), null);

        #endregion //Properties

        #region Methods

        public override CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return new CLPHistoryResizeObject(ObjectId, NewHeight, NewWidth, OldHeight, OldWidth);
        }

        public override CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return new CLPHistoryResizeObject(ObjectId, OldHeight, OldWidth, NewHeight, NewWidth);
        }

        override public void Undo(CLPPage page)
        {
            GetPageObjectByUniqueID(page, ObjectId).Height = OldHeight;
            GetPageObjectByUniqueID(page, ObjectId).Width = OldWidth;    
        }

        override public void Redo(CLPPage page)
        {
            GetPageObjectByUniqueID(page, ObjectId).Height = NewHeight;
            GetPageObjectByUniqueID(page, ObjectId).Width = NewWidth;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryResizeObject))
            {
                return false;
            }
            CLPHistoryResizeObject other = obj as CLPHistoryResizeObject;
            if(other.ObjectId !=  ObjectId ||
                other.OldHeight != OldHeight ||
                other.OldWidth != OldWidth ||
                other.NewHeight != NewHeight ||
                other.NewWidth != NewWidth)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}
