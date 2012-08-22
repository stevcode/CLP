using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    public enum HistoryItemType
    {
        AddPageObject,
        RemovePageObject,
        MovePageObject,
        ResizePageObject,
        AddInk,
        EraseInk,
        SnapTileSnap,
        SnapTileRemoveTile,
        Send,
        Submit,
        Copy,
        Duplicate,
        Save,
        StartRecord,
        StopRecord
    }

    /// <summary>
    /// CLPHistoryItem Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPHistoryItem : DataObjectBase<CLPHistoryItem>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPHistoryItem(HistoryItemType itemType, string objectID, string oldValue, string newValue)
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ItemType = itemType;
            ObjectID = objectID;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Creation date of historyItem.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        /// <summary>
        /// ObjectID to refer to object in CLPHistory hashmap.
        /// </summary>
        public string ObjectID
        {
            get { return GetValue<string>(ObjectIDProperty); }
            set { SetValue(ObjectIDProperty, value); }
        }

        /// <summary>
        /// Register the ObjectID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ObjectIDProperty = RegisterProperty("ObjectID", typeof(string), null);

        /// <summary>
        /// UniqueID of the historyItem.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Type of history item.
        /// </summary>
        public HistoryItemType ItemType
        {
            get { return GetValue<HistoryItemType>(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }

        /// <summary>
        /// Register the ItemType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ItemTypeProperty = RegisterProperty("ItemType", typeof(HistoryItemType), null);

        /// <summary>
        /// Old value of history item, used for Undo.
        /// </summary>
        public string OldValue
        {
            get { return GetValue<string>(OldValueProperty); }
            set { SetValue(OldValueProperty, value); }
        }

        /// <summary>
        /// Register the OldValue property so it is known in the class.
        /// </summary>
        public static readonly PropertyData OldValueProperty = RegisterProperty("OldValue", typeof(string), null);

        /// <summary>
        /// New value of history item, used for Redo.
        /// </summary>
        public string NewValue
        {
            get { return GetValue<string>(NewValueProperty); }
            set { SetValue(NewValueProperty, value); }
        }

        /// <summary>
        /// Register the NewValue property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NewValueProperty = RegisterProperty("NewValue", typeof(string), null);

        #endregion //Properties
    }
}
