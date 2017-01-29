using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
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
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof (MajorRibbonViewModel))]
    public abstract class ACLPPageBaseViewModel : ViewModelBase
    {
        #region Constructor

        private readonly IDataService _dataService;
        private readonly IPageInteractionService _pageInteractionService;

        /// <summary>Initializes a new instance of the CLPPageViewModel class.</summary>
        protected ACLPPageBaseViewModel(CLPPage page, IDataService dataService)
        {
            Page = page;
            _dataService = dataService;
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            Submissions.CollectionChanged += Submissions_CollectionChanged;

            MouseMoveCommand = new Command<MouseEventArgs>(OnMouseMoveCommandExecute);
            MouseDownCommand = new Command<MouseEventArgs>(OnMouseDownCommandExecute);
            MouseUpCommand = new Command<MouseEventArgs>(OnMouseUpCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
            SetCorrectnessCommand = new Command<string>(OnSetCorrectnessCommandExecute);

            InitializedAsync += ACLPPageBaseViewModel_InitializedAsync;
            ClosedAsync += ACLPPageBaseViewModel_ClosedAsync;
        }

        private Task ACLPPageBaseViewModel_InitializedAsync(object sender, EventArgs e)
        {
            _dataService.CurrentPageChanged += _dataService_CurrentPageChanged;

            return TaskHelper.Completed;
        }

        private Task ACLPPageBaseViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentNotebookChanged -= _dataService_CurrentPageChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentPageChanged(object sender, EventArgs e)
        {
            ClearAdorners();
        }

        #endregion //Constructor

        #region Overrides of ViewModelBase

        protected override async Task OnClosingAsync()
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            Submissions.CollectionChanged -= Submissions_CollectionChanged;
            await base.OnClosingAsync();
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
                RaisePropertyChanged(nameof(Correctness));
            }
        }

        /// <summary>Whether the page has submissions or not.</summary>
        public bool HasSubmissions
        {
            get
            {
                if (Page == null ||
                    Page.Owner == null)
                {
                    return false;
                }

                if (Page.Owner.IsStudent)
                {
                    return Submissions.Any() || Page.LastVersionIndex != null;
                }

                return _dataService.LoadedNotebooks.Any(n => n.Pages.Any(p => p.ID == Page.ID && p.Owner.IsStudent && p.VersionIndex != 0));
            }
        }

        public int NumberOfDistinctSubmissions
        {
            get
            {
                if (Page == null ||
                    Page.Owner == null)
                {
                    return 0;
                }

                if (Page.Owner.IsStudent)
                {
                    return Submissions.Count();
                }

                var count =
                    _dataService.LoadedNotebooks.Where(n => n.Owner.IsStudent)
                               .Select(n => n.Pages.Any(p => p.ID == Page.ID && p.Submissions.Any()) ? n.Owner.FullName : string.Empty)
                               .Where(s => !string.IsNullOrEmpty(s))
                               .Distinct()
                               .Count();

                return Math.Max(0, count);
            }
        }

        #endregion //Calculated Properties

        #endregion //Bindings

        #region Events

        protected void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsPagePreview ||
                _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None ||
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

                    pageObject.ChangeAcceptedPageObjects(addedPageObjects, removedPageObjects);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
            }
        }

        protected void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (IsPagePreview || History.IsAnimating)
            {
                return;
            }

            if (History.RedoActions.Any())
            {
                InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                InkStrokes.Add(e.Removed);
                InkStrokes.Remove(e.Added);
                MessageBox.Show("Sorry, you need to play all the way to the end and then write.", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                return;
            }

            StrokesChanged(e);

            //QueueTask(() => StrokesChanged(e));
        }

        #endregion //Events

        #region Commands

        #region Canvas Commands

        public Canvas TopCanvas;

        /// <summary>Gets the MouseMoveCommand command.</summary>
        public Command<MouseEventArgs> MouseMoveCommand { get; private set; }

        private void OnMouseMoveCommandExecute(MouseEventArgs e)
        {
            if (TopCanvas == null ||
                IsPagePreview ||
                _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.Draw)
            {
                return;
            }
        }

        /// <summary>Gets the MouseDownCommand command.</summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e)
        {
            if (TopCanvas == null ||
                IsPagePreview ||
                _pageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Select)
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

        private void OnMouseUpCommandExecute(MouseEventArgs e)
        {
            if (TopCanvas == null ||
                IsPagePreview ||
                _pageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Mark)
            {
                return;
            }

            var point = e.GetPosition(TopCanvas);
            var newMark = new Mark(Page, _pageInteractionService.CurrentMarkShape, _pageInteractionService.PenColor.ToString())
                          {
                              XPosition = point.X - 10,
                              YPosition = point.Y - 10
                          };

            if (newMark.YPosition + newMark.Height >= Height)
            {
                newMark.YPosition = Height - newMark.Height;
            }
            if (newMark.XPosition + newMark.Width >= Width)
            {
                newMark.XPosition = Width - newMark.Width;
            }

            AddPageObjectToPage(newMark, forceSelectMode: false);
        }

        #endregion //Canvas Commands

        #region Obsolete Commands?

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

        #endregion //Obsolete Commands?

        #endregion //Commands

        #region Methods

        public void UpdateSubmissionCount()
        {
            RaisePropertyChanged("HasSubmissions");
            RaisePropertyChanged("NumberOfDistinctSubmissions");
        }

        protected void Submissions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("HasSubmissions");
            RaisePropertyChanged("NumberOfDistinctSubmissions");
        }

        public static void ClearAdorners(CLPPage page)
        {
            // TODO: Handle GridDisplays?
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

        private void StrokesChanged(StrokeCollectionChangedEventArgs e)
        {
            switch (_pageInteractionService.CurrentPageInteractionMode)
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
            //if (propertyName == "CanSendToTeacher" &&
            //    viewModel is RibbonViewModel)
            //{
            //    RaisePropertyChanged("HasSubmissions");
            //}

            //if (propertyName == "IsSending" &&
            //    viewModel is RibbonViewModel)
            //{
            //    RaisePropertyChanged("HasSubmissions");
            //}

            if (IsPagePreview)
            {
                return;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods   

        #region Page Interaction Methods

        private void RemoveStrokes(IEnumerable<Stroke> removedStrokes, IEnumerable<Stroke> addedStrokes)
        {
            try
            {
                var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
                var removedOwnerStrokes = new List<Stroke>();
                foreach (var stroke in removedStrokesList)
                {
                    //var strokeOwner = stroke.GetStrokeOwnerID();
                    //if (strokeOwner != App.MainWindowViewModel.CurrentUser.ID)
                    //{
                    //    InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                    //    InkStrokes.Add(stroke);
                    //    InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                    //    continue;
                    //}
                    removedOwnerStrokes.Add(stroke);
                }

                //Avoid uniqueID duplication
                var removedStrokeIDs = removedOwnerStrokes.Select(stroke => stroke.GetStrokeID()).ToList();

                var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
                foreach (var stroke in addedStrokesList)
                {
                    var newStrokeID = Guid.NewGuid().ToCompactID();
                    stroke.SetStrokeID(newStrokeID);
                    stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);

                    //Ensures truly uniqueIDs
                    var strokeReference = stroke;
                    foreach (var newUniqueID in from id in removedStrokeIDs
                                                where id == strokeReference.GetStrokeID()
                                                select Guid.NewGuid().ToCompactID())
                    {
                        stroke.SetStrokeID(newUniqueID);
                    }
                }

                ChangeAcceptedStrokes(addedStrokesList, removedOwnerStrokes);
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

                var addedStrokes = new List<Stroke>
                                   {
                                       stroke
                                   };

                ChangeAcceptedStrokes(addedStrokes, new List<Stroke>());
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

        private void ChangeAcceptedStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (_pageInteractionService == null)
            {
                return;
            }

            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();

            var canInteract = _pageInteractionService.IsInkInteracting;
            var didInteractionOccur = false;
            var isStrokeSingleCapture = true; //TODO: Implement duplicating of stroke to be captured by more than one PageObject

            foreach (var pageObject in PageObjects.OfType<IStrokeAccepter>().Where(x => x.CreatorID == App.MainWindowViewModel.CurrentUser.ID || x.IsBackgroundInteractable))
            {
                var didInteract = InteractWithChangedStrokes(pageObject, new List<Stroke>(), removedStrokesList, canInteract);

                if (didInteract)
                {
                    didInteractionOccur = true;
                    continue;
                }

                pageObject.ChangeAcceptedStrokes(new List<Stroke>(), removedStrokesList);
            }

            foreach (var stroke in addedStrokesList)
            {
                var validStrokeAccepters =
                    PageObjects.OfType<IStrokeAccepter>()
                               .Where(p => (p.CreatorID == App.MainWindowViewModel.CurrentUser.ID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke))
                               .ToList();
                if (isStrokeSingleCapture)
                {
                    IStrokeAccepter closestPageObject = null;
                    foreach (var pageObject in validStrokeAccepters)
                    {
                        if (closestPageObject == null)
                        {
                            closestPageObject = pageObject;
                            continue;
                        }

                        if (closestPageObject.PercentageOfStrokeOverPageObject(stroke) < pageObject.PercentageOfStrokeOverPageObject(stroke))
                        {
                            closestPageObject = pageObject;
                        }
                    }

                    if (closestPageObject == null)
                    {
                        continue;
                    }

                    var didInteract = InteractWithChangedStrokes(closestPageObject,
                                                                 new List<Stroke>
                                                                 {
                                                                     stroke
                                                                 },
                                                                 new List<Stroke>(),
                                                                 canInteract);
                    if (didInteract)
                    {
                        didInteractionOccur = true;
                        continue;
                    }

                    closestPageObject.ChangeAcceptedStrokes(new List<Stroke>
                                                            {
                                                                stroke
                                                            },
                                                            new List<Stroke>());
                }
            }

            if (!didInteractionOccur)
            {
                AddHistoryActionToPage(Page, new ObjectsOnPageChangedHistoryAction(Page, App.MainWindowViewModel.CurrentUser, addedStrokesList, removedStrokesList));
            }
        }

        private void LassoStroke(Stroke stroke)
        {
            // ALEX
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
            strokeGeometry.FillRule = FillRule.EvenOdd;

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
                    const double TOLERANCE = 0.5;
                    var width = pageObject.Width * TOLERANCE;
                    var height = pageObject.Height * TOLERANCE;
                    var xOffset = (pageObject.Width - width) / 2.0;
                    var yOffset = (pageObject.Height - height) / 2.0;
                    pageObjectGeometry =
                        new RectangleGeometry(new Rect(pageObject.XPosition + xOffset,
                                                       pageObject.YPosition + yOffset,
                                                       width,
                                                       height));
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

            var acceptedPageObjects = (from lassoedPageObject in lassoedPageObjects
                                       from pageObjectAccepter in lassoedPageObjects.OfType<IPageObjectAccepter>()
                                       where pageObjectAccepter.AcceptedPageObjectIDs.Contains(lassoedPageObject.ID)
                                       select lassoedPageObject).ToList();
            foreach (var acceptedPageObject in acceptedPageObjects)
            {
                lassoedPageObjects.Remove(acceptedPageObject);
            }

            var lassoedStrokes = new List<Stroke>(); // = InkStrokes.Where(inkStroke => inkStroke.HitTest(stroke.StylusPoints.Select(x => x.ToPoint()).ToList(), 90)).ToList();

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
            if (_pageInteractionService == null)
            {
                return;
            }

            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            var newUniqueID = Guid.NewGuid().ToCompactID();
            stroke.SetStrokeID(newUniqueID);
            stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
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

            if (!halvedPageObjects.Any())
            {
                pageObjectToCut = null;
            }
            var halvedPageObjectIDs = new List<string>();
            foreach (var pageObject in halvedPageObjects)
            {
                halvedPageObjectIDs.Add(pageObject.ID);
                AddPageObjectToPage(Page, pageObject, false, false);
            }
            AddHistoryActionToPage(Page, new PageObjectCutHistoryAction(Page, App.MainWindowViewModel.CurrentUser, stroke, pageObjectToCut, halvedPageObjectIDs));

            if (halvedPageObjects.Any())
            {
                var oldPageObjects = new List<IPageObject>
                                     {
                                         pageObjectToCut
                                     };
                AStrokeAccepter.SplitAcceptedStrokes(oldPageObjects, halvedPageObjects);
                APageObjectAccepter.SplitAcceptedPageObjects(oldPageObjects, halvedPageObjects);

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

        [Obsolete("Should now be taken care of by InteractWithChangedStrokes")]
        private void DividerStroke(Stroke stroke)
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            var newUniqueId = Guid.NewGuid().ToCompactID();
            stroke.SetStrokeID(newUniqueId);
            stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
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

        private static bool InteractWithChangedStrokes(IStrokeAccepter pageObject, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();

            bool didInteract;

            var multipleChoice = pageObject as MultipleChoice;
            if (multipleChoice != null)
            {
                didInteract = MultipleChoiceViewModel.InteractWithAcceptedStrokes(multipleChoice, addedStrokesList, removedStrokesList, canInteract);
                if (didInteract)
                {
                    return true;
                }
            }

            var interpretationRegion = pageObject as InterpretationRegion;
            if (interpretationRegion != null)
            {
                didInteract = InterpretationRegionViewModel.InteractWithAcceptedStrokes(interpretationRegion, addedStrokesList, removedStrokesList, canInteract);
                if (didInteract)
                {
                    return true;
                }
            }

            var array = pageObject as CLPArray;
            if (array != null)
            {
                didInteract = CLPArrayViewModel.InteractWithAcceptedStrokes(array, addedStrokesList, removedStrokesList, canInteract);
                if (didInteract)
                {
                    return true;
                }
            }

            var numberLine = pageObject as NumberLine;
            if (numberLine != null)
            {
                didInteract = NumberLineViewModel.InteractWithAcceptedStrokes(numberLine, addedStrokesList, removedStrokesList, canInteract);
                if (didInteract)
                {
                    return true;
                }
            }

            return false;
        }

        public static void RemoveStrokes(CLPPage page, IEnumerable<Stroke> strokesToRemove)
        {
            var pageViewModel = page.GetAllViewModels().First(x => (x is ACLPPageBaseViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview) as
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
            var viewModels = page.GetAllViewModels();
            if (viewModels == null ||
                !viewModels.Any())
            {
                return;
            }
            var pageViewModel = viewModels.First(x => (x is ACLPPageBaseViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview);

            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(pageViewModel);
            var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
            if (pageView == null)
            {
                return;
            }

            var bitmapImage = pageView.ToBitmapImage(492);
            page.PageThumbnail = bitmapImage;
        }

        public static bool IsPointOverPageObject(IPageObject pageObject, Point point)
        {
            return pageObject.XPosition <= point.X && point.X <= pageObject.XPosition + pageObject.Width && pageObject.YPosition <= point.Y &&
                   point.Y <= pageObject.YPosition + pageObject.Height;
        }

        private static readonly TaskQueue TaskQueue = new TaskQueue();

        public static void AddHistoryActionToPage(CLPPage page, IHistoryAction historyAction, bool isBatch = false)
        {
            if (!isBatch)
            {
                page.History.AddHistoryAction(historyAction);
            }

            //IsBroadcastHistoryDisabled needs to take into account that the Property is now gone from the Ribbon.
            //if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher ||
            //    App.Network.ProjectorProxy == null ||
            //    !(historyAction is ObjectsOnPageChangedHistoryAction))
            //{
            //    return;
            //}

            TaskQueue.Enqueue(async () =>
                               {
                                   var historyActionCopy = historyAction.CreatePackagedHistoryAction();
                                   if (historyActionCopy == null)
                                   {
                                       return;
                                   }

                                   //var st = Stopwatch.StartNew();
                                   //var jsonString = (historyActionCopy as AEntityBase).ToJsonString();
                                   //var zjson = jsonString.CompressWithGZip();
                                   //st.Stop();
                                   //var jTime = st.ElapsedMilliseconds;
                                   //var jLength = jsonString.Length;

                                   //st.Restart();
                                   //var backToJson = zjson.DecompressFromGZip();
                                   //var unjHistoryAction = AEntityBase.FromJsonString<object>(backToJson);
                                   //st.Stop();
                                   //var unjsonTime = st.ElapsedMilliseconds;

                                   //st.Restart();
                                   //var historyActionString = ObjectSerializer.ToString(historyActionCopy);
                                   //var zippedHistoryAction = historyActionString.CompressWithGZip();
                                   //st.Stop();
                                   //var zTime = st.ElapsedMilliseconds;
                                   //var toStringLength = historyActionString.Length;
                                   //var toZipLength = zippedHistoryAction.Length;

                                   //st.Restart();
                                   //var unzippedHistoryAction = zippedHistoryAction.DecompressFromGZip();
                                   //var uhistoryAction = ObjectSerializer.ToObject(unzippedHistoryAction) as IHistoryAction;
                                   //st.Stop();
                                   //var unzipTime = st.ElapsedMilliseconds;

                                   //Debug.WriteLine();
                                   //Debug.WriteLine("Json conversion time: {0}", jTime);
                                   //Debug.WriteLine("Zip conversion time {0}", zTime);
                                   //Debug.WriteLine("UnJson conversion time: {0}", unjsonTime);
                                   //Debug.WriteLine("UnZip conversion time {0}", unzipTime);
                                   //Debug.WriteLine("Json string length: {0}", jLength);
                                   //Debug.WriteLine("Json zipped length: {0}", zjson.Length);
                                   //Debug.WriteLine("ToString string length: {0}", toStringLength);
                                   //Debug.WriteLine("Zip string length: {0}", toZipLength);

                                   //try
                                   //{
                                   //    var compositePageID = page.ID + ";" + page.OwnerID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex;
                                   //    App.Network.ProjectorProxy.AddHistoryAction(compositePageID, zippedHistoryAction);
                                   //}
                                   //catch (Exception)
                                   //{
                                   //    Logger.Instance.WriteToLog("Failed to send historyAction to Projector");
                                   //}

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
                AddHistoryActionToPage(page,
                                     new ObjectsOnPageChangedHistoryAction(page,
                                                                         App.MainWindowViewModel.CurrentUser,
                                                                         new List<IPageObject>
                                                                         {
                                                                             pageObject
                                                                         },
                                                                         new List<IPageObject>()));
            }

            pageObject.OnAdded();

            if (forceSelectMode)
            {
                App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
            }
        }

        public static void AddPageObjectsToPage(CLPPage page, IEnumerable<IPageObject> pageObjects, bool addToHistory = true, bool forceSelectMode = true)
        {
            var pageObjectsAdded = pageObjects as IList<IPageObject> ?? pageObjects.ToList();
            foreach (var pageObject in pageObjectsAdded)
            {
                if (string.IsNullOrEmpty(pageObject.CreatorID))
                {
                    pageObject.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
                }
                page.PageObjects.Add(pageObject);
            }

            if (addToHistory)
            {
                AddHistoryActionToPage(page, new ObjectsOnPageChangedHistoryAction(page, App.MainWindowViewModel.CurrentUser, pageObjectsAdded, new List<IPageObject>()));
            }

            foreach (var pageObject in pageObjectsAdded)
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
                AddHistoryActionToPage(page,
                                     new ObjectsOnPageChangedHistoryAction(page,
                                                                         App.MainWindowViewModel.CurrentUser,
                                                                         new List<IPageObject>(),
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
                AddHistoryActionToPage(page, new ObjectsOnPageChangedHistoryAction(page, App.MainWindowViewModel.CurrentUser, new List<IPageObject>(), pageObjects));
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