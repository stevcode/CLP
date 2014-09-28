using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class LassoRegionViewModel : APageObjectBaseViewModel
    {
        public LassoRegionViewModel(LassoRegion lassoRegion)
        {
            PageObject = lassoRegion;

            RemovePageObjectsCommand = new Command(OnRemovePageObjectsCommandExecute);
            DuplicateCommand = new Command(OnDuplicateCommandExecute);
            UnselectRegionCommand = new Command(OnUnselectRegionCommandExecute);

            DragStartLassoCommand = new Command<DragStartedEventArgs>(OnDragStartLassoCommandExecute);
            DragLassoCommand = new Command<DragDeltaEventArgs>(OnDragLassoCommandExecute);
            DragStopLassoCommand = new Command<DragCompletedEventArgs>(OnDragStopLassoCommandExecute);
        }

        public override void ClearAdorners()
        {
            IsAdornerVisible = false;
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        #region Commands

        /// <summary>Removes pageObjects from page when Delete button is pressed.</summary>
        public Command RemovePageObjectsCommand { get; set; }

        private void OnRemovePageObjectsCommandExecute()
        {
            var region = PageObject as LassoRegion;
            if (region == null)
            {
                return;
            }

            var pageObjectsToRemove = region.LassoedPageObjects.ToList();

            ACLPPageBaseViewModel.RemovePageObjectsFromPage(region.ParentPage, pageObjectsToRemove);
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        /// <summary>Brings up a menu to make multiple copies of page objects in the region</summary>
        public Command DuplicateCommand { get; private set; }

        private void OnDuplicateCommandExecute()
        {
            //var keyPad = new KeypadWindowView("How many copies?", 21)
            //             {
            //                 Owner = Application.Current.MainWindow,
            //                 WindowStartupLocation = WindowStartupLocation.Manual,
            //                 Top = 100,
            //                 Left = 100
            //             };
            //keyPad.ShowDialog();
            //if(keyPad.DialogResult != true ||
            //   keyPad.NumbersEntered.Text.Length <= 0) { return; }
            //var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);

            //double xPosition = 50.0;
            //double yPosition = YPosition;
            //const double GAP = 35.0;
            //if(XPosition + Width * (numberOfCopies + 1) + GAP * numberOfCopies <= PageObject.ParentPage.Width) { xPosition = XPosition + Width + GAP; }
            //else if(YPosition + 2 * Height < PageObject.ParentPage.Height) { yPosition = YPosition + Height; }
            //foreach(var lassoedPageObject in PageObject.GetPageObjectsOverPageObject())
            //{
            //    for(int i = 0; i < numberOfCopies; i++)
            //    {
            //        var duplicatePageObject = lassoedPageObject.Duplicate();
            //        double xOffset = lassoedPageObject.XPosition - XPosition;
            //        double yOffset = lassoedPageObject.YPosition - YPosition;

            //        if(xPosition + Width * (i + 1) + GAP * i <= PageObject.ParentPage.Width)
            //        {
            //            duplicatePageObject.XPosition = xPosition + xOffset + i * (Width + GAP);
            //            duplicatePageObject.YPosition = yPosition + yOffset;
            //        }
            //        else
            //        { ACLPPageObjectBase.ApplyDistinctPosition(duplicatePageObject); }
            //        ACLPPageBaseViewModel.AddPageObjectToPage(PageObject.ParentPage, duplicatePageObject, true);
            //        //TODO: Steve - add MassPageObjectAdd history item and MassPageObjectRemove history item.
            //    }
            //}
        }

        /// <summary>Unselects the region</summary>
        public Command UnselectRegionCommand { get; private set; }

        private void OnUnselectRegionCommandExecute() { ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false); }

        /// <summary>Gets the DragStartPageObjectCommand command.</summary>
        public Command<DragStartedEventArgs> DragStartLassoCommand { get; set; }

        /// <summary>Method to invoke when the DragStartPageObjectCommand command is executed.</summary>
        private void OnDragStartLassoCommandExecute(DragStartedEventArgs e)
        {
            var region = PageObject as LassoRegion;
            if (region == null)
            {
                return;
            }

            PageObject.ParentPage.History.BeginBatch(new PageObjectsMoveBatchHistoryItem(PageObject.ParentPage,
                                                                                         App.MainWindowViewModel.CurrentUser,
                                                                                         region.LassoedPageObjects.Select(x => x.ID).ToList(),
                                                                                         new Point(PageObject.XPosition, PageObject.YPosition)));
        }

        /// <summary>Gets the DragPageObjectCommand command.</summary>
        public Command<DragDeltaEventArgs> DragLassoCommand { get; set; }

        private void OnDragLassoCommandExecute(DragDeltaEventArgs e)
        {
            var oldXPos = PageObject.XPosition;
            var oldYPos = PageObject.YPosition;
            var parentPage = PageObject.ParentPage;

            var newX = Math.Max(0, PageObject.XPosition + e.HorizontalChange);
            newX = Math.Min(newX, parentPage.Width - Width);
            var newY = Math.Max(0, PageObject.YPosition + e.VerticalChange);
            newY = Math.Min(newY, parentPage.Height - Height);

            var xDelta = newX - oldXPos;
            var yDelta = newY - oldYPos;

            var xDiff = Math.Abs(xDelta);
            var yDiff = Math.Abs(yDelta);
            var diff = xDiff + yDiff;
            if (diff > PageHistory.SAMPLE_RATE)
            {
                var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
                if (batch is PageObjectsMoveBatchHistoryItem)
                {
                    (batch as PageObjectsMoveBatchHistoryItem).AddPositionPointToBatch(new Point(newX, newY));
                }
                else
                {
                    Logger.Instance.WriteToLog("Error: Current Batch not ChangePositionBatch.");
                    var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
                    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
                }
            }

            PageObject.XPosition = newX;
            PageObject.YPosition = newY;
            PageObject.OnMoving(oldXPos, oldYPos);
        }

        /// <summary>Gets the DragStopPageObjectCommand command.</summary>
        public Command<DragCompletedEventArgs> DragStopLassoCommand { get; set; }

        private void OnDragStopLassoCommandExecute(DragCompletedEventArgs e)
        {
            var initialX = XPosition;
            var initialY = YPosition;

            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is PageObjectsMoveBatchHistoryItem)
            {
                (batch as PageObjectsMoveBatchHistoryItem).AddPositionPointToBatch(new Point(PageObject.XPosition, PageObject.YPosition));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved(initialX, initialY);
        }

        #endregion //Commands
    }
}