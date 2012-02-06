using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using Classroom_Learning_Partner.ViewModels;
using Catel.Data;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// CLPHistory Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPHistory : DataObjectBase<CLPHistory>
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPHistory() { }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistory(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion

        #region Properties

        /// <summary>
        /// Dictionary mapping UniqueID of an object to a reference of the object.
        /// </summary>
        public Dictionary<string, object> ObjectReferences
        {
            get { return GetValue<Dictionary<string, object>>(ObjectReferencesProperty); }
            private set { SetValue(ObjectReferencesProperty, value); }
        }

        /// <summary>
        /// Register the ObjectReferences property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ObjectReferencesProperty = RegisterProperty("ObjectReferences", typeof(Dictionary<string, object>), new Dictionary<string, object>());

        /// <summary>
        /// List of history items.
        /// </summary>
        public ObservableCollection<CLPHistoryItem> HistoryItems
        {
            get { return GetValue<ObservableCollection<CLPHistoryItem>>(HistoryItemsProperty); }
            private set { SetValue(HistoryItemsProperty, value); }
        }

        /// <summary>
        /// Register the HistoryItems property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HistoryItemsProperty = RegisterProperty("HistoryItems", typeof(ObservableCollection<CLPHistoryItem>), new ObservableCollection<CLPHistoryItem>());

        /// <summary>
        /// List to enable undo/redo functionality.
        /// </summary>
        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        {
            get { return GetValue<ObservableCollection<CLPHistoryItem>>(UndoneHistoryItemsProperty); }
            private set { SetValue(UndoneHistoryItemsProperty, value); }
        }

        /// <summary>
        /// Register the UndoneHistoryItems property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UndoneHistoryItemsProperty = RegisterProperty("UndoneHistoryItems", typeof(ObservableCollection<CLPHistoryItem>), new ObservableCollection<CLPHistoryItem>());

        #endregion

        #region Methods
        /// <summary>
        /// Validates the fields.
        /// </summary>
        protected override void ValidateFields()
        {
            // TODO: Implement any field validation of this object. Simply set any error by using the SetFieldError method
        }

        /// <summary>
        /// Validates the business rules.
        /// </summary>
        protected override void ValidateBusinessRules()
        {
            // TODO: Implement any business rules of this object. Simply set any error by using the SetBusinessRuleError method
        }

        public void AddHistoryItem(object obj, CLPHistoryItem historyItem)
        {
            string objectID = null;
            if (obj is CLPPageObjectBase)
            {
                objectID = (obj as CLPPageObjectBase).UniqueID;
            }
            else if (obj is Stroke)
            {
                objectID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }

            if (objectID != null && !ObjectReferences.ContainsKey(objectID))
            {
                AddObjectToReferences(objectID, obj);
            }

            historyItem.ObjectID = objectID;
            HistoryItems.Add(historyItem);

            System.Console.WriteLine("AddHistoryItem: HistoryItems.Count: " + HistoryItems.Count());
            System.Console.WriteLine("ObjectRefIds: " + ObjectReferences.Count());
        }

        public void AddUndoneHistoryItem(object obj, CLPHistoryItem historyItem)
        {
            string objectID = null;
            if (obj is CLPPageObjectBase)
            {
                objectID = (obj as CLPPageObjectBase).UniqueID;
            }
            else if (obj is Stroke)
            {
                objectID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }

            if (objectID != null && !ObjectReferences.ContainsKey(objectID))
            {
                AddObjectToReferences(objectID, obj);
            }

            historyItem.ObjectID = objectID;
            UndoneHistoryItems.Add(historyItem);
        }

        private void AddObjectToReferences(string key, object obj)
        {
            if (obj is Stroke)
            {
                ObjectReferences.Add(key, CLPPageViewModel.StrokeToString(obj as Stroke));
            }
            else if (obj is CLPPageObjectBase)
            {
                ObjectReferences.Add(key, obj);
            }
            else
            {
                Logger.Instance.WriteToLog("Unknown Object attempted to write to History");
            }
        }

        #endregion
    }

    //[Serializable]
    //public class CLPHistory
    //{
    //    public CLPHistory()
    //    {
            
    //    }

    //    private MetaDataContainer _metaData = new MetaDataContainer();
    //    public MetaDataContainer MetaData
    //    {
    //        get
    //        {
    //            return _metaData;
    //        }
    //    }

    //    private Dictionary<string, object> _objectReferences = new Dictionary<string, object>();
    //    public Dictionary<string, object> ObjectReferences
    //    {
    //        get
    //        {
    //            return _objectReferences;
    //        }
    //        set 
    //        {
    //            _objectReferences = value;
    //        }
    //    }

    //    private ObservableCollection<CLPHistoryItem> _historyItems = new ObservableCollection<CLPHistoryItem>();
    //    public ObservableCollection<CLPHistoryItem> HistoryItems
    //    {
    //        get
    //        {
    //            return _historyItems;
    //        }
            
    //    }

    //    //List to enable undo/redo functionality
    //    private ObservableCollection<CLPHistoryItem> _undoneHistoryItems = new ObservableCollection<CLPHistoryItem>();
    //    public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
    //    {
    //        get
    //        {
    //            return _undoneHistoryItems;
    //        }
            
    //    }

    //    #region Public Methods

    //    public void AddHistoryItem(object obj, CLPHistoryItem historyItem)
    //    {
    //        string uniqueID = null;
    //        if (obj is CLPPageObjectBase)
    //        {
    //            uniqueID = (obj as CLPPageObjectBase).UniqueID;
    //        }
    //        else if (obj is Stroke)
    //        {
    //            uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
    //        }

    //        if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
    //        {
    //            AddObjectToReferences(uniqueID, obj);
    //        }

    //        historyItem.ObjectID = uniqueID;
    //        _historyItems.Add(historyItem);

    //        System.Console.WriteLine("AddHistoryItem: HistoryItems.Count: " + HistoryItems.Count());
    //        System.Console.WriteLine("ObjectRefIds: " + ObjectReferences.Count());
    //    }
    //    public void AddUndoneHistoryItem(object obj, CLPHistoryItem historyItem)
    //    {
    //        string uniqueID = null;
    //        if (obj is CLPPageObjectBase)
    //        {
    //            uniqueID = (obj as CLPPageObjectBase).UniqueID;
    //        }
    //        else if (obj is Stroke)
    //        {
    //            uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
    //        }

    //        if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
    //        {
    //            AddObjectToReferences(uniqueID, obj);
    //        }

    //        historyItem.ObjectID = uniqueID;
    //        _undoneHistoryItems.Add(historyItem);
    //    }

    //    private void AddObjectToReferences(string key, object obj)
    //    {
    //        if (obj is Stroke)
    //        {
    //            ObjectReferences.Add(key, CLPPageViewModel.StrokeToString(obj as Stroke));
    //        }
            
    //        else if (obj is CLPPageObjectBase)
    //        {
    //            ObjectReferences.Add(key, obj);
    //        }
    //        else
    //        {
    //            Logger.Instance.WriteToLog("Unknown Object attempted to write to History");
    //        }
    //    }

    //    #endregion //Public Methods

    //    /*   public void add(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("ADD",new CLPAttributeValue("ADD","true"));
    //        _historyItems.Add(item);
    //        Console.WriteLine("ADD to History");
    //    }
    //    public void erase(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("ERASE", new CLPAttributeValue("ERASE", "true"));
    //        _historyItems.Add(item);
    //    }
    //    public void move(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("MOVE", new CLPAttributeValue("MOVE", "true"));
    //        _historyItems.Add(item);
    //    }
    //    public void copy(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("COPY", new CLPAttributeValue("COPY", "true"));
    //        _historyItems.Add(item);
    //    }
    //    private CLPHistoryItem createHistoryItem(object obj)
    //    {
    //        int itemID = obj.GetHashCode();
    //        if (!ObjectReferences.ContainsKey(itemID))
    //        {
    //            ObjectReferences.Add(itemID, obj);
    //        }
    //        CLPHistoryItem item = new CLPHistoryItem(itemID);
    //        return item;
            
    //    }
    //    public void undo(DateTime time)
    //    {
    //        if (HistoryItems.Count <= 0) { return; }
    //        CLPHistoryItem item = HistoryItems.ElementAt(HistoryItems.Count - 1);
    //        HistoryItems.Remove(item);
    //        UndoneHistoryItems.Add(item);
    //        Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
    //        //Send message to dispatch agent
    //        Console.WriteLine("Undo message received");
    //        return;
    //    }
    //    public void redo()
    //    {
    //        if (UndoneHistoryItems.Count <= 0) { return; }
    //        CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
    //        UndoneHistoryItems.Remove(item);
    //        HistoryItems.Add(item);
    //        Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
    //        //Send message to dispatch agent
    //        return;
    //    }
    //  */
    //}
}
