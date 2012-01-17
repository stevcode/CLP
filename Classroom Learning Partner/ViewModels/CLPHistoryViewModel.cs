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

        private Dictionary<string, object> _objectReferences = new Dictionary<string, object>();
        public Dictionary<string, object> ObjectReferences
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
            set
            {
                _undoneHistoryItems = value;
            }
        }
        public void add(CLPHistoryItem item)
        {
            _historyItems.Add(item);
            //this.History.HistoryItems.Add(item);
            includeHistoryItem(item);
            return;
        }
        public void erase(object obj)
        {
            return;
        }
        public void move(object obj)
        {
            return;
        }
        public void copy(object obj)
        {
            return;
        }
        private void includeHistoryItem(CLPHistoryItem item)
        {
            //object obj = item.CLPHistoryObjectReference;
            //int itemID = obj.GetHashCode();
            //if (!ObjectReferences.ContainsKey(itemID))
            //{
            //    ObjectReferences.Add(itemID, obj);
            //    this.History.ObjectReferences.Add(itemID, obj);
            //}
            return;

        }
        public void undo()
        {
            
            if (HistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = HistoryItems[HistoryItems.Count - 1];
            HistoryItems.Remove(item);
            //this.History.HistoryItems.Remove(item);
            //UndoneHistoryItems.Add(item);
            //this.History.UndoneHistoryItems.Add(item);
            Object obj = ObjectReferences[item.MetaData.GetValue("UniqueID")];
           // CLPImage image;
           // if(obj is CLPImage){
           //     image = (CLPImage)obj;
           // }
            // Waiting for Steve to add handle for ObjectContainerViewModel.
           // CLPService.RemovePageObjectFromPage(PageObjectContainerViewModel);
            return;
        }
        public void redo()
        {
            if (UndoneHistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
            UndoneHistoryItems.Remove(item);
            //History.UndoneHistoryItems.Remove(item);
            Object obj = ObjectReferences[item.MetaData.GetValue("UniqueID")];
            CLPService.AddPageObjectToPage((CLPPageObjectBase)obj);
            return;
        }
        public void startPlayback()
        {
            //replay history of this page
            //System.Console.WriteLine("Start Playback");


        }
        public void stopPlayback()
        {
            //pause playback history
            //System.Console.WriteLine("Stop Playback");
        }
        public void resetHistory()
        {
            //reset history to the beginning
        }
    }
}