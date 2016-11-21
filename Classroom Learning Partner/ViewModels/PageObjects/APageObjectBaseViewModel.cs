using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APageObjectBaseViewModel : ViewModelBase, IPageObjectAdorners
    {
        protected static ContextRibbonViewModel ContextRibbon
        {
            get { return NotebookWorkspaceViewModel.GetContextRibbon(); }
        }

        protected readonly ObservableCollection<UIElement> _contextButtons = new ObservableCollection<UIElement>();
        protected readonly IPageInteractionService _pageInteractionService;

        #region Constructor

        protected APageObjectBaseViewModel()
        {
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            InitializeCommands();
            _contextButtons.Add(new RibbonButton("Delete", "pack://application:,,,/Resources/Images/Delete.png", RemovePageObjectCommand, null, true));
        }

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
        }

        #endregion //Constructor

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public IPageObject PageObject
        {
            get { return GetValue<IPageObject>(PageObjectProperty); }
            protected set { SetValue(PageObjectProperty, value); }
        }

        public static readonly PropertyData PageObjectProperty = RegisterProperty("PageObject", typeof (IPageObject));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof (double));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof (double));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof (double));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof (double));

        #endregion //Model

        #region Properties

        public bool IsBackgroundPageObject
        {
            get { return App.MainWindowViewModel.CurrentUser.ID != PageObject.CreatorID && !PageObject.IsManipulatableByNonCreator; }
        }

        #endregion //Properties

        #region IPageObjectAdorners

        /// <summary>Shows or hides the adorner. Set to 'true' to show the adorner or 'false' to hide the adorner.</summary>
        public bool IsAdornerVisible
        {
            get { return GetValue<bool>(IsAdornerVisibleProperty); }
            set
            {
                var previousValue = IsAdornerVisible;
                SetValue(IsAdornerVisibleProperty, value);
                PopulateContextRibbon(previousValue != value);
            }
        }

        public static readonly PropertyData IsAdornerVisibleProperty = RegisterProperty("IsAdornerVisible", typeof (bool), false);

        /// <summary>
        ///     Set to 'true' to make the adorner automatically fade-in and become visible when the mouse is hovered over the adorned control.  Also the
        ///     adorner automatically fades-out when the mouse cursor is moved away from the adorned control (and the adorner).
        /// </summary>
        public bool IsMouseOverShowEnabled
        {
            get { return GetValue<bool>(IsMouseOverShowEnabledProperty); }
            set { SetValue(IsMouseOverShowEnabledProperty, value); }
        }

        public static readonly PropertyData IsMouseOverShowEnabledProperty = RegisterProperty("IsMouseOverShowEnabled", typeof (bool), false);

        /// <summary>Specifies the time (in seconds) after the mouse cursor moves over the adorned control (or the adorner) when the adorner begins to fade in.</summary>
        public double OpenAdornerTimeOut
        {
            get { return GetValue<double>(OpenAdornerTimeOutProperty); }
            set { SetValue(OpenAdornerTimeOutProperty, value); }
        }

        public static readonly PropertyData OpenAdornerTimeOutProperty = RegisterProperty("OpenAdornerTimeOut", typeof (double), 0.0);

        /// <summary>
        ///     Specifies the time (in seconds) after the mouse cursor moves away from the adorned control (or the adorner) when the adorner begins to fade
        ///     out.
        /// </summary>
        public double CloseAdornerTimeOut
        {
            get { return GetValue<double>(CloseAdornerTimeOutProperty); }
            set { SetValue(CloseAdornerTimeOutProperty, value); }
        }

        public static readonly PropertyData CloseAdornerTimeOutProperty = RegisterProperty("CloseAdornerTimeOut", typeof (double), 1.0);

        #endregion //IPageObjectAdorners

        #region Commands

        #region Default Adorners

        /// <summary>Removes pageObject from page when Delete button is pressed.</summary>
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

        /// <summary>Removes pageObject from page if back of pen (or middle mouse button) is pressed while passing over the pageObject.</summary>
        public Command<MouseEventArgs> ErasePageObjectCommand { get; set; }

        private void OnErasePageObjectCommandExecute(MouseEventArgs e)
        {
            if (App.MainWindowViewModel.CurrentUser.ID != PageObject.CreatorID &&
                !PageObject.IsManipulatableByNonCreator)
            {
                return;
            }

            if ((e.StylusDevice != null && e.StylusDevice.Inverted && e.LeftButton == MouseButtonState.Pressed) ||
                e.MiddleButton == MouseButtonState.Pressed)
            {
                ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
            }
        }

        /// <summary>Gets the DragStartPageObjectCommand command.</summary>
        public Command<DragStartedEventArgs> DragStartPageObjectCommand { get; set; }

        /// <summary>Method to invoke when the DragStartPageObjectCommand command is executed.</summary>
        private void OnDragStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
            if (_pageInteractionService == null ||
                _pageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Select)
            {
                e.Handled = false;
                return;
            }

            if (IsBackgroundPageObject)
            {
                return;
            }

            if (!IsAdornerVisible)
            {
                ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
            }

            PageObject.ParentPage.History.BeginBatch(new ObjectsMovedBatchHistoryAction(PageObject.ParentPage,
                                                                                        App.MainWindowViewModel.CurrentUser,
                                                                                        PageObject.ID,
                                                                                        new Point(PageObject.XPosition, PageObject.YPosition)));
        }

        /// <summary>Gets the DragPageObjectCommand command.</summary>
        public Command<DragDeltaEventArgs> DragPageObjectCommand { get; set; }

        private void OnDragPageObjectCommandExecute(DragDeltaEventArgs e)
        {
            if (_pageInteractionService == null ||
                _pageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Select)
            {
                e.Handled = false;
                return;
            }

            if (IsBackgroundPageObject)
            {
                return;
            }

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

        /// <summary>Gets the DragStopPageObjectCommand command.</summary>
        public Command<DragCompletedEventArgs> DragStopPageObjectCommand { get; set; }

        private void OnDragStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            if (_pageInteractionService == null ||
                _pageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Select)
            {
                e.Handled = false;
                return;
            }

            if (IsBackgroundPageObject)
            {
                ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
                return;
            }

            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is ObjectsMovedBatchHistoryAction)
            {
                (batch as ObjectsMovedBatchHistoryAction).AddPositionPointToBatch(new Point(PageObject.XPosition, PageObject.YPosition));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch() as ObjectsMovedBatchHistoryAction;

            var startingPoint = batchHistoryItem.TravelledPositions.FirstOrDefault();

            var deltaX = Math.Abs(startingPoint.X - XPosition);
            var deltaY = Math.Abs(startingPoint.Y - YPosition);
            var wasDraggedTolerance = 10.0; // App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Projector ? 5.0 : 1.0;
            var wasDragged = Math.Max(deltaX, deltaY) > wasDraggedTolerance;

            if (wasDragged)
            {
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
                PageObject.OnMoved(XPosition, YPosition);
            }
            else
            {
                IsAdornerVisible = true;
            }
        }

        /// <summary>Gets the ResizeStartPageObjectCommand command.</summary>
        public Command<DragStartedEventArgs> ResizeStartPageObjectCommand { get; set; }

        private void OnResizeStartPageObjectCommandExecute(DragStartedEventArgs e)
        {
            PageObject.ParentPage.History.BeginBatch(new PageObjectResizeBatchHistoryAction(PageObject.ParentPage,
                                                                                          App.MainWindowViewModel.CurrentUser,
                                                                                          PageObject.ID,
                                                                                          new Point(PageObject.Width, PageObject.Height)));
        }

        /// <summary>Gets the ResizePageObjectCommand command.</summary>
        public Command<DragDeltaEventArgs> ResizePageObjectCommand { get; set; }

        private void OnResizePageObjectCommandExecute(DragDeltaEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var parentPage = PageObject.ParentPage;
            var MIN_WIDTH = PageObject.MinimumWidth;
            var MIN_HEIGHT = PageObject.MinimumHeight;

            var newWidth = Math.Max(MIN_WIDTH, Width + e.HorizontalChange);
            newWidth = Math.Min(newWidth, parentPage.Width - XPosition);
            var newHeight = Math.Max(MIN_HEIGHT, Height + e.VerticalChange);
            newHeight = Math.Min(newHeight, parentPage.Height - YPosition);

            ChangePageObjectDimensions(PageObject, newHeight, newWidth);
            PageObject.OnResizing(initialWidth, initialHeight);
        }

        /// <summary>Gets the ResizeStopPageObjectCommand command.</summary>
        public Command<DragCompletedEventArgs> ResizeStopPageObjectCommand { get; set; }

        private void OnResizeStopPageObjectCommandExecute(DragCompletedEventArgs e)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is PageObjectResizeBatchHistoryAction)
            {
                (batch as PageObjectResizeBatchHistoryAction).AddResizePointToBatch(PageObject.ID, new Point(Width, Height));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnResized(initialWidth, initialHeight);
        }

        #endregion //Default Adorners

        #endregion //Commands

        #region Methods

        public virtual void ClearAdorners() { IsAdornerVisible = false; }

        protected virtual void PopulateContextRibbon(bool isChangedValueMeaningful)
        {
            if (isChangedValueMeaningful)
            {
                ContextRibbon.Buttons.Clear();
            }

            if (!IsAdornerVisible)
            {
                return;
            }

            ContextRibbon.Buttons = new ObservableCollection<UIElement>(_contextButtons);
        }

        #endregion //Methods

        #region Static Methods

        public static void ApplyDistinctPosition(IPageObject pageObject)
        {
            if (pageObject == null)
            {
                return;
            }

            var currentPage = pageObject.ParentPage;
            if (currentPage == null)
            {
                return;
            }

            var pageObjectsToAvoid = currentPage.PageObjects.Where(p => p.ID != pageObject.ID && p.OwnerID == App.MainWindowViewModel.CurrentUser.ID).ToList();
            while (pageObjectsToAvoid.Any())
            {
                var overlappingPageObject = pageObjectsToAvoid.FirstOrDefault(a => APageObjectBase.IsOverlapping(a, pageObject));
                if (overlappingPageObject == null)
                {
                    break;
                }

                pageObjectsToAvoid.Remove(overlappingPageObject);

                pageObject.XPosition = overlappingPageObject.XPosition + overlappingPageObject.Width + 5;
                if (pageObject.XPosition + pageObject.Width >= currentPage.Width)
                {
                    pageObject.XPosition = 0.0;
                    pageObject.YPosition = overlappingPageObject.YPosition + overlappingPageObject.Height + 5;
                }
            }

            var rnd = new Random();

            if (pageObject.YPosition + pageObject.Height >= currentPage.Height)
            {
                pageObject.YPosition = currentPage.Height - pageObject.Height - rnd.Next(30);
            }
            if (pageObject.XPosition + pageObject.Width >= currentPage.Width)
            {
                pageObject.XPosition = currentPage.Width - pageObject.Width - rnd.Next(30);
            }
        }

        public static void ApplyDistinctPosition(CLPPage page, List<IPageObject> pageObjects)
        {
            if (pageObjects == null ||
                !pageObjects.Any() ||
                page == null)
            {
                return;
            }

            var newPageObjectsAlreadyAddedToPage = new List<IPageObject>();

            foreach (var pageObject in pageObjects)
            {
                var o = pageObject;
                var pageObjectsToAvoid = page.PageObjects.Where(p => p.ID != o.ID && p.OwnerID == App.MainWindowViewModel.CurrentUser.ID).ToList();
                pageObjectsToAvoid.AddRange(newPageObjectsAlreadyAddedToPage);
                while (pageObjectsToAvoid.Any())
                {
                    var overlappingPageObject = pageObjectsToAvoid.FirstOrDefault(a => APageObjectBase.IsOverlapping(a, pageObject));
                    if (overlappingPageObject == null)
                    {
                        break;
                    }

                    pageObjectsToAvoid.Remove(overlappingPageObject);

                    pageObject.XPosition = overlappingPageObject.XPosition + overlappingPageObject.Width + 5;
                    if (pageObject.XPosition + pageObject.Width >= page.Width)
                    {
                        pageObject.XPosition = 0.0;
                        pageObject.YPosition = overlappingPageObject.YPosition + overlappingPageObject.Height + 5;
                    }
                }

                var rnd = new Random();

                if (pageObject.YPosition + pageObject.Height >= page.Height)
                {
                    pageObject.YPosition = page.Height - pageObject.Height - rnd.Next(30);
                }
                if (pageObject.XPosition + pageObject.Width >= page.Width)
                {
                    pageObject.XPosition = page.Width - pageObject.Width - rnd.Next(30);
                }

                newPageObjectsAlreadyAddedToPage.Add(pageObject);
            }
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
            if (diff > PageHistory.SAMPLE_RATE && useHistory)
            {
                var batch = pageObject.ParentPage.History.CurrentHistoryBatch;
                if (batch is ObjectsMovedBatchHistoryAction)
                {
                    ((ObjectsMovedBatchHistoryAction)batch).AddPositionPointToBatch(new Point(newX, newY));
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
            if (diff > PageHistory.SAMPLE_RATE && useHistory)
            {
                var batch = pageObject.ParentPage.History.CurrentHistoryBatch;
                if (batch is PageObjectResizeBatchHistoryAction)
                {
                    (batch as PageObjectResizeBatchHistoryAction).AddResizePointToBatch(pageObject.ID, new Point(width, height));
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