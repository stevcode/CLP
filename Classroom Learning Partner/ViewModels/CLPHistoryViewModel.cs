﻿using GalaSoft.MvvmLight;
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
using GalaSoft.MvvmLight.Command;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media;


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
            PageVM = page;
            _historyItems = history.HistoryItems;
            _undoneHistoryItems = history.UndoneHistoryItems;
            _objectReferences = history.ObjectReferences;
            _history = history;
            StartRecordNotReached = true;
            CLPService = new CLPServiceAgent();
                   
            AppMessages.ChangePlayback.Register(this, (playback) =>
            {
                if (this.PlaybackControlsVisibility == Visibility.Collapsed)
                    this.PlaybackControlsVisibility = Visibility.Visible;
                else
                    this.PlaybackControlsVisibility = Visibility.Collapsed;


            });
        }
        #region properties
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

        private Dictionary<string, object> _objectReferences;
        public Dictionary<string, object> ObjectReferences
        {
            get
            {
                return _objectReferences;
            }
        }

        private ObservableCollection<CLPHistoryItem> _historyItems;
        public ObservableCollection<CLPHistoryItem> HistoryItems
        {
            get
            {
                return _historyItems;
            }
            
        }
        private Visibility _playbackControlsVisibility = Visibility.Collapsed;
        public Visibility PlaybackControlsVisibility
        {
            get
            {
                return _playbackControlsVisibility;
            }
            set
            {
                _playbackControlsVisibility = value;
                RaisePropertyChanged("PlaybackControlsVisibility");


            }
        }
        
        
        //List to enable undo/redo functionality
        private ObservableCollection<CLPHistoryItem> _undoneHistoryItems;
        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        {
            get
            {
                return _undoneHistoryItems;
            }
            
        }
        private object _inkCanvas;
        public object InkCanvas
        {
            get
            {
                return _inkCanvas as System.Windows.Controls.InkCanvas;
            }
            set
            {
                _inkCanvas = value;
            }

        }
