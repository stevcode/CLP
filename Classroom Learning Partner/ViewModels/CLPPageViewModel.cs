using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum PageInteractionMode
    {
        None,
        SnapTile,
        Pen,
        Marker,
        Eraser,
        StrokeEraser,
        EditObjectProperties
    }

    public enum PageEraserInteractionMode
    {
        None,
        Eraser,
        StrokeEraser,
        ObjectEraser
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
            DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
            EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
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

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<List<byte>> ByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(ByteStrokesProperty); }
            set { SetValue(ByteStrokesProperty, value); }
        }

        /// <summary>
        /// Register the ByteStrokes property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the PageObjects property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the PageHistory property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the PageHeight property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the PageWidth property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the PageAspectRatio property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the NumberOfGroupSubmissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfGroupSubmissionsProperty = RegisterProperty("NumberOfGroupSubmissions", typeof(int));

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

        public static readonly PropertyData DefaultDAProperty = RegisterProperty("DefaultDA", typeof(DrawingAttributes));

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

        public T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if(child != null)
                    break;
            }
            return child;
        }

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
            if(TopCanvas == null || IsPagePreview)
            {
                return;
            }
            var pageObjectCanvas = FindNamedChild<Canvas>(TopCanvas, "PageObjectCanvas");
            if(!_isMouseDown)
            {
                VisualTreeHelper.HitTest(pageObjectCanvas, HitFilter, HitResult, new PointHitTestParameters(e.GetPosition(pageObjectCanvas)));
            }

            if((_isMouseDown && EditingMode == InkCanvasEditingMode.EraseByStroke) || (_isMouseDown && e.StylusDevice != null && e.StylusDevice.Inverted))
            {
                VisualTreeHelper.HitTest(pageObjectCanvas, HitFilter, EraseResult, new PointHitTestParameters(e.GetPosition(pageObjectCanvas)));
            }
        }

        /// <summary>
        /// Gets the MouseMoveCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e)
        {
            _isMouseDown = true;
            if (App.MainWindowViewModel.Ribbon.PageInteractionMode == PageInteractionMode.SnapTile)
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
        /// Gets the MouseMoveCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseUpCommand { get; private set; }

        private void OnMouseUpCommandExecute(MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        #endregion //Commands

        #region Methods

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
                pageObjectViewModel.EraserHitTest(shape.Name);
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
                    Page.PageHistory.push(new CLPHistoryAddObject(Page, item));
                }
            }
            else if(action == "Remove")
            {
                foreach(ICLPPageObject item in e.OldItems)
                {
                    Page.PageHistory.push(new CLPHistoryRemoveObject(Page, item));
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
        }

        void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if(Page.IsInkAutoAdding || IsPagePreview)
            {
                return;
            }
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - do this in thread queue instead, strokes aren't arriving on projector in correct order.
        //    Task.Factory.StartNew(() =>
          //      {
                    try
                    {
                        var removedStrokeIDs = e.Removed.Select(stroke => stroke.GetStrokeUniqueID()).ToList();
                        foreach (var stroke in e.Removed)
                        {
                            Page.PageHistory.push(new CLPHistoryRemoveStroke(Page, CLPPage.StrokeToByte(stroke)));
                        }

                        foreach(var stroke in e.Added)
                        {
                            //TODO: Steve - Add Property for time created if necessary.
                            //TODO: Steve - Add Property for Mutability.
                            //TODO: Steve - Add Property for UserName of person who created the stroke.
                            if(!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                            {
                                var newUniqueID = Guid.NewGuid().ToString();
                                stroke.SetStrokeUniqueID(newUniqueID);
                            }
                            
                                
                                Page.PageHistory.push(new CLPHistoryAddStroke(Page, CLPPage.StrokeToByte(stroke)));  

                            
                            //Ensures truly uniqueIDs
                            foreach(string id in removedStrokeIDs)
                            {
                                if(id != stroke.GetStrokeUniqueID())
                                {
                                    continue;
                                }
                                var newUniqueID = Guid.NewGuid().ToString();
                                stroke.SetStrokeUniqueID(newUniqueID);
                            }
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

                            ICLPPageObject o = pageObject;
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                                       (DispatcherOperationCallback)delegate
                                                                           {
                                                                               o.AcceptStrokes(new StrokeCollection(addedStrokesOverObject), new StrokeCollection(removedStrokesOverObject));

                                                                               return null;
                                                                           }, null);
                        }

                        if(App.CurrentUserMode != App.UserMode.Instructor)
                        {
                            return;
                        }
                        var add = new List<List<byte>>(CLPPage.StrokesToBytes(e.Added));
                        var remove = new List<List<byte>>(CLPPage.StrokesToBytes(e.Removed));

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
                    }
               // });
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "EditingMode" && viewModel is RibbonViewModel)
            {
                EditingMode = (viewModel as RibbonViewModel).EditingMode;
            }

            if(propertyName == "PenSize" && viewModel is RibbonViewModel)
            {
                double x = (viewModel as RibbonViewModel).PenSize;
                EraserShape = new RectangleStylusShape(x, x);
                DefaultDA.Height = x;
                DefaultDA.Width = x;
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods        
    }
}