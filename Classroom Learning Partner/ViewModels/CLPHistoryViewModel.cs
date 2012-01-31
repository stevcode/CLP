using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Threading;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using System.Windows.Ink;
using System.Windows;


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
        public CLPHistoryViewModel(CLPPageViewModel page, CLPHistory history)
        {
            PlaybackCounter = 0;
            PageVM = page;
            _historyItems = history.HistoryItems;
            _undoneHistoryItems = history.UndoneHistoryItems;
            _objectReferences = history.ObjectReferences;
            //shouldn't really be an audio message...
            AppMessages.Audio.Register(this, (item) =>
            {
                if (item == "startPlayback")
                {
                    startPlayback();
                    
                }
                else if (item == "Redo")
                {
                    redo();
                }
                else if (item == "Undo")
                {
                    undo();
                }
            });
            
        
        
            _history = history;
            CLPService = new CLPServiceAgent();
        }
        private CLPPageViewModel _pageVM;
        public CLPPageViewModel PageVM
        {
            get
            {
                return _pageVM;
            }
            set
            {
                _pageVM = value;
            }
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

        private Dictionary<string, object> _objectReferences;// = new Dictionary<string, object>();
        public Dictionary<string, object> ObjectReferences
        {
            get
            {
                return _objectReferences;
            }
        }

        private ObservableCollection<CLPHistoryItem> _historyItems;// = new ObservableCollection<CLPHistoryItem>();
        public ObservableCollection<CLPHistoryItem> HistoryItems
        {
            get
            {
                return _historyItems;
            }
            
        }

        //List to enable undo/redo functionality
        private ObservableCollection<CLPHistoryItem> _undoneHistoryItems;// = new ObservableCollection<CLPHistoryItem>();
        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        {
            get
            {
                return _undoneHistoryItems;
            }
            
        }

        private int _playbackCounter;
        public int PlaybackCounter
        {
            get { return _playbackCounter; }
            set 
            {
                _playbackCounter = value;
            }

        }
        #region addhistoryitems
        public void AddHistoryItem(object obj, CLPHistoryItem historyItem)
        {
            string uniqueID = null;
            if (obj is CLPPageObjectBase)
            {
                uniqueID = (obj as CLPPageObjectBase).UniqueID;
            }
            else if (obj is Stroke)
            {
                uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }

            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
            {
                AddObjectToReferences(uniqueID, obj);
            }

            historyItem.ObjectID = uniqueID;
            _historyItems.Add(historyItem);

            System.Console.WriteLine("AddHistoryItem: HistoryItems.Count: " + HistoryItems.Count());
            System.Console.WriteLine("ObjectRefIds: " + ObjectReferences.Count());
        }
        public void AddUndoneHistoryItem(object obj, CLPHistoryItem historyItem)
        {
            string uniqueID = null;
            if (obj is CLPPageObjectBase)
            {
                uniqueID = (obj as CLPPageObjectBase).UniqueID;
            }
            else if (obj is Stroke)
            {
                uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            else if (obj is String)
            {
                uniqueID = (CLPPageViewModel.StringToStroke(obj as string) as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
            {
                AddObjectToReferences(uniqueID, obj);
            }

            historyItem.ObjectID = uniqueID;
            _undoneHistoryItems.Add(historyItem);
        }

        private void AddObjectToReferences(string key, object obj)
        {
            if (obj is Stroke)
            {
                ObjectReferences.Add(key, CLPPageViewModel.StrokeToString(obj as Stroke));
            }
            else if (obj is String)
            {
                ObjectReferences.Add(key, obj as string);
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
        private CLPPageObjectBaseViewModel GetPageObject(CLPHistoryItem item)
        {
            CLPPageObjectBaseViewModel pageObjectViewModel;
            CLPPageObjectBase pageObject = ObjectReferences[item.ObjectID] as CLPPageObjectBase;
            CLPPageViewModel pageViewModel = PageVM;

            if (pageObject is CLPImage)
            {
                pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, pageViewModel);
            }
            else if (pageObject is CLPImageStamp)
            {
                pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp, pageViewModel);
            }
            else if (pageObject is CLPBlankStamp)
            {
                pageObjectViewModel = new CLPBlankStampViewModel(pageObject as CLPBlankStamp, pageViewModel);
            }
            else if (pageObject is CLPTextBox)
            {
                pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, pageViewModel);
            }
            else
            {
                pageObjectViewModel = null;
            }
            return pageObjectViewModel;
        }
        public void undo()
        {
            
            if (HistoryItems.Count <= 0) { return; }
            
            CLPHistoryItem item = HistoryItems[HistoryItems.Count - 1];
            //CLPHistoryItem actionItem;
            //String type = "";
            if (item.ItemType == "ADD")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                    Console.WriteLine("Stroke to be removed");
                    String strokeString = ObjectReferences[item.ObjectID] as String;
                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
                    CLPService.RemoveStrokeFromPage(stroke, PageVM, true);
                }
                else
                {
                    CLPService.RemovePageObjectFromPage(GetPageObject(item), true);
                }
            }
            else if (item.ItemType == "ERASE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                    Console.WriteLine("Stroke to be added");
                    String strokeString = ObjectReferences[item.ObjectID] as String;
                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
                    CLPService.AddStrokeToPage(stroke, PageVM, true);
                }
                else
                {
                    CLPService.AddPageObjectToPage(GetPageObject(item).PageObject, true); 
                }
                
            }
            else if (item.ItemType == "MOVE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                }
                else
                {
                    CLPService.ChangePageObjectPosition(GetPageObject(item), Point.Parse(item.OldValue), true);
                }
            }
            else if (item.ItemType == "RESIZE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                }
                else
                {
                    string h = item.OldValue.Split(',').ElementAt(0).Trim('(');
                    string w = item.OldValue.Split(',').ElementAt(1).Trim(')'); ;
                    double height = Double.Parse(h);
                    double width = Double.Parse(w);
                    CLPService.ChangePageObjectDimensions(GetPageObject(item), height, width, true);
                }
            }

            //actionItem = new CLPHistoryItem(type);
            //actionItem.ObjectID = item.ObjectID;
            //TODO: Need to add in the other HistoryItem types
            
           // Waiting for Steve to add handle for ObjectContainerViewModel.
           // CLPService.RemovePageObjectFromPage(PageObjectContainerViewModel);
            HistoryItems.Remove(item);
            AddUndoneHistoryItem(ObjectReferences[item.ObjectID], item);
            //History.AddHistoryItem(ObjectReferences[undoItem.ObjectID], undoItem);
            //AppMessages.SendPlaybackItem.Send(actionItem);
            return;
        }
         
        public void redo()
        {
            if (UndoneHistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
            
           
            if (item.ItemType == "ERASE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                    Console.WriteLine("Stroke to be removed");
                    String strokeString = ObjectReferences[item.ObjectID] as String;
                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
                    CLPService.RemoveStrokeFromPage(stroke, PageVM, true);
                }
                else
                {
                    CLPService.RemovePageObjectFromPage(GetPageObject(item), true);
                }
            }
            else if (item.ItemType == "ADD")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                    Console.WriteLine("Stroke to be added");
                    String strokeString = ObjectReferences[item.ObjectID] as String;
                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
                    CLPService.AddStrokeToPage(stroke, PageVM, true);
                }
                else
                {
                    CLPService.AddPageObjectToPage(GetPageObject(item).PageObject, true);
                }

            }
            else if (item.ItemType == "MOVE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                }
                else
                {
                    CLPService.ChangePageObjectPosition(GetPageObject(item), Point.Parse(item.NewValue), true);
                }
            }
            else if (item.ItemType == "RESIZE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
                }
                else
                {
                    string h = item.NewValue.Split(',').ElementAt(0).Trim('(');
                    string w = item.NewValue.Split(',').ElementAt(1).Trim(')');
                    double height = Double.Parse(h);
                    double width = Double.Parse(w);
                    CLPService.ChangePageObjectDimensions(GetPageObject(item), height, width, true);
                }
            }
            UndoneHistoryItems.Remove(item);
            AddHistoryItem(ObjectReferences[item.ObjectID], item);
            return;
             
            /*CLPHistoryItem redoItem = new CLPHistoryItem("REDO");
            if (UndoneHistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
            redoItem.ObjectID = item.ObjectID;
            History.UndoneHistoryItems.Remove(item);
            AppMessages.SendPlaybackItem.Send(item);
            */
        }
        //For the interaction history playback feature:
        public void startPlayback()
        {
            
            //replay history of this page
            //System.Console.WriteLine("Start Playback");
            //HistoryItems.ElementAt(PlaybackCounter).  make container of only this object visible
            // System.Console.WriteLine("playback counter at " + PlaybackCounter);
            //System.Console.WriteLine("historyItems at " + HistoryItems.Count);
            /// Version shows actions click by click
          /*  try
            {
                AppMessages.SendPlaybackItem.Send(HistoryItems.ElementAt(PlaybackCounter));
                if (PlaybackCounter < HistoryItems.Count - 1)
                {
                    PlaybackCounter++;
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.ToString());
            }
            */


                int size = HistoryItems.Count;
                 for(int i = 0; i < size; i++)
                 {
                     undo();
                 }
           
                 for(int i = 0; i < size; i++)
                 {
                     Console.WriteLine(i + " loop of replay");
                    // TimeSpan waittime = new TimeSpan(
                    /* if (UndoneHistoryItems.Count > 1)
                     {
                         int len = UndoneHistoryItems.Count;
                         waittime = DateTime.Parse(UndoneHistoryItems.ElementAt(len-2).MetaData.GetValue("CreationDate")) - DateTime.Parse(UndoneHistoryItems.ElementAt(len-1).MetaData.GetValue("CreationDate"));
                     }
                     * */
                     //Console.WriteLine("waittime " + waittime);
                     redo();
                     //System.Threading.Thread.Sleep(waittime);
                     
                 }
                 
                
             
            
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