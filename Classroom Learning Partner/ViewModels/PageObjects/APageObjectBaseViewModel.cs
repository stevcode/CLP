using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;
using System.Windows.Threading;
using System;
using System.Timers;

namespace Classroom_Learning_Partner.ViewModels
{
    abstract public class APageObjectBaseViewModel : ViewModelBase, IPageObjectAdorners
    {
        #region Constructor

        protected APageObjectBaseViewModel()
        {
            InitializeCommands();

            //TODO: Steve - move this to Adorner.cs and expand adorner API
            hoverTimer = new Timer();
            hoverTimer.Interval = 800;
            hoverTimer.Elapsed += hoverTimer_Elapsed;
        }

        private void InitializeCommands()
        {
            RemovePageObjectCommand = new Command(OnRemovePageObjectCommandExecute);
            ErasePageObjectCommand = new Command<MouseEventArgs>(OnErasePageObjectCommandExecute);

            DragPageObjectCommand = new Command<DragDeltaEventArgs>(OnDragPageObjectCommandExecute);
            DragStartPageObjectCommand = new Command<DragStartedEventArgs>(OnDragStartPageObjectCommandExecute);
            DragStopPageObjectCommand = new Command<DragCompletedEventArgs>(OnDragStopPageObjectCommandExecute);

            ResizePageObjectCommand = new Command<DragDeltaEventArgs>(OnResizePageObjectCommandExecute);
            ResizeStartPageObjectCommand = new Command<DragStartedEventArgs>(OnResizeStartPageObjectCommandExecute);
            ResizeStopPageObjectCommand = new Command<DragCompletedEventArgs>(OnResizeStopPageObjectCommandExecute);

            ToggleMainAdornersCommand = new Command<MouseButtonEventArgs>(OnToggleMainAdornersCommandExecute);
        }

        public override string Title { get { return "APageObjectBaseVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public IPageObject PageObject
        {
            get { return GetValue<IPageObject>(PageObjectProperty); }
            protected set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof(IPageObject));

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
     //TODO: Entities, remove?   [ViewModelToModel("PageObject")]
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
   //TODO: Entities, remove     [ViewModelToModel("PageObject")]
        public bool CanAdornersShow
        {
            get { return GetValue<bool>(CanAdornersShowProperty); }
            set { SetValue(CanAdornersShowProperty, value); }
        }

        /// <summary>
        /// Register the CanAdornersShow property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanAdornersShowProperty = RegisterProperty("CanAdornersShow", typeof(bool), true);

        #endregion //Model

        #region IPageObjectAdorners

        /// <summary>
        /// Shows or hides the adorner.
        /// Set to 'true' to show the adorner or 'false' to hide the adorner.
        /// </summary>
        public bool IsAdornerVisible
        {
            get { return GetValue<bool>(IsAdornerVisibleProperty); }
            set { SetValue(IsAdornerVisibleProperty, value); }
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

        public static readonly PropertyData OpenAdornerTimeOutProperty = RegisterProperty("OpenAdornerTimeOut", typeof(double), 0.0);

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

        protected Timer hoverTimer;

        protected bool hoverTimeElapsed = false;

        protected bool timerRunning = false;
        void hoverTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            hoverTimer.Stop();
            timerRunning = false;
            hoverTimeElapsed = true;
        }

        #endregion //IPageObjectAdorners

        public virtual void ClearAdorners()
        {
            IsAdornerVisible = false;
        }

        #region Commands

        #region Default Adorners

        /// <summary>
        /// Removes pageObject from page when Delete button is pressed.
        /// </summary>
        public Command RemovePageObjectCommand { get; set; }

        private void OnRemovePageObjectCommandExecute()
        {
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Removes pageObject from page if back of pen (or middle mouse button)
        /// is pressed while passing over the pageObject.
        /// </summary>
        public Command<MouseEventArgs> ErasePageObjectCommand { get; set; }

        private void OnErasePageObjectCommandExecute(MouseEventArgs e)
        {
            if(!App.MainWindowViewModel.IsAuthoring && IsBackground)
            {
                return;
            }

            if((e.StylusDevice != null && e.StylusDevice.Inverted && e.LeftButton == MouseButtonState.Pressed) || e.MiddleButton == MouseButtonState.Pressed)
            {
                ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
            }
        }

        /// <summary>
        /// Gets the DragPageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragPageObjectCommand { get; set; }

        private void OnDragPageObjectCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;

            var newX = Math.Max(0, PageObject.XPosition + e.HorizontalChange);
            newX = Math.Min(newX, parentPage.Width - PageObject.Width);
            var newY = Math.Max(0, PageObject.YPosition + e.VerticalChange);
            newY = Math.Min(newY, parentPage.Height - PageObject.Height);

            ChangePageObjectPosition(PageObject, newX, newY);
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
            // TODO: Entities
            //PageObject.ParentPage.PageHistory.BeginBatch(new CLPHistoryPageObjectMoveBatch(PageObject.ParentPage,
            //                                                                               PageObject.ID,
            //                                                                               new Point(PageObject.XPosition,
            //                                                                                         PageObject.YPosition)));
        }

        /// <summary>
        /// Gets the DragStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> DragStopPageObjectCommand { get; set; }

        private void OnDragStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            // TODO: Entities
            //var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            //if(batch is CLPHistoryPageObjectMoveBatch)
            //{
            //    (batch as CLPHistoryPageObjectMoveBatch).AddPositionPointToBatch(PageObject.ID,
            //                                                                     new Point(PageObject.XPosition,
            //                                                                               PageObject.YPosition));
            //}
            //var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved();
        }

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizePageObjectCommand { get; set; }

        private void OnResizePageObjectCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;
            const int MIN_WIDTH = 20;
            const int MIN_HEIGHT = 20;

            var newWidth = Math.Max(MIN_WIDTH, PageObject.Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - PageObject.XPosition);
            var newHeight = Math.Max(MIN_HEIGHT, PageObject.Height + e.VerticalChange);
            newHeight = Math.Min(newHeight, parentPage.Height - PageObject.YPosition);
            
            ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

        /// <summary>
        /// Gets the ResizeStartPageObjectCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> ResizeStartPageObjectCommand { get; set; }

        private void OnResizeStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
            // TODO: Entities
            //PageObject.ParentPage.PageHistory.BeginBatch(new CLPHistoryPageObjectResizeBatch(PageObject.ParentPage,
            //                                                                               PageObject.ID,
            //                                                                               new Point(PageObject.Width,
            //                                                                                         PageObject.Height)));
        }

