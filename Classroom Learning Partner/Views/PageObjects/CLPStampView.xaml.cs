using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model;
using System.Threading;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class CLPStampView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampView"/> class.
        /// </summary>
        public CLPStampView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStampViewModel);
        }

        private void StampHandleHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Green);
        }

        private void StampHandleHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if(ViewModel != null)
            {
                (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Black);
                (ViewModel as CLPStampViewModel).timer.Stop();
            }
        }

        private void AdornerClose_Click(object sender, RoutedEventArgs e)
        {
            //foreach(CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(PageObject.ParentPage))
            //{
            //    pageVM.IsInkCanvasHitTestVisible = true;
            //}
            CLPServiceAgent.Instance.RemovePageObjectFromPage((ViewModel as CLPStampViewModel).PageObject);
        }

        private void StampHandleHitBox_DragDelta(object sender, DragDeltaEventArgs e)
        {
            (ViewModel as CLPStampViewModel).IsAdornerVisible = false;
            (ViewModel as CLPStampViewModel).timer.Stop();
            CLPStamp PageObject = (ViewModel as CLPStampViewModel).PageObject as CLPStamp;

            double x = PageObject.XPosition + e.HorizontalChange;
            double y = PageObject.YPosition + e.VerticalChange;
            if(x < 0)
            {
                x = 0;
            }
            if(y < -CLPStamp.HANDLE_HEIGHT)
            {
                y = -CLPStamp.HANDLE_HEIGHT;
            }
            if(x > 1056 - PageObject.Width)
            {
                x = 1056 - PageObject.Width;
            }
            if(y > 816 - PageObject.Height)
            {
                y = 816 - PageObject.Height;
            }

            Point pt = new Point(x, y);
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(PageObject, pt);
        }

        private void StampHandleHitBox_DragStarted(object sender, DragStartedEventArgs e)
        {
            (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Green);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    CopyStamp();

                    return null;
                }, null);

            (ViewModel as CLPStampViewModel).StrokePathContainer.PageObjectStrokeParentIDs = (ViewModel as CLPStampViewModel).PageObject.PageObjectStrokeParentIDs;
            (ViewModel as CLPStampViewModel).StrokePathContainer.IsStrokePathsVisible = true;
        }

        private void StampHandleHitBox_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            (ViewModel as CLPStampViewModel).StampHandleColor = new SolidColorBrush(Colors.Black);
            (ViewModel as CLPStampViewModel).StrokePathContainer.IsStrokePathsVisible = false;
            
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                (DispatcherOperationCallback)delegate(object arg)
                {

                    CLPStrokePathContainer droppedContainer = (ViewModel as CLPStampViewModel).StrokePathContainer.Duplicate() as CLPStrokePathContainer;
                    droppedContainer.XPosition = (ViewModel as CLPStampViewModel).PageObject.XPosition;
                    droppedContainer.YPosition = (ViewModel as CLPStampViewModel).PageObject.YPosition + CLPStamp.HANDLE_HEIGHT;
                    droppedContainer.ParentID = (ViewModel as CLPStampViewModel).PageObject.UniqueID;
                    droppedContainer.IsStamped = true;

                    double deltaX = Math.Abs((ViewModel as CLPStampViewModel).PageObject.XPosition - originalX);
                    double deltaY = Math.Abs((ViewModel as CLPStampViewModel).PageObject.YPosition - originalY);

                    if(deltaX > (ViewModel as CLPStampViewModel).PageObject.Width + 5 || deltaY > (ViewModel as CLPStampViewModel).PageObject.Height)
                    {
                        if((ViewModel as CLPStampViewModel).StrokePathContainer.InternalPageObject != null || (ViewModel as CLPStampViewModel).PageObjectStrokes.Count > 0)
                        {
                            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID((ViewModel as CLPStampViewModel).PageObject.ParentPageID);
                            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(parentPage, droppedContainer);
                        }
                    }

                    Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage((ViewModel as CLPStampViewModel).PageObject);

                    return null;
                }, null);
        }

        double originalX;
        double originalY;
        private void CopyStamp()
        {
            try
            {
                CLPStamp PageObject = (ViewModel as CLPStampViewModel).PageObject as CLPStamp;
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
            catch(System.Exception ex)
            {
                Classroom_Learning_Partner.Model.Logger.Instance.WriteToLog("[ERROR]: Failed to copy left behind container. " + ex.Message);
            }
        }
    }
}
