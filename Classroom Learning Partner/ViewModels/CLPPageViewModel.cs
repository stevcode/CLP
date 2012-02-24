using Classroom_Learning_Partner.Model;
using System.Windows.Ink;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Windows.Threading;
using System.Threading;
using Catel.MVVM;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(MainWindowViewModel))]
    public class CLPPageViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel(CLPPage page) : base()
        {
            Console.WriteLine(Title + " created");
            PlaybackControlsVisibility = Visibility.Collapsed;
            DefaultDA = App.MainWindowViewModel.DrawingAttributes;
            EditingMode = App.MainWindowViewModel.EditingMode;

            //History Stuff
            //AppMessages.ChangePlayback.Register(this, (playback) =>
            //{
            //    if (this.PlaybackControlsVisibility == Visibility.Collapsed)
            //        this.PlaybackControlsVisibility = Visibility.Visible;
            //    else
            //        this.PlaybackControlsVisibility = Visibility.Collapsed;


            //});
             
            Page = page;

            OtherStrokes = new StrokeCollection();

            //foreach (string stringStroke in Page.Strokes)
            //{
            //    Stroke stroke = CLPPage.StringToStroke(stringStroke);
            //    if (stroke.ContainsPropertyData(CLPPage.Immutable))
            //    {
            //        if (stroke.GetPropertyData(CLPPage.Immutable).ToString() == "true")
            //        {
            //            OtherStrokes.Add(stroke);
            //        }
            //        else
            //        {
            //            InkStrokes.Add(stroke);
            //        }
            //    }
            //}
            
            InkStrokes.StrokesChanged += new StrokeCollectionChangedEventHandler(InkStrokes_StrokesChanged);
            PageObjects.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PageObjects_CollectionChanged);

            //AudioViewModel avm = new AudioViewModel(page.MetaData.GetValue("UniqueID"));
        }

        public override string Title { get { return "PageVM"; } }

        public bool undoFlag;
        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject=false)]
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            private set { SetValue(PageProperty, value); }
        }

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page","Strokes")]
        public ObservableCollection<string> StringStrokes
        {
            get { return GetValue<ObservableCollection<string>>(StringStrokesProperty); }
            set { SetValue(StringStrokesProperty, value); }
        }

        /// <summary>
        /// Register the name property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StringStrokesProperty = RegisterProperty("StringStrokes", typeof(ObservableCollection<string>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<ICLPPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectsProperty); }
            private set { SetValue(PageObjectsProperty, value); }
        }

        /// <summary>
        /// Register the PageObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<ICLPPageObject>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            private set { SetValue(InkStrokesProperty, value); }
        }

        /// <summary>
        /// Register the InkStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public CLPHistory PageHistory
        {
            get { return GetValue<CLPHistory>(PageHistoryProperty); }
            set { SetValue(PageHistoryProperty, value); }
        }

        /// <summary>
        /// Register the PageHistory property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageHistoryProperty = RegisterProperty("PageHistory", typeof(CLPHistory));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public string SubmitterName
        {
            get { return GetValue<string>(SubmitterNameProperty); }
            set { SetValue(SubmitterNameProperty, value); }
        }

        /// <summary>
        /// Register the SubmitterName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmitterNameProperty = RegisterProperty("SubmitterName", typeof(string));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public bool IsSubmission
        {
            get { return GetValue<bool>(IsSubmissionProperty); }
            set { SetValue(IsSubmissionProperty, value); }
        }

        /// <summary>
        /// Register the IsSubmission property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsSubmissionProperty = RegisterProperty("IsSubmission", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility PlaybackControlsVisibility
        {
            get { return GetValue<Visibility>(PlaybackControlsVisibilityProperty); }
            set { SetValue(PlaybackControlsVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the PlaybackControlsVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PlaybackControlsVisibilityProperty = RegisterProperty("PlaybackControlsVisibility", typeof(Visibility));
        
        #endregion //Properties

        #region Bindings

        

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public StrokeCollection OtherStrokes
        {
            get { return GetValue<StrokeCollection>(OtherStrokesProperty); }
            set { SetValue(OtherStrokesProperty, value); }
        }

        /// <summary>
        /// Register the OtherStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData OtherStrokesProperty = RegisterProperty("OtherStrokes", typeof(StrokeCollection));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        /// <summary>
        /// Register the EditingMode property so it is known in the class.
        /// </summary>
        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof(InkCanvasEditingMode));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DrawingAttributes DefaultDA
        {
            get { return GetValue<DrawingAttributes>(DefaultDAProperty); }
            set { SetValue(DefaultDAProperty, value); }
        }

        /// <summary>
        /// Register the DefaultDA property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DefaultDAProperty = RegisterProperty("DefaultDA", typeof(DrawingAttributes));

        #endregion //Bindings

        #region Methods

        void PageObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            App.MainWindowViewModel.CanSendToTeacher = true;
            Console.WriteLine("adding page ofject to page with uniqueID: " + Page.UniqueID);
        }

        void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            App.MainWindowViewModel.CanSendToTeacher = true;

            foreach (var stroke in e.Removed)
            {
                Page.Strokes.Remove(CLPPage.StrokeToString(stroke));
                //if (!undoFlag)
                //{
                //    //CLPHistoryItem item = new CLPHistoryItem("ERASE");
                //    //HistoryVM.AddHistoryItem(stroke, item);
                //}
            }

            StrokeCollection addedStrokes = new StrokeCollection();
            foreach (Stroke stroke in e.Added)
            {
                if (!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                {
                    string newUniqueID = Guid.NewGuid().ToString();
                    stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                }
                foreach (var strokeRemoved in e.Removed)
                {
                    string a = strokeRemoved.GetPropertyData(CLPPage.StrokeIDKey) as string;
                    string b = stroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                    if (a == b)
                    {
                        string newUniqueID = Guid.NewGuid().ToString();
                        stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                    }
                }
                addedStrokes.Add(stroke);
            }


            foreach (var stroke in addedStrokes)
            {
                stroke.AddPropertyData(CLPPage.Immutable, "false");
                Page.Strokes.Add(CLPPage.StrokeToString(stroke));
                //if (!undoFlag)
                //{
                //    CLPHistoryItem item = new CLPHistoryItem("ADD");
                //    HistoryVM.AddHistoryItem(stroke, item);
                //}
            }


            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                List<string> add = new List<string>(CLPPage.StrokesToStrings(addedStrokes));
                List<string> remove = new List<string>(CLPPage.StrokesToStrings(e.Removed));
                //Steve - re-write BroadcastInk (add, remove, uniqueID, submissionID)
                if (Page.IsSubmission)
                {
                    if (App.Peer.Channel != null)
                    {
                        App.Peer.Channel.BroadcastInk(add, remove, Page.SubmissionID);
                    }
                }
                else
                {
                    if (App.Peer.Channel != null)
                    {
                        App.Peer.Channel.BroadcastInk(add, remove, Page.UniqueID);
                    }
                }
            }

            foreach (CLPPageObjectBase pageObject in PageObjects)
            {
                //add bool to pageObjectBase for accept strokes, that way you don't need to check if it's over if it's not going to accept

                Rect rect = new Rect(pageObject.Position.X, pageObject.Position.Y, pageObject.Width, pageObject.Height);

                StrokeCollection addedStrokesOverObject = new StrokeCollection();
                foreach (Stroke stroke in addedStrokes)
                {
                    if (stroke.HitTest(rect, 3))
                    {
                        addedStrokesOverObject.Add(stroke);
                    }
                }

                StrokeCollection removedStrokesOverObject = new StrokeCollection();
                foreach (Stroke stroke in e.Removed)
                {
                    if (stroke.HitTest(rect, 3))
                    {
                        removedStrokesOverObject.Add(stroke);
                    }
                }
                //Steve - account for removal of pageObjectContainers
                //pageObjectContainerViewModel.PageObjectViewModel.AcceptStrokes(addedStrokesOverObject, removedStrokesOverObject);
            }
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "EditingMode")
            {
                EditingMode = (viewModel as MainWindowViewModel).EditingMode;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods

        

        #region Commands

       //private RelayCommand _startPlaybackCommand;

       // /// <summary>
       // /// Gets the StartPlaybackCommand.
       // /// </summary>
       //private delegate void NoArgDelegate();
       // public RelayCommand StartPlaybackCommand
       // {
       //     get
       //     {
       //         return _startPlaybackCommand
       //             ?? (_startPlaybackCommand = new RelayCommand(
       //                                   () =>
       //                                   {
       //                                       Console.WriteLine("PageVM startplayback");
       //                                       // Start fetching the playback items asynchronously.
       //                                       NoArgDelegate fetcher = new NoArgDelegate(HistoryVM.startPlayback);
       //                                       fetcher.BeginInvoke(null, null);
                                              

       //                                   }));
       //     }
       // }
        
  
        
       // private RelayCommand _stopPlaybackCommand;

       // /// <summary>
       // /// Gets the StartPlaybackCommand.
       // /// </summary>
       // public RelayCommand StopPlaybackCommand
       // {
       //     get
       //     {
       //         return _stopPlaybackCommand
       //             ?? (_stopPlaybackCommand = new RelayCommand(
       //                                   () =>
       //                                   {
       //                                       NoArgDelegate fetcher = new NoArgDelegate(HistoryVM.stopPlayback);
       //                                       fetcher.BeginInvoke(null, null);
                                            
       //                                   }));
       //     }
       // }

        #endregion //Commands

        #region CLPHistoryVM Import

        //        public CLPHistoryViewModel(CLPPageViewModel page, CLPHistory history)
        //        {
        //            PageVM = page;
        //            _historyItems = history.HistoryItems;
        //            _undoneHistoryItems = history.UndoneHistoryItems;
        //            _objectReferences = history.ObjectReferences;
        //            _history = history;
        //            CLPService = new CLPServiceAgent();

        //            AppMessages.ChangePlayback.Register(this, (playback) =>
        //            {
        //                if (this.PlaybackControlsVisibility == Visibility.Collapsed)
        //                    this.PlaybackControlsVisibility = Visibility.Visible;
        //                else
        //                    this.PlaybackControlsVisibility = Visibility.Collapsed;


        //            });
        //        }
        //        #region properties
        //        private CLPPageViewModel _pageVM;
        //        public CLPPageViewModel PageVM
        //        {
        //            get
        //            {
        //                return _pageVM;
        //            }
        //            set
        //            {
        //                _pageVM = value;
        //            }
        //        }
        //        private CLPHistory _history;
        //        public CLPHistory History
        //        {
        //            get
        //            {
        //                return _history;
        //            }
        //            set
        //            {
        //                _history = value;
        //            }
        //        }

        //        private Dictionary<string, object> _objectReferences;
        //        public Dictionary<string, object> ObjectReferences
        //        {
        //            get
        //            {
        //                return _objectReferences;
        //            }
        //        }

        //        private ObservableCollection<CLPHistoryItem> _historyItems;
        //        public ObservableCollection<CLPHistoryItem> HistoryItems
        //        {
        //            get
        //            {
        //                return _historyItems;
        //            }

        //        }
        //        private Visibility _playbackControlsVisibility = Visibility.Collapsed;
        //        public Visibility PlaybackControlsVisibility
        //        {
        //            get
        //            {
        //                return _playbackControlsVisibility;
        //            }
        //            set
        //            {
        //                _playbackControlsVisibility = value;
        //                RaisePropertyChanged("PlaybackControlsVisibility");


        //            }
        //        }
        //        //List to enable undo/redo functionality
        //        private ObservableCollection<CLPHistoryItem> _undoneHistoryItems;
        //        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        //        {
        //            get
        //            {
        //                return _undoneHistoryItems;
        //            }

        //        }
        //        private object _inkCanvas;
        //        public object InkCanvas
        //        {
        //            get
        //            {
        //                return _inkCanvas as System.Windows.Controls.InkCanvas;
        //            }
        //            set
        //            {
        //                _inkCanvas = value;
        //            }

        //        }
        //#endregion //properties
        //        #region addhistoryitems
        //        public void AddHistoryItem(object obj, CLPHistoryItem historyItem)
        //        {
        //            string uniqueID = null;
        //            if (obj is CLPPageObjectBase)
        //            {
        //                uniqueID = (obj as CLPPageObjectBase).UniqueID;
        //            }
        //            else if (obj is Stroke)
        //            {
        //                uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
        //            }
        //            else if (obj is String)
        //            {
        //                uniqueID = (CLPPageViewModel.StringToStroke(obj as string) as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
        //            }

        //            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
        //            {
        //                AddObjectToReferences(uniqueID, obj);
        //            }

        //            historyItem.ObjectID = uniqueID;
        //            _historyItems.Add(historyItem);
        //        }
        //        public void AddUndoneHistoryItem(object obj, CLPHistoryItem historyItem)
        //        {
        //            string uniqueID = null;
        //            if (obj is CLPPageObjectBase)
        //            {
        //                uniqueID = (obj as CLPPageObjectBase).UniqueID;
        //            }
        //            else if (obj is Stroke)
        //            {
        //                uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
        //            }
        //            else if (obj is String)
        //            {
        //                uniqueID = (CLPPageViewModel.StringToStroke(obj as string) as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
        //            }
        //            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
        //            {
        //                AddObjectToReferences(uniqueID, obj);
        //            }

        //            historyItem.ObjectID = uniqueID;
        //            _undoneHistoryItems.Add(historyItem);
        //        }

        //        private void AddObjectToReferences(string key, object obj)
        //        {
        //            if (obj is Stroke)
        //            {
        //                ObjectReferences.Add(key, CLPPageViewModel.StrokeToString(obj as Stroke));
        //            }
        //            else if (obj is String)
        //            {
        //                ObjectReferences.Add(key, obj as string);
        //            }
        //            else if (obj is CLPPageObjectBase)
        //            {
        //                ObjectReferences.Add(key, obj);
        //            }
        //            else
        //            {
        //                Logger.Instance.WriteToLog("Unknown Object attempted to write to History");
        //            }
        //        }

        //        #endregion

        //        private CLPPageObjectBaseViewModel GetPageObject(CLPHistoryItem item)
        //        {
        //            CLPPageObjectBaseViewModel pageObjectViewModel;
        //            CLPPageObjectBase pageObject = ObjectReferences[item.ObjectID] as CLPPageObjectBase;
        //            CLPPageViewModel pageViewModel = PageVM;

        //            if (pageObject is CLPImage)
        //            {
        //                pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, pageViewModel);
        //            }
        //            else if (pageObject is CLPImageStamp)
        //            {
        //                pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp, pageViewModel);
        //            }
        //            else if (pageObject is CLPBlankStamp)
        //            {
        //                pageObjectViewModel = new CLPBlankStampViewModel(pageObject as CLPBlankStamp, pageViewModel);
        //            }
        //            else if (pageObject is CLPTextBox)
        //            {
        //                pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, pageViewModel);
        //            }
        //            else
        //            {
        //                pageObjectViewModel = null;
        //            }
        //            return pageObjectViewModel;
        //        }
        //        public void undo()
        //        {

        //            if (HistoryItems.Count <= 0) { return; }
        //            CLPHistoryItem item = HistoryItems[HistoryItems.Count - 1];
        //            if (item.ItemType == "ADD")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                    String strokeString = ObjectReferences[item.ObjectID] as String;
        //                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
        //                    CLPService.RemoveStrokeFromPage(stroke, PageVM, true);
        //                }
        //                else
        //                {
        //                    CLPService.RemovePageObjectFromPage(GetPageObject(item), true);
        //                }
        //            }
        //            else if (item.ItemType == "ERASE")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                    String strokeString = ObjectReferences[item.ObjectID] as String;
        //                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
        //                    CLPService.AddStrokeToPage(stroke, PageVM, true);
        //                }
        //                else
        //                {
        //                    CLPService.AddPageObjectToPage(GetPageObject(item).PageObject, true); 
        //                }

        //            }
        //            else if (item.ItemType == "MOVE")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                }
        //                else
        //                {
        //                    CLPService.ChangePageObjectPosition(GetPageObject(item), Point.Parse(item.OldValue), true);
        //                }
        //            }
        //            else if (item.ItemType == "RESIZE")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                }
        //                else
        //                {
        //                    string h = item.OldValue.Split(',').ElementAt(0).Trim('(');
        //                    string w = item.OldValue.Split(',').ElementAt(1).Trim(')'); ;
        //                    double height = Double.Parse(h);
        //                    double width = Double.Parse(w);
        //                    CLPService.ChangePageObjectDimensions(GetPageObject(item), height, width, true);
        //                }
        //            }
        //            HistoryItems.Remove(item);
        //            AddUndoneHistoryItem(ObjectReferences[item.ObjectID], item);
        //            return;
        //        }

        //        public void redo()
        //        {
        //            if (UndoneHistoryItems.Count <= 0) { return; }
        //            CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);


        //            if (item.ItemType == "ERASE")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                    String strokeString = ObjectReferences[item.ObjectID] as String;
        //                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
        //                    CLPService.RemoveStrokeFromPage(stroke, PageVM, true);
        //                }
        //                else
        //                {
        //                    CLPService.RemovePageObjectFromPage(GetPageObject(item), true);
        //                }
        //            }
        //            else if (item.ItemType == "ADD")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                    String strokeString = ObjectReferences[item.ObjectID] as String;
        //                    Stroke stroke = CLPPageViewModel.StringToStroke(strokeString);
        //                    CLPService.AddStrokeToPage(stroke, PageVM, true);
        //                }
        //                else
        //                {
        //                    CLPService.AddPageObjectToPage(GetPageObject(item).PageObject, true);
        //                }

        //            }
        //            else if (item.ItemType == "MOVE")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                }
        //                else
        //                {
        //                    CLPService.ChangePageObjectPosition(GetPageObject(item), Point.Parse(item.NewValue), true);
        //                }
        //            }
        //            else if (item.ItemType == "RESIZE")
        //            {
        //                if (ObjectReferences[item.ObjectID] is String)
        //                {
        //                }
        //                else
        //                {
        //                    string h = item.NewValue.Split(',').ElementAt(0).Trim('(');
        //                    string w = item.NewValue.Split(',').ElementAt(1).Trim(')');
        //                    double height = Double.Parse(h);
        //                    double width = Double.Parse(w);
        //                    CLPService.ChangePageObjectDimensions(GetPageObject(item), height, width, true);
        //                }
        //            }
        //            UndoneHistoryItems.Remove(item);
        //            AddHistoryItem(ObjectReferences[item.ObjectID], item);
        //            return;
        //        }
        //        #region playback
        //        //For the interaction history playback feature
        //        //invokes another thread to make the UI update at the correct times
        //        private delegate void NoArgDelegate();
        //        public void startPlayback()
        //        {
        //            System.Windows.Controls.InkCanvas inkCanvas = this.InkCanvas as System.Windows.Controls.InkCanvas;

        //            this.AbortPlayback = false;
        //            int size = HistoryItems.Count;
        //                 for(int i = 0; i < size; i++)
        //                 {
        //                    inkCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(undo));
        //                 }
        //                 System.Threading.Thread.Sleep(new TimeSpan(0, 0, 2));
        //                 for(int i = 0; i < size; i++)
        //                 {
        //                     TimeSpan waittime = new TimeSpan(0, 0, 2);
        //                     try
        //                     {
        //                         if (UndoneHistoryItems.Count >= 2)
        //                         {
        //                             int len = UndoneHistoryItems.Count;
        //                             waittime = DateTime.Parse(UndoneHistoryItems.ElementAt(len - 2).MetaData.GetValue("CreationDate")) - DateTime.Parse(UndoneHistoryItems.ElementAt(len - 1).MetaData.GetValue("CreationDate"));
        //                         }
        //                     }
        //                     catch (ArgumentOutOfRangeException e)
        //                     {
        //                         Logger.Instance.WriteToLog(e.ToString());
        //                     }
        //                     inkCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(redo));

        //                     if (waittime > new TimeSpan(0, 0, 0))
        //                     {
        //                         if(waittime > new TimeSpan(0, 0, 15))
        //                         {
        //                             waittime = new TimeSpan(0, 0, 15);
        //                         }
        //                         DateTime wait = DateTime.Now + waittime;
        //                         while(DateTime.Now < wait)
        //                         {
        //                             if(AbortPlayback == true)
        //                             {
        //                                 abortPlayback();
        //                                 return;
        //                             }
        //                         }

        //                     }
        //                     else
        //                     {
        //                         DateTime wait = DateTime.Now + new TimeSpan(0,0,0,0,100);
        //                         while (DateTime.Now < wait)
        //                         {
        //                             if (AbortPlayback == true)
        //                             {
        //                                 abortPlayback();
        //                                 return;
        //                             }
        //                         }
        //                     }

        //                 }

        //        }
        //        private bool _abortPlayback;
        //        private bool AbortPlayback
        //        {
        //            get
        //            {
        //                return _abortPlayback;
        //            }
        //            set
        //            {
        //                _abortPlayback = value;
        //            }

        //        }
        //        private void abortPlayback()
        //        {
        //            System.Windows.Controls.InkCanvas inkCanvas = this.InkCanvas as System.Windows.Controls.InkCanvas;

        //            foreach (var i in UndoneHistoryItems)
        //            {
        //                inkCanvas.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(redo));   
        //            }

        //        }

        //        public void stopPlayback()
        //        {
        //            //stops and resets playback history
        //            this.AbortPlayback = true;
        //        }

        //        #endregion //playback
        //        #region relayCommands
        //        /*
        //         * Doesn't work for unknown reasons, it calls the relayCommand in PageViewModel
        //        private RelayCommand _startPlaybackCommand;
        //        public RelayCommand StartPlaybackCommand
        //        {
        //            get
        //            {
        //                return _startPlaybackCommand
        //                    ?? (_startPlaybackCommand = new RelayCommand(
        //                                          () =>
        //                                          {
        //                                              Console.WriteLine("START PLAYBACK COMMAND");
        //                                              startPlayback();
        //                                          }));
        //            }
        //        }
        //    */
        //        #endregion //relayCommands

        #endregion
    }
}