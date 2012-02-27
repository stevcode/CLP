using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
using System.Windows.Media.Imaging;

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
    public class CLPPageViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel(CLPNotebookViewModel notebookViewModel) : this(new CLPPage(), notebookViewModel)
        {
        }

        public CLPNotebookViewModel NotebookViewModel { get; set; }

        public CLPPageViewModel(CLPPage page, CLPNotebookViewModel notebookViewModel)
        {
            NotebookViewModel = notebookViewModel;
            
            AppMessages.ChangeInkMode.Register(this, (newInkMode) =>
                                                                    {
                                                                        this.EditingMode = newInkMode;
                                                                    });

            AppMessages.ChangePlayback.Register(this, (playback) =>
            {
                if (this.PlaybackControlsVisibility == Visibility.Collapsed)
                    this.PlaybackControlsVisibility = Visibility.Visible;
                else
                    this.PlaybackControlsVisibility = Visibility.Collapsed;


            });
             
            Page = page;
            foreach (string stringStroke in page.Strokes)
            {
                Stroke stroke = StringToStroke(stringStroke);
                if (stroke.ContainsPropertyData(CLPPage.Mutable))
                {
                    if (stroke.GetPropertyData(CLPPage.Mutable).ToString() == "false")
                    {
                        _otherStrokes.Add(stroke);
                    }
                    else
                    {
                        _strokes.Add(stroke);
                    }
                }
            }
            foreach (var pageObject in page.PageObjects)
            {
                CLPPageObjectBaseViewModel pageObjectViewModel = null;
                if (pageObject is CLPImage)
                {
                    pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, this);      
                }
                else if (pageObject is CLPStamp)
                {
                    pageObjectViewModel = new CLPStampViewModel(pageObject as CLPStamp, this);
                }
                else if (pageObject is CLPTextBox)
                {
                    pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, this);
                }
                else if (pageObject is CLPSquare)
                {
                    pageObjectViewModel = new CLPSquareViewModel(pageObject as CLPSquare, this);
                }
                else if (pageObject is CLPCircle)
                {
                    pageObjectViewModel = new CLPCircleViewModel(pageObject as CLPCircle, this);
                }
                else if (pageObject is CLPSnapTile)
                {
                    pageObjectViewModel = new CLPSnapTileViewModel(pageObject as CLPSnapTile, this);
                }

                PageObjectContainerViewModel pageObjectContainer = new PageObjectContainerViewModel(pageObjectViewModel);

                PageObjectContainerViewModels.Add(pageObjectContainer);
            }

            _strokes.StrokesChanged += new StrokeCollectionChangedEventHandler(_strokes_StrokesChanged);
            _pageObjectContainerViewModels.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_pageObjectContainerViewModels_CollectionChanged);

            _historyVM = new CLPHistoryViewModel(this, page.PageHistory);
             this.Avm = new AudioViewModel(page.MetaData.GetValue("UniqueID"));
        }

        void _pageObjectContainerViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
        }
        public bool undoFlag;
        void _strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;

            foreach (var stroke in e.Removed)
            {
                Page.Strokes.Remove(StrokeToString(stroke));
                if (!undoFlag)
                {
                    CLPHistoryItem item = new CLPHistoryItem("ERASE");
                    HistoryVM.AddHistoryItem(stroke, item);
                }
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
                stroke.AddPropertyData(CLPPage.Mutable, "true");
                Page.Strokes.Add(StrokeToString(stroke));
                if (!undoFlag)
                {
                    CLPHistoryItem item = new CLPHistoryItem("ADD");
                    HistoryVM.AddHistoryItem(stroke, item);
                }
            }
            
            
            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                List<string> add = new List<string>(StrokesToStrings(addedStrokes));
                List<string> remove = new List<string>(StrokesToStrings(e.Removed));
                if (Page.IsSubmission)
                {
                    if (App.Peer.Channel != null)
                    {
                        App.Peer.Channel.BroadcastInk(add, remove, new Tuple<bool,string,string>(true,Page.UniqueID, Page.SubmissionID));
                    }
                }
                else
                {
                    if (App.Peer.Channel != null)
                    {
                        App.Peer.Channel.BroadcastInk(add, remove, new Tuple<bool, string, string>(false, Page.UniqueID, ""));
                    }
                }
                
            }


            foreach (PageObjectContainerViewModel pageObjectContainerViewModel in PageObjectContainerViewModels)
            {
                //add bool to pageObjectBase for accept strokes, that way you don't need to check if it's over if it's not going to accept

                Rect rect = new Rect(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y, pageObjectContainerViewModel.Width, pageObjectContainerViewModel.Height);

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
                pageObjectContainerViewModel.PageObjectViewModel.AcceptStrokes(addedStrokesOverObject, removedStrokesOverObject);
            }
        }

        #endregion //Constructors

        #region Properties

        private CLPPage _page;
        public CLPPage Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }
        private CLPHistoryViewModel _historyVM;
        public CLPHistoryViewModel HistoryVM
        {
            get
            {
                return _historyVM;
            }
            set
            {
                _historyVM = value;
            }
        }
        private AudioViewModel _avm;
        public AudioViewModel Avm
        {
            get
            {
                return _avm;
            }
            set
            {
                _avm = value;
            }
        }
        public string SubmitterName
        {
            get
            {
                return Page.SubmitterName;
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
        
        #endregion //Properties

        #region Bindings

        private StrokeCollection _strokes = new StrokeCollection();
        public StrokeCollection Strokes
        {
            get
            {
                return _strokes;
            }
        }

        private StrokeCollection _otherStrokes = new StrokeCollection();
        public StrokeCollection OtherStrokes
        {
            get
            {
                return _otherStrokes;
            }
        }
        
        private readonly ObservableCollection<PageObjectContainerViewModel> _pageObjectContainerViewModels = new ObservableCollection<PageObjectContainerViewModel>();
        public ObservableCollection<PageObjectContainerViewModel> PageObjectContainerViewModels
        {
            get
            {
                return _pageObjectContainerViewModels;
            }
        }
        public const string PlaybackImagePropertyName = "PlaybackImage";
        private Uri _playbackImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
        public Uri PlaybackImage
        {
            get
            {
                return _playbackImage;
            }
            set
            {
                _playbackImage = value;

                RaisePropertyChanged("PlaybackImage");
            }
        }
        /// <summary>
        /// The <see cref="EditingMode" /> property's name.
        /// </summary>
        public const string EditingModePropertyName = "EditingMode";

        private InkCanvasEditingMode _editingMode = InkCanvasEditingMode.None;

        /// <summary>
        /// Sets and gets the EditingMode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get
            {
                return _editingMode;
            }

            set
            {
                if (_editingMode == value)
                {
                    return;
                }

                _editingMode = value;
                RaisePropertyChanged(EditingModePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DefaultDA" /> property's name.
        /// </summary>
        public const string DefaultDAPropertyName = "DefaultDA";

        private DrawingAttributes _defaultDrawingAttributes = new DrawingAttributes();

        /// <summary>
        /// Sets and gets the DefaultDA property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DrawingAttributes DefaultDA
        {
            get
            {
                return _defaultDrawingAttributes;
            }

            set
            {
                if (_defaultDrawingAttributes == value)
                {
                    return;
                }

                _defaultDrawingAttributes = value;
                RaisePropertyChanged(DefaultDAPropertyName);
            }
        }

        public const string NumberOfSubmissionsPropertyName = "NumberOfSubmissions";

        public int NumberOfSubmissions
        {
            get
            {
                return NotebookViewModel.SubmissionViewModels[Page.UniqueID].Count;
            }
            set
            {
                RaisePropertyChanged(NumberOfSubmissionsPropertyName);
            }
        }


        #endregion //Bindings

        #region Methods

        public static Stroke StringToStroke(string stroke)
        {
            StrokeCollectionConverter converter = new StrokeCollectionConverter();
            StrokeCollection sc = new StrokeCollection();
            sc = (StrokeCollection)converter.ConvertFromString(stroke);
            return sc[0];
        }

        public static string StrokeToString(Stroke stroke)
        {
            StrokeCollection sc = new StrokeCollection();
            sc.Add(stroke);
            StrokeCollectionConverter converter = new StrokeCollectionConverter();
            string stringStroke = (string)converter.ConvertToString(sc);
            return stringStroke;
        }

        /**
         * Helper method that converts a ObservableCollection of strings to a StrokeCollection
         */
        public static StrokeCollection StringsToStrokes(ObservableCollection<string> strings)
        {
            StrokeCollection strokes = new StrokeCollection();
            foreach (string s in strings)
            {
                strokes.Add(CLPPageViewModel.StringToStroke(s));
            }
            return strokes;
        }

        /**
         * Helper method that converts a StrokeCollection to an ObservableCollection of strings
         */
        public static ObservableCollection<string> StrokesToStrings(StrokeCollection strokes)
        {
            ObservableCollection<string> strings = new ObservableCollection<string>();
            foreach (Stroke stroke in strokes)
            {
                strings.Add(CLPPageViewModel.StrokeToString(stroke));
            }
            return strings;
        }
       
        #endregion //Methods

        #region Commands
       private RelayCommand _startPlaybackCommand;

        /// <summary>
        /// Gets the StartPlaybackCommand.
        /// </summary>
       private delegate void NoArgDelegate();
        public RelayCommand StartPlaybackCommand
        {
            get
            {
                return _startPlaybackCommand
                    ?? (_startPlaybackCommand = new RelayCommand(
                                          () =>
                                          {
                                              //Console.WriteLine("PageVM startplayback");
                                              // Start fetching the playback items asynchronously.
                                          /*    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    HistoryVM.start_pausePlayback();
                    return null;
                }, null);
                                              */
                                              NoArgDelegate fetcher = new NoArgDelegate(HistoryVM.start_pausePlayback);
                                              fetcher.BeginInvoke(null, null);
                                              

                                          }));
            }
        }
        
  
        
        private RelayCommand _stopPlaybackCommand;

        /// <summary>
        /// Gets the StartPlaybackCommand.
        /// </summary>
        public RelayCommand StopPlaybackCommand
        {
            get
            {
                return _stopPlaybackCommand
                    ?? (_stopPlaybackCommand = new RelayCommand(
                                          () =>
                                          {
                                              NoArgDelegate fetcher = new NoArgDelegate(HistoryVM.stopPlayback);
                                              fetcher.BeginInvoke(null, null);
                                            
                                          }));
            }
        }
        private RelayCommand _pausePlaybackCommand;
        public RelayCommand PausePlaybackCommand
        {
            get
            {
                return _pausePlaybackCommand
                    ?? (_pausePlaybackCommand = new RelayCommand(
                                          () =>
                                          {
                                              NoArgDelegate fetcher = new NoArgDelegate(HistoryVM.pausePlayback);
                                              fetcher.BeginInvoke(null, null);

                                          }));
            }
        }
        #endregion //Commands
    }
}