#endregion //properties
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
            else if (obj is String)
            {
                uniqueID = (CLPPageViewModel.StringToStroke(obj as string) as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }

            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
            {
                AddObjectToReferences(uniqueID, obj);
            }

            historyItem.ObjectID = uniqueID;
            _historyItems.Add(historyItem);
        }
        public void AddHistoryItem(CLPHistoryItem item)
        {
            _historyItems.Add(item);
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
        public void AddUndoneHistoryItem(CLPHistoryItem historyItem)
        {
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
        public CLPHistory Downsample(double size)
        {
            bool resizing = false;
            bool even = true;
            bool moving = false;
            int total = HistoryItems.Count;
            CLPHistory smallerHistory = new CLPHistory();
            smallerHistory.ObjectReferences = History.ObjectReferences;
            foreach (var item in History.HistoryItems)
            {
                if (item.ItemType == "RESIZE")
                {
                    if (resizing == true)
                    {
                        if (!even)
                        {
                            smallerHistory.HistoryItems.Add(item);
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        resizing = true;
                    }
                }
            }
            return smallerHistory;
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
            else if (pageObject is CLPSnapTile)
                       {
                pageObjectViewModel = new CLPSnapTileViewModel(pageObject as CLPSnapTile, pageViewModel);
            }

            else if (pageObject is CLPSquare)
                        {
                pageObjectViewModel = new CLPSquareViewModel(pageObject as CLPSquare, pageViewModel);
            }
            else if (pageObject is CLPInkRegion)
            {
                pageObjectViewModel = new CLPInkRegionViewModel(pageObject as CLPInkRegion, pageViewModel);
            }
            else if (pageObject is CLPCircle)
            {
                pageObjectViewModel = new CLPCircleViewModel(pageObject as CLPCircle, pageViewModel);
            }
            else if (pageObject is CLPAnimation)
            {
                pageObjectViewModel = new CLPAnimationViewModel(pageObject as CLPAnimation, pageViewModel);
            }
            else
            {
                pageObjectViewModel = null;
            }
            return pageObjectViewModel;
        }
        #region undo_redo
        public void undo()
        {
            
            if (HistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = HistoryItems[HistoryItems.Count - 1];
            if (item.ItemType == "START_RECORD")
            {
                //StartRecordNotReached = false; 
            }
            else if(item.ItemType == "STOP_RECORD")
            {
                Console.WriteLine("STOP_RECORD found");
            }
            else if (item.ItemType == "SAVE")
            {
                
            }
            else if (item.ItemType == "ADD")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
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
            else if (item.ItemType == "STACK_TILE")
            {

                if (ObjectReferences[item.ObjectID] is CLPSnapTile)
                {
                    CLPSnapTile tile = ObjectReferences[item.ObjectID] as CLPSnapTile;
                    
                    foreach (var container in PageVM.PageObjectContainerViewModels)
                    {
                        if (container.PageObjectViewModel.PageObject == tile)
                        {
                            CLPSnapTileViewModel tileVM = container.PageObjectViewModel as CLPSnapTileViewModel;
                            int diff = Int32.Parse(item.NewValue) - Int32.Parse(item.OldValue);
                            for (int i = 0; i < diff; i++)
                            {
                                tileVM.Tiles.Remove("SpringGreen");
                            }
                            
                            container.Height = CLPSnapTile.TILE_HEIGHT * tile.Tiles.Count;

                        }
                    }

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
            HistoryItems.Remove(item);
            if (item.ItemType == "START_RECORD" || item.ItemType == "STOP_RECORD" || item.ItemType == "SAVE")
            {
                AddUndoneHistoryItem(item);
            }
            else
            {
                AddUndoneHistoryItem(ObjectReferences[item.ObjectID], item);
            }
            return;
        }
         
        public void redo()
        {
            if (UndoneHistoryItems.Count <= 0) { return; }
            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);

            if (item.ItemType == "STOP_RECORD")
            {
                if (Recording)
                {
                    StopRecordReached = true;
                }
            }
            else if (item.ItemType == "SAVE")
            {
                //do nothing
            }
            else if (item.ItemType == "ERASE")
            {
                if (ObjectReferences[item.ObjectID] is String)
                {
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
                    String strokeString = ObjectReferences[item.ObjectID] as String;
                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
                    CLPService.AddStrokeToPage(stroke, PageVM, true);
                }
                else
                {
                    CLPService.AddPageObjectToPage(GetPageObject(item).PageObject, true);
                }

            }
            else if (item.ItemType == "STACK_TILE")
            {

                if (ObjectReferences[item.ObjectID] is CLPSnapTile)
                {
                    CLPSnapTile tile = ObjectReferences[item.ObjectID] as CLPSnapTile;

                    foreach (var container in PageVM.PageObjectContainerViewModels)
                    {
                        if (container.PageObjectViewModel.PageObject == ObjectReferences[item.ObjectID] as CLPSnapTile)
                        {
                            CLPSnapTileViewModel tileVM = container.PageObjectViewModel as CLPSnapTileViewModel;
                            int diff = Int32.Parse(item.NewValue) - Int32.Parse(item.OldValue);
                            for (int i = 0; i < diff; i++)
                            {
                                (container.PageObjectViewModel as CLPSnapTileViewModel).Tiles.Add("SpringGreen");
                            }
                            container.Height = CLPSnapTile.TILE_HEIGHT * tile.Tiles.Count;
                        }
                    }

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
            if (item.ItemType == "STOP_RECORD" || item.ItemType == "START_RECORD" || item.ItemType == "SAVE")
            {
                AddHistoryItem(item);
            }
            else
            {
                AddHistoryItem(ObjectReferences[item.ObjectID], item);
            }
            return;
        }
        #endregion //undo_redo
        #region playback
        //For the interaction history playback feature
        //invokes another thread to make the UI update at the correct times
        private delegate void NoArgDelegate();
        private bool _recording = false;
        public bool Recording
        {
            get
            {
                return _recording;
            }
            set
            {
                _recording = value;
            }
        }
        private bool _startRecordNotReached = true;
        private bool StartRecordNotReached
        {
            get
            {
                return _startRecordNotReached;
            }
            set
            {
                _startRecordNotReached = value;
            }
        }
        private bool _stopRecordReached = false;
        private bool StopRecordReached
        {
            get
            {
                return _stopRecordReached;
            }
            set
            {
                _stopRecordReached = value;
            }
        }

        public void startPlayback()
        {
            PlaybackStarted = true;
            PausePlayback = false;
            System.Windows.Controls.InkCanvas inkCanvas = this.InkCanvas as System.Windows.Controls.InkCanvas;
            if (inkCanvas == null)
            {
                inkCanvas = this.PageVM.NotebookViewModel.InkCanvas;
            }
            this.AbortPlayback = false;
            int size;
            int start;
            if (Recording)
            {
                CLPHistoryItem item = HistoryItems.Last(i => i.ItemType == "START_RECORD");
                start = HistoryItems.IndexOf(item);
                CLPHistoryItem endItem = HistoryItems.Last(i => i.ItemType == "STOP_RECORD");
                size = HistoryItems.IndexOf(endItem);
            }
            else
            {
                size = HistoryItems.Count;
                start = 0;
            }
                 for(int i = start; i < size; i++)
                 {
                     try
                     {
                   /*      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    undo();
                    return null;
                }, null);
                    */
                         inkCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(undo));
                     }

                     catch (NullReferenceException n)
                     {
                         Logger.Instance.WriteToLog("InkCanvas Null in HistoryVM: " + n.ToString());
                     }
                 }
                 System.Threading.Thread.Sleep(new TimeSpan(0, 0, 2));
                 StartRecordNotReached = true;
                 for(int i = 0; i < size; i++)
                 {
                     TimeSpan waittime = new TimeSpan(0, 0, 2);
                     try
                     {
                         if (UndoneHistoryItems.Count >= 2 )
                         {
                             
                             int len = UndoneHistoryItems.Count;
                             if (UndoneHistoryItems.ElementAt(len - 2).ItemType == "STOP_RECORD" || UndoneHistoryItems.ElementAt(len - 1).ItemType == "STOP_RECORD" || UndoneHistoryItems.ElementAt(len - 1).ItemType == "START_RECORD" || UndoneHistoryItems.ElementAt(len - 2).ItemType == "START_RECORD")
                             {
                                 
                             }
                             else
                             {
                                 waittime = DateTime.Parse(UndoneHistoryItems.ElementAt(len - 2).MetaData.GetValue("CreationDate")) - DateTime.Parse(UndoneHistoryItems.ElementAt(len - 1).MetaData.GetValue("CreationDate"));
                             }
                         }
                         if (UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1).ItemType == "STOP_RECORD")
                         {
                             PlaybackStarted = false;
                             if (Recording)
                             {
                                 foreach (var container in PageVM.PageObjectContainerViewModels)
                                 {
                                     if (container.PageObjectViewModel is CLPAnimationViewModel)
                                     {
                                         CLPAnimationViewModel animation = container.PageObjectViewModel as CLPAnimationViewModel;
                                         animation.PlaybackImage = new Uri("..\\..\\Images\\play_green.png", UriKind.Relative);
                                     }
                                 }
                             }
                             abortPlayback();
                             return;
                         }
                     }
                     catch (ArgumentOutOfRangeException e)
                     {
                         Logger.Instance.WriteToLog(e.ToString());
                     }
                     try
                     {
                         /*
                         Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    redo();
                    return null;
                }, null);
                         */
                         inkCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(redo));
                     }
                     catch (NullReferenceException n)
                     {
                         Logger.Instance.WriteToLog("InkCanvas null reference in historyVM. " + n.ToString());
                     }
                     if (waittime > new TimeSpan(0, 0, 0))
                     {
                         if(waittime > new TimeSpan(0, 0, 3))
                         {
                             waittime = new TimeSpan(0, 0, 3);
                         }
                         DateTime wait = DateTime.Now + waittime;
                         while(DateTime.Now < wait || PausePlayback == true)
                         {
                             if(AbortPlayback == true)
                             {
                                 PlaybackStarted = false;
                                 PageVM.PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                                 abortPlayback();
                                 return;
                             }
                             
                         }
                          
                     }
                     else
                     {
                         DateTime wait = DateTime.Now + new TimeSpan(0,0,0,0,50);
                         while (DateTime.Now < wait || PausePlayback == true)
                         {
                             if (AbortPlayback == true)
                             {
                                 PlaybackStarted = false;
                                 PageVM.PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                                 abortPlayback();
                                 return;
                             }
                         }
                     }
                     
                 }

                 if (Recording)
                 {
                     foreach (var container in PageVM.PageObjectContainerViewModels)
                     {
                         if (container.PageObjectViewModel is CLPAnimationViewModel)
                         {
                             CLPAnimationViewModel animation = container.PageObjectViewModel as CLPAnimationViewModel;
                             animation.PlaybackImage = new Uri("..\\..\\Images\\play_green.png", UriKind.Relative);
                         }
                     }
                 }
                 else
                 {
                     PageVM.PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                 }
                 PlaybackStarted = false;
           
        }
        
        private bool _pausePlayback;
        private bool PausePlayback
        {
            get
            {
                return _pausePlayback;
            }
            set
            {
                _pausePlayback = value;
            }
        }
        private bool _playbackStarted;
        private bool PlaybackStarted
        {
            get
            {
                return _playbackStarted;
            }
            set
            {
                _playbackStarted = value;
            }
        }
        private bool _abortPlayback;
        private bool AbortPlayback
        {
            get
            {
                return _abortPlayback;
            }
            set
            {
                _abortPlayback = value;
            }

        }
        public void start_pausePlayback()
        {
            if (PlaybackStarted)
            {
                if (PausePlayback)
                {
                    PausePlayback = false;
                    PageVM.PlaybackImage = new Uri("..\\Images\\pause_blue.png", UriKind.Relative);
                }
                else
                {
                    PausePlayback = true;
                    PageVM.PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                }
            }
            else
            {
                PageVM.PlaybackImage = new Uri("..\\Images\\pause_blue.png", UriKind.Relative);
                startPlayback();
            }

        }
        private void abortPlayback()
        {
            System.Windows.Controls.InkCanvas inkCanvas = this.InkCanvas as System.Windows.Controls.InkCanvas;
                        
            foreach (var i in UndoneHistoryItems)
            {
                inkCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(redo));   
            }
                   
        }
        public void ClearHistory()
        {
            HistoryItems.Clear();
            History.HistoryItems.Clear();
            UndoneHistoryItems.Clear();
            History.UndoneHistoryItems.Clear();
        }
        public void stopPlayback()
        {
            //stops and resets playback history
            this.AbortPlayback = true;
        }

        public void pausePlayback()
        {
            if (this.PausePlayback)
            {
                this.PausePlayback = false;
            }
            else
            {
                this.PausePlayback = true;
            }
        }
        #endregion //playback
        //IsSaved == true means that the history has not been updated since the last save
        //(to be used by Jessie- DB stuff)
        public bool IsSaved()
        {
            return HistoryItems[HistoryItems.Count - 1].ItemType == "SAVE";
        }
        #region relayCommands
        /*
         * Doesn't work for unknown reasons, it calls the relayCommand in PageViewModel
        private RelayCommand _startPlaybackCommand;
        public RelayCommand StartPlaybackCommand
        {
            get
            {
                return _startPlaybackCommand
                    ?? (_startPlaybackCommand = new RelayCommand(
                                          () =>
                                          {
                                              Console.WriteLine("START PLAYBACK COMMAND");
                                              startPlayback();
                                          }));
            }
        }
    */
        #endregion //relayCommands
    }
}