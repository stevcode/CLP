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
    public enum PageObjectAddMode
    {
        None,
        SnapTile
    }

    [InterestedIn(typeof(MainWindowViewModel))]
    public class CLPPageViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel(CLPPage page)
            : base()
        {
            PlaybackControlsVisibility = Visibility.Collapsed;
            DefaultDA = App.MainWindowViewModel.DrawingAttributes;
            EditingMode = App.MainWindowViewModel.EditingMode;
            PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
            NumberOfSubmissions = 0;

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

            StartPlaybackCommand = new Command(OnStartPlaybackCommandExecute);
            StopPlaybackCommand = new Command(OnStopPlaybackCommandExecute);

           /* CLPSnapTileContainer tile1 = new CLPSnapTileContainer(new Point(10, 10), "green");
            tile1.Tiles.Add("green");
            tile1.Height = tile1.Tiles.Count * CLPSnapTileContainer.TILE_HEIGHT;
            CLPServiceAgent.Instance.AddPageObjectToPage(Page, tile1);

            CLPSnapTileContainer tile2 = new CLPSnapTileContainer(new Point(200, 400), "green");
            tile2.Tiles.Add("green");
            tile2.Tiles.Add("green");
            tile2.Tiles.Add("green");
            tile2.Tiles.Add("green");
            tile2.Height = tile2.Tiles.Count * CLPSnapTileContainer.TILE_HEIGHT;
            CLPServiceAgent.Instance.AddPageObjectToPage(Page, tile2);
            */
            //AudioViewModel avm = new AudioViewModel(page.MetaData.GetValue("UniqueID"));
        }

        public override string Title { get { return "PageVM"; } }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
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
        [ViewModelToModel("Page", "Strokes")]
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
            set { SetValue(PageObjectsProperty, value); }
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
            set { SetValue(InkStrokesProperty, value); }
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

        //lock for the playback
        private static readonly object _locker = new object();
        #endregion //Properties

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); }
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the NumberOfSubmissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int));

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri PlaybackImage
        {
            get { return GetValue<Uri>(PlaybackImageProperty); }
            set { SetValue(PlaybackImageProperty, value); }
        }

        /// <summary>
        /// Register the PlaybackImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PlaybackImageProperty = RegisterProperty("PlaybackImage", typeof(Uri));

        #endregion //Bindings

        #region Methods

        void PageObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            App.MainWindowViewModel.CanSendToTeacher = true;
            Console.WriteLine("adding page ofject to page with uniqueID: " + Page.UniqueID);
        }

        void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            Console.WriteLine("inking on pageVM: " + Page.UniqueID);
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            App.MainWindowViewModel.CanSendToTeacher = true;

            foreach (var stroke in e.Removed)
            {
                Page.Strokes.Remove(CLPPage.StrokeToString(stroke));
                //if (!undoFlag)
                //{
                //    //CLPHistoryItem item = new CLPHistoryItem("ERASE");
                //    //HistoryVM.AddHistoryItem(stroke, item);
                //}
                if (!PageHistory.IgnoreHistory)
                {
                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.EraseInk, stroke.GetPropertyData(CLPPage.StrokeIDKey).ToString(), null, null);
                    PageHistory.HistoryItems.Add(item);
                    PageHistory.TrashedInkStrokes.Add(stroke.GetPropertyData(CLPPage.StrokeIDKey).ToString(), CLPPage.StrokeToString(stroke));
                }
            }

            StrokeCollection addedStrokes = new StrokeCollection();
            foreach (Stroke stroke in e.Added)
            {
                if (!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                {
                    string newUniqueID = Guid.NewGuid().ToString();
                    stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                    stroke.AddPropertyData(CLPPage.ParentPageID, Page.UniqueID);
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
                StringStrokes.Add(CLPPage.StrokeToString(stroke));
                //if (!undoFlag)
                //{
                //    CLPHistoryItem item = new CLPHistoryItem("ADD");
                //    HistoryVM.AddHistoryItem(stroke, item);
                //}
                if (!PageHistory.IgnoreHistory)
                {
                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.AddInk, stroke.GetPropertyData(CLPPage.StrokeIDKey).ToString(), null, null);
                    PageHistory.HistoryItems.Add(item);
                }
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
                if (pageObject.CanAcceptStrokes)
                {
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

                    pageObject.AcceptStrokes(addedStrokesOverObject, removedStrokesOverObject);
                }
            }

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "EditingMode")
            {
                EditingMode = (viewModel as MainWindowViewModel).EditingMode;
            }

            if (propertyName == "IsPlaybackEnabled")
            {
                if ((viewModel as MainWindowViewModel).IsPlaybackEnabled)
                {
                    PlaybackControlsVisibility = Visibility.Visible;
                }
                else
                {
                    PlaybackControlsVisibility = Visibility.Collapsed;
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        protected override void Close()
        {
            Console.WriteLine("VM closed");
            base.Close();
        }

        public ICLPPageObject GetPageObjectByID(string uniqueID)
        {
            if (PageHistory.TrashedPageObjects.ContainsKey(uniqueID))
            {
                return PageHistory.TrashedPageObjects[uniqueID];
            }
            foreach (var pageObject in PageObjects)
            {
                if (pageObject.UniqueID == uniqueID)
                {
                    return pageObject;
                }
            }

            return null;
        }

        int i = 0;
        DispatcherTimer timer;
        bool hasReachedStart = false;
        bool hasReachedStop = false;
        public void StartPlayBack()
        {
            PlaybackImage = new Uri("..\\Images\\pause_blue.png", UriKind.Relative);
            while (PageHistory.HistoryItems.Count > 0)
            {
                Undo();
                i++;
            }
            System.Threading.Thread.Sleep(new TimeSpan(0, 0, 2));
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(0);
            timer.Tick += new EventHandler(timer_Tick);

            timer.Start();



        }

        void timer_Tick(object sender, EventArgs e)
        {
            
            if (i == 1)
            {
                Redo();
                i = 0;
                PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                timer.Stop();
                if (PlayingRecorded)
                {
                    hasReachedStart = false;
                    hasReachedStop = false;
                    PlayingRecorded = false;
                    App.MainWindowViewModel.PlayPauseVisualImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                    App.MainWindowViewModel.currentlyPlayingVisual = false;
                }
            }
            lock(_locker)
            {
                if(this.PageHistory.UndoneHistoryItems.Count >= 2 && i > 0)
                {
               
                    int len = this.PageHistory.UndoneHistoryItems.Count;
                    TimeSpan interval = this.PageHistory.UndoneHistoryItems[len - 2].CreationDate - this.PageHistory.UndoneHistoryItems[len - 1].CreationDate;
                    //if there's more than two seconds between the actions just wait for two seconds
                    if(interval > new TimeSpan(0, 0, 2))
                    {
                        interval = new TimeSpan(0, 0, 2);
                    }
                    timer.Interval = interval;
                    i--;
                }
           }
           if ((PlayingRecorded && !hasReachedStart) || (PlayingRecorded && hasReachedStop))
           {
               timer.Interval = new TimeSpan(0);
           }
           
           Redo();
        }

        public void Undo()
        {
            PageHistory.IgnoreHistory = true;
            if (PageHistory.HistoryItems.Count > 0)
            {
                CLPHistoryItem item = PageHistory.HistoryItems[PageHistory.HistoryItems.Count - 1];
                PageHistory.HistoryItems.Remove(item);
                ICLPPageObject pageObject = null;
                if (item.ObjectID != null)
                {
                    pageObject = GetPageObjectByID(item.ObjectID);
                }
                switch (item.ItemType)
                {
                    case HistoryItemType.AddPageObject:
                        if (pageObject != null)
                        {
                            PageHistory.TrashedPageObjects.Add(item.ObjectID, pageObject);
                            CLPServiceAgent.Instance.RemovePageObjectFromPage(Page, pageObject);
                        }
                        break;
                    case HistoryItemType.RemovePageObject:
                        CLPServiceAgent.Instance.AddPageObjectToPage(Page, ObjectSerializer.ToObject(item.OldValue) as ICLPPageObject);
                        break;
                    case HistoryItemType.MovePageObject:
                        if (pageObject != null)
                        {
                            CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, Point.Parse(item.OldValue));
                        }
                        break;
                    case HistoryItemType.ResizePageObject:
                        break;
                    case HistoryItemType.AddInk:
                        foreach (Stroke s in Page.InkStrokes )
                        {
                            if (s.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                            {
                                Page.InkStrokes.Remove(s);
                                PageHistory.TrashedInkStrokes.Add(s.GetPropertyData(CLPPage.StrokeIDKey).ToString(), CLPPage.StrokeToString(s));
                                break;
                            }
                        }
                        break;
                    case HistoryItemType.EraseInk:
                        foreach (string s in PageHistory.TrashedInkStrokes.Keys)
                        {
                            Stroke inkStroke = CLPPage.StringToStroke(PageHistory.TrashedInkStrokes[s]);
                            if (inkStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                            {
                                PageHistory.TrashedInkStrokes.Remove(s);
                                Page.InkStrokes.Add(inkStroke);
                                break;
                            }
                        }
                        break;
                    case HistoryItemType.SnapTileSnap:
                        break;
                    case HistoryItemType.SnapTileRemoveTile:
                        CLPSnapTileContainer tile = GetPageObjectByID(item.ObjectID) as CLPSnapTileContainer;
                        tile.Tiles.Add("SpringGreen");
                        break;
                    default:
                        break;
                }

                PageHistory.UndoneHistoryItems.Add(item);
            }
            PageHistory.IgnoreHistory = false;
        }

        public void Redo()
        {
            PageHistory.IgnoreHistory = true;
            if (PageHistory.UndoneHistoryItems.Count > 0)
            {
                CLPHistoryItem item = PageHistory.UndoneHistoryItems[PageHistory.UndoneHistoryItems.Count - 1];
                lock (_locker)
                {
                    PageHistory.UndoneHistoryItems.Remove(item);
                }
                 ICLPPageObject pageObject = null;
                 if (item.ObjectID != null)
                 {
                     pageObject = GetPageObjectByID(item.ObjectID);
                 }
                switch (item.ItemType)
                {
                    case HistoryItemType.StartRecord:
                        hasReachedStart = true;
                        break;
                    case HistoryItemType.StopRecord:
                        hasReachedStop = true;
                        break;
                    case HistoryItemType.AddPageObject:
                        if (pageObject != null)
                        {
                            CLPServiceAgent.Instance.AddPageObjectToPage(Page, pageObject);
                            PageHistory.TrashedPageObjects.Remove(item.ObjectID);
                        }
                        break;
                    case HistoryItemType.RemovePageObject:
                        CLPServiceAgent.Instance.RemovePageObjectFromPage(ObjectSerializer.ToObject(item.OldValue) as ICLPPageObject);
                        break;
                    case HistoryItemType.MovePageObject:
                        if (pageObject != null)
                        {
                            CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, Point.Parse(item.NewValue));
                        }
                        break;
                    case HistoryItemType.ResizePageObject:
                        break;
                    case HistoryItemType.AddInk:
                        foreach (string s in PageHistory.TrashedInkStrokes.Keys)
                        {
                            Stroke inkStroke = CLPPage.StringToStroke(PageHistory.TrashedInkStrokes[s]);
                            if (inkStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                            {
                                Page.InkStrokes.Add(inkStroke);
                                PageHistory.TrashedInkStrokes.Remove(s);
                                break;
                            }
                        }
                        break;
                    case HistoryItemType.EraseInk:
                        foreach (Stroke s in Page.InkStrokes)
                        {
                            if (s.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                            {
                                Page.InkStrokes.Remove(s);
                                PageHistory.TrashedInkStrokes.Add(s.GetPropertyData(CLPPage.StrokeIDKey).ToString(), CLPPage.StrokeToString(s));
                                break;
                            }
                        }
                        break;
                    case HistoryItemType.SnapTileSnap:
                        break;
                    case HistoryItemType.SnapTileRemoveTile:
                        CLPSnapTileContainer tile = GetPageObjectByID(item.ObjectID) as CLPSnapTileContainer;
                        tile.Tiles.RemoveAt(tile.Tiles.Count - 1);
                        break;
                    default:
                        break;
                }

                PageHistory.HistoryItems.Add(item);
            }

            PageHistory.IgnoreHistory = false;
        }
        bool PlayingRecorded = false;
        public void StartRecordedPlayback()
        {
            PlayingRecorded = true;
            OnStartPlaybackCommandExecute();
            
        }
        public void PausePlayback()
        {
            OnStartPlaybackCommandExecute();
        }
        public void StopPlayback()
        {
            OnStopPlaybackCommandExecute();
        }
        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the StartPlaybackCommand command.
        /// </summary>
        public Command StartPlaybackCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the StartPlaybackCommand command is executed.
        /// </summary>
        private void OnStartPlaybackCommandExecute()
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.IsEnabled = false;
                PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
            }
            else if (Page.PageHistory.UndoneHistoryItems.Count > 0)
            {
                timer.IsEnabled = true;
                PlaybackImage = new Uri("..\\Images\\pause_blue.png", UriKind.Relative);
            }
            else
            {
                StartPlayBack();
            }

        }

        /// <summary>
        /// Gets the StopPlaybackCommand command.
        /// </summary>
        public Command StopPlaybackCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the StopPlaybackCommand command is executed.
        /// </summary>
        private void OnStopPlaybackCommandExecute()
        {
            // TODO: Handle command logic here
            while (this.PageHistory.UndoneHistoryItems.Count > 0)
            {
                Redo();
                i--;
            }
            timer.Stop();
            if (PlayingRecorded)
            {
                hasReachedStart = false;
                hasReachedStop = false;
                PlayingRecorded = false;
                App.MainWindowViewModel.PlayPauseVisualImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                App.MainWindowViewModel.currentlyPlayingVisual = false;
            }
            PlaybackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
        }
       

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

    }
}