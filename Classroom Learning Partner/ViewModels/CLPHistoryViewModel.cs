using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;


namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class CLPHistoryViewModel : ViewModelBase
    {

        CLPServiceAgent CLPService;
        /// <summary>
        /// Initializes a new instance of the CLPHistoryViewModel class.
        /// </summary>
        public CLPHistoryViewModel() : this(new CLPHistory())
        { 
        }
        public CLPHistoryViewModel(CLPHistory history) 
        {
            _history = history;
            CLPService = new CLPServiceAgent();
            //change to updateHistory type
            AppMessages.UpdateCLPHistory.Register(this, (action) =>
            {
                   Console.WriteLine("HistoryViewModel registered with messages");
                   //Add historyItem to the list in history
                   
                   //switch: if undo, if redo, if move, etc
                   string type = action.ItemType;
                        if(type == "UNDO")
                        {
                            DateTime timeUndo = (DateTime) action.CLPHistoryObjectReference;
                            Console.WriteLine("Undo received by HistoryViewModel.");
                           //_historyItems.Add(action);
                           // includeHistoryItem(action);
                            this.undo(timeUndo);
                        }
                        else if(type == "REDO")
                        {
                            DateTime timeRedo = (DateTime) action.CLPHistoryObjectReference;
                            Console.WriteLine("Redo received by HistoryViewModel.");
                            //_historyItems.Add(action);
                            //includeHistoryItem(action);
                            redo(timeRedo);
                        }
                        else if(type == "ADD")
                        {
                            add(action);
                        }
                        else if(type == "COPY")
                        {
                            copy(action);
                        }
                        else if(type == "ERASE")
                        {
                            erase(action);
                        }
                        else if(type == "MOVE")
                        {
                            move(action);
                        }
                   
            });

        }
        private CLPHistory _history;
        public CLPHistory History
        {
            get
            {
                return _history;
            }
            set
            {
                _history = value;
            }
        }
        private Dictionary<string, CLPAttribute> _metaData = new Dictionary<string, CLPAttribute>();
        public Dictionary<string, CLPAttribute> MetaData
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
            protected set
            {
                _historyItems = value;
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
            protected set
            {
                _undoneHistoryItems = value;
            }
        }
        public void add(CLPHistoryItem item)
        {
            //CLPHistoryItem item = createHistoryItem(obj, CLPHistoryItem.CLPHistoryItemTypes.add);C:\classroom-learning-partner\Classroom Learning Partner\ViewModels\NotebookSelectorViewModel.cs
            //item.MetaData.Add("ADD", new CLPAttribute("ADD", "true"));
            //Console.WriteLine("ADD object received by History");
            _historyItems.Add(item);
            this.History.HistoryItems.Add(item);
            includeHistoryItem(item);
            
            Console.WriteLine("SIZE OF HISTORYITEMS LIST: " + _historyItems.Count);
        }
        public void erase(object obj)
        {
            //CLPHistoryItem item = createHistoryItem(obj, CLPHistoryItem.CLPHistoryItemTypes.erase);
            //item.MetaData.Add("ERASE", new CLPAttribute("ERASE", "true"));
            //_historyItems.Add(item);
        }
        public void move(object obj)
        {
            //CLPHistoryItem item = createHistoryItem(obj, CLPHistoryItem.CLPHistoryItemTypes.move);
           // item.MetaData.Add("MOVE", new CLPAttribute("MOVE", "true"));
           // _historyItems.Add(item);
        }
        public void copy(object obj)
        {
            //CLPHistoryItem item = includeHistoryItem(obj);
            //item.MetaData.Add("COPY", new CLPAttribute("COPY", "true"));
            //_historyItems.Add(item);
        }
        private void includeHistoryItem(CLPHistoryItem item)
        {
            object obj = item.CLPHistoryObjectReference;
            int itemID = obj.GetHashCode();
            if (!ObjectReferences.ContainsKey(itemID))
            {
                ObjectReferences.Add(itemID, obj);
                this.History.ObjectReferences.Add(itemID, obj);
            }
            //CLPHistoryItem item = new CLPHistoryItem(itemID, type);
            return;

        }
        public void undo(DateTime time)
        {
            
            if (HistoryItems.Count <= 0) { return; }
            int c = HistoryItems.Count;
            for (int i = 0; i < c; i++)
            {
                Console.WriteLine(HistoryItems[i]);
            }
            CLPHistoryItem item = HistoryItems[HistoryItems.Count - 1];
            if(HistoryItems.Contains(item)){
                Console.WriteLine("item is in HistoryItems");}
            bool removed = HistoryItems.Remove(item);
            bool Hremoved = this.History.HistoryItems.Remove(item);
            Console.WriteLine(removed + " " + Hremoved);
            int d = HistoryItems.Count;
            for (int i = 0; i < d; i++)
            {
                Console.WriteLine(HistoryItems[i]);
            }
            UndoneHistoryItems.Add(item);
            this.History.UndoneHistoryItems.Add(item);
            Object obj = ObjectReferences[item.CLPHistoryObjectReference.GetHashCode()];
            //Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
            //Send message to dispatch agent to display undo action
            //CLPHistoryItem item2 = new CLPHistoryItem(obj, "UNDOSEND");
            //AppMessages.UpdateCLPHistory.Send(item2);
            Console.WriteLine("UNDO: ITEMS IN UNDONE = " + UndoneHistoryItems.Count + " ITEMS IN HISTITEMS = " + HistoryItems.Count);
            return;
        }
        public void redo(DateTime time)
        {
            if (UndoneHistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
            UndoneHistoryItems.Remove(item);
            HistoryItems.Add(item);
            //check if the last action was an undo, because we want to skip over that.
            Object obj = ObjectReferences[item.CLPHistoryObjectReference.GetHashCode()];
            //Send message to dispatch agent
            CLPHistoryItem item2 = new CLPHistoryItem(obj, "REDOSEND");
            //CLPPageObjectBase objBase = new CLPPageObjectBase(
            //AppMessages.UpdateCLPHistory.Send(item2);
            CLPService.AddPageObjectToPage((CLPPageObjectBase)item2.CLPHistoryObjectReference);
            return;
        }
    }
}