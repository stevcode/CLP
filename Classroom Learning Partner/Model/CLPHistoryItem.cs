﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catel.Data;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// CLPHistoryItem Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPHistoryItem : DataObjectBase<CLPHistoryItem>
    {
        #region Variables

        public enum HistoryItemType
        {
            Add,
            Move,
            Erase,
            Copy,
            Remove,
            Duplicate
        }

        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPHistoryItem(HistoryItemType itemType)
        {
            CreationDate = DateTime.Now;
            ItemType = itemType;
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
            private set { SetValue(CreationDateProperty, value); }
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
            private set { SetValue(UniqueIDProperty, value); }
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
            private set { SetValue(ItemTypeProperty, value); }
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

        #endregion

        #region Methods

        #endregion
    }

    //[Serializable]
    //public class CLPHistoryItem
    //{
    //     public CLPHistoryItem(string itemType)
    //    {
    //         /*CLPHistoryItem Types:
    //          * UNDO
    //          * REDO
    //          * ADD
    //          * MOVE
    //          * ERASE
    //          * COPY
    //          */

    //        MetaData.SetValue("CreationDate", DateTime.Now.ToString());
    //        MetaData.SetValue("UniqueID", Guid.NewGuid().ToString());
    //        MetaData.SetValue("ItemType", itemType);

    //    }
    //     private MetaDataContainer _metaData = new MetaDataContainer();
    //     public MetaDataContainer MetaData
    //     {
    //         get
    //         {
    //             return _metaData;
    //         }
    //     }
       
    //    #region MetaData

    //    public string ItemType
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("ItemType");
    //        }

    //    }

    //    public string ObjectID
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("ObjectID");
    //        }
    //        set
    //        {
    //            MetaData.SetValue("ObjectID", value);
    //        }
    //    }

    //    public string OldValue
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("OldValue");
    //        }
    //        set
    //        {
    //            MetaData.SetValue("OldValue", value);
    //        }
    //    }

    //    public string NewValue
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("NewValue");
    //        }
    //        set
    //        {
    //            MetaData.SetValue("NewValue", value);
    //        }
    //    }

    //    #endregion //MetaData
        
       
    //}
}
