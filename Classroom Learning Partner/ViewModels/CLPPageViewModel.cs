using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Windows.Ink;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using System.Collections.Generic;
using System;

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
                else if (pageObject is CLPImageStamp)
                {
                    pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp, this);
                }
                else if (pageObject is CLPBlankStamp)
                {
                    pageObjectViewModel = new CLPBlankStampViewModel(pageObject as CLPBlankStamp, this);
                }
                else if (pageObject is CLPTextBox)
                {
                    pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, this);
                }

                PageObjectContainerViewModel pageObjectContainer = new PageObjectContainerViewModel(pageObjectViewModel);

                PageObjectContainerViewModels.Add(pageObjectContainer);
            }

            _strokes.StrokesChanged += new StrokeCollectionChangedEventHandler(_strokes_StrokesChanged);

            _historyVM = new CLPHistoryViewModel(page.PageHistory);
        }

        void _strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            //limit send to teacher by change bool value here



            StrokeCollection addedStrokes = new StrokeCollection();
            foreach (Stroke stroke in e.Added)
            {
                if (!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                {
                    string newUniqueID = Guid.NewGuid().ToString();
                    stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                }
                addedStrokes.Add(stroke);    
            }

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                List<string> add = new List<string>(StrokesToStrings(addedStrokes));
                List<string> remove = new List<string>(StrokesToStrings(e.Removed));
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

            foreach (var stroke in addedStrokes)
            {
                stroke.AddPropertyData(CLPPage.Mutable, "true");
                Page.Strokes.Add(StrokeToString(stroke));
            }
            foreach (var stroke in e.Removed)
            {
                Page.Strokes.Remove(StrokeToString(stroke));
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
        private CLPHistoryViewModel _historyVM = new CLPHistoryViewModel();
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

        public string SubmitterName
        {
            get
            {
                return Page.SubmitterName;
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
    }
}