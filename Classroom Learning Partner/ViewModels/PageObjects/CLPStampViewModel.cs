using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPStampViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampViewModel"/> class.
        /// </summary>
        public CLPStampViewModel(CLPStamp stamp)
            : base()
        {
            PageObject = stamp;
            StrokePathContainer.IsStrokePathsVisible = false;

            CopyStampCommand = new Command(OnCopyStampCommandExecute);
            PlaceStampCommand = new Command(OnPlaceStampCommandExecute);
            DragStampCommand = new Command<DragDeltaEventArgs>(OnDragStampCommandExecute);

            BottomBarClickCommand = new Command(OnBottomBarClickCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StampVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public CLPStrokePathContainer StrokePathContainer
        {
            get { return GetValue<CLPStrokePathContainer>(StrokePathContainerProperty); }
            set { SetValue(StrokePathContainerProperty, value); }
        }

        public static readonly PropertyData StrokePathContainerProperty = RegisterProperty("StrokePathContainer", typeof(CLPStrokePathContainer));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public CLPHandwritingRegion HandwritingRegionParts
        {
            get { return GetValue<CLPHandwritingRegion>(HandwritingRegionPartsProperty); }
            set { SetValue(HandwritingRegionPartsProperty, value); }
        }

        /// <summary>
        /// Register the HandwritingRegionParts property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HandwritingRegionPartsProperty = RegisterProperty("HandwritingRegionParts", typeof(CLPHandwritingRegion));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public SolidColorBrush StampHandleColor
        {
            get { return GetValue<SolidColorBrush>(StampHandleColorProperty); }
            set { SetValue(StampHandleColorProperty, value); }
        }

        /// <summary>
        /// Register the StampHandleColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StampHandleColorProperty = RegisterProperty("StampHandleColor", typeof(SolidColorBrush), new SolidColorBrush(Colors.Black));

        #region Commands

        /// <summary>
        /// Places copy of stamp below and displays StrokePathViews for dragging stamp.
        /// </summary>
        public Command CopyStampCommand { get; private set; }

        private void OnCopyStampCommandExecute()
        {
            StampHandleColor = new SolidColorBrush(Colors.Green);

            CopyStamp(PageObject.ParentPage.PageObjects.IndexOf(PageObject));

            StrokeCollection originalStrokes = PageObject.GetStrokesOverPageObject();
            StrokeCollection clonedStrokes = new StrokeCollection();

            foreach(Stroke stroke in originalStrokes)
            {
                Stroke newStroke = stroke.Clone();
                Matrix transform = new Matrix();
                transform.Translate(-XPosition, -YPosition - CLPStamp.HANDLE_HEIGHT);
                newStroke.Transform(transform, true);
                clonedStrokes.Add(newStroke);
            }

            StrokePathContainer.ByteStrokes = CLPPage.StrokesToBytes(clonedStrokes);
            StrokePathContainer.IsStrokePathsVisible = true;
        }

        private bool dragStarted = false;
        private bool copyMade = false;

        double originalX;
        double originalY;

        private void CopyStamp(int stampIndex)
        {
            if (HasParts())
            {
                try
                {
                    CLPStamp leftBehindStamp = PageObject.Duplicate() as CLPStamp;
                    leftBehindStamp.UniqueID = PageObject.UniqueID;

                    originalX = leftBehindStamp.XPosition;
                    originalY = leftBehindStamp.YPosition;

                    CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                    if(stampIndex > -1)
                    {
                        parentPage.PageObjects.Insert(stampIndex, leftBehindStamp);
                    }
                    else
                    {
                        parentPage.PageObjects.Add(leftBehindStamp);
                    }
                }
                catch(System.Exception ex)
                {
                    Classroom_Learning_Partner.Model.Logger.Instance.WriteToLog("[ERROR]: Failed to copy left behind container. " + ex.Message);
                }
            }
            else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               (DispatcherOperationCallback)delegate(object arg)
               {
                   MessageBox.Show("What are you counting on the stamp?  Please write the number on the line below the stamp before making copies.", "What are you counting?");
                   return null;
               }, null);
            }
        }

        /// <summary>
        /// Copies StrokePathContainer to page on Stamp Placed (DragCompleted Event)
        /// </summary>
        public Command PlaceStampCommand { get; private set; }

        private void OnPlaceStampCommandExecute()
        {
            if (HasParts())
            {
                CLPStrokePathContainer droppedContainer = StrokePathContainer.Duplicate() as CLPStrokePathContainer;
                droppedContainer.XPosition = PageObject.XPosition;
                droppedContainer.YPosition = PageObject.YPosition + CLPStamp.HANDLE_HEIGHT;
                droppedContainer.ParentID = PageObject.UniqueID;
                droppedContainer.IsStamped = true;
                droppedContainer.Parts = PageObject.Parts;

                double deltaX = Math.Abs(PageObject.XPosition - originalX);
                double deltaY = Math.Abs(PageObject.YPosition - originalY);

                if (deltaX > PageObject.Width + 5 || deltaY > PageObject.Height)
                {
                if(StrokePathContainer.InternalPageObject != null || PageObject.GetStrokesOverPageObject().Count > 0)
                    {
                        CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                        Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(parentPage, droppedContainer);
                    }
                }

                Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
            }
        }

        /// <summary>
        /// Stamp Dragged By Adorner
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
            IsAdornerVisible = false;

            double x = PageObject.XPosition + e.HorizontalChange;
            double y = PageObject.YPosition + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < - CLPStamp.HANDLE_HEIGHT)
            {
                y = -CLPStamp.HANDLE_HEIGHT;
            }
            if (x > 1056 - PageObject.Width)
            {
                x = 1056 - PageObject.Width;
            }
            if (y > 816 - PageObject.Height)
            {
                y = 816 - PageObject.Height;
            }

            Point pt = new Point(x, y);
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(PageObject, pt);
        }
        
        /// <summary>
        /// Shows/Hides Adorners
        /// </summary>
        public Command BottomBarClickCommand { get; private set; }

        private void OnBottomBarClickCommandExecute()
        {
            IsAdornerVisible = !IsAdornerVisible;
        }

        #endregion //Commands

        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(hitBoxName == "BottomBarHitBox")
            {
                if(IsBackground && !App.MainWindowViewModel.IsAuthoring)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool HasParts()
        {
            (PageObject as CLPStamp).UpdatePartsFromHandwritingRegion();
            return PageObject.Parts > 0;
        }

        #endregion //Methods
    }
}