        /// <summary>
        /// Gets the ResizeStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> ResizeStopPageObjectCommand { get; set; }

        private void OnResizeStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            // TODO: Entities
            //var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            //if(batch is CLPHistoryPageObjectResizeBatch)
            //{
            //    (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.ID,
            //                                                                     new Point(PageObject.Width,
            //                                                                               PageObject.Height));
            //}
            //var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnResized();
        }

        #endregion //Default Adorners

        #region Control Adorners

        /// <summary>
        /// Gets the ToggleMainAdornersCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> ToggleMainAdornersCommand { get; set; }

        private void OnToggleMainAdornersCommandExecute(MouseButtonEventArgs e)
        {
            if(!App.MainWindowViewModel.IsAuthoring && IsBackground)
            {
                return;
            }

            if(e.ChangedButton == MouseButton.Left && !(e.StylusDevice != null && e.StylusDevice.Inverted))
            {
                var tempAdornerState = IsAdornerVisible;
                ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
                IsAdornerVisible = !tempAdornerState;
            }
        }

        #endregion //Control Adorners

        #endregion //Commands

        #region Static Methods

        public static void ChangePageObjectPosition(IPageObject pageObject, double newX, double newY, bool useHistory = true)
        {
            var oldXPos = pageObject.XPosition;
            var oldYPos = pageObject.YPosition;
            var xDelta = newX - oldXPos;
            var yDelta = newY - oldYPos;

            // TODO: Entities
            //if(pageObject.CanAcceptPageObjects && pageObject.PageObjectObjectParentIDs.Any())
            //{
            //    foreach(var childPageObject in pageObject.GetPageObjectsOverPageObject())
            //    {
            //        ChangePageObjectPosition(childPageObject, xDelta + childPageObject.XPosition, yDelta + childPageObject.YPosition, false);
            //    }
            //}

            //if(pageObject.CanAcceptStrokes && pageObject.PageObjectStrokeParentIDs.Any())
            //{
            //    var moveStroke = new Matrix();
            //    moveStroke.Translate(xDelta, yDelta);

            //    foreach(var stroke in pageObject.GetStrokesOverPageObject())
            //    {
            //        stroke.Transform(moveStroke, true);
            //    }
            //}

            //var xDiff = Math.Abs(xDelta);
            //var yDiff = Math.Abs(yDelta);
            //var diff = xDiff + yDiff;
            //if(diff > CLPHistory.SAMPLE_RATE && useHistory)
            //{
            //    var batch = pageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            //    if(batch is CLPHistoryPageObjectMoveBatch)
            //    {
            //        (batch as CLPHistoryPageObjectMoveBatch).AddPositionPointToBatch(pageObject.UniqueID, new Point(newX, newY));
            //    }
            //    else
            //    {
            //        Logger.Instance.WriteToLog("Error: Current Batch not ChangePositionBatch.");
            //        var batchHistoryItem = pageObject.ParentPage.PageHistory.EndBatch();
            //        ACLPPageBaseViewModel.AddHistoryItemToPage(pageObject.ParentPage, batchHistoryItem, true);
            //    }
            //}

            pageObject.XPosition = newX;
            pageObject.YPosition = newY;
        }

        public static void ChangePageObjectDimensions(IPageObject pageObject, double height, double width, bool useHistory = true)
        {
            var oldHeight = pageObject.Height;
            var oldWidth = pageObject.Width;
            var heightDiff = Math.Abs(oldHeight - height);
            var widthDiff = Math.Abs(oldWidth - width);
            var diff = heightDiff + widthDiff;
            // TODO: Entities
            //if(diff > CLPHistory.SAMPLE_RATE && useHistory)
            //{
            //    var batch = pageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            //    if(batch is CLPHistoryPageObjectResizeBatch)
            //    {
            //        (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(pageObject.UniqueID,
            //                                                                         new Point(width, height));
            //    }
            //    else
            //    {
            //        Logger.Instance.WriteToLog("Error: Current Batch not ResizeBatch.");
            //        var batchHistoryItem = pageObject.ParentPage.PageHistory.EndBatch();
            //        ACLPPageBaseViewModel.AddHistoryItemToPage(pageObject.ParentPage, batchHistoryItem, true);
            //    }
            //}

            pageObject.Height = height;
            pageObject.Width = width;
        }

        #endregion //Static Methods
    }
}