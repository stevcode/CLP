﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using System.Collections.ObjectModel;

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

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;

            CopyStampCommand = new Command(OnCopyStampCommandExecute);
            PlaceStampCommand = new Command(OnPlaceStampCommandExecute);
            DragStampCommand = new Command<DragDeltaEventArgs>(OnDragStampCommandExecute);
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
            if (HasParts())
            {
                CopyStamp(PageObject.ParentPage.PageObjects.IndexOf(PageObject));

                StrokeCollection originalStrokes = PageObject.GetStrokesOverPageObject();
                StrokeCollection clonedStrokes = new StrokeCollection();

                foreach (Stroke stroke in originalStrokes)
                {
                    Stroke newStroke = stroke.Clone();
                    Matrix transform = new Matrix();
                    transform.Translate(-XPosition, -YPosition - CLPStamp.HANDLE_HEIGHT);
                    newStroke.Transform(transform, true);
                    clonedStrokes.Add(newStroke);
                }

                StrokePathContainer.ByteStrokes = CLPPage.StrokesToBytes(clonedStrokes);
                StrokePathContainer.IsStrokePathsVisible = true;
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               (DispatcherOperationCallback)delegate(object arg)
               {
                   MessageBox.Show("What are you counting on the stamp?  Please write the number on the line below the stamp before making copies.", "What are you counting?");
                   return null;
               }, null);
            }
        }

        double originalX;
        double originalY;

        private void CopyStamp(int stampIndex)
        {
                try
                {
                    CLPStamp leftBehindStamp = PageObject.Duplicate() as CLPStamp;
                    leftBehindStamp.UniqueID = PageObject.UniqueID;

                    originalX = leftBehindStamp.XPosition;
                    originalY = leftBehindStamp.YPosition;

                    CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                    PageObject.CanAcceptPageObjects = false;
                    leftBehindStamp.PageObjectObjectParentIDs = new ObservableCollection<string>();
                    foreach (ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
                    {
                        ICLPPageObject newObject = pageObject.Duplicate();
                        pageObject.Parts = 0;
                        parentPage.PageObjects.Add(newObject);
                        pageObject.CanAdornersShow = false;
                        leftBehindStamp.PageObjectObjectParentIDs.Add(newObject.UniqueID);
                    }

                    if(stampIndex > -1)
                    {
                        parentPage.PageObjects.Insert(stampIndex, leftBehindStamp);
                    }
                    else
                    {
                        parentPage.PageObjects.Add(leftBehindStamp);
                    }
                    leftBehindStamp.Parts = PageObject.Parts;
                }
                catch(System.Exception ex)
                {
                    Classroom_Learning_Partner.Model.Logger.Instance.WriteToLog("[ERROR]: Failed to copy left behind container. " + ex.Message);
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
                StrokePathContainer.IsStamped = true;
                CLPStrokePathContainer droppedContainer = StrokePathContainer.Duplicate() as CLPStrokePathContainer;
                droppedContainer.XPosition = PageObject.XPosition;
                droppedContainer.YPosition = PageObject.YPosition + CLPStamp.HANDLE_HEIGHT;
                droppedContainer.ParentID = PageObject.UniqueID;
                //droppedContainer.IsStamped = true;
                droppedContainer.Parts = PageObject.Parts;
                droppedContainer.PageObjectObjectParentIDs = PageObject.PageObjectObjectParentIDs;
                PageObject.PageObjectObjectParentIDs = new ObservableCollection<string>();

                double deltaX = Math.Abs(PageObject.XPosition - originalX);
                double deltaY = Math.Abs(PageObject.YPosition - originalY);

                if ((deltaX > PageObject.Width + 5 || deltaY > PageObject.Height) &&
                    (StrokePathContainer.InternalPageObject != null || PageObject.GetStrokesOverPageObject().Count > 0 ||
                     PageObject.PageObjectObjectParentIDs.Count > 0))
                {
                    CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);
                    Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(parentPage, droppedContainer);
                    Console.WriteLine("containers objects: " + droppedContainer.PageObjectObjectParentIDs.Count);
                }
                // Stamp not placed
                else {
                    foreach (ICLPPageObject po in PageObject.GetPageObjectsOverPageObject())
                    {
                        Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(po);
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
            timer.Stop();

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
            if (x > PageObject.ParentPage.PageWidth - PageObject.Width)
            {
                x = PageObject.ParentPage.PageWidth - PageObject.Width;
            }
            if(y > PageObject.ParentPage.PageHeight - PageObject.Height)
            {
                y = PageObject.ParentPage.PageHeight - PageObject.Height;
            }

            double xDelta = x - PageObject.XPosition;
            double yDelta = y - PageObject.YPosition;

            foreach (ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject()) {
                Point pageObjectPt = new Point((xDelta + pageObject.XPosition), (yDelta + pageObject.YPosition));
                Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pageObjectPt);
            }

            Point pt = new Point(x, y);
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(PageObject, pt);
        }
        
        #endregion //Commands

        #region Methods

        private DispatcherTimer timer = null;

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            IsAdornerVisible = true;
        }

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if (IsBackground)
            {
                if (App.MainWindowViewModel.IsAuthoring) //Adorners pop-up immediately while in Authoring Mode
                {
                    IsAdornerVisible = true;
                }
            }
            else //If not a background object, adorners pop up after a delay
            {
                if (isMouseDown)
                {
                    timer.Stop();
                    IsAdornerVisible = false;
                }
                else
                {
                    timer.Start();
                }
            }

            return false;
        }

        private bool HasParts()
        {
            (PageObject as CLPStamp).UpdatePartsFromHandwritingRegion();
            return PageObject.Parts > 0;
        }

        #endregion //Methods
    }
}
