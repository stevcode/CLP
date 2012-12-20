using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class ACLPPageObjectBaseViewModel : ViewModelBase, IPageObjectAdorners
    {
        protected ACLPPageObjectBaseViewModel()
            : base()
        {
            RemovePageObjectCommand = new Command(OnRemovePageObjectCommandExecute);

            DragPageObjectCommand = new Command<DragDeltaEventArgs>(OnDragPageObjectCommandExecute);
            DragStartPageObjectCommand = new Command<DragStartedEventArgs>(OnDragStartPageObjectCommandExecute);
            DragStopPageObjectCommand = new Command<DragCompletedEventArgs>(OnDragStopPageObjectCommandExecute);

            ResizePageObjectCommand = new Command<DragDeltaEventArgs>(OnResizePageObjectCommandExecute);
            ResizeStartPageObjectCommand = new Command<DragStartedEventArgs>(OnResizeStartPageObjectCommandExecute);
            ResizeStopPageObjectCommand = new Command<DragCompletedEventArgs>(OnResizeStopPageObjectCommandExecute);
        }

        public override string Title { get { return "APageObjectBaseVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsInternalPageObject
        {
            get { return GetValue<bool>(IsInternalPageObjectProperty); }
            set { SetValue(IsInternalPageObjectProperty, value); }
        }

        public static readonly PropertyData IsInternalPageObjectProperty = RegisterProperty("IsInternalPageObject", typeof(bool));


        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public ICLPPageObject PageObject
        {
            get { return GetValue<ICLPPageObject>(PageObjectProperty); }
            protected set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof(ICLPPageObject));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool CanAdornersShow
        {
            get { return GetValue<bool>(CanAdornersShowProperty); }
            set { SetValue(CanAdornersShowProperty, value); }
        }

        /// <summary>
        /// Register the CanAdornersShow property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanAdornersShowProperty = RegisterProperty("CanAdornersShow", typeof(bool));


        #endregion //Model

        #region IPageObjectAdorners

        /// <summary>
        /// Shows or hides the adorner.
        /// Set to 'true' to show the adorner or 'false' to hide the adorner.
        /// </summary>
        public bool IsAdornerVisible
        {
            get { return GetValue<bool>(IsAdornerVisibleProperty); }
            set
            {
                SetValue(IsAdornerVisibleProperty, value);
                if (!value)
                {
                    CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                    foreach (CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(parentPage))
                    {
                        pageVM.IsInkCanvasHitTestVisible = true;
                    }
                }
            }
        }

        public static readonly PropertyData IsAdornerVisibleProperty = RegisterProperty("IsAdornerVisible", typeof(bool), false);

        /// <summary>
        /// Set to 'true' to make the adorner automatically fade-in and become visible when the mouse is hovered
        /// over the adorned control.  Also the adorner automatically fades-out when the mouse cursor is moved
        /// away from the adorned control (and the adorner).
        /// </summary>
        public bool IsMouseOverShowEnabled
        {
            get { return GetValue<bool>(IsMouseOverShowEnabledProperty); }
            set { SetValue(IsMouseOverShowEnabledProperty, value); }
        }

        public static readonly PropertyData IsMouseOverShowEnabledProperty = RegisterProperty("IsMouseOverShowEnabled", typeof(bool), false);

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves over the 
        /// adorned control (or the adorner) when the adorner begins to fade in.
        /// </summary>
        public double OpenAdornerTimeOut
        {
            get { return GetValue<double>(OpenAdornerTimeOutProperty); }
            set { SetValue(OpenAdornerTimeOutProperty, value); }
        }

        public static readonly PropertyData OpenAdornerTimeOutProperty = RegisterProperty("OpenAdornerTimeOut", typeof(double), 0.8);

        /// <summary>
        /// Specifies the time (in seconds) after the mouse cursor moves away from the 
        /// adorned control (or the adorner) when the adorner begins to fade out.
        /// </summary>
        public double CloseAdornerTimeOut
        {
            get { return GetValue<double>(CloseAdornerTimeOutProperty); }
            set { SetValue(CloseAdornerTimeOutProperty, value); }
        }

        public static readonly PropertyData CloseAdornerTimeOutProperty = RegisterProperty("CloseAdornerTimeOut", typeof(double), 1.0);

        public virtual bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(IsBackground)
            {
                if(App.MainWindowViewModel.IsAuthoring)
                {
                    IsMouseOverShowEnabled = true;
                    return false;
                }
                else
                {
                    IsMouseOverShowEnabled = false;
                    return true;
                }
            }
            else
            {
                IsMouseOverShowEnabled = true;
                return false;
            }
        }

        public virtual void EraserHitTest(string hitBoxName)
        {
            if(IsBackground && !App.MainWindowViewModel.IsAuthoring)
            {
                //don't erase
            }
            else
            {
                OnRemovePageObjectCommandExecute();
            }
        }


        #endregion //IPageObjectAdorners

        #region Commands

        #region Default Adorners

        /// <summary>
        /// Gets the RemovePageObjectCommand command.
        /// </summary>
        public Command RemovePageObjectCommand { get; set; }

        private void OnRemovePageObjectCommandExecute()
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            foreach (CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(parentPage))
            {
                pageVM.IsInkCanvasHitTestVisible = true;
            }

            CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Gets the DragPageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragPageObjectCommand { get; set; }

        private void OnDragPageObjectCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double x = PageObject.XPosition + e.HorizontalChange;
            double y = PageObject.YPosition + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > parentPage.PageWidth - PageObject.Width)
            {
                x = parentPage.PageWidth - PageObject.Width;
            }
            if (y > parentPage.PageHeight - PageObject.Height)
            {
                y = parentPage.PageHeight - PageObject.Height;
            }
            Point pt = new Point(x, y);

            if (PageObject.CanAcceptStrokes)
            {
                double xDelta = x - PageObject.XPosition;
                double yDelta = y - PageObject.YPosition;
                Matrix moveStroke = new Matrix();
                moveStroke.Translate(xDelta, yDelta);

                StrokeCollection strokesToMove = PageObject.GetStrokesOverPageObject();
                foreach(Stroke stroke in strokesToMove)
                {  
                    stroke.Transform(moveStroke, true);  
                }
            }

            if (PageObject.PageObjectObjectParentIDs.Count > 0)
            {
                double xDelta = x - PageObject.XPosition;
                double yDelta = y - PageObject.YPosition;

                foreach(ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
                {
                    Point pageObjectPt = new Point((xDelta + pageObject.XPosition), (yDelta + pageObject.YPosition));
                    CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pageObjectPt);
                }
            }

            CLPServiceAgent.Instance.ChangePageObjectPosition(PageObject, pt);
        }

        /// <summary>
        /// Gets the DragStartPageObjectCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> DragStartPageObjectCommand { get; set; }

        /// <summary>
        /// Method to invoke when the DragStartPageObjectCommand command is executed.
        /// </summary>
        private void OnDragStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
        }

        /// <summary>
        /// Gets the DragStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> DragStopPageObjectCommand { get; set; }

        /// <summary>
        /// Method to invoke when the DragStopPageObjectCommand command is executed.
        /// </summary>
        private void OnDragStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            AddRemovePageObjectFromOtherObjects();
        }

        protected void AddRemovePageObjectFromOtherObjects() {
            if (!PageObject.CanAcceptPageObjects)
            {
                var containerQuery = from po in PageObject.ParentPage.PageObjects
                                     where (po.CanAcceptPageObjects == true && !PageObject.ParentID.Equals(po.UniqueID))
                                     select po;

                foreach (ICLPPageObject container in containerQuery)
                {
                    if (container.PageObjectIsOver(this.PageObject, .50))
                    {
                        List<string> addObject = new List<string>();
                        addObject.Add(this.PageObject.UniqueID);
                        container.AcceptObjects(addObject, new ObservableCollection<ICLPPageObject>());
                    }
                    else
                    {
                        ObservableCollection<ICLPPageObject> removeObject = new ObservableCollection<ICLPPageObject>();
                        removeObject.Add(this.PageObject);
                        container.AcceptObjects(new List<string>(), removeObject);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizePageObjectCommand { get; set; }

        private void OnResizePageObjectCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double newHeight = PageObject.Height + e.VerticalChange;
            double newWidth = PageObject.Width + e.HorizontalChange;
            if (newHeight < 10)
            {
                newHeight = 10;
            }
            if (newWidth < 10)
            {
                newWidth = 10;
            }
            if (newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
            }
            if (newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

        /// <summary>
        /// Gets the ResizeStartPageObjectCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> ResizeStartPageObjectCommand { get; set; }

        private void OnResizeStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
        }

        /// <summary>
        /// Gets the ResizeStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> ResizeStopPageObjectCommand { get; set; }

        private void OnResizeStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
        }

        #endregion //Default Adorners

        #endregion //Commands

        #region Methods

        

        #endregion //Methods
    }
}