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
using System.Windows.Shapes;
using System.Windows.Threading;
using CLP.Models;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum PageInteractionMode
    {
        None,
        Select,
        Tile,
        Pen,
        Highlighter,
        PenAndSelect,
        Scissors,
        EditObjectProperties
    }

    [InterestedIn(typeof(RibbonViewModel))]
    public class CLPPageViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel(CLPPage page)
        {
            PageInteractionMode = App.MainWindowViewModel.Ribbon.PageInteractionMode;
            DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
            EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
            EraserMode = App.MainWindowViewModel.Ribbon.EraserMode;
            Page = page;

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            Page.PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            
            MouseMoveCommand = new Command<MouseEventArgs>(OnMouseMoveCommandExecute);
            MouseDownCommand = new Command<MouseEventArgs>(OnMouseDownCommandExecute);
            MouseUpCommand = new Command<MouseEventArgs>(OnMouseUpCommandExecute);
            
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

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        [ViewModelToModel("Page")]
        public double ProofProgressCurrent
        {
            get { return GetValue<double>(ProofProgressCurrentProperty); }
            set { SetValue(ProofProgressCurrentProperty, value); }
        }

        public static volatile PropertyData ProofProgressCurrentProperty = RegisterProperty("ProofProgressCurrent", typeof(double));

        ////////////////////////////////
        [ViewModelToModel("Page")]
        public string ProofProgressVisible
        {
            get { return GetValue<string>(ProofProgressVisibleProperty); }
            set { SetValue(ProofProgressVisibleProperty, value); }
        }

        
        public static volatile PropertyData ProofProgressVisibleProperty = RegisterProperty("ProofProgressVisible", typeof(string));

        [ViewModelToModel("Page")]
        public string ProofPresent
        {
            get { return GetValue<string>(ProofPresentProperty); }
            set { SetValue(ProofPresentProperty, value); }
        }

        public static volatile PropertyData ProofPresentProperty = RegisterProperty("ProofPresent", typeof(string));

        /////////////////////////////////


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<List<byte>> ByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(ByteStrokesProperty); }
            set { SetValue(ByteStrokesProperty, value); }
        }

        public static readonly PropertyData ByteStrokesProperty = RegisterProperty("ByteStrokes", typeof(ObservableCollection<List<byte>>));

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
        public virtual ICLPHistory PageHistory
        {
            get { return GetValue<ICLPHistory>(PageHistoryProperty); }
            set { SetValue(PageHistoryProperty, value); }
        }

        public static readonly PropertyData PageHistoryProperty = RegisterProperty("PageHistory", typeof(ICLPHistory));

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
        public double PageAspectRatio
        {
            get { return GetValue<double>(PageAspectRatioProperty); }
            set { SetValue(PageAspectRatioProperty, value); }
        }

        public static readonly PropertyData PageAspectRatioProperty = RegisterProperty("PageAspectRatio", typeof(double));

        //Steve - Replace with User's UniqueID to match against Person Model
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public string SubmitterName
        {
            get { return GetValue<string>(SubmitterNameProperty); }
            set { SetValue(SubmitterNameProperty, value); }
        }

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

        public static readonly PropertyData IsSubmissionProperty = RegisterProperty("IsSubmission", typeof(bool));

        /// <summary>
        /// Sets the PageInteractionMode.
        /// </summary>
        public PageInteractionMode PageInteractionMode
        {
            get { return GetValue<PageInteractionMode>(PageInteractionModeProperty); }
            set
            {
                SetValue(PageInteractionModeProperty, value);
                switch(value)
                {
                    case PageInteractionMode.None:
                        IsInkCanvasHitTestVisible = true;
                        EditingMode = InkCanvasEditingMode.None;
                        PageCursor = Cursors.No;
                        break;
                    case PageInteractionMode.Select:
                        IsInkCanvasHitTestVisible = false;
                        PageCursor = Cursors.Hand;
                        break;
                    case PageInteractionMode.Tile:
                        IsInkCanvasHitTestVisible = false;
                        break;
                    case PageInteractionMode.Pen:
                        IsInkCanvasHitTestVisible = true;
                        EditingMode = InkCanvasEditingMode.Ink;
                        var penStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Images/PenCursor.cur", UriKind.Relative));
                        PageCursor = new Cursor(penStream.Stream); 
                        break;
                    case PageInteractionMode.Highlighter:
                        IsInkCanvasHitTestVisible = true;
                        EditingMode = InkCanvasEditingMode.Ink;
                        var hightlighterStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Images/HighlighterCursor.cur", UriKind.Relative));
                        PageCursor = new Cursor(hightlighterStream.Stream);
                        break;
                    case PageInteractionMode.PenAndSelect:
                        IsInkCanvasHitTestVisible = true;
                        EditingMode = InkCanvasEditingMode.Ink;
                        var penAndSelectStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Images/PenCursor.cur", UriKind.Relative));
                        PageCursor = new Cursor(penAndSelectStream.Stream); 
                        break;
                    case PageInteractionMode.Scissors:
                        IsInkCanvasHitTestVisible = true;
                        EditingMode = InkCanvasEditingMode.Ink;
                        var scissorsStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Images/ScissorsCursor.cur", UriKind.Relative));
                        PageCursor = new Cursor(scissorsStream.Stream); 
                        break;
                    case PageInteractionMode.EditObjectProperties:
                        IsInkCanvasHitTestVisible = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ClearAdorners();
            }
        }

        public static readonly PropertyData PageInteractionModeProperty = RegisterProperty("PageInteractionMode", typeof(PageInteractionMode), PageInteractionMode.Pen);

        /// <summary>
        /// Sets the page's visible cursor.
        /// </summary>
        public Cursor PageCursor
        {
            get { return GetValue<Cursor>(PageCursorProperty); }
            set { SetValue(PageCursorProperty, value); }
        }

        public static readonly PropertyData PageCursorProperty = RegisterProperty("PageCursor", typeof(Cursor), Cursors.Pen);

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
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); }
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public int NumberOfGroupSubmissions
        {
            get { return GetValue<int>(NumberOfGroupSubmissionsProperty); }
            set { SetValue(NumberOfGroupSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfGroupSubmissionsProperty = RegisterProperty("NumberOfGroupSubmissions", typeof(int));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof(InkCanvasEditingMode));
        
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DrawingAttributes DefaultDA
        {
            get { return GetValue<DrawingAttributes>(DefaultDAProperty); }
            set { SetValue(DefaultDAProperty, value); }
        }

        public static readonly PropertyData DefaultDAProperty = RegisterProperty("DefaultDA", typeof(DrawingAttributes));

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
        
        #endregion //Bindings

        #region Commands

        private bool _isMouseDown;
        public Canvas TopCanvas = null;

        public T GetVisualParent<T>(Visual child) where T : Visual
        {
            var p = (Visual)VisualTreeHelper.GetParent(child);
            var parent = p as T ?? GetVisualParent<T>(p);

            return parent;
        }

        public T FindNamedChild<T>(FrameworkElement obj, string name)
        {
            var dep = obj as DependencyObject;
            T ret = default(T);

            if(dep != null)
            {
                int childcount = VisualTreeHelper.GetChildrenCount(dep);
                for(int i = 0; i < childcount; i++)
                {
                    var childDep = VisualTreeHelper.GetChild(dep, i);
                    var child = childDep as FrameworkElement;

                    if(child != null && (child.GetType() == typeof(T) && child.Name == name))
                    {
                        ret = (T)Convert.ChangeType(child, typeof(T));
                        break;
                    }

                    ret = FindNamedChild<T>(child, name);
                    if(ret != null)
                        break;
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



            //var pageObjectCanvas = FindNamedChild<Canvas>(TopCanvas, "PageObjectCanvas");

            //VisualTreeHelper.HitTest(pageObjectCanvas, HitFilter, HitResult, new PointHitTestParameters(e.GetPosition(pageObjectCanvas)));

            //if((_isMouseDown && EditingMode == InkCanvasEditingMode.EraseByStroke) || (_isMouseDown && e.StylusDevice != null && e.StylusDevice.Inverted))
            //{
            //    VisualTreeHelper.HitTest(pageObjectCanvas, HitFilter, EraseResult, new PointHitTestParameters(e.GetPosition(pageObjectCanvas)));
            //}
        }

        /// <summary>
        /// Gets the MouseDownCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e)
        {
           // Page.PageHistory.BeginEventGroup();
            _isMouseDown = true;
            if (App.MainWindowViewModel.Ribbon.PageInteractionMode == PageInteractionMode.Tile)
            {
                var pageObjectCanvas = FindNamedChild<Canvas>(TopCanvas, "PageObjectCanvas");
                Point pt = e.GetPosition(pageObjectCanvas);
                var tile = new CLPSnapTileContainer(pt, Page);
                Page.PageObjects.Add(tile);
            }
            else if (App.MainWindowViewModel.Ribbon.PageInteractionMode == PageInteractionMode.EditObjectProperties) {
                var dummyShape = new CLPShape(CLPShape.CLPShapeType.Rectangle, Page) {Height = 1, Width = 1};
                Point mousePosition = e.GetPosition(TopCanvas);
                dummyShape.XPosition = mousePosition.X;
                dummyShape.YPosition = mousePosition.Y;
                ICLPPageObject selectedObject = null;
                foreach (ICLPPageObject po in Page.PageObjects) {
                    if (dummyShape.PageObjectIsOver(po, .8)) {
                        selectedObject = po;
                    }
                }
                if(selectedObject == null)
                {
                    return;
                }
                var properties = new UpdatePropertiesWindowView
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Top = 100,
                        Left = 100,
                        UniqueIdTextBlock = {Text = selectedObject.UniqueID},
                        ParentIdTextBox = {Text = selectedObject.ParentID},
                        PartsTextBox = {Text = selectedObject.Parts.ToString()},
                        WidthTextBox = {Text = selectedObject.Width.ToString()},
                        HeightTextBox = {Text = selectedObject.Height.ToString()},
                        XPositionTextBox = {Text = selectedObject.XPosition.ToString()},
                        YPositionTextBox = {Text = selectedObject.YPosition.ToString()}
                    };
                properties.ShowDialog();
                if(properties.DialogResult != true)
                {
                    return;
                }

                int partNum;
                bool isNum = Int32.TryParse(properties.PartsTextBox.Text, out partNum);
                selectedObject.Parts = (properties.PartsTextBox.Text.Length > 0 && isNum) ?
                                           partNum : selectedObject.Parts;
                selectedObject.ParentID = properties.ParentIdTextBox.Text;
                int height;
                isNum = Int32.TryParse(properties.HeightTextBox.Text, out height);
                selectedObject.Height = (properties.HeightTextBox.Text.Length > 0 && isNum &&
                                         height <= Page.PageHeight) ? height : selectedObject.Height;
                int width;
                isNum = Int32.TryParse(properties.WidthTextBox.Text, out width);
                selectedObject.Width = (properties.WidthTextBox.Text.Length > 0 &&
                                        isNum && width <= Page.PageWidth) ? width : selectedObject.Width;
                int x;
                isNum = Int32.TryParse(properties.XPositionTextBox.Text, out x);
                selectedObject.XPosition = (properties.XPositionTextBox.Text.Length > 0 && isNum &&
                                            x + width <= Page.PageWidth) ? x : selectedObject.XPosition;
                int y;
                isNum = Int32.TryParse(properties.YPositionTextBox.Text, out y);
                selectedObject.YPosition = (properties.YPositionTextBox.Text.Length > 0 && isNum
                                            && y + height <= Page.PageHeight) ? y : selectedObject.YPosition;
            }
        }

        /// <summary>
        /// Gets the MouseUpCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseUpCommand { get; private set; }

        private void OnMouseUpCommandExecute(MouseEventArgs e)
        {
           // Page.PageHistory.EndEventGroup();
            _isMouseDown = false;
        }

     


        
        
        #endregion //Commands

        #region Methods
       

        public static void ClearAdorners(CLPPage page)
        {
            if(page == null)
            {
                return;
            }
            foreach(var clpPageViewModel in ViewModelManager.GetViewModelsOfModel(page).OfType<CLPPageViewModel>())
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
                    aclpPageObjectBaseViewModel.IsAdornerVisible = false;
                }
            }
        }

        Type _lastType;

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            if(_lastType == typeof(Canvas) && o is Canvas)
            {
                IsInkCanvasHitTestVisible = true;
            }
            else
            {
                if(o is Shape)
                {
                    if((o as Shape).Name.Contains("HitBox"))
                    {
                        _lastType = o.GetType();
                        return HitTestFilterBehavior.Continue;
                    }
                }
            }

            _lastType = o.GetType();
            return HitTestFilterBehavior.ContinueSkipSelf;
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            var pageObjectView = GetVisualParent<Catel.Windows.Controls.UserControl>(result.VisualHit as Shape);
            var pageObjectViewModel = pageObjectView.ViewModel as ACLPPageObjectBaseViewModel;

            //TODO: Steve - First Parameter, Tag, not needed
            if(pageObjectViewModel == null || pageObjectViewModel.IsInternalPageObject)
            {
                return HitTestResultBehavior.Continue;
            }

            var shape = result.VisualHit as Shape;
            if(shape != null)
            {
                IsInkCanvasHitTestVisible = pageObjectViewModel.SetInkCanvasHitTestVisibility(shape.Tag as string, shape.Name, IsInkCanvasHitTestVisible, _isMouseDown, false, false);
            }
            return HitTestResultBehavior.Stop;
        }

        private HitTestResultBehavior EraseResult(HitTestResult result)
        {
            var pageObjectView = GetVisualParent<Catel.Windows.Controls.UserControl>(result.VisualHit as Shape);
            var pageObjectViewModel = pageObjectView.ViewModel as ACLPPageObjectBaseViewModel;

            if(pageObjectViewModel == null || pageObjectViewModel.IsInternalPageObject)
            {
                return HitTestResultBehavior.Continue;
            }
            var shape = result.VisualHit as Shape;
            if(shape != null)
            {
                pageObjectViewModel.EraserHitTest(shape.Name, shape.Tag);
            }
            return HitTestResultBehavior.Stop;
        }

        void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsPagePreview) return;
            String action = e.Action.ToString().Trim();
            if(action == "Add"){
                foreach(ICLPPageObject item in e.NewItems)
                {
                    if(item != null)
                    {
                        Page.PageHistory.Push(new CLPHistoryAddObject(item.UniqueID));
                        Page.updateProgress();
                    }
                }
            }
            else if(action == "Remove")
            {
                foreach(ICLPPageObject item in e.OldItems)
                {
                    if(item != null)
                    {
                        Page.PageHistory.Push(new CLPHistoryRemoveObject(item));
                        Page.updateProgress();
                    }
                }
            }
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - Catel? causing this to be called twice
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
            Page.updateProgress();
        }

        void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if(Page.IsInkAutoAdding || IsPagePreview)
            {
                return;
            }
            
            foreach(Stroke stroke in e.Added)
            {
                if(PageInteractionMode == PageInteractionMode.Scissors)
                {
                    InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                    Page.PageHistory.SingleCutting = true;
                    double topY = stroke.GetBounds().Top;
                    double leftX = stroke.GetBounds().Left;
                    double rightX = stroke.GetBounds().Right;
                    double botY = stroke.GetBounds().Bottom;
                    Page.InkStrokes.Remove(stroke);
                    List<ObservableCollection<ICLPPageObject>> lr = Page.CutObjects(leftX, rightX, topY, botY);
                    ObservableCollection<ICLPPageObject> c1 = lr[0];
                    List<ICLPPageObject> c1List = new List<ICLPPageObject>(c1);
                    ObservableCollection<ICLPPageObject> c2 = lr[1];
                    var AllShapesInkStrokes = new ObservableCollection<Stroke>();

                    int i = 0;
                    foreach(ICLPPageObject no in c2)
                    {
                        StrokeCollection shapeInkStrokes = no.GetStrokesOverPageObject();
                        foreach(Stroke inkStroke in shapeInkStrokes)
                        {
                            AllShapesInkStrokes.Add(inkStroke);
                        }
                        CLPServiceAgent.Instance.RemovePageObjectFromPage(no);

                        ICLPPageObject noc1 = c1List[i];
                        CLPServiceAgent.Instance.AddPageObjectToPage(noc1);

                        ICLPPageObject noc2 = c1List[i + 1];
                        CLPServiceAgent.Instance.AddPageObjectToPage(noc2);
                        i = i + 2;
                    }

                    /*foreach(ICLPPageObject no in c1)
                    {
                        CLPServiceAgent.Instance.AddPageObjectToPage(no);
                    }*/
                    
                    foreach(Stroke inkStroke in AllShapesInkStrokes)
                    {
                        Page.InkStrokes.Add(inkStroke);
                    }

                    
                    RefreshInkStrokes();
                    Page.PageHistory.SingleCutting = false;
                    InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                    return;
                }
            }

            
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - do this in thread queue instead, strokes aren't arriving on projector in correct order.
        //    Task.Factory.StartNew(() =>
          //      {
                    try
                    {
                        var removedStrokeIDs = new List<string>();
                        Page.PageHistory.BeginEventGroup();
                        foreach (var stroke in e.Removed)
                        {
                            removedStrokeIDs.Add(stroke.GetPropertyData(CLPPage.StrokeIDKey) as string);

                            Page.PageHistory.Push(new CLPHistoryRemoveStroke(new StrokeDTO(stroke)));
                            Page.updateProgress();
                  
                        }

                        foreach(var stroke in e.Added)
                        {
                            if(!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                            {
                                var newUniqueID = Guid.NewGuid().ToString();
                                stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
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
                            Page.PageHistory.Push(new CLPHistoryAddStroke(stroke.GetStrokeUniqueID()));
                            Page.updateProgress();
                        }
                        Page.PageHistory.EndEventGroup();

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
                        var add = new List<StrokeDTO>(CLPPage.SaveInkStrokes(e.Added));
                        var remove = new List<StrokeDTO>(CLPPage.SaveInkStrokes(e.Removed));

                        var pageID = Page.IsSubmission ? Page.SubmissionID : Page.UniqueID;

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

                        if(!App.MainWindowViewModel.Ribbon.BroadcastInkToStudents || Page.IsSubmission || !App.Network.ClassList.Any())
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
             //  });
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "EditingMode" && viewModel is RibbonViewModel)
            {
                EditingMode = (viewModel as RibbonViewModel).EditingMode;
            }

            if(propertyName == "EraserMode" && viewModel is RibbonViewModel)
            {
                EraserMode = (viewModel as RibbonViewModel).EraserMode;
            }

            if(propertyName == "PenSize" && viewModel is RibbonViewModel)
            {
                double x = (viewModel as RibbonViewModel).PenSize;
                EraserShape = new RectangleStylusShape(x, x);
                DefaultDA.Height = x;
                DefaultDA.Width = x;
                PageInteractionMode = PageInteractionMode.Pen;
            }

            if(propertyName == "PageInteractionMode" && viewModel is RibbonViewModel)
            {
                PageInteractionMode = (viewModel as RibbonViewModel).PageInteractionMode;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        private void RefreshInkStrokes() {
            var pageObjects = Page.PageObjects;
            foreach(ICLPPageObject po in pageObjects) {
                po.RefreshStrokeParentIDs();
            }
        }
        #endregion //Methods        
    }
}