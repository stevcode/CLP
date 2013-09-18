using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CLP.Models;
using Catel.Data;
using Catel.MVVM;
using System.Windows.Shapes;
using Classroom_Learning_Partner.Views;

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
    abstract public class ACLPPageBaseViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        protected ACLPPageBaseViewModel(ICLPPage page)
        {
            Page = page;

            PageInteractionMode = App.MainWindowViewModel.Ribbon.PageInteractionMode;
            EraserMode = App.MainWindowViewModel.Ribbon.EraserMode;
            DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            
            MouseMoveCommand = new Command<MouseEventArgs>(OnMouseMoveCommandExecute);
            MouseDownCommand = new Command<MouseEventArgs>(OnMouseDownCommandExecute);
            MouseUpCommand = new Command<MouseEventArgs>(OnMouseUpCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
        }
        
        public override string Title { get { return "APageBaseVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public ICLPPage Page
        {
            get { return GetValue<ICLPPage>(PageProperty); }
            private set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(ICLPPage));

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
        public ObservableCollection<ICLPPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<ICLPPageObject>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public CLPHistory PageHistory
        {
            get { return GetValue<CLPHistory>(PageHistoryProperty); }
            set { SetValue(PageHistoryProperty, value); }
        }

        public static readonly PropertyData PageHistoryProperty = RegisterProperty("PageHistory", typeof(CLPHistory));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public double PageHeight
        {
            get { return GetValue<double>(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        public static readonly PropertyData PageHeightProperty = RegisterProperty("PageHeight", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public double PageWidth
        {
            get { return GetValue<double>(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        public static readonly PropertyData PageWidthProperty = RegisterProperty("PageWidth", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public double InitialPageAspectRatio
        {
            get { return GetValue<double>(InitialPageAspectRatioProperty); }
            set { SetValue(InitialPageAspectRatioProperty, value); }
        }

        public static readonly PropertyData InitialPageAspectRatioProperty = RegisterProperty("InitialPageAspectRatio", typeof(double));

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
                    break;
                case PageInteractionMode.Highlighter:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = InkCanvasEditingMode.Ink;
                    pageViewModel.IsUsingCustomCursors = false;

                    pageViewModel.DefaultDA.IsHighlighter = true;
                    pageViewModel.DefaultDA.Height = 12;
                    pageViewModel.DefaultDA.Width = 12;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
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
                    break;
                case PageInteractionMode.Eraser:
                    pageViewModel.IsInkCanvasHitTestVisible = true;
                    pageViewModel.EditingMode = pageViewModel.EraserMode;
                    pageViewModel.IsUsingCustomCursors = false;

                    pageViewModel.DefaultDA.IsHighlighter = false;
                    pageViewModel.DefaultDA.Height = pageViewModel.PenSize;
                    pageViewModel.DefaultDA.Width = pageViewModel.PenSize;
                    pageViewModel.DefaultDA.StylusTip = StylusTip.Rectangle;
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
                    break;
                case PageInteractionMode.EditObjectProperties:
                    pageViewModel.IsInkCanvasHitTestVisible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            pageViewModel.ClearAdorners();
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

        public static readonly PropertyData EraserShapeProperty = RegisterProperty("EraserShape", typeof(StylusShape), new RectangleStylusShape(5,5));

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
        
        #endregion //Bindings

        #region Commands

        public Canvas TopCanvas = null;

        //TODO: steve - move to serviceagent
        public T GetVisualParent<T>(Visual child) where T : Visual
        {
            var p = (Visual)VisualTreeHelper.GetParent(child);
            var parent = p as T ?? GetVisualParent<T>(p);

            return parent;
        }

        //TODO: steve - move to serviceagent
        public static T FindNamedChild<T>(FrameworkElement obj, string name)
        {
            var dep = obj as DependencyObject;
            var ret = default(T);

            if(dep != null)
            {
                var childcount = VisualTreeHelper.GetChildrenCount(dep);
                for(int i = 0; i < childcount; i++)
                {
                    var childDep = VisualTreeHelper.GetChild(dep, i);
                    var child = childDep as FrameworkElement;

                    if(child != null &&
                       (child.GetType() == typeof(T) && child.Name == name))
                    {
                        ret = (T)Convert.ChangeType(child, typeof(T));
                        break;
                    }

                    ret = FindNamedChild<T>(child, name);
                    if(ret != null) break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Gets the MouseMoveCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseMoveCommand { get; private set; }

        private void OnMouseMoveCommandExecute(MouseEventArgs e)
        {
            if(TopCanvas == null || IsPagePreview || PageInteractionMode == PageInteractionMode.Pen)
            {
                return;
            }
        }

        /// <summary>
        /// Gets the MouseDownCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e)
        {

        }

        /// <summary>
        /// Gets the MouseUpCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseUpCommand { get; private set; }

        private void OnMouseUpCommandExecute(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Clears all non-background pageObjects, all strokes, and deletes History.
        /// If in AuthoringMode, even background pageObjects will be removed.
        /// </summary>
        public Command ClearPageCommand { get; private set; }

        private void OnClearPageCommandExecute()
        {
            if(MessageBox.Show("Are you sure you want to clear everything on this page? All strokes, arrays, and animations will be erased!",
                                "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var nonBackgroundPageObjects = Page.PageObjects.Where(pageObject => pageObject.IsBackground != true).ToList();
            foreach(var pageObject in nonBackgroundPageObjects)
            {
                Page.PageObjects.Remove(pageObject);
            }

            Page.InkStrokes.Clear();
            Page.SerializedStrokes.Clear();
            Page.PageHistory.ClearHistory();
        }

        #endregion //Commands

        #region Methods

        public static void ClearAdorners(ICLPPage page)
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
            if(PageObjects != null)
            {
                foreach(var aclpPageObjectBaseViewModel in PageObjects.SelectMany(pageObject => ViewModelManager.GetViewModelsOfModel(pageObject)).OfType<ACLPPageObjectBaseViewModel>()) 
                {
                    aclpPageObjectBaseViewModel.ClearAdorners();
                }
            }
        }

        protected void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(IsPagePreview)
            {
                return;
            }

            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - Move to CLPServiceAgent
            //Task.Factory.StartNew( () =>
            //    {
            try
            {
                foreach (ICLPPageObject pageObject in PageObjects)
                {
                    if (pageObject.CanAcceptPageObjects)
                    {
                        var removedPageObjects = new ObservableCollection<ICLPPageObject>();
                        if (e.OldItems != null)
                        {
                            foreach (ICLPPageObject removedPageObject in e.OldItems)
                            {
                                removedPageObjects.Add(removedPageObject);
                            }
                        }

                        var addedPageObjects = new ObservableCollection<ICLPPageObject>();
                        if (e.NewItems != null)
                        {
                            foreach (ICLPPageObject addedPageObject in e.NewItems)
                            {
                                if (!pageObject.UniqueID.Equals(addedPageObject.UniqueID)
                                    && !pageObject.UniqueID.Equals(addedPageObject.ParentID)
                                    && !pageObject.PageObjectObjectParentIDs.Contains(addedPageObject.UniqueID)
                                    && pageObject.PageObjectIsOver(addedPageObject, .50))
                                {
                                    addedPageObjects.Add(addedPageObject);
                                }
                            }
                        }

                        pageObject.AcceptObjects(addedPageObjects, removedPageObjects);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
            }
            //});
        }

        protected void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if(IsPagePreview)
            {
                return;
            }

            if(PageInteractionMode == PageInteractionMode.Scissors)
            {
                foreach(var stroke in e.Added)
                {
                    InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                    PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
                    if(!stroke.ContainsPropertyData(ACLPPageBase.StrokeIDKey))
                    {
                        var newUniqueID = Guid.NewGuid().ToString();
                        stroke.AddPropertyData(ACLPPageBase.StrokeIDKey, newUniqueID);
                    }
                    Page.InkStrokes.Remove(stroke);

                    var allCutPageObjects = new List<ICLPPageObject>();
                    var allHalvedPageObjects = new List<ICLPPageObject>();
                    foreach(var pageObject in PageObjects)
                    {
                        var halvedPageObjects = pageObject.Cut(stroke);
                        if(!halvedPageObjects.Any())
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
                    foreach(var pageObject in allHalvedPageObjects)
                    {
                        allHalvedPageObjectIDs.Add(pageObject.UniqueID);
                        CLPServiceAgent.Instance.AddPageObjectToPage(Page, pageObject, false, false);
                    }

                    Page.PageHistory.AddHistoryItem(new CLPHistoryPageObjectCut(Page, stroke, allCutPageObjects, allHalvedPageObjectIDs));

                    RefreshInkStrokes();
                    RefreshPageObjects(allHalvedPageObjects);

                    InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                    PageObjects.CollectionChanged += PageObjects_CollectionChanged;
                    return;
                }
            }

            if(PageInteractionMode == PageInteractionMode.Lasso)
            {
                foreach(var stroke in e.Added)
                {
                    InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                    PageObjects.CollectionChanged -= PageObjects_CollectionChanged;

                    var lassoedPageObjects = new List<ICLPPageObject>();

                    var strokeGeometry = new PathGeometry();
                    var pathFigure = new PathFigure();
                    pathFigure.StartPoint = stroke.StylusPoints.First().ToPoint();
                    pathFigure.Segments = new PathSegmentCollection();
                    var polyLine = new PolyLineSegment
                                   {
                                       Points = new PointCollection((Point[])stroke.StylusPoints)
                                                       {
                                                           stroke.StylusPoints.First().ToPoint()
                                                       }
                                   };
                    pathFigure.Segments.Add(polyLine);

                    strokeGeometry.Figures.Add(pathFigure);
    

                    foreach(var pageObject in PageObjects)
                    {
                        var pageObjectGeometry =
                            new RectangleGeometry(new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width,
                                                           pageObject.Height));

                        if(strokeGeometry.FillContains(pageObjectGeometry))
                        {
                            lassoedPageObjects.Add(pageObject);
                        }
                    }

                    var lassoedPageObjectParameterizationView = new LassoedPageObjectParameterizationView() { Owner = Application.Current.MainWindow };
                    lassoedPageObjectParameterizationView.ShowDialog();

                    if(lassoedPageObjectParameterizationView.DialogResult == true)
                    {
                        int numberOfCopies;
                        try
                        {
                            numberOfCopies = Convert.ToInt32(lassoedPageObjectParameterizationView.NumberOfCopies.Text);
                        }
                        catch(FormatException)
                        {
                            numberOfCopies = 1;
                        }

                        var isHorizontallyAligned = lassoedPageObjectParameterizationView.HorizontalToggle.IsChecked != null &&
                                                    (bool)lassoedPageObjectParameterizationView.HorizontalToggle.IsChecked;
                        var isVerticallyAligned = lassoedPageObjectParameterizationView.VerticalToggle.IsChecked != null &&
                                                  (bool)lassoedPageObjectParameterizationView.VerticalToggle.IsChecked;

                        foreach(var lassoedPageObject in lassoedPageObjects)
                        {
                            for(int i = 0; i < numberOfCopies; i++)
                            {
                                var duplicatePageObject = lassoedPageObject.Duplicate();
                                if(isHorizontallyAligned)
                                {
                                    duplicatePageObject.XPosition += duplicatePageObject.Width + 10;
                                }
                                else if(isVerticallyAligned)
                                {
                                    duplicatePageObject.YPosition += duplicatePageObject.Height + 10;
                                }
                                ACLPPageObjectBase.ApplyDistinctPosition(duplicatePageObject);
                                CLPServiceAgent.Instance.AddPageObjectToPage(duplicatePageObject, false);
                                //TODO: Steve - add MassPageObjectAdd history item and MassPageObjectRemove history item.
                            }
                        }
                    }

                    if(!stroke.ContainsPropertyData(ACLPPageBase.StrokeIDKey))
                    {
                        var newUniqueID = Guid.NewGuid().ToString();
                        stroke.AddPropertyData(ACLPPageBase.StrokeIDKey, newUniqueID);
                    }
                    Page.InkStrokes.Remove(stroke);

                    InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                    PageObjects.CollectionChanged += PageObjects_CollectionChanged;
                    return;
                }
            }


            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - do this in thread queue instead, strokes aren't arriving on projector in correct order.
            //Task.Factory.StartNew(() =>
            //    {
                    try
                    {
                        var removedStrokeIDs = new List<string>();
                        foreach (var stroke in e.Removed)
                        {
                            removedStrokeIDs.Add(stroke.GetPropertyData(CLPPage.StrokeIDKey) as string);
                            //TODO: Make batch inking to recognize point erasing.
                            Page.PageHistory.AddHistoryItem(new CLPHistoryStrokeRemove(Page, stroke));
                        }
                        foreach(var stroke in e.Added)
                        {
                            if(!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                            {
                                var newUniqueID = Guid.NewGuid().ToString();
                                stroke.AddPropertyData(ACLPPageBase.StrokeIDKey, newUniqueID);
                            }

                            //Ensures truly uniqueIDs
                            foreach(string id in removedStrokeIDs)
                            {
                                if(id == stroke.GetStrokeUniqueID())
                                {
                                    var newUniqueID = Guid.NewGuid().ToString();
                                    stroke.SetStrokeUniqueID(newUniqueID);
                                }  
                            }
                            Page.PageHistory.AddHistoryItem(new CLPHistoryStrokeAdd(Page, stroke.GetStrokeUniqueID()));
                        }

                        foreach(ICLPPageObject pageObject in PageObjects)
                        {
                            if(!pageObject.CanAcceptStrokes)
                            {
                                continue;
                            }

                            var rect = new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width, pageObject.Height);

                            var addedStrokesOverObject =
                                from stroke in e.Added
                                where stroke.HitTest(rect, 3)
                                select stroke;

                            var removedStrokesOverObject =
                                from stroke in e.Removed
                                where stroke.HitTest(rect, 3)
                                select stroke;

                            var addStrokes = new StrokeCollection(addedStrokesOverObject);
                            var removeStrokes = new StrokeCollection(removedStrokesOverObject);
                            ICLPPageObject o = pageObject;
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                                       (DispatcherOperationCallback)delegate
                                                                           {
                                                                               o.AcceptStrokes(addStrokes, removeStrokes);

                                                                               return null;
                                                                           }, null);
                        }

                        if(App.CurrentUserMode != App.UserMode.Instructor)
                        {
                            return;
                        }
                        var add = new List<StrokeDTO>(StrokeDTO.SaveInkStrokes(e.Added));
                        var remove = new List<StrokeDTO>(StrokeDTO.SaveInkStrokes(e.Removed));

                        var pageID = Page.SubmissionType != SubmissionType.None ? Page.SubmissionID : Page.UniqueID;

                        if(App.Network.ProjectorProxy != null)
                        {
                            try
                            {
                                App.Network.ProjectorProxy.ModifyPageInkStrokes(add, remove, pageID);
                            }
                            catch(Exception)
                            {
                            }
                        }
                        //TODO: Steve - Add pages to a queue and send when a projector is found in Else statement

                        if(!App.MainWindowViewModel.Ribbon.BroadcastInkToStudents || Page.SubmissionType != SubmissionType.None || !App.Network.ClassList.Any())
                        {
                            return;
                        }

                        foreach(var student in App.Network.ClassList)
                        {
                            try
                            {
                                var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
                                studentProxy.ModifyPageInkStrokes(add, remove, pageID);
                                (studentProxy as ICommunicationObject).Close();
                            }
                            catch(Exception)
                            {
                            }
                        }
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
               //});
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(IsPagePreview)
            {
                return;
            }

            if(propertyName == "EraserMode" && viewModel is RibbonViewModel)
            {
                EraserMode = (viewModel as RibbonViewModel).EraserMode;
                if(PageInteractionMode == PageInteractionMode.Eraser)
                {
                    EditingMode = EraserMode;
                }
            }

            if(propertyName == "PenSize" && viewModel is RibbonViewModel)
            {
                var x = (viewModel as RibbonViewModel).PenSize;
                EraserShape = new RectangleStylusShape(x, x);
                DefaultDA.Height = x;
                DefaultDA.Width = x;
                PenSize = x;

                if(PageInteractionMode == PageInteractionMode.Eraser || PageInteractionMode == PageInteractionMode.Pen || PageInteractionMode == PageInteractionMode.Highlighter)
                {
                    return;
                }

                (viewModel as RibbonViewModel).PageInteractionMode = PageInteractionMode.Pen;
            }

            if(propertyName == "PageInteractionMode" && viewModel is RibbonViewModel)
            {
                PageInteractionMode = (viewModel as RibbonViewModel).PageInteractionMode;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        private void RefreshInkStrokes()
        {
            var pageObjects = Page.PageObjects;
            foreach(var pageObject in pageObjects)
            {
                pageObject.RefreshStrokeParentIDs();
            }
        }

        private void RefreshPageObjects(List<ICLPPageObject> AllShapesPageObjects)
        {
            try
            {
                foreach(var pageObject in PageObjects)
                {
                    if(pageObject.CanAcceptPageObjects)
                    {
                        var removedPageObjects = new ObservableCollection<ICLPPageObject>();

                        var addedPageObjects = new ObservableCollection<ICLPPageObject>();
                        if(AllShapesPageObjects.Any())
                        {
                            foreach(ICLPPageObject addedPageObject in AllShapesPageObjects)
                            {
                                if(!pageObject.UniqueID.Equals(addedPageObject.UniqueID) &&
                                   !pageObject.UniqueID.Equals(addedPageObject.ParentID) &&
                                   !pageObject.PageObjectObjectParentIDs.Contains(addedPageObject.UniqueID) &&
                                   pageObject.PageObjectIsOver(addedPageObject, .50))
                                {
                                    addedPageObjects.Add(addedPageObject);
                                }
                            }
                        }

                        pageObject.AcceptObjects(addedPageObjects, removedPageObjects);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
            }
        }

        #endregion //Methods        
    }
}