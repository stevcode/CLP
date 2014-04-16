using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum PageInteractionMode
    {
        None,
        Select,
        Pen,
        Highlighter,
        PenAndSelect,
        Eraser,
        Lasso,
        Scissors,
        EditObjectProperties
    }

    [InterestedIn(typeof(RibbonViewModel))]
    public abstract class ACLPPageBaseViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        protected ACLPPageBaseViewModel(CLPPage page)
        {
            Page = page;

            PageInteractionMode = App.MainWindowViewModel.Ribbon.PageInteractionMode;
            EraserMode = App.MainWindowViewModel.Ribbon.EraserMode;
            DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            Submissions.CollectionChanged += Submissions_CollectionChanged;

            MouseMoveCommand = new Command<MouseEventArgs>(OnMouseMoveCommandExecute);
            MouseDownCommand = new Command<MouseEventArgs>(OnMouseDownCommandExecute);
            MouseUpCommand = new Command<MouseEventArgs>(OnMouseUpCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
        }

        public override string Title
        {
            get { return "APageBaseVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            private set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            set { SetValue(InkStrokesProperty, value); }
        }

        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<IPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<IPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<IPageObject>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double));

        /// <summary>
        /// Submissionf for the page.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<CLPPage> Submissions
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionsProperty); }
            set { SetValue(SubmissionsProperty, value); }
        }

        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        #region Properties

        /// <summary>
        /// The radius of the Pen tip.
        /// </summary>
        public double PenSize
        {
            get { return GetValue<double>(PenSizeProperty); }
            set { SetValue(PenSizeProperty, value); }
        }

        public static readonly PropertyData PenSizeProperty = RegisterProperty("PenSize", typeof(double), 3.0);

        /// <summary>
        /// Sets the PageInteractionMode.
        /// </summary>
        public PageInteractionMode PageInteractionMode
        {
            get { return GetValue<PageInteractionMode>(PageInteractionModeProperty); }
            set { SetValue(PageInteractionModeProperty, value); }
        }

        public static readonly PropertyData PageInteractionModeProperty = RegisterProperty("PageInteractionMode", typeof(PageInteractionMode), PageInteractionMode.Pen, PageInteractionModeChanged);

        private static void PageInteractionModeChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var pageViewModel = args.LatestSender as ACLPPageBaseViewModel;
            if(pageViewModel == null)
            {
                return;
            }

            switch(pageViewModel.PageInteractionMode)
            {
                case PageInteractionMode.None:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.None;
                    pageViewModel.IsUsingCustomCursors = true;
                    pageViewModel.PageCursor = Cursors.No;
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.Select:
                    pageViewModel.IsInkCanvasHitTestVisible = false;
                    pageViewModel.IsUsingCustomCursors = true;
                    pageViewModel.PageCursor = Cursors.Hand;
                    break;
                case PageInteractionMode.Pen:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                    pageViewModel.IsUsingCustomCursors = false;

                    pageViewModel.DefaultDA.IsHighlighter = false;
                    pageViewModel.DefaultDA.Height = pageViewModel.PenSize;
                    pageViewModel.DefaultDA.Width = pageViewModel.PenSize;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.Highlighter:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                    pageViewModel.IsUsingCustomCursors = false;

                    pageViewModel.DefaultDA.IsHighlighter = true;
                    pageViewModel.DefaultDA.Height = 12;
                    pageViewModel.DefaultDA.Width = 12;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.PenAndSelect:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                    pageViewModel.IsUsingCustomCursors = true;
                    var penAndSelectStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/PenCursor.cur", UriKind.Relative));
                    if(penAndSelectStream != null)
                    {
                        pageViewModel.PageCursor = new Cursor(penAndSelectStream.Stream);
                    }
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.Eraser:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = pageViewModel.EraserMode;
                    pageViewModel.IsUsingCustomCursors = false;

                    pageViewModel.DefaultDA.IsHighlighter = false;
                    pageViewModel.DefaultDA.Height = pageViewModel.PenSize;
                    pageViewModel.DefaultDA.Width = pageViewModel.PenSize;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.Lasso:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                    pageViewModel.IsUsingCustomCursors = true;
                    var lassoStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/LassoCursor.cur", UriKind.Relative));
                    if(lassoStream != null)
                    {
                        pageViewModel.PageCursor = new Cursor(lassoStream.Stream);
                    }

                    pageViewModel.DefaultDA.IsHighlighter = false;
                    pageViewModel.DefaultDA.Height = 2.0;
                    pageViewModel.DefaultDA.Width = 2.0;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.Scissors:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                    pageViewModel.IsUsingCustomCursors = true;
                    var scissorsStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Resources/Cursors/ScissorsCursor.cur", UriKind.Relative));
                    if(scissorsStream != null)
                    {
                        pageViewModel.PageCursor = new Cursor(scissorsStream.Stream);
                    }

                    pageViewModel.DefaultDA.IsHighlighter = false;
                    pageViewModel.DefaultDA.Height = 2.0;
                    pageViewModel.DefaultDA.Width = 2.0;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Ellipse;
                    pageViewModel.ClearAdorners();
                    break;
                case PageInteractionMode.EditObjectProperties:
                    pageViewModel.IsInkCanvasHitTestVisible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion //Properties

        #region Bindings

        /// <summary>
        /// Signifies the viewModel's view is a CLPPagePreviewView.
        /// </summary>
        public bool IsPagePreview
        {
            get { return GetValue<bool>(IsPagePreviewProperty); }
            set { SetValue(IsPagePreviewProperty, value); }
        }

        public static readonly PropertyData IsPagePreviewProperty = RegisterProperty("IsPagePreview", typeof(bool), true);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            protected set { SetValue(EditingModeProperty, value); }
        }

        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof(InkCanvasEditingMode), InkCanvasEditingMode.Ink);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DrawingAttributes DefaultDA
        {
            get { return GetValue<DrawingAttributes>(DefaultDAProperty); }
            set { SetValue(DefaultDAProperty, value); }
        }

        public static readonly PropertyData DefaultDAProperty = RegisterProperty("DefaultDA", typeof(DrawingAttributes), () => new DrawingAttributes());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public InkCanvasEditingMode EraserMode
        {
            get { return GetValue<InkCanvasEditingMode>(EraserModeProperty); }
            set { SetValue(EraserModeProperty, value); }
        }

        public static readonly PropertyData EraserModeProperty = RegisterProperty("EraserMode", typeof(InkCanvasEditingMode), InkCanvasEditingMode.EraseByStroke);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public StylusShape EraserShape
        {
            get { return GetValue<StylusShape>(EraserShapeProperty); }
            set { SetValue(EraserShapeProperty, value); }
        }

        public static readonly PropertyData EraserShapeProperty = RegisterProperty("EraserShape", typeof(StylusShape), new RectangleStylusShape(5, 5));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsInkCanvasHitTestVisible
        {
            get { return GetValue<bool>(IsInkCanvasHitTestVisibleProperty); }
            set { SetValue(IsInkCanvasHitTestVisibleProperty, value); }
        }

        public static readonly PropertyData IsInkCanvasHitTestVisibleProperty = RegisterProperty("IsInkCanvasHitTestVisible", typeof(bool), true);

        /// <summary>
        /// Sets the page's visible cursor.
        /// </summary>
        public Cursor PageCursor
        {
            get { return GetValue<Cursor>(PageCursorProperty); }
            set { SetValue(PageCursorProperty, value); }
        }

        public static readonly PropertyData PageCursorProperty = RegisterProperty("PageCursor", typeof(Cursor), Cursors.Pen);

        /// <summary>
        /// Forces the InkCanvas to use custom, imported cursors instead of the default ones.
        /// </summary>
        public bool IsUsingCustomCursors
        {
            get { return GetValue<bool>(IsUsingCustomCursorsProperty); }
            set { SetValue(IsUsingCustomCursorsProperty, value); }
        }

        public static readonly PropertyData IsUsingCustomCursorsProperty = RegisterProperty("IsUsingCustomCursors", typeof(bool), false);

        /// <summary>
        /// Whether or not the submissions for this page are showing.
        /// </summary>
        public bool IsShowingSubmissions
        {
            get { return GetValue<bool>(IsShowingSubmissionsProperty); }
            set { SetValue(IsShowingSubmissionsProperty, value); }
        }

        public static readonly PropertyData IsShowingSubmissionsProperty = RegisterProperty("IsShowingSubmissions", typeof(bool), false);

        /// <summary>
        /// Whether the page has submissions or not.
        /// </summary>
        public bool HasSubmissions
        {
            get { return Submissions.Any(); }
        }

        public int NumberOfDistinctSubmissions
        {
            get { return Submissions.Distinct().Count(); }
        }

        #endregion //Bindings

        #region Commands

        public Canvas TopCanvas;

        /// <summary>
        /// Gets the MouseMoveCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseMoveCommand { get; private set; }

        private void OnMouseMoveCommandExecute(MouseEventArgs e)
        {
            if(TopCanvas == null ||
               IsPagePreview ||
               PageInteractionMode == PageInteractionMode.Pen)
            {
                return;
            }
        }

        /// <summary>
        /// Gets the MouseDownCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e) { }

        /// <summary>
        /// Gets the MouseUpCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseUpCommand { get; private set; }

        private void OnMouseUpCommandExecute(MouseEventArgs e) { }

        /// <summary>
        /// Clears all non-background pageObjects, all strokes, and deletes History.
        /// If in AuthoringMode, even background pageObjects will be removed.
        /// </summary>
        public Command ClearPageCommand { get; private set; }

        private void OnClearPageCommandExecute()
        {
            if(MessageBox.Show("Are you sure you want to clear everything on this page? All strokes, arrays, and animations will be erased!",
                                "Warning!",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Page.InkStrokes.Clear();
            Page.PageObjects.Clear();
            //Page.SerializedStrokes.Clear();
            //Page.PageHistory.ClearHistory();
        }

        #endregion //Commands

        #region Methods

        public static void ClearAdorners(CLPPage page)
        {
            if(page == null)
            {
                return;
            }
            foreach(var clpPageViewModel in ViewModelManager.GetViewModelsOfModel(page).OfType<ACLPPageBaseViewModel>())
            {
                clpPageViewModel.ClearAdorners();
            }
        }

        public void ClearAdorners()
        {
            if(PageObjects == null)
            {
                return;
            }
            foreach(var aclpPageObjectBaseViewModel in
                PageObjects.SelectMany(pageObject => ViewModelManager.GetViewModelsOfModel(pageObject)).OfType<APageObjectBaseViewModel>().ToList())
            {
                aclpPageObjectBaseViewModel.ClearAdorners();
            }
        }

        protected void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(IsPagePreview || PageInteractionMode == PageInteractionMode.None)
            {
                return;
            }

            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            try
            {
                foreach(var pageObject in PageObjects.OfType<IPageObjectAccepter>().Where(pageObject => pageObject.CanAcceptPageObjects))
                {
                    var removedPageObjects = new List<IPageObject>();
                    if(e.OldItems != null)
                    {
                        removedPageObjects.AddRange(e.OldItems.Cast<IPageObject>());
                    }

                    var addedPageObjects = new ObservableCollection<IPageObject>();
                    if(e.NewItems != null)
                    {
                        var o = pageObject;
                        foreach(
                            var addedPageObject in
                                e.NewItems.Cast<IPageObject>()
                                 .Where(addedPageObject => o.ID != addedPageObject.ID && !o.AcceptedPageObjectIDs.Contains(addedPageObject.ID) && o.PageObjectIsOver(addedPageObject, .50)))
                        {
                            addedPageObjects.Add(addedPageObject);
                        }
                    }

                    pageObject.AcceptPageObjects(addedPageObjects, removedPageObjects);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
            }
        }

        protected void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if(IsPagePreview)
            {
                return;
            }

            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;

            switch(App.MainWindowViewModel.Ribbon.PageInteractionMode)
            {
                case PageInteractionMode.Scissors:
                {
                    var stroke = e.Added.FirstOrDefault();
                    if(stroke == null)
                    {
                        return;
                    }
                    CutStroke(stroke);
                }
                    break;
                case PageInteractionMode.Lasso:
                {
                    var stroke = e.Added.FirstOrDefault();
                    if(stroke == null)
                    {
                        return;
                    }
                    LassoStroke(stroke);
                }
                    break;
                case PageInteractionMode.Select:
                case PageInteractionMode.Highlighter:
                case PageInteractionMode.Pen:
                    if(e.Removed.Any())
                    {
                        RemoveStroke(e.Removed, e.Added);
                    }
                    else
                    {
                        var stroke = e.Added.FirstOrDefault();
                        if(stroke == null)
                        {
                            return;
                        }
                        AddStroke(stroke);
                    }
                    break;
                case PageInteractionMode.Eraser:
                    RemoveStroke(e.Removed, e.Added);
                    break;
            }
        }

        protected void Submissions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("HasSubmissions");
            RaisePropertyChanged("NumberOfDistinctSubmissions");
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "CanSendToTeacher" &&
               viewModel is RibbonViewModel)
            {
                RaisePropertyChanged("HasSubmissions");
            }

            if(propertyName == "IsSending" &&
               viewModel is RibbonViewModel)
            {
                RaisePropertyChanged("HasSubmissions");
            }

            if(IsPagePreview)
            {
                return;
            }

            if(propertyName == "EraserMode" &&
               viewModel is RibbonViewModel)
            {
                EraserMode = (viewModel as RibbonViewModel).EraserMode;
                if(PageInteractionMode == PageInteractionMode.Eraser)
                {
                    EditingMode = EraserMode;
                }
            }

            if(propertyName == "PenSize" &&
               viewModel is RibbonViewModel)
            {
                var x = (viewModel as RibbonViewModel).PenSize;
                EraserShape = new RectangleStylusShape(x, x);
                DefaultDA.Height = x;
                DefaultDA.Width = x;
                PenSize = x;

                if(PageInteractionMode == PageInteractionMode.Eraser ||
                   PageInteractionMode == PageInteractionMode.Pen ||
                   PageInteractionMode == PageInteractionMode.Highlighter)
                {
                    return;
                }

                (viewModel as RibbonViewModel).PageInteractionMode = PageInteractionMode.Pen;
            }

            if(propertyName == "PageInteractionMode" &&
               viewModel is RibbonViewModel)
            {
                PageInteractionMode = (viewModel as RibbonViewModel).PageInteractionMode;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        private void RefreshAcceptedStrokes(List<Stroke> addedStrokes, List<Stroke> removedStrokes)
        {
            foreach(var pageObject in PageObjects.OfType<IStrokeAccepter>().Where(pageObject => pageObject.CanAcceptStrokes))
            {
                var pageObjectBounds = new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width, pageObject.Height);

                var addedStrokesOverObject = from stroke in addedStrokes
                                             where stroke.HitTest(pageObjectBounds, 3)
                                             select stroke;

                var removedStrokesOverObject = from stroke in removedStrokes
                                               where stroke.HitTest(pageObjectBounds, 3)
                                               select stroke;

                var addStrokes = new StrokeCollection(addedStrokesOverObject);
                var removeStrokes = new StrokeCollection(removedStrokesOverObject);
                var o = pageObject;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                           (DispatcherOperationCallback)delegate
                                                               {
                                                                   o.AcceptStrokes(addStrokes, removeStrokes);

                                                                   return null;
                                                               }, null);
            }
        }

        private void RefreshInkStrokes()
        {
            var pageObjects = Page.PageObjects;
            foreach(var pageObject in pageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.RefreshAcceptedStrokes();
            }
        }

        private void RefreshPageObjects(IEnumerable<IPageObject> pageObjects)
        {
            try
            {
                foreach(var pageObject in pageObjects.OfType<IPageObjectAccepter>())
                {
                    pageObject.RefreshAcceptedPageObjects();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(@"PageObjectCollectionChanged Exception: " + ex.Message);
            }
        }

        #endregion //Methods   

        #region Page Interaction Methods

        private void CutStroke(Stroke stroke)
        {
            InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
            var newUniqueID = Guid.NewGuid().ToString();
            stroke.SetStrokeID(newUniqueID);
            stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
            stroke.SetStrokeVersionIndex(0);
            Page.InkStrokes.Remove(stroke);

            var allCutPageObjects = new List<ICuttable>();
            var allHalvedPageObjects = new List<IPageObject>();
            foreach(var pageObject in PageObjects.OfType<ICuttable>())
            {
                var halvedPageObjects = pageObject.Cut(stroke);
                if(!halvedPageObjects.Any() ||
                   pageObject.OwnerID == Person.Author.ID)
                {
                    continue;
                }
                allCutPageObjects.Add(pageObject);
                allHalvedPageObjects.AddRange(halvedPageObjects);
            }

            foreach(var pageObject in allCutPageObjects)
            {
                PageObjects.Remove(pageObject);
            }

            var allHalvedPageObjectIDs = new List<string>();
            foreach(IPageObject pageObject in allHalvedPageObjects)
            {
                allHalvedPageObjectIDs.Add(pageObject.ID);
                AddPageObjectToPage(Page, pageObject, false, false);
            }

            // AddHistoryItemToPage(Page, new CLPHistoryPageObjectCut(Page, stroke, allCutPageObjects, allHalvedPageObjectIDs));

            RefreshInkStrokes();
            RefreshPageObjects(allHalvedPageObjects);

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
        }

        private void LassoStroke(Stroke stroke)
        {
            // TODO: Entities
            //InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
            //PageObjects.CollectionChanged -= PageObjects_CollectionChanged;

            //var lassoedPageObjects = new List<ICLPPageObject>();

            //var strokeGeometry = new PathGeometry();
            //var pathFigure = new PathFigure();
            //pathFigure.StartPoint = stroke.StylusPoints.First().ToPoint();
            //pathFigure.Segments = new PathSegmentCollection();
            //var polyLine = new PolyLineSegment
            //{
            //    Points = new PointCollection((Point[])stroke.StylusPoints)
            //                                           {
            //                                               stroke.StylusPoints.First().ToPoint()
            //                                           }
            //};
            //pathFigure.Segments.Add(polyLine);

            //strokeGeometry.Figures.Add(pathFigure);

            //foreach(var pageObject in PageObjects)
            //{
            //    if(pageObject.IsBackground && !App.MainWindowViewModel.IsAuthoring)
            //    {
            //        continue;
            //    }
            //    RectangleGeometry pageObjectGeometry;
            //    if(pageObject.Width > 10.0 || pageObject.Height > 10.0)
            //    {
            //        pageObjectGeometry =
            //          new RectangleGeometry(new Rect(pageObject.XPosition + Math.Max(pageObject.Width / 2 - 5.0, 0.0),
            //                                         pageObject.YPosition + Math.Max(pageObject.Height / 2 - 5.0, 0.0),
            //                                         Math.Min(10.0, pageObject.Width),
            //                                         Math.Min(10.0, pageObject.Height)));
            //    }
            //    else
            //    {
            //        pageObjectGeometry =
            //            new RectangleGeometry(new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width,
            //                                           pageObject.Height));
            //    }

            //    if(strokeGeometry.FillContains(pageObjectGeometry))
            //    {
            //        lassoedPageObjects.Add(pageObject);
            //    }
            //}

            //if(lassoedPageObjects.Count > 0)
            //{
            //    double xPosition = lassoedPageObjects.First().XPosition;
            //    double yPosition = lassoedPageObjects.First().YPosition;
            //    double endXPosition = lassoedPageObjects.First().XPosition + lassoedPageObjects.First().Width;
            //    double endYPosition = lassoedPageObjects.First().YPosition + lassoedPageObjects.First().Height;
            //    foreach(var pageObject in lassoedPageObjects)
            //    {
            //        if(pageObject.XPosition < xPosition)
            //        {
            //            xPosition = pageObject.XPosition;
            //        }
            //        if(pageObject.YPosition < yPosition)
            //        {
            //            yPosition = pageObject.YPosition;
            //        }
            //        if(pageObject.XPosition + pageObject.Width > endXPosition)
            //        {
            //            endXPosition = pageObject.XPosition + pageObject.Width;
            //        }
            //        if(pageObject.YPosition + pageObject.Height > endYPosition)
            //        {
            //            endYPosition = pageObject.YPosition + pageObject.Height;
            //        }
            //    }

            //    var pageObjectIDs = new ObservableCollection<string>();
            //    foreach(var pageObject in lassoedPageObjects)
            //    {
            //        pageObjectIDs.Add(pageObject.UniqueID);
            //    }
            //    var width = endXPosition - xPosition;
            //    var height = endYPosition - yPosition;

            //    var region = new CLPRegion(pageObjectIDs, xPosition, yPosition, height, width, Page);
            //    AddPageObjectToPage(region, false);
            //}

            //if(!stroke.ContainsPropertyData(ACLPPageBase.StrokeIDKey))
            //{
            //    var newUniqueID = Guid.NewGuid().ToString();
            //    stroke.AddPropertyData(ACLPPageBase.StrokeIDKey, newUniqueID);
            //}
            //Page.InkStrokes.Remove(stroke);

            //InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            //PageObjects.CollectionChanged += PageObjects_CollectionChanged;
        }

        private void AddStroke(Stroke stroke)
        {
            try
            {
                var addedStrokeIDs = new List<string>();
                var removedStrokes = new List<Stroke>();
                var strokeID = Guid.NewGuid().ToString();
                stroke.SetStrokeID(strokeID);
                stroke.SetStrokeOwnerID(App.MainWindowViewModel.CurrentUser.ID);
                stroke.SetStrokeVersionIndex(0);
                addedStrokeIDs.Add(strokeID);

                RefreshAcceptedStrokes(new List<Stroke>
                                       {
                                           stroke
                                       },
                                       removedStrokes);
                //  AddHistoryItemToPage(Page, new CLPHistoryStrokesChanged(Page, addedStrokeIDs, removedStrokes));
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteToLog("InkStrokeCollectionChanged Exception: " + ex.Message);
                Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " + (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
            }
        }

        private void RemoveStroke(IEnumerable<Stroke> removedStrokes, IEnumerable<Stroke> addedStrokes)
        {
            try
            {
                //Avoid uniqueID duplication
                var enumerable = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
                var removedStrokeIDs = enumerable.Select(stroke => stroke.GetStrokeID()).ToList();
                var addedStrokeIDs = new List<string>();
                var strokes = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
                foreach(var stroke in strokes)
                {
                    var newStrokeID = Guid.NewGuid().ToString();
                    stroke.SetStrokeID(newStrokeID);

                    //Ensures truly uniqueIDs
                    var stroke1 = stroke;
                    foreach(string newUniqueID in from id in removedStrokeIDs
                                                  where id == stroke1.GetStrokeID()
                                                  select Guid.NewGuid().ToString())
                    {
                        stroke.SetStrokeID(newUniqueID);
                    }

                    addedStrokeIDs.Add(stroke.GetStrokeID());
                }
                RefreshAcceptedStrokes(strokes.ToList(), enumerable.ToList());
                // AddHistoryItemToPage(Page, new CLPHistoryStrokesChanged(Page, addedStrokeIDs, enumerable.ToList()));
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteToLog("InkStrokeCollectionChanged Exception: " + ex.Message);
                Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " + (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
            }
        }

        #endregion //Page Interaction methods

        #region Static Methods

        // TODO: Entities
        //public static void AddHistoryItemToPage(CLPPage page, IHistoryItem historyItem, bool isBatch = false)
        //{
        //    App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
        //    App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;
        //    if(!isBatch)
        //    {
        //        page.PageHistory.AddHistoryItem(historyItem); 
        //    }

        //    if(App.CurrentUserMode != App.UserMode.Instructor || App.Network.ProjectorProxy == null || App.MainWindowViewModel.Ribbon.IsBroadcastHistoryDisabled)
        //    {
        //        return;
        //    }

        //    var historyItemCopy = historyItem.UndoRedoCompleteClone();
        //    if(historyItemCopy == null)
        //    {
        //        Logger.Instance.WriteToLog("Failed to UndoRedoCompleteClone history item");
        //        return;
        //    }
        //    var historyItemString = ObjectSerializer.ToString(historyItemCopy);
        //    var zippedHistoryItem = CLPServiceAgent.Instance.Zip(historyItemString);

        //    var pageID = page.SubmissionType != SubmissionType.None ? page.SubmissionID : page.UniqueID;

        //    try
        //    {
        //        App.Network.ProjectorProxy.AddHistoryItem(pageID, zippedHistoryItem);
        //    }
        //    catch(Exception)
        //    {
        //        Logger.Instance.WriteToLog("Failed to send historyItem to Projector");
        //    }

        //    //if(!App.MainWindowViewModel.Ribbon.BroadcastInkToStudents || page.SubmissionType != SubmissionType.None || !App.Network.ClassList.Any())
        //    //{
        //    //    return;
        //    //}

        //    //foreach(var student in App.Network.ClassList)
        //    //{
        //    //    try
        //    //    {
        //    //        var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
        //    //        studentProxy.ModifyPageInkStrokes(add, remove, pageID);
        //    //        (studentProxy as ICommunicationObject).Close();
        //    //    }
        //    //    catch(Exception)
        //    //    {
        //    //    }
        //    //}
        //}

        public static void AddPageObjectToPage(IPageObject pageObject, bool addToHistory = true, bool forceSelectMode = true, int index = -1)
        {
            var parentPage = pageObject.ParentPage;
            if(parentPage == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in AddPageObjectToPage().");
                return;
            }
            AddPageObjectToPage(parentPage, pageObject, addToHistory, forceSelectMode, index);
        }

        public static void AddPageObjectToPage(CLPPage page, IPageObject pageObject, bool addToHistory = true, bool forceSelectMode = true, int index = -1)
        {
            if(page == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in AddPageObjectToPage().");
                return;
            }
            // TODO: Entities
            //pageObject.IsBackground = App.MainWindowViewModel.IsAuthoring;
            if(index == -1)
            {
                page.PageObjects.Add(pageObject);
                pageObject.OnAdded();
            }
            else
            {
                page.PageObjects.Insert(index, pageObject);
            }

            // TODO: Entities
            //if(addToHistory)
            //{
            //    AddHistoryItemToPage(page, new CLPHistoryPageObjectAdd(page, pageObject.UniqueID, (index == -1) ? (page.PageObjects.Count - 1) : index));
            //}

            if(forceSelectMode)
            {
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Select;
            }
        }

        public static void AddPageObjectsToPage(CLPPage page, IEnumerable<IPageObject> pageObjects, bool addToHistory = true, bool forceSelectMode = true)
        {
            var pageObjectIDs = new List<string>();
            // TODO: Entities
            foreach(var pageObject in pageObjects)
            {
                //   pageObject.IsBackground = App.MainWindowViewModel.IsAuthoring;
                pageObjectIDs.Add(pageObject.ID);
                page.PageObjects.Add(pageObject);
                pageObject.OnAdded();
            }

            //if(addToHistory)
            //{
            //    AddHistoryItemToPage(page, new CLPHistoryPageObjectsMassAdd(page, pageObjectIDs));
            //}

            if(forceSelectMode)
            {
                App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Select;
            }
        }

        public static void RemovePageObjectFromPage(CLPPage page, IPageObject pageObject, bool addToHistory = true)
        {
            if(page == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in RemovePageObjectFromPage().");
                return;
            }

            // TODO: Entities
            //if(addToHistory)
            //{
            //    var currentIndex = page.PageObjects.IndexOf(pageObject);
            //    AddHistoryItemToPage(page, new CLPHistoryPageObjectRemove(page, pageObject, currentIndex));
            //}
            
            page.PageObjects.Remove(pageObject);
            pageObject.OnDeleted();
        }

        public static void RemovePageObjectFromPage(IPageObject pageObject, bool addToHistory = true)
        {
            var parentPage = pageObject.ParentPage;
            if(parentPage == null)
            {
                Logger.Instance.WriteToLog("ParentPage for pageObject not set in RemovePageObjectFromPage().");
                return;
            }
            RemovePageObjectFromPage(parentPage, pageObject, addToHistory);
        }

        #endregion //Static Methods
    }
}