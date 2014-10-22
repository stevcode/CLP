using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APageObjectBaseViewModel : ViewModelBase, IPageObjectAdorners
    {
        #region Constructor

        protected APageObjectBaseViewModel() { InitializeCommands(); }

        private void InitializeCommands()
        {
            RemovePageObjectCommand = new Command(OnRemovePageObjectCommandExecute);
            ErasePageObjectCommand = new Command<MouseEventArgs>(OnErasePageObjectCommandExecute);

            DragStartPageObjectCommand = new Command<DragStartedEventArgs>(OnDragStartPageObjectCommandExecute);
            DragPageObjectCommand = new Command<DragDeltaEventArgs>(OnDragPageObjectCommandExecute);
            DragStopPageObjectCommand = new Command<DragCompletedEventArgs>(OnDragStopPageObjectCommandExecute);

            ResizeStartPageObjectCommand = new Command<DragStartedEventArgs>(OnResizeStartPageObjectCommandExecute);
            ResizePageObjectCommand = new Command<DragDeltaEventArgs>(OnResizePageObjectCommandExecute);
            ResizeStopPageObjectCommand = new Command<DragCompletedEventArgs>(OnResizeStopPageObjectCommandExecute);

            ToggleMainAdornersCommand = new Command<MouseButtonEventArgs>(OnToggleMainAdornersCommandExecute);
        }

        public override string Title
        {
            get { return "APageObjectBaseVM"; }
        }

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

        #endregion //IPageObjectAdorners

        public virtual void ClearAdorners() { IsAdornerVisible = false; }

        #region Commands

        #region Default Adorners

        /// <summary>
        /// Removes pageObject from page when Delete button is pressed.
        /// </summary>
        public Command RemovePageObjectCommand { get; set; }

        private void OnRemovePageObjectCommandExecute()
        {
            var contextRibbon = NotebookWorkspaceViewModel.GetContextRibbon();
            if (contextRibbon != null)
            {
                contextRibbon.Buttons.Clear();
            }

            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Removes pageObject from page if back of pen (or middle mouse button)
        /// is pressed while passing over the pageObject.
        /// </summary>
        public Command<MouseEventArgs> ErasePageObjectCommand { get; set; }

        private void OnErasePageObjectCommandExecute(MouseEventArgs e)
        {
            if(App.MainWindowViewModel.CurrentUser.ID != PageObject.CreatorID &&
               !PageObject.IsManipulatableByNonCreator)
            {
                return;
            }

            if((e.StylusDevice != null && e.StylusDevice.Inverted && e.LeftButton == MouseButtonState.Pressed) ||
               e.MiddleButton == MouseButtonState.Pressed)
            {
                ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
            }
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
            PageObject.ParentPage.History.BeginBatch(new PageObjectMoveBatchHistoryItem(PageObject.ParentPage,
                                                                                        App.MainWindowViewModel.CurrentUser,
                                                                                        PageObject.ID,
                                                                                        new Point(PageObject.XPosition, PageObject.YPosition)));
        }

        /// <summary>
        /// Gets the DragPageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragPageObjectCommand { get; set; }

        private void OnDragPageObjectCommandExecute(DragDeltaEventArgs e)
        {
            var initialX = XPosition;
            var initialY = YPosition;

            var parentPage = PageObject.ParentPage;

            var newX = Math.Max(0, PageObject.XPosition + e.HorizontalChange);
            newX = Math.Min(newX, parentPage.Width - Width);
            var newY = Math.Max(0, PageObject.YPosition + e.VerticalChange);
            newY = Math.Min(newY, parentPage.Height - Height);

            ChangePageObjectPosition(PageObject, newX, newY);
            PageObject.OnMoving(initialX, initialY);
        }

        /// <summary>
        /// Gets the DragStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> DragStopPageObjectCommand { get; set; }

        private void OnDragStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            var initialX = XPosition;
            var initialY = YPosition;

            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if(batch is PageObjectMoveBatchHistoryItem)
            {
                (batch as PageObjectMoveBatchHistoryItem).AddPositionPointToBatch(PageObject.ID, new Point(PageObject.XPosition, PageObject.YPosition));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved(initialX, initialY);
        }

        /// <summary>
        /// Gets the ResizeStartPageObjectCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> ResizeStartPageObjectCommand { get; set; }

        private void OnResizeStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
            PageObject.ParentPage.History.BeginBatch(new PageObjectResizeBatchHistoryItem(PageObject.ParentPage,
                                                                                          App.MainWindowViewModel.CurrentUser,
                                                                                          PageObject.ID,
                                                                                          new Point(PageObject.Width, PageObject.Height)));
        }

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizePageObjectCommand { get; set; }

        private void OnResizePageObjectCommandExecute(DragDeltaEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var parentPage = PageObject.ParentPage;
            const int MIN_WIDTH = 20;
            const int MIN_HEIGHT = 20;

            var newWidth = Math.Max(MIN_WIDTH, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);
            var newHeight = Math.Max(MIN_HEIGHT, Height + e.VerticalChange);
            newHeight = Math.Min(newHeight, parentPage.Height - YPosition);

            ChangePageObjectDimensions(PageObject, newHeight, newWidth);
            PageObject.OnResizing(initialWidth, initialHeight);
        }

        /// <summary>
        /// Gets the ResizeStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> ResizeStopPageObjectCommand { get; set; }

        private void OnResizeStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if(batch is PageObjectResizeBatchHistoryItem)
            {
                (batch as PageObjectResizeBatchHistoryItem).AddResizePointToBatch(PageObject.ID, new Point(Width, Height));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnResized(initialWidth, initialHeight);
        }

        #endregion //Default Adorners

        #region Control Adorners

        /// <summary>
        /// Gets the ToggleMainAdornersCommand command.
        /// </summary>
        public Command<MouseButtonEventArgs> ToggleMainAdornersCommand { get; set; }

        private void OnToggleMainAdornersCommandExecute(MouseButtonEventArgs e)
        {
            if(App.MainWindowViewModel.CurrentUser.ID != PageObject.CreatorID &&
               !PageObject.IsManipulatableByNonCreator)
            {
                return;
            }

            if(e.ChangedButton != MouseButton.Left ||
               e.StylusDevice != null && e.StylusDevice.Inverted)
            {
                return;
            }

            var tempAdornerState = IsAdornerVisible;
            ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
            IsAdornerVisible = !tempAdornerState;
        }

        #endregion //Control Adorners

        #endregion //Commands

        #region Static Methods

        public static void ApplyDistinctPosition(IPageObject pageObject)
        {

        }

        public static void ApplyDistinctPosition(IEnumerable<IPageObject> pageObjects)
        {

        }

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

            var xDiff = Math.Abs(xDelta);
            var yDiff = Math.Abs(yDelta);
            var diff = xDiff + yDiff;
            if(diff > PageHistory.SAMPLE_RATE && useHistory)
            {
                var batch = pageObject.ParentPage.History.CurrentHistoryBatch;
                if(batch is PageObjectMoveBatchHistoryItem)
                {
                    (batch as PageObjectMoveBatchHistoryItem).AddPositionPointToBatch(pageObject.ID, new Point(newX, newY));
                }
                else
                {
                    Logger.Instance.WriteToLog("Error: Current Batch not ChangePositionBatch.");
                    var batchHistoryItem = pageObject.ParentPage.History.EndBatch();
                    ACLPPageBaseViewModel.AddHistoryItemToPage(pageObject.ParentPage, batchHistoryItem, true);
                }
            }

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
            if(diff > PageHistory.SAMPLE_RATE && useHistory)
            {
                var batch = pageObject.ParentPage.History.CurrentHistoryBatch;
                if(batch is PageObjectResizeBatchHistoryItem)
                {
                    (batch as PageObjectResizeBatchHistoryItem).AddResizePointToBatch(pageObject.ID, new Point(width, height));
                }
                else
                {
                    Logger.Instance.WriteToLog("Error: Current Batch not ResizeBatch.");
                    var batchHistoryItem = pageObject.ParentPage.History.EndBatch();
                    ACLPPageBaseViewModel.AddHistoryItemToPage(pageObject.ParentPage, batchHistoryItem, true);
                }
            }

            pageObject.Height = height;
            pageObject.Width = width;
        }

        #endregion //Static Methods
    }
}