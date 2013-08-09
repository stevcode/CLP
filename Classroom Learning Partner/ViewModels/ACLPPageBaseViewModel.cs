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
    abstract public class ACLPPageBaseViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        protected ACLPPageBaseViewModel(ICLPPage page)
        {
            PageInteractionMode = App.MainWindowViewModel.Ribbon.PageInteractionMode;
            DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
            EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
            EraserMode = App.MainWindowViewModel.Ribbon.EraserMode;
            Page = page;

            InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
            PageObjects.CollectionChanged += PageObjects_CollectionChanged;
            
            MouseMoveCommand = new Command<MouseEventArgs>(OnMouseMoveCommandExecute);
            MouseDownCommand = new Command<MouseEventArgs>(OnMouseDownCommandExecute);
            MouseUpCommand = new Command<MouseEventArgs>(OnMouseUpCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);
        }
        
        public override string Title { get { return "APageBaseVM"; } }

        #endregion //Constructor

        #region Properties
       
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

        /// <summary>
        /// Sets the PageInteractionMode.
        /// </summary>
        public PageInteractionMode PageInteractionMode
        {
            get { return GetValue<PageInteractionMode>(PageInteractionModeProperty); }
            set
            {
                SetValue(PageInteractionModeProperty, value);
                //TODO: Implement catel's OnPropertyChanged method for the below code.
                Logger.Instance.WriteToLog("PageInteractionMode Set to: " + value);
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
                        var tileStream = Application.GetResourceStream(new Uri("/Classroom Learning Partner;component/Images/GreenTile.cur", UriKind.Relative));
                        PageCursor = new Cursor(tileStream.Stream);
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
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); } //TODO: make setter Private to force use of PageInteractionMode
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
            if(PageInteractionMode == PageInteractionMode.Tile)
            {
                var pageObjectCanvas = FindNamedChild<Canvas>(TopCanvas, "PageObjectCanvas");
                var pt = e.GetPosition(pageObjectCanvas);
                var tile = new CLPSnapTileContainer(pt, Page);
                PageObjects.Add(tile);
            }
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
            //TODO: make message a string, make separate string for AuthoringMode to warn that all pageObjects will be deleted.
            if(MessageBox.Show("Are you sure you want to clear everything on this page? All strokes, arrays, and animations will be erased!",
                                "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).LinkedDisplay.SetPageBorderColor();

            Page.PageHistory.ClearHistory();

            //TODO: if in AuthoringMode, just PageObjects.Clear();
            var nonBackgroundPageObjects = Page.PageObjects.Where(pageObject => pageObject.IsBackground != true).ToList();
            foreach(var pageObject in nonBackgroundPageObjects)
            {
                Page.PageObjects.Remove(pageObject);
            }

            Page.InkStrokes.Clear();
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
                    aclpPageObjectBaseViewModel.IsAdornerVisible = false;
                }
            }
        }

        void PageObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        void InkStrokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if(IsPagePreview)
            {
                return;
            }
            
            foreach(var stroke in e.Added.Where(stroke => PageInteractionMode == PageInteractionMode.Scissors)) 
            {
                InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                PageObjects.CollectionChanged -= PageObjects_CollectionChanged;
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
                    CLPServiceAgent.Instance.AddPageObjectToPage(Page, pageObject, false);
                }

                Page.PageHistory.AddHistoryItem(new CLPHistoryPageObjectCut(Page, stroke, allCutPageObjects, allHalvedPageObjectIDs));
                    
                RefreshInkStrokes();
                RefreshPageObjects(allHalvedPageObjects);

                InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                PageObjects.CollectionChanged += PageObjects_CollectionChanged;
                return;
            }
            
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
            App.MainWindowViewModel.Ribbon.CanGroupSendToTeacher = true;

            //TODO: Steve - do this in thread queue instead, strokes aren't arriving on projector in correct order.
            //Task.Factory.StartNew(() =>
            //    {
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
                Logger.Instance.WriteToLog("PageViewModel Received PageInteractionMode Command: " + PageInteractionMode);
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