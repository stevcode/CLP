using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class ACLPPageObjectBaseViewModel : ViewModelBase
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

        /// <summary>
        /// Register the PageObject property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the Height property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the Width property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the XPosition property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the YPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double));

        private StrokeCollection _pageObjectStrokes = new StrokeCollection();
        public StrokeCollection PageObjectStrokes
        {
            get
            {
                _pageObjectStrokes = CLPPage.BytesToStrokes(PageObject.PageObjectByteStrokes);
                return _pageObjectStrokes;
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<ICLPPageObject> PageObjectObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectObjectsProperty); }
            set { SetValue(PageObjectObjectsProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectObjectsProperty = RegisterProperty("PageObjectObjects", typeof(ObservableCollection<ICLPPageObject>), new ObservableCollection<ICLPPageObject>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        /// <summary>
        /// Register the IsBackground property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool));

        #endregion //Model

        #region Bindings

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

        public Visibility AllowAdorner
        {
            get { return GetValue<Visibility>(AllowAdornerProperty); }
            set { SetValue(AllowAdornerProperty, value); }
        }

        public static readonly PropertyData AllowAdornerProperty = RegisterProperty("AllowAdorner", typeof(Visibility), Visibility.Visible);

        #endregion //Bindings

        #region Commands

        #region Default Adorners

        /// <summary>
        /// Gets the RemovePageObjectCommand command.
        /// </summary>
        public Command RemovePageObjectCommand { get; set; }

        /// <summary>
        /// Method to invoke when the RemovePageObjectCommand command is executed.
        /// </summary>
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

        /// <summary>
        /// Method to invoke when the DragPageObjectCommand command is executed.
        /// </summary>
        private void OnDragPageObjectCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double x = PageObject.XPosition + e.HorizontalChange;
            double y = PageObject.YPosition + e.VerticalChange;
            double xStrokeOffset = e.HorizontalChange;
            double yStrokeOffset = e.VerticalChange;
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
                foreach (Stroke stroke in parentPage.InkStrokes)
                {
                    foreach (Stroke vmStroke in PageObjectStrokes)
                    {
                        if (stroke.GetPropertyData(CLPPage.StrokeIDKey).Equals(vmStroke.GetPropertyData(CLPPage.StrokeIDKey)))
                        {
                            stroke.Transform(moveStroke, false);
                        }
                    }
                }
            }

            if (PageObject.CanAcceptObjects)
            {
                double xDelta = x - PageObject.XPosition;
                double yDelta = y - PageObject.YPosition;
                Matrix moveStroke = new Matrix();
                moveStroke.Translate(xDelta, yDelta);
                foreach (ICLPPageObject pageObject in parentPage.PageObjects)
                {
                    foreach (ICLPPageObject vmPageObject in PageObjectObjects)
                    {
                        if (pageObject.UniqueID.Equals(vmPageObject.UniqueID))
                        {
                            Point pageObjectPt = new Point((xDelta + pageObject.XPosition), (yDelta + pageObject.YPosition));
                            CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pageObjectPt);
                        }
                    }
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
            if (!GetType().Equals(typeof(CLPStampViewModel)))
            {
                var stampQuery = from po in this.PageObject.ParentPage.PageObjects where (po.GetType().Equals(typeof(CLPStamp))) select po;
                foreach (CLPStamp stamp in stampQuery)
                {
                    if (!this.PageObject.ParentID.Equals(stamp.UniqueID) && stamp.HitTest(this.PageObject, .50))
                    {
                        if (!stamp.PageObjectObjects.Contains(this.PageObject))
                        {
                            stamp.AcceptObject(this.PageObject);
                            Console.WriteLine("Success Add Move  " + this.PageObject.UniqueID + " to " + stamp.UniqueID + " length: " + stamp.PageObjectObjects.Count);
                        }
                    }
                    else
                    {
                        if (stamp.PageObjectObjects.Contains(this.PageObject))
                        {
                            stamp.RemoveObject(this.PageObject);
                            Console.WriteLine("Success Remove Move " + this.PageObject.UniqueID + " to " + stamp.UniqueID + " length: " + stamp.PageObjectObjects.Count);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizePageObjectCommand { get; set; }

        /// <summary>
        /// Method to invoke when the ResizePageObjectCommand command is executed.
        /// </summary>
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

        /// <summary>
        /// Method to invoke when the ResizeStartPageObjectCommand command is executed.
        /// </summary>
        private void OnResizeStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
        }

        /// <summary>
        /// Gets the ResizeStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> ResizeStopPageObjectCommand { get; set; }

        /// <summary>
        /// Method to invoke when the ResizeStopPageObjectCommand command is executed.
        /// </summary>
        private void OnResizeStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
        }

        #endregion //Default Adorners

        #endregion //Commands

        #region Methods

        public virtual bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if (IsBackground)
            {
                if (App.MainWindowViewModel.IsAuthoring)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        // Returns a boolean stating if the @percentage of the @pageObject is contained within the item.
        public virtual bool HitTest(ICLPPageObject pageObject, double percentage)
        {
            double areaObject = pageObject.Height * pageObject.Width;
            double top = Math.Max(YPosition, pageObject.YPosition);
            double bottom = Math.Min(YPosition + Height, pageObject.YPosition + pageObject.Height);
            double left = Math.Max(XPosition, pageObject.XPosition);
            double right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            double deltaY = bottom - top;
            double deltaX = right - left;
            double intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && intersectionArea / areaObject >= percentage;
        }

        #endregion //Methods
    }
}