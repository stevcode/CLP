using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof (MajorRibbonViewModel))]
    public abstract class ACLPPageBaseViewModel : ViewModelBase
    {
        #region Constructor

        protected IPageInteractionService PageInteractionService;

        /// <summary>Initializes a new instance of the CLPPageViewModel class.</summary>
        protected ACLPPageBaseViewModel(CLPPage page)
        {
            Page = page;
            PageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            Submissions.CollectionChanged += Submissions_CollectionChanged;

            MouseMoveCommand = new Command<MouseEventArgs>(OnMouseMoveCommandExecute);
            MouseDownCommand = new Command<MouseEventArgs>(OnMouseDownCommandExecute);
            MouseUpCommand = new Command<MouseEventArgs>(OnMouseUpCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
            SetCorrectnessCommand = new Command<string>(OnSetCorrectnessCommandExecute);
        }

        public override string Title
        {
            get { return "APageBaseVM"; }
        }

        #endregion //Constructor

        #region Overrides of ViewModelBase

        protected override void OnClosing()
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            Submissions.CollectionChanged -= Submissions_CollectionChanged;
            base.OnClosing();
        }

        #endregion

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            private set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof (CLPPage));

        /// <summary>The thumbnail for the <see cref="CLPPage" />
        /// </summary>
        [ViewModelToModel("Page")]
        public ImageSource PageThumbnail
        {
            get { return GetValue<ImageSource>(PageThumbnailProperty); }
            set { SetValue(PageThumbnailProperty, value); }
        }

        public static readonly PropertyData PageThumbnailProperty = RegisterProperty("PageThumbnail", typeof (ImageSource));

        /// <summary>The type of page.</summary>
        [ViewModelToModel("Page")]
        public PageTypes PageType
        {
            get { return GetValue<PageTypes>(PageTypeProperty); }
            set { SetValue(PageTypeProperty, value); }
        }

        public static readonly PropertyData PageTypeProperty = RegisterProperty("PageType", typeof (PageTypes));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("Page")]
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            set { SetValue(InkStrokesProperty, value); }
        }

        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof (StrokeCollection));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<IPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<IPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof (ObservableCollection<IPageObject>));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("Page")]
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof (double));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("Page")]
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof (double));

        /// <summary>Submissionf for the page.</summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<CLPPage> Submissions
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionsProperty); }
            set { SetValue(SubmissionsProperty, value); }
        }

        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof (ObservableCollection<CLPPage>));

        [ViewModelToModel("Page")]
        public PageHistory History
        {
            get { return GetValue<PageHistory>(HistoryProperty); }
            set { SetValue(HistoryProperty, value); }
        }

        public static readonly PropertyData HistoryProperty = RegisterProperty("History", typeof (PageHistory));

        #endregion //Model

        #region Bindings

        /// <summary>Signifies the viewModel's view is a CLPPagePreviewView.</summary>
        public bool IsPagePreview
        {
            get { return GetValue<bool>(IsPagePreviewProperty); }
            set { SetValue(IsPagePreviewProperty, value); }
        }

        public static readonly PropertyData IsPagePreviewProperty = RegisterProperty("IsPagePreview", typeof (bool), true);

        /// <summary>Gets or sets the property value.</summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof (InkCanvasEditingMode), InkCanvasEditingMode.Ink);

        /// <summary>Gets or sets the property value.</summary>
        public DrawingAttributes DefaultDA
        {
            get { return GetValue<DrawingAttributes>(DefaultDAProperty); }
            set { SetValue(DefaultDAProperty, value); }
        }

        public static readonly PropertyData DefaultDAProperty = RegisterProperty("DefaultDA", typeof (DrawingAttributes), () => new DrawingAttributes());

        /// <summary>Gets or sets the property value.</summary>
        public InkCanvasEditingMode EraserMode
        {
            get { return GetValue<InkCanvasEditingMode>(EraserModeProperty); }
            set { SetValue(EraserModeProperty, value); }
        }

        public static readonly PropertyData EraserModeProperty = RegisterProperty("EraserMode", typeof (InkCanvasEditingMode), InkCanvasEditingMode.EraseByStroke);

        /// <summary>Gets or sets the property value.</summary>
        public bool IsInkCanvasHitTestVisible
        {
            get { return GetValue<bool>(IsInkCanvasHitTestVisibleProperty); }
            set { SetValue(IsInkCanvasHitTestVisibleProperty, value); }
        }

        public static readonly PropertyData IsInkCanvasHitTestVisibleProperty = RegisterProperty("IsInkCanvasHitTestVisible", typeof (bool), true);

        /// <summary>Sets the page's visible cursor.</summary>
        public Cursor PageCursor
        {
            get { return GetValue<Cursor>(PageCursorProperty); }
            set { SetValue(PageCursorProperty, value); }
        }

        public static readonly PropertyData PageCursorProperty = RegisterProperty("PageCursor", typeof (Cursor), Cursors.Pen);

        /// <summary>Forces the InkCanvas to use custom, imported cursors instead of the default ones.</summary>
        public bool IsUsingCustomCursors
        {
            get { return GetValue<bool>(IsUsingCustomCursorsProperty); }
            set { SetValue(IsUsingCustomCursorsProperty, value); }
        }

        public static readonly PropertyData IsUsingCustomCursorsProperty = RegisterProperty("IsUsingCustomCursors", typeof (bool), false);

        #region Calculated Properties

        public bool IsStarred
        {
            get
            {
                var starredTag = Page.Tags.FirstOrDefault(x => x is StarredTag) as StarredTag;
                if (starredTag == null)
                {
                    return false;
                }
                return starredTag.Value == StarredTag.AcceptedValues.Starred;
            }
            set
            {
                Page.AddTag(value
                                ? new StarredTag(Page, Origin.Teacher, StarredTag.AcceptedValues.Starred)
                                : new StarredTag(Page, Origin.Teacher, StarredTag.AcceptedValues.Unstarred));
            }
        }

        public bool IsDotted
        {
            get
            {
                var dottedTag = Page.Tags.FirstOrDefault(x => x is DottedTag) as DottedTag;
                if (dottedTag == null)
                {
                    return false;
                }
                return dottedTag.Value == DottedTag.AcceptedValues.Dotted;
            }
            set
            {
                Page.AddTag(value ? new DottedTag(Page, Origin.Teacher, DottedTag.AcceptedValues.Dotted) : new DottedTag(Page, Origin.Teacher, DottedTag.AcceptedValues.Undotted));
            }
        }

        public Correctness Correctness
        {
            get
            {
                var correctnessTag = Page.Tags.FirstOrDefault(x => x is CorrectnessTag) as CorrectnessTag;
                return correctnessTag == null ? Correctness.Unknown : correctnessTag.Correctness;
            }
            set
            {
                Page.AddTag(new CorrectnessTag(Page, Origin.Teacher, value, false));
                SetValue(CorrectnessProperty, value);
            }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof (Correctness), Correctness.Unknown);

        /// <summary>Whether the page has submissions or not.</summary>
        public bool HasSubmissions
        {
            get { return Submissions.Any() || Page.LastVersionIndex != null; }
        }

        public int NumberOfDistinctSubmissions
        {
            get { return Submissions.Select(submission => submission.OwnerID).Distinct().Count(); }
        }

        #endregion //Calculated Properties

        #endregion //Bindings

        #region Commands

        public Canvas TopCanvas;

        /// <summary>Gets the MouseMoveCommand command.</summary>
        public Command<MouseEventArgs> MouseMoveCommand { get; private set; }

        private void OnMouseMoveCommandExecute(MouseEventArgs e)
        {
            if (TopCanvas == null ||
                IsPagePreview ||
                PageInteractionService.CurrentPageInteractionMode == PageInteractionModes.Draw)
            {
                return;
            }
        }

        /// <summary>Gets the MouseDownCommand command.</summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e)
        {
            if (PageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Select ||
                TopCanvas == null ||
                IsPagePreview)
            {
                return;
            }

            var point = e.GetPosition(TopCanvas);
            var isOverPageObject = false;

            foreach (var pageObject in PageObjects)
            {
                isOverPageObject = IsPointOverPageObject(pageObject, point) &&
                                   !(App.MainWindowViewModel.CurrentUser.ID != pageObject.CreatorID && !pageObject.IsManipulatableByNonCreator);
                if (isOverPageObject)
                {
                    break;
                }
            }

            if (isOverPageObject)
            {
                return;
            }

            ClearAdorners();
        }

        /// <summary>Gets the MouseUpCommand command.</summary>
        public Command<MouseEventArgs> MouseUpCommand { get; private set; }

        private void OnMouseUpCommandExecute(MouseEventArgs e) { }

        /// <summary>Clears all non-background pageObjects, all strokes, and deletes History. If in AuthoringMode, even background pageObjects will be removed.</summary>
        public Command ClearPageCommand { get; private set; }

        private void OnClearPageCommandExecute()
        {
            if (
                MessageBox.Show("Are you sure you want to clear everything on this page? All strokes, arrays, and animations will be erased!",
                                "Warning!",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var pageObjectsToRemove = PageObjects.Where(pageObject => App.MainWindowViewModel.CurrentUser.ID == pageObject.CreatorID).ToList();
            foreach (var pageObject in pageObjectsToRemove)
            {
                PageObjects.Remove(pageObject);
            }

            Page.InkStrokes.Clear();
            Page.SerializedStrokes.Clear();
            Page.History.ClearHistory();
        }

        /// <summary>Manually sets the Correctness of the page.</summary>
        public Command<string> SetCorrectnessCommand { get; private set; }

        private void OnSetCorrectnessCommandExecute(string buttonType)
        {
            if (buttonType == "Correct")
            {
                Correctness = Correctness == Correctness.Correct ? Correctness.PartiallyCorrect : Correctness.Correct;
                return;
            }

            if (buttonType == "Incorrect")
            {
                Correctness = Correctness.Incorrect;
                return;
            }
        }

        #endregion //Commands

        #region Methods

        protected void Submissions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("HasSubmissions");
            RaisePropertyChanged("NumberOfDistinctSubmissions");
        }

        public static void ClearAdorners(CLPPage page)
        {
            if (page == null)
            {
                return;
            }
            foreach (var clpPageViewModel in ViewModelManager.GetViewModelsOfModel(page).OfType<ACLPPageBaseViewModel>())
            {
                clpPageViewModel.ClearAdorners();
            }
        }

        public void ClearAdorners()
        {
            if (PageObjects == null)
            {
                return;
            }
            foreach (var aclpPageObjectBaseViewModel in
                PageObjects.SelectMany(pageObject => ViewModelManager.GetViewModelsOfModel(pageObject)).OfType<APageObjectBaseViewModel>().ToList())
            {
                aclpPageObjectBaseViewModel.ClearAdorners();
            }
        }

        protected void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsPagePreview ||
                PageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None ||
                History.IsAnimating)
            {
                return;
            }

            try
            {
                foreach (var pageObject in PageObjects.OfType<IPageObjectAccepter>().Where(pageObject => pageObject.CanAcceptPageObjects))
                {
                    var removedPageObjects = new List<IPageObject>();
                    if (e.OldItems != null)
                    {
                        removedPageObjects.AddRange(e.OldItems.Cast<IPageObject>());
                    }

                    var addedPageObjects = new ObservableCollection<IPageObject>();
                    if (e.NewItems != null)
                    {
                        var o = pageObject;
                        foreach (var addedPageObject in
                            e.NewItems.Cast<IPageObject>()
                             .Where(
                                    addedPageObject =>
                                    o.ID != addedPageObject.ID && !o.AcceptedPageObjectIDs.Contains(addedPageObject.ID) && o.PageObjectIsOver(addedPageObject, .50)))
                        {
                            addedPageObjects.Add(addedPageObject);
                        }
                    }

                    pageObject.AcceptPageObjects(addedPageObjects, removedPageObjects);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
            }
        }

        protected void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (IsPagePreview || History.IsAnimating)
            {
                return;
            }

            if (History.RedoItems.Any())
            {
                InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                InkStrokes.Add(e.Removed);
                InkStrokes.Remove(e.Added);
                MessageBox.Show("Sorry, you need to play all the way to the end and then write.");
                InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                return;
            }

            StrokesChanged(e);

            //QueueTask(() => StrokesChanged(e));
        }

        private void StrokesChanged(StrokeCollectionChangedEventArgs e)
        {
            switch (PageInteractionService.CurrentPageInteractionMode)
            {
                case PageInteractionModes.Select:
                    break;
                case PageInteractionModes.Draw:
                    if (e.Removed.Any())
                    {
                        RemoveStrokes(e.Removed, e.Added);
                    }
                    else
                    {
                        var stroke = e.Added.FirstOrDefault();
                        if (stroke == null)
                        {
                            return;
                        }
                        AddStroke(stroke);
                    }
                    break;
                case PageInteractionModes.Erase:
                    RemoveStrokes(e.Removed, e.Added);
                    break;
                case PageInteractionModes.Lasso:
                {
                    var stroke = e.Added.FirstOrDefault();
                    if (stroke == null)
                    {
                        return;
                    }
                    LassoStroke(stroke);
                }
                    break;
                case PageInteractionModes.Cut:
                {
                    var stroke = e.Added.FirstOrDefault();
                    if (stroke == null)
                    {
                        return;
                    }
                    CutStroke(stroke);
                }
                    break;
                case PageInteractionModes.DividerCreation:
                {
                    var stroke = e.Added.FirstOrDefault();
                    if (stroke == null)
                    {
                        return;
                    }
                    DividerStroke(stroke);
                }
                    break;
            }
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "CanSendToTeacher" &&
                viewModel is RibbonViewModel)
            {
                RaisePropertyChanged("HasSubmissions");
            }

            if (propertyName == "IsSending" &&
                viewModel is RibbonViewModel)
            {
                RaisePropertyChanged("HasSubmissions");
            }

            if (IsPagePreview)
            {
                return;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        private void RefreshInkStrokes()
        {
            var pageObjects = Page.PageObjects;
            foreach (var pageObject in pageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.RefreshAcceptedStrokes();
            }
        }

        private void RefreshPageObjects(IEnumerable<IPageObject> pageObjects)
        {
            try
            {
                foreach (var pageObject in pageObjects.OfType<IPageObjectAccepter>())
                {
                    pageObject.RefreshAcceptedPageObjects();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
            }
        }

        #endregion //Methods   

        #region Page Interaction Methods

        private void RemoveStrokes(IEnumerable<Stroke> removedStrokes, IEnumerable<Stroke> addedStrokes)
        {
            try
            {
                //TODO: test to see if OwnerID == CurrentUser.ID. If not, remove CollectionChanged handler and re-add stroke

                //Avoid uniqueID duplication
                var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
                var removedStrokeIDs = removedStrokesList.Select(stroke => stroke.GetStrokeID()).ToList();

                var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
                foreach (var stroke in addedStrokesList)
                {
                    var newStrokeID = Guid.NewGuid().ToCompactID();
                    stroke.SetStrokeID(newStrokeID);
                    stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
                    stroke.SetStrokeVersionIndex(0);

                    //Ensures truly uniqueIDs
                    var strokeReference = stroke;
                    foreach (var newUniqueID in from id in removedStrokeIDs
                                                where id == strokeReference.GetStrokeID()
                                                select Guid.NewGuid().ToCompactID())
                    {
                        stroke.SetStrokeID(newUniqueID);
                    }
                }
                AcceptStrokes(addedStrokesList.ToList(), removedStrokesList.ToList());
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("InkStrokeCollectionChanged Exception: " + ex.Message);
                Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " + (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
            }
        }

        private void AddStroke(Stroke stroke)
        {
            try
            {
                if (stroke.HasStrokeID())
                {
                    return;
                }

                var strokeID = Guid.NewGuid().ToCompactID();
                stroke.SetStrokeID(strokeID);
                stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
                stroke.SetStrokeVersionIndex(0);

                var addedStrokes = new List<Stroke>
                                   {
                                       stroke
                                   };
                var removedStrokes = new List<Stroke>();
                AcceptStrokes(addedStrokes, removedStrokes);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("InkStrokeCollectionChanged Exception: " + ex.Message);
                Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " + (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
            }
        }

        private void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (PageInteractionService == null)
            {
                return;
            }

            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();

            var canInteract = PageInteractionService.IsInkInteracting;

            foreach (var pageObject in PageObjects.OfType<IStrokeAccepter>().Where(x => x.CreatorID == App.MainWindowViewModel.CurrentUser.ID || x.IsBackgroundInteractable))
            {
                bool didInteract;

                if (pageObject is CLPArray)
                {
                    didInteract = CLPArrayViewModel.InteractWithAcceptedStrokes(pageObject as CLPArray, addedStrokesList, removedStrokesList, canInteract);
                    if (didInteract)
                    {
                        return;
                    }
                }

                if (pageObject is MultipleChoiceBox)
                {
                    didInteract = MultipleChoiceBoxViewModel.InteractWithAcceptedStrokes(pageObject as MultipleChoiceBox, addedStrokesList, removedStrokesList, canInteract);
                    if (didInteract)
                    {
                        return;
                    }
                }

                if (pageObject is NumberLine)
                {
                    didInteract = NumberLineViewModel.InteractWithAcceptedStrokes(pageObject as NumberLine, addedStrokesList, removedStrokesList, canInteract);
                    if (didInteract)
                    {
                        return;
                    }
                }

                //BUG: Find way to limit stroke acceptance to single pageObject.
                pageObject.ChangeAcceptedStrokes(addedStrokesList, removedStrokesList);

                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                //                                           (DispatcherOperationCallback)delegate
                //                                           {
                //                                               pageObject.ChangeAcceptedStrokes(addedStrokesList, removedStrokesList);

                //                                               return null;
                //                                           },
                //                                           null);
            }

            AddHistoryItemToPage(Page, new ObjectsOnPageChangedHistoryItem(Page, App.MainWindowViewModel.CurrentUser, addedStrokesList, removedStrokesList));
        }

        private void LassoStroke(Stroke stroke)
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;

            Page.InkStrokes.Remove(stroke);

            var lassoedPageObjects = new List<IPageObject>();

            var strokeGeometry = new PathGeometry();
            var pathFigure = new PathFigure
                             {
                                 StartPoint = stroke.StylusPoints.First().ToPoint(),
                                 Segments = new PathSegmentCollection()
                             };
            var polyLine = new PolyLineSegment
                           {
                               Points = new PointCollection((Point[])stroke.StylusPoints)
                                        {
                                            stroke.StylusPoints.First().ToPoint()
                                        }
                           };
            pathFigure.Segments.Add(polyLine);

            strokeGeometry.Figures.Add(pathFigure);

            foreach (var pageObject in PageObjects)
            {
                if (App.MainWindowViewModel.CurrentUser.ID != pageObject.CreatorID &&
                    !pageObject.IsManipulatableByNonCreator)
                {
                    continue;
                }

                RectangleGeometry pageObjectGeometry;
                if (pageObject.Width > 10.0 ||
                    pageObject.Height > 10.0)
                {
                    pageObjectGeometry =
                        new RectangleGeometry(new Rect(pageObject.XPosition + Math.Max(pageObject.Width / 2 - 5.0, 0.0),
                                                       pageObject.YPosition + Math.Max(pageObject.Height / 2 - 5.0, 0.0),
                                                       Math.Min(10.0, pageObject.Width),
                                                       Math.Min(10.0, pageObject.Height)));
                }
                else
                {
                    pageObjectGeometry = new RectangleGeometry(new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width, pageObject.Height));
                }

                if (strokeGeometry.FillContains(pageObjectGeometry))
                {
                    lassoedPageObjects.Add(pageObject);
                }
            }

            var lassoedStrokes = InkStrokes.Where(inkStroke => inkStroke.HitTest(stroke.StylusPoints.Select(x => x.ToPoint()).ToList(), 90)).ToList();

            var pageObjectBoundsX1Position = Page.Width;
            var pageObjectBoundsY1Position = Page.Height;
            var pageObjectBoundsX2Position = 0.0;
            var pageObjectBoundsY2Position = 0.0;

            if (lassoedPageObjects.Any())
            {
                pageObjectBoundsX1Position = lassoedPageObjects.First().XPosition;
                pageObjectBoundsY1Position = lassoedPageObjects.First().YPosition;
                pageObjectBoundsX2Position = lassoedPageObjects.First().XPosition + lassoedPageObjects.First().Width;
                pageObjectBoundsY2Position = lassoedPageObjects.First().YPosition + lassoedPageObjects.First().Height;
                foreach (var pageObject in lassoedPageObjects)
                {
                    if (pageObject.XPosition < pageObjectBoundsX1Position)
                    {
                        pageObjectBoundsX1Position = pageObject.XPosition;
                    }
                    if (pageObject.YPosition < pageObjectBoundsY1Position)
                    {
                        pageObjectBoundsY1Position = pageObject.YPosition;
                    }
                    if (pageObject.XPosition + pageObject.Width > pageObjectBoundsX2Position)
                    {
                        pageObjectBoundsX2Position = pageObject.XPosition + pageObject.Width;
                    }
                    if (pageObject.YPosition + pageObject.Height > pageObjectBoundsY2Position)
                    {
                        pageObjectBoundsY2Position = pageObject.YPosition + pageObject.Height;
                    }
                }
            }

            var strokeBoundsX1Position = Page.Width;
            var strokeBoundsY1Position = Page.Height;
            var strokeBoundsX2Position = 0.0;
            var strokeBoundsY2Position = 0.0;

            if (lassoedStrokes.Any())
            {
                foreach (var bounds in lassoedStrokes.Select(s => s.GetBounds()))
                {
                    strokeBoundsX1Position = Math.Min(strokeBoundsX1Position, bounds.Left);
                    strokeBoundsY1Position = Math.Min(strokeBoundsY1Position, bounds.Top);
                    strokeBoundsX2Position = Math.Max(strokeBoundsX2Position, bounds.Right);
                    strokeBoundsY2Position = Math.Max(strokeBoundsY2Position, bounds.Bottom);
                }
            }

            if (lassoedPageObjects.Any() ||
                lassoedStrokes.Any())
            {
                var xPosition = Math.Min(pageObjectBoundsX1Position, strokeBoundsX1Position);
                var yPosition = Math.Min(pageObjectBoundsY1Position, strokeBoundsY1Position);
                var width = Math.Max(pageObjectBoundsX2Position, strokeBoundsX2Position) - xPosition;
                var height = Math.Max(pageObjectBoundsY2Position, strokeBoundsY2Position) - yPosition;

                var region = new LassoRegion(Page, lassoedPageObjects, new StrokeCollection(lassoedStrokes), xPosition, yPosition, height, width);
                AddPageObjectToPage(region, false);
            }

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
        }

        private void CutStroke(Stroke stroke)
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            var newUniqueID = Guid.NewGuid().ToCompactID();
            stroke.SetStrokeID(newUniqueID);
            stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
            stroke.SetStrokeVersionIndex(0);
            Page.InkStrokes.Remove(stroke);

            var pageObjectToCut =
                PageObjects.OfType<ICuttable>()
                           .Where(c => App.MainWindowViewModel.CurrentUser.ID == c.CreatorID || c.IsManipulatableByNonCreator)
                           .OrderBy(c => c.CuttingStrokeDistance(stroke))
                           .LastOrDefault();

            
            var halvedPageObjects = new List<IPageObject>();
            if (pageObjectToCut != null)
            {
                halvedPageObjects = pageObjectToCut.Cut(stroke);
            }

            var halvedPageObjectIDs = new List<string>();
            foreach (var pageObject in halvedPageObjects)
            {
                halvedPageObjectIDs.Add(pageObject.ID);
                AddPageObjectToPage(Page, pageObject, false, false);
            }
            AddHistoryItemToPage(Page, new PageObjectCutHistoryItem(Page, App.MainWindowViewModel.CurrentUser, stroke, pageObjectToCut, halvedPageObjectIDs));

            RefreshInkStrokes();
            RefreshPageObjects(halvedPageObjects);

            if (halvedPageObjects.Any())
            {
                if (pageObjectToCut != null && 
                    PageObjects.Contains(pageObjectToCut))
                {
                    PageObjects.Remove(pageObjectToCut);
                }
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
            }

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
        }

        private void DividerStroke(Stroke stroke)
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            var newUniqueId = Guid.NewGuid().ToCompactID();
            stroke.SetStrokeID(newUniqueId);
            stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
            stroke.SetStrokeVersionIndex(0);
            Page.InkStrokes.Remove(stroke);

            var wasArrayDivided = PageObjects.OfType<CLPArray>()
                                             .Where(array => App.MainWindowViewModel.CurrentUser.ID == array.CreatorID)
                                             .Aggregate(false, (current, array) => CLPArrayViewModel.CreateDivision(array, stroke) || current);
            if (wasArrayDivided)
            {
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
            }

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
        }

        #endregion //Page Interaction methods

        #region Static Methods

        public static void RemoveStrokes(CLPPage page, IEnumerable<Stroke> strokesToRemove)
        {
            var pageViewModel =
                CLPServiceAgent.Instance.GetViewModelsFromModel(page).First(x => (x is ACLPPageBaseViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview) as
                ACLPPageBaseViewModel;
            if (pageViewModel == null)
            {
                return;
            }
            var removedStrokes = strokesToRemove as IList<Stroke> ?? strokesToRemove.ToList();
            page.InkStrokes.Remove(new StrokeCollection(removedStrokes));
            pageViewModel.RemoveStrokes(removedStrokes, new List<Stroke>());
        }

        public static void TakePageThumbnail(CLPPage page)
        {
            var viewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(page);
            if (viewModels == null ||
                !viewModels.Any())
            {
                return;
            }
            var pageViewModel = viewModels.First(x => (x is ACLPPageBaseViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview);

            var viewManager = Catel.IoC.ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(pageViewModel);
            var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
            if (pageView == null)
            {
                return;
            }

            var thumbnail = CLPServiceAgent.Instance.UIElementToImageByteArray(pageView, 492);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.StreamSource = new MemoryStream(thumbnail);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            page.PageThumbnail = bitmapImage;
        }

        public static bool IsPointOverPageObject(IPageObject pageObject, Point point)
        {
            return pageObject.XPosition <= point.X && point.X <= pageObject.XPosition + pageObject.Width && pageObject.YPosition <= point.Y &&
                   point.Y <= pageObject.YPosition + pageObject.Height;
        }

        private static Task _currentTask = Task.FromResult(Type.Missing);
        private static readonly object _lock = new Object();

        public static void QueueTask(Action action)
        {
            lock (_lock)
            {
                _currentTask = _currentTask.ContinueWith(lastTask =>
                                                         {
                                                             // re-throw the error of the last completed task (if any)
                                                             try
                                                             {
                                                                 lastTask.GetAwaiter().GetResult();
                                                             }
                                                             catch (Exception ex)
                                                             {
                                                                 Logger.Instance.WriteToLog("Error on lastTask:");
                                                                 Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " +
                                                                                            (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                                                                 Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                                                                 Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                                                                 Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                                                                 Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
                                                             }
                                                             // run the new task
                                                             try
                                                             {
                                                                 action();
                                                             }
                                                             catch (Exception ex)
                                                             {
                                                                 Logger.Instance.WriteToLog("Error on task action execute:");
                                                                 Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " +
                                                                                            (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                                                                 Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                                                                 Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                                                                 Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                                                                 Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
                                                             }
                                                         },
                                                         CancellationToken.None,
                                                         TaskContinuationOptions.LazyCancellation,
                                                         TaskScheduler.Default);
            }
        }

        public static void AddHistoryItemToPage(CLPPage page, IHistoryItem historyItem, bool isBatch = false)
        {
            if (!isBatch)
            {
                page.History.AddHistoryItem(historyItem);
            }

            //IsBroadcastHistoryDisabled needs to take into account that the Property is now gone from the Ribbon.
            return;

            //if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher ||
            //    App.Network.ProjectorProxy == null ||
            //    App.MainWindowViewModel.Ribbon.IsBroadcastHistoryDisabled)
            //{
            //    return;
            //}

            QueueTask(() =>
                      {
                          var historyItemCopy = historyItem.CreatePackagedHistoryItem();
                          if (historyItemCopy == null)
                          {
                              Logger.Instance.WriteToLog("Failed to CreatePackagedHistoryItem");
                              return;
                          }
                          var historyItemString = ObjectSerializer.ToString(historyItemCopy);
                          var zippedHistoryItem = CLPServiceAgent.Instance.Zip(historyItemString);

                          try
                          {
                              var compositePageID = page.ID + ";" + page.OwnerID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex;
                              App.Network.ProjectorProxy.AddHistoryItem(compositePageID, zippedHistoryItem);
                          }
                          catch (Exception)
                          {
                              Logger.Instance.WriteToLog("Failed to send historyItem to Projector");
                          }

                          //if(!App.MainWindowViewModel.Ribbon.BroadcastInkToStudents || page.SubmissionType != SubmissionType.None || !App.Network.ClassList.Any())
                          //{
                          //    return;
                          //}

                          //foreach(var student in App.Network.ClassList)
                          //{
                          //    try
                          //    {
                          //        var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
                          //        studentProxy.ModifyPageInkStrokes(add, remove, pageID);
                          //        (studentProxy as ICommunicationObject).Close();
                          //    }
                          //    catch(Exception)
                          //    {
                          //    }
                          //}
                      });
        }

        public static void AddPageObjectToPage(IPageObject pageObject, bool addToHistory = true, bool forceSelectMode = true, int index = -1)
        {
            var parentPage = pageObject.ParentPage;
            if (parentPage == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in AddPageObjectToPage().");
                return;
            }
            AddPageObjectToPage(parentPage, pageObject, addToHistory, forceSelectMode, index);
        }

        public static void AddPageObjectToPage(CLPPage page, IPageObject pageObject, bool addToHistory = true, bool forceSelectMode = true, int index = -1)
        {
            if (page == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in AddPageObjectToPage().");
                return;
            }
            if (string.IsNullOrEmpty(pageObject.CreatorID))
            {
                pageObject.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
            }

            if (index == -1)
            {
                page.PageObjects.Add(pageObject);
            }
            else
            {
                page.PageObjects.Insert(index, pageObject);
            }

            if (addToHistory)
            {
                var pageObjectIDs = new List<string>
                                    {
                                        pageObject.ID
                                    };

                AddHistoryItemToPage(page, new PageObjectsAddedHistoryItem(page, App.MainWindowViewModel.CurrentUser, pageObjectIDs));
            }

            pageObject.OnAdded();

            if (forceSelectMode)
            {
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
            }
        }

        public static void AddPageObjectsToPage(CLPPage page, IEnumerable<IPageObject> pageObjects, bool addToHistory = true, bool forceSelectMode = true)
        {
            var pageObjectIDs = new List<string>();
            foreach (var pageObject in pageObjects)
            {
                if (string.IsNullOrEmpty(pageObject.CreatorID))
                {
                    pageObject.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
                }
                pageObjectIDs.Add(pageObject.ID);
                page.PageObjects.Add(pageObject);
            }

            if (addToHistory)
            {
                AddHistoryItemToPage(page, new PageObjectsAddedHistoryItem(page, App.MainWindowViewModel.CurrentUser, pageObjectIDs));
            }

            foreach (var pageObject in pageObjects)
            {
                pageObject.OnAdded();
            }

            if (forceSelectMode)
            {
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
            }
        }

        public static void RemovePageObjectFromPage(CLPPage page, IPageObject pageObject, bool addToHistory = true)
        {
            if (page == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in RemovePageObjectFromPage().");
                return;
            }

            if (addToHistory)
            {
                AddHistoryItemToPage(page,
                                     new PageObjectsRemovedHistoryItem(page,
                                                                       App.MainWindowViewModel.CurrentUser,
                                                                       new List<IPageObject>
                                                                       {
                                                                           pageObject
                                                                       }));
            }

            page.PageObjects.Remove(pageObject);
            pageObject.OnDeleted();
        }

        public static void RemovePageObjectFromPage(IPageObject pageObject, bool addToHistory = true)
        {
            var parentPage = pageObject.ParentPage;
            if (parentPage == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in RemovePageObjectFromPage().");
                return;
            }
            RemovePageObjectFromPage(parentPage, pageObject, addToHistory);
        }

        public static void RemovePageObjectsFromPage(CLPPage page, List<IPageObject> pageObjects, bool addToHistory = true)
        {
            if (page == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in RemovePageObjectFromPage().");
                return;
            }

            if (addToHistory)
            {
                AddHistoryItemToPage(page, new PageObjectsRemovedHistoryItem(page, App.MainWindowViewModel.CurrentUser, pageObjects));
            }

            foreach (var pageObject in pageObjects)
            {
                page.PageObjects.Remove(pageObject);
                pageObject.OnDeleted();
            }
        }

        #endregion //Static Methods
    }
}