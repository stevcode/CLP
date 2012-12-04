﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        /// Gets the CopyStampCommand command.
        /// </summary>
        public Command CopyStampCommand { get; private set; }


        private bool dragStarted = false;
        private bool copyMade = false;

        /// <summary>
        /// Method to invoke when the CopyStampCommand command is executed.
        /// </summary>
        private void OnCopyStampCommandExecute()
        {
            StampHandleColor = new SolidColorBrush(Colors.Green);
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
            //    (DispatcherOperationCallback)delegate(object arg)
            //    {


            CopyStamp();

            //        return null;
            //    }, null);

            StrokePathContainer.PageObjectStrokeParentIDs = PageObject.PageObjectStrokeParentIDs;
            StrokePathContainer.IsStrokePathsVisible = true;
        }

        double originalX;
        double originalY;

        private void CopyStamp()
        {
            try
            {
                CLPStamp leftBehindStamp = PageObject.Duplicate() as CLPStamp;
                leftBehindStamp.UniqueID = PageObject.UniqueID;

                originalX = leftBehindStamp.XPosition;
                originalY = leftBehindStamp.YPosition;

                //int originalIndex = PageObject.ParentPage.PageObjects.  .IndexOf(PageObject);

                CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                parentPage.PageObjects.Add(leftBehindStamp);

                    //if (!page.PageHistory.IgnoreHistory)
                    //{
                    //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.AddPageObject, leftBehindStamp.UniqueID, null, null);
                    //    page.PageHistory.HistoryItems.Add(item);
                    //}
                
            }
            catch (System.Exception ex)
            {
                Classroom_Learning_Partner.Model.Logger.Instance.WriteToLog("[ERROR]: Failed to copy left behind stamp. " + ex.Message);
            }
        }

                /// <summary>
        /// Gets the PlaceStampCommand command.
        /// </summary>
        public Command PlaceStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the PlaceStampCommand command is executed.
        /// </summary>
        private void OnPlaceStampCommandExecute()
        {
            StampHandleColor = new SolidColorBrush(Colors.Black);
            CLPStrokePathContainer droppedContainer = StrokePathContainer.Duplicate() as CLPStrokePathContainer;
            droppedContainer.XPosition = PageObject.XPosition;
            droppedContainer.YPosition = PageObject.YPosition + CLPStamp.HANDLE_HEIGHT;
            droppedContainer.ParentID = PageObject.UniqueID;
            droppedContainer.IsStamped = true;
            
            double deltaX = Math.Abs(PageObject.XPosition - originalX);
            double deltaY = Math.Abs(PageObject.YPosition - originalY);

            if (deltaX > PageObject.Width + 5 || deltaY > PageObject.Height)
            {
                if (StrokePathContainer.InternalPageObject != null || PageObjectStrokes.Count > 0)
                {
                    CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

                    Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(parentPage, droppedContainer);
                }
            }

            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Gets the DragStampCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the DragStampCommand command is executed.
        /// </summary>
        private void OnDragStampCommandExecute(DragDeltaEventArgs e)
        {
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

        #endregion //Methods
    }
}
