using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    public enum HistoryItemType
    {
        AddStroke,
        RemoveStroke,
        AddObject,
        RemoveObject,
        ResizeObject,
        MoveObject,
        AddArrayLine,
        RemoveArrayLine,
        Aggregation
    }

    /// <summary>
    /// CLPHistoryItem Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public abstract class CLPHistoryItem : DataObjectBase<CLPHistoryItem>
    {
        public CLPHistoryItem(HistoryItemType type)
        {
            ItemType = type;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #region Properties

        /// <summary>
        /// Type of history item - what action was taken?
        /// </summary>
        public HistoryItemType ItemType
        {
            get { return GetValue<HistoryItemType>(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }

        public static readonly PropertyData ItemTypeProperty = RegisterProperty("ItemType", typeof(HistoryItemType), null);

        #endregion //Properties

        #region Methods

        // Undo this action
        public abstract void Undo(CLPPage page);

        // Redo this action
        public abstract void Redo(CLPPage page);

        // Return a CLPHistoryItem corresponding to what undoing this action would look like
        public abstract CLPHistoryItem GetUndoFingerprint(CLPPage page);

        // Return a CLPHistoryItem corresponding to what redoing this action would look like
        public abstract CLPHistoryItem GetRedoFingerprint(CLPPage page);

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryItem))
            {
                return false;
            }
            return true;
        }

        public ICLPPageObject GetPageObjectByUniqueID(CLPPage page, String uniqueID)
        {
            ICLPPageObject result = null;
            foreach(ICLPPageObject obj in page.PageObjects) {
                if(obj.UniqueID == uniqueID)
                {
                    result = obj;
                    break;
                }
            }
            return result;
        }

        public StrokeDTO GetSerializedStrokeByUniqueID(CLPPage page, String uniqueID)
        {
            StrokeDTO result = null;
            foreach(Stroke s in page.InkStrokes)
            {
                if(s.GetStrokeUniqueID() == uniqueID)
                {
                    result = new StrokeDTO(s);
                    break;
                }
            }
            return result;
        }

        #endregion //Methods
    }
}
