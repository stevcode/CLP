using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models.CLPHistoryItems
{
    public enum HistoryItemType
    {
        AddStroke,
        RemoveStroke,
        AddObject,
        RemoveObject,
        ResizeObject,
        MoveObject,
        Undo,
        Redo
    }

    /// <summary>
    /// CLPHistoryItem Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public abstract class CLPHistoryItem : DataObjectBase<CLPHistoryItem>
    {
        public CLPHistoryItem(HistoryItemType type, CLPPage page)
        {
            ItemType = type;
            Page = page;
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

        /// <summary>
        /// Page on which this action happened
        /// </summary>
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage), null);

        #endregion //Properties

        #region Methods

        // Undo this action
        public abstract void Undo();

        // Redo this action
        public abstract void Redo();

        // Return a CLPHistoryItem corresponding to what undoing this action would look like
        public abstract CLPHistoryItem GetUndoFingerprint();

        // Return a CLPHistoryItem corresponding to what redoing this action would look like
        public abstract CLPHistoryItem GetRedoFingerprint();

        public override bool Equals(object obj)
        {
            if(!(obj is CLPHistoryItem))
            {
                return false;
            }
            return ((obj as CLPHistoryItem).Page.UniqueID == Page.UniqueID);
        }

        #endregion //Methods
    }
}
