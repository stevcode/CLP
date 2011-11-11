using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.Model
{
    public class CLPHistory
    {
        public CLPHistory()
        {
            
        }

        private MetaDataContainer _metaData = new MetaDataContainer();
        public MetaDataContainer MetaData
        {
            get
            {
                return _metaData;
            }
        }

        private Dictionary<int, object> _objectReferences = new Dictionary<int, object>();
        public Dictionary<int, object> ObjectReferences
        {
            get
            {
                return _objectReferences;
            }
        }

        private ObservableCollection<CLPHistoryItem> _historyItems = new ObservableCollection<CLPHistoryItem>();
        public ObservableCollection<CLPHistoryItem> HistoryItems
        {
            get
            {
                return _historyItems;
            }
        }

        //List to enable undo/redo functionality
        private ObservableCollection<CLPHistoryItem> _undoneHistoryItems = new ObservableCollection<CLPHistoryItem>();
        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        {
            get
            {
                return _undoneHistoryItems;
            }
        }

     /*   public void add(object obj)
        {
            CLPHistoryItem item = createHistoryItem(obj);
            item.MetaData.Add("ADD",new CLPAttributeValue("ADD","true"));
            _historyItems.Add(item);
            Console.WriteLine("ADD to History");
        }
        public void erase(object obj)
        {
            CLPHistoryItem item = createHistoryItem(obj);
            item.MetaData.Add("ERASE", new CLPAttributeValue("ERASE", "true"));
            _historyItems.Add(item);
        }
        public void move(object obj)
        {
            CLPHistoryItem item = createHistoryItem(obj);
            item.MetaData.Add("MOVE", new CLPAttributeValue("MOVE", "true"));
            _historyItems.Add(item);
        }
        public void copy(object obj)
        {
            CLPHistoryItem item = createHistoryItem(obj);
            item.MetaData.Add("COPY", new CLPAttributeValue("COPY", "true"));
            _historyItems.Add(item);
        }
        private CLPHistoryItem createHistoryItem(object obj)
        {
            int itemID = obj.GetHashCode();
            if (!ObjectReferences.ContainsKey(itemID))
            {
                ObjectReferences.Add(itemID, obj);
            }
            CLPHistoryItem item = new CLPHistoryItem(itemID);
            return item;
            
        }
        public void undo(DateTime time)
        {
            if (HistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = HistoryItems.ElementAt(HistoryItems.Count - 1);
            HistoryItems.Remove(item);
            UndoneHistoryItems.Add(item);
            Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
            //Send message to dispatch agent
            Console.WriteLine("Undo message received");
            return;
        }
        public void redo()
        {
            if (UndoneHistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
            UndoneHistoryItems.Remove(item);
            HistoryItems.Add(item);
            Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
            //Send message to dispatch agent
            return;
        }
      */
    }
}
