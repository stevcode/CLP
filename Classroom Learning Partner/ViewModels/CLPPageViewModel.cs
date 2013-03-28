using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        static public int count = 0;
        public int ID;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel(CLPPage page)
        {
            count++;
            ID = count;
            Console.WriteLine("I am a PageVM. My ID is " + ID);
            DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
            EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
            Page = page;

            InkStrokes.StrokesChanged += new StrokeCollectionChangedEventHandler(InkStrokes_StrokesChanged);
            Page.PageObjects.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PageObjects_CollectionChanged);
        
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
            private set { SetValue(PageProperty, value); 
            Console.WriteLine("Page of PageVM ID " + ID + " has been set to Page with UniqueID " + Page.UniqueID);}
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

        private bool IsMouseDown = false;
        public Canvas TopCanvas = null;

        public T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if(child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if(child != null)
                    break;
            }
            return child;
        }

        public T GetVisualParent<T>(Visual child) where T : Visual
        {
            T parent = default(T);

            Visual p = (Visual)VisualTreeHelper.GetParent(child);
            parent = p as T;
            if(parent == null)
            {
                parent = GetVisualParent<T>(p);
            }

            return parent;
        }

        public T FindNamedChild<T>(FrameworkElement obj, string name)
        {
            DependencyObject dep = obj as DependencyObject;
            T ret = default(T);

            if(dep != null)
            {
                int childcount = VisualTreeHelper.GetChildrenCount(dep);
                for(int i = 0; i < childcount; i++)
                {
                    DependencyObject childDep = VisualTreeHelper.GetChild(dep, i);
                    FrameworkElement child = childDep as FrameworkElement;

                    if(child.GetType() == typeof(T) && child.Name == name)
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

        /// <summary>
        /// Method to invoke when the MouseMoveCommand command is executed.
        /// </summary>
        private void OnMouseMoveCommandExecute(MouseEventArgs e)
        {
            if(TopCanvas != null)
            {
                Canvas pageObjectCanvas = FindNamedChild<Canvas>(TopCanvas, "PageObjectCanvas");
                if(!IsMouseDown)
                {
                    VisualTreeHelper.HitTest(pageObjectCanvas, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(pageObjectCanvas)));
                }

                if((IsMouseDown && EditingMode == InkCanvasEditingMode.EraseByStroke) || (IsMouseDown && e.StylusDevice != null && e.StylusDevice.Inverted))
                {
                    VisualTreeHelper.HitTest(pageObjectCanvas, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(EraseResult), new PointHitTestParameters(e.GetPosition(pageObjectCanvas)));
                }
                
            }
        }

        /// <summary>
        /// Gets the MouseMoveCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseDownCommand { get; private set; }

        private void OnMouseDownCommandExecute(MouseEventArgs e)
        {
            IsMouseDown = true;
            if (App.MainWindowViewModel.Ribbon.PageInteractionMode == PageInteractionMode.SnapTile)
            {
                Canvas pageObjectCanvas = FindNamedChild<Canvas>(TopCanvas, "PageObjectCanvas");
                Point pt = e.GetPosition(pageObjectCanvas);
                CLPSnapTileContainer tile = new CLPSnapTileContainer(pt, Page);
                Page.PageObjects.Add(tile);
            }
            else if (App.MainWindowViewModel.Ribbon.PageInteractionMode == PageInteractionMode.EditObjectProperties) {
                CLPShape dummyShape = new CLPShape(CLPShape.CLPShapeType.Rectangle, Page);
                dummyShape.Height = 1;
                dummyShape.Width = 1;
                System.Windows.Point mousePosition = e.GetPosition(TopCanvas);
                dummyShape.XPosition = mousePosition.X;
                dummyShape.YPosition = mousePosition.Y;
                ICLPPageObject selectedObject = null;
                foreach (ICLPPageObject po in Page.PageObjects) {
                    if (dummyShape.PageObjectIsOver(po, .8)) {
                        selectedObject = po;
                    }
                }
                if (selectedObject != null)
                {
                    UpdatePropertiesWindowView properties = new UpdatePropertiesWindowView();
                    properties.Owner = Application.Current.MainWindow;
                    properties.WindowStartupLocation = WindowStartupLocation.Manual;
                    properties.Top = 100;
                    properties.Left = 100;
                    properties.UniqueIdTextBlock.Text = selectedObject.UniqueID;
                    properties.ParentIdTextBox.Text = selectedObject.ParentID;
                    properties.PartsTextBox.Text = selectedObject.Parts.ToString();
                    properties.WidthTextBox.Text = selectedObject.Width.ToString();
                    properties.HeightTextBox.Text = selectedObject.Height.ToString();
                    properties.XPositionTextBox.Text = selectedObject.XPosition.ToString();
                    properties.YPositionTextBox.Text = selectedObject.YPosition.ToString();
                    properties.ShowDialog();
                    if (properties.DialogResult == true)
                    {
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
            }
        }

        /// <summary>
        /// Gets the MouseMoveCommand command.
        /// </summary>
        public Command<MouseEventArgs> MouseUpCommand { get; private set; }

        private void OnMouseUpCommandExecute(MouseEventArgs e)
        {
            IsMouseDown = false;
        }

        #endregion //Commands

        #region Methods

        Type lastType = null;

        private HitTestFilterBehavior HitFilter(DependencyObject o)
        {
            if(lastType == typeof(Canvas) && o is Canvas)
            {
                IsInkCanvasHitTestVisible = true;
            }
            else
            {
                //Console.WriteLine(o.GetType().ToString());
                if(o is Shape)
                {
                    if((o as Shape).Name.Contains("HitBox"))
                    {
                        lastType = o.GetType();
                        return HitTestFilterBehavior.Continue;
                    }
                }
            }

            lastType = o.GetType();
            return HitTestFilterBehavior.ContinueSkipSelf;
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            Catel.Windows.Controls.UserControl pageObjectView = GetVisualParent<Catel.Windows.Controls.UserControl>(result.VisualHit as Shape);
            ACLPPageObjectBaseViewModel pageObjectViewModel = pageObjectView.ViewModel as ACLPPageObjectBaseViewModel;

            //TODO: Steve - First Parameter, Tag, not needed
            if (!pageObjectViewModel.IsInternalPageObject)
            {
                IsInkCanvasHitTestVisible = pageObjectViewModel.SetInkCanvasHitTestVisibility((result.VisualHit as Shape).Tag as string, (result.VisualHit as Shape).Name, IsInkCanvasHitTestVisible, IsMouseDown, false, false);
                return HitTestResultBehavior.Stop;
            }
            
            return HitTestResultBehavior.Continue;
        }

        private HitTestResultBehavior EraseResult(HitTestResult result)
        {
            Catel.Windows.Controls.UserControl pageObjectView = GetVisualParent<Catel.Windows.Controls.UserControl>(result.VisualHit as Shape);
            ACLPPageObjectBaseViewModel pageObjectViewModel = pageObjectView.ViewModel as ACLPPageObjectBaseViewModel;

            if(!pageObjectViewModel.IsInternalPageObject)
            {
                pageObjectViewModel.EraserHitTest((result.VisualHit as Shape).Name);
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Continue;
        }

        void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - Catel? causing this to be called twice
            //Task.Factory.StartNew( () =>
            //    {
                    try
                    {
                        foreach(ICLPPageObject pageObject in PageObjects)
                        {
                            if(pageObject.CanAcceptPageObjects)
                            {
                                ObservableCollection<ICLPPageObject> removedPageObjects = new ObservableCollection<ICLPPageObject>();
                                if(e.OldItems != null)
                                {
                                    foreach (ICLPPageObject removedPageObject in e.OldItems) {
                                        removedPageObjects.Add(removedPageObject);
                                    }
                                }

                                ObservableCollection<ICLPPageObject> addedPageObjects = new ObservableCollection<ICLPPageObject>();
                                if(e.NewItems != null)
                                {
                                    foreach(ICLPPageObject addedPageObject in e.NewItems)
                                    {
                                        if(!pageObject.UniqueID.Equals(addedPageObject.UniqueID)
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
                    catch(System.Exception ex)
                    {
                        Console.WriteLine("PageObjectCollectionChanged Exception: " + ex.Message);
                    }
                //});

        }

        void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if(!Page.IsInkAutoAdding)
            {
                App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
                App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;


                //TODO: Steve - do this in thread pool instead, strokes aren't arriving on projector in correct order.
                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            List<string> removedStrokeIDs = new List<string>();
                            foreach(Stroke stroke in e.Removed)
                            {
                                removedStrokeIDs.Add(stroke.GetStrokeUniqueID());
                            }

                            foreach(Stroke stroke in e.Added)
                            {
                                if(!stroke.ContainsPropertyData(CLPPage.StrokeIDKey))
                                {
                                    string newUniqueID = Guid.NewGuid().ToString();
                                    stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                                    //TODO: Steve - Add Property for time created if necessary.
                                    //TODO: Steve - Add Property for Mutability.
                                    //TODO: Steve - Add Property for UserName of person who created the stroke.
                                }
                                //Ensures truly uniqueIDs
                                foreach(string id in removedStrokeIDs)
                                {
                                    if(id == stroke.GetStrokeUniqueID())
                                    {
                                        stroke.RemovePropertyData(CLPPage.StrokeIDKey);

                                        string newUniqueID = Guid.NewGuid().ToString();
                                        stroke.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                                    }
                                }
                            }

                            foreach(ICLPPageObject pageObject in PageObjects)
                            {
                                if(pageObject.CanAcceptStrokes)
                                {
                                    Rect rect = new Rect(pageObject.XPosition, pageObject.YPosition, pageObject.Width, pageObject.Height);

                                    var addedStrokesOverObject =
                                        from stroke in e.Added
                                        where stroke.HitTest(rect, 3)
                                        select stroke;

                                    var removedStrokesOverObject =
                                        from stroke in e.Removed
                                        where stroke.HitTest(rect, 3)
                                        select stroke;

                                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                        (DispatcherOperationCallback)delegate(object arg)
                                        {
                                            pageObject.AcceptStrokes(new StrokeCollection(addedStrokesOverObject), new StrokeCollection(removedStrokesOverObject));

                                            return null;
                                        }, null);
                                }
                            }

                            if(App.CurrentUserMode == App.UserMode.Instructor)
                            {
                                List<List<byte>> add = new List<List<byte>>(CLPPage.StrokesToBytes(e.Added));
                                List<List<byte>> remove = new List<List<byte>>(CLPPage.StrokesToBytes(e.Removed));

                                string pageID;

                                if(Page.IsSubmission)
                                {
                                    pageID = Page.SubmissionID;
                                }
                                else
                                {
                                    pageID = Page.UniqueID;
                                }

                                if(App.Network.ProjectorProxy != null)
                                {
                                    try
                                    {
                                        App.Network.ProjectorProxy.ModifyPageInkStrokes(add, remove, pageID);
                                    }
                                    catch(System.Exception)
                                    {

                                    }
                                }
                                else
                                {
                                    //TODO: Steve - add pages to a queue and send when a projector is found
                                    Console.WriteLine("Projector NOT Available");
                                }

                                if(App.MainWindowViewModel.Ribbon.BroadcastInkToStudents && !Page.IsSubmission)
                                {
                                    if(App.Network.ClassList.Count > 0)
                                    {
                                        foreach(Person student in App.Network.ClassList)
                                        {
                                            try
                                            {
                                                NetTcpBinding binding = new NetTcpBinding();
                                                binding.Security.Mode = SecurityMode.None;
                                                IStudentContract StudentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
                                                StudentProxy.ModifyPageInkStrokes(add, remove, pageID);
                                                (StudentProxy as ICommunicationObject).Close();
                                            }
                                            catch(System.Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Logger.Instance.WriteToLog("No Students Found");
                                    }
                                }
                            }
                        }
                        catch(System.Exception ex)
                        {
                            Console.WriteLine("InkStrokeCollectionChanged Exception: " + ex.Message);
                        }
                    });
            }
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            Console.WriteLine("PageVM with ID " + ID + " has been notified of InterestedIn Property Change for propertyName " + propertyName);
            if (propertyName == "EditingMode")
            {
                EditingMode = (viewModel as RibbonViewModel).EditingMode;
            }

            if(propertyName == "PenSize")
            {
                if(viewModel is RibbonViewModel)
                {
                    double x = (viewModel as RibbonViewModel).PenSize;
                    EraserShape = new RectangleStylusShape(x, x);
                    DefaultDA.Height = x;
                    DefaultDA.Width = x;
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
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

        /************** UNDO **************/
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
                    //if (pageObject.PageID != Page.UniqueID)
                    //{
                    //    PageHistory.UndoneHistoryItems.Add(item);
                    //    PageHistory.IgnoreHistory = false;
                    //    return;
                    //}
                }
                switch (item.ItemType)
                {
                    case HistoryItemType.AddPageObject:
                        if (pageObject != null)
                        {
                            if (!PageHistory.TrashedPageObjects.ContainsKey(item.ObjectID))
                            {
                                PageHistory.TrashedPageObjects.Add(item.ObjectID, pageObject);
                            }
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
                        //TODO: Steve - fix for no Page.InkStrokes
                        //foreach (Stroke s in Page.InkStrokes )
                        //{
                        //    if (s.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                        //    {
                                 
                        //        Page.InkStrokes.Remove(s);
                        //        PageHistory.TrashedInkStrokes.Add(s.GetPropertyData(CLPPage.StrokeIDKey).ToString(), CLPPage.StrokeToString(s));
                        //        break;
                        //    }
                        //}
                        //if its not in page.inkstrokes then maybe its in stamps inkstrokes?
                        foreach (ICLPPageObject obj in Page.PageObjects)
                        {
                            //if (obj.CanAcceptStrokes && obj.PageObjectByteStrokes.Count > 0)
                            //{
                            //    foreach (byte[] s in obj.PageObjectByteStrokes)
                            //    {
                            //        if (s == item.ObjectID)
                            //        {

                            //            obj.PageObjectByteStrokes.Remove(s.ToString());
                            //            PageHistory.TrashedInkStrokes.Add((CLPPage.ByteToStroke(s) as Stroke).GetPropertyData(CLPPage.StrokeIDKey).ToString(), s);
                            //            break;
                            //        }
                            //    }
                            //}
                        }
                        break;
                    case HistoryItemType.EraseInk:
                        /* foreach (string s in PageHistory.TrashedInkStrokes.Keys)
                         //{
                         //    Stroke inkStroke = CLPPage.StringToStroke(PageHistory.TrashedInkStrokes[s]);
                         //    if (inkStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                         //    {
                         //        PageHistory.TrashedInkStrokes.Remove(s);
                         //        Page.InkStrokes.Add(inkStroke);
                         //        break;
                         //    }
                         //}
                         * } */
                        //Stroke inkStroke = CLPPage.StringToStroke(item.OldValue);
                        //Page.InkStrokes.Add(inkStroke);
                        break;
                        
                    case HistoryItemType.SnapTileSnap:
                        CLPSnapTileContainer t = GetPageObjectByID(item.ObjectID) as CLPSnapTileContainer;
                        if (t.NumberOfTiles != Int32.Parse(item.NewValue))
                        {
                            Console.WriteLine("not newvalue");
                        }
                        t.NumberOfTiles = Int32.Parse(item.OldValue);
                        
                        break;
                    case HistoryItemType.SnapTileRemoveTile:
                        CLPSnapTileContainer tile = GetPageObjectByID(item.ObjectID) as CLPSnapTileContainer;
                        tile.NumberOfTiles++;
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
                //lock (_locker)
                //{
                //    PageHistory.UndoneHistoryItems.Remove(item);
                //}
                 ICLPPageObject pageObject = null;
                 if (item.ObjectID != null)
                 {
                     pageObject = GetPageObjectByID(item.ObjectID);
                     //if (pageObject.PageID != Page.UniqueID)
                     //{
                     //    PageHistory.HistoryItems.Add(item);
                     //    PageHistory.IgnoreHistory = false;
                     //    return;
                     //}
                 }
                switch (item.ItemType)
                {
                    case HistoryItemType.AddPageObject:
                        if (pageObject != null)
                        {
                            CLPServiceAgent.Instance.AddPageObjectToPage(Page, pageObject);
                            if(PageHistory.TrashedPageObjects.ContainsKey(item.ObjectID))
                            {
                                PageHistory.TrashedPageObjects.Remove(item.ObjectID);
                            }
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
                            //Stroke inkStroke = CLPPage.ByteToStroke(PageHistory.TrashedInkStrokes[s]);
                            //steve
                            //if (inkStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                            //{
                            //    Page.InkStrokes.Add(inkStroke);
                            //    PageHistory.TrashedInkStrokes.Remove(s);
                            //    break;
                            //}
                        } 
                        break;
                    case HistoryItemType.EraseInk:
                        //steve
                        //foreach (Stroke s in Page.InkStrokes)
                        //{
                        //    if (s.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                        //    {
                        //        Page.InkStrokes.Remove(s);
                        //        //PageHistory.TrashedInkStrokes.Add(s.GetPropertyData(CLPPage.StrokeIDKey).ToString(), CLPPage.StrokeToString(s));
                        //        break;
                        //    }
                        //}
                       
                      //  Stroke ink = CLPPage.StringToStroke(item.OldValue);
                      //  Page.InkStrokes.Add(ink);
                        break;
                    case HistoryItemType.SnapTileSnap:
                        CLPSnapTileContainer t = GetPageObjectByID(item.ObjectID) as CLPSnapTileContainer;
                        if (t.NumberOfTiles != Int32.Parse(item.OldValue))
                        {
                            Console.WriteLine("not oldvalue");
                        }
                        t.NumberOfTiles = Int32.Parse(item.NewValue);
                       
                        break;
                    case HistoryItemType.SnapTileRemoveTile:
                        CLPSnapTileContainer tile = GetPageObjectByID(item.ObjectID) as CLPSnapTileContainer;
                        tile.NumberOfTiles--;
                        break;
                    default:
                        break;
                }

                Page.PageHistory.HistoryItems.Add(item);
            }

            PageHistory.IgnoreHistory = false;
        }

        #endregion //Methods
                
    }
}