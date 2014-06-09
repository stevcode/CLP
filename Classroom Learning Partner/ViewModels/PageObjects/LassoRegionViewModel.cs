using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
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

            DragLassoCommand = new Command<DragDeltaEventArgs>(OnDragLassoCommandExecute);
            DragStartLassoCommand = new Command<DragStartedEventArgs>(OnDragStartLassoCommandExecute);
            DragStopLassoCommand = new Command<DragCompletedEventArgs>(OnDragStopLassoCommandExecute);
        }

        /// <summary>
        /// List of all the IDs of the <see cref="IPageObject" />s inside the <see cref="LassoRegion" />.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public List<string> ContainedPageObjectIDs
        {
            get { return GetValue<List<string>>(ContainedPageObjectIDsProperty); }
            set { SetValue(ContainedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData ContainedPageObjectIDsProperty = RegisterProperty("ContainedPageObjectIDs", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// List of all the IDs of the <see cref="StrokeDTO" />s inside the <see cref="LassoRegion" />.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public List<string> ContainedInkStrokeIDs
        {
            get { return GetValue<List<string>>(ContainedInkStrokeIDsProperty); }
            set { SetValue(ContainedInkStrokeIDsProperty, value); }
        }

        public static readonly PropertyData ContainedInkStrokeIDsProperty = RegisterProperty("ContainedInkStrokeIDs", typeof(List<string>), () => new List<string>());

        public override void ClearAdorners()
        {
            IsAdornerVisible = false;
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        #region Commands

        /// <summary>
        /// Removes pageObjects from page when Delete button is pressed.
        /// </summary>
        public Command RemovePageObjectsCommand { get; set; }

        private void OnRemovePageObjectsCommandExecute()
        {
            var region = PageObject as LassoRegion;
            var pageObjectsToRemove = region.ContainedPageObjectIDs.Select(id => region.ParentPage.GetPageObjectByID(id)).Where(pageObject => pageObject != null).ToList();

            ACLPPageBaseViewModel.RemovePageObjectsFromPage(region.ParentPage, pageObjectsToRemove);
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        /// <summary>
        /// Brings up a menu to make multiple copies of page objects in the region
        /// </summary>
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

        /// <summary>
        /// Unselects the region
        /// </summary>
        public Command UnselectRegionCommand { get; private set; }

        private void OnUnselectRegionCommandExecute()
        {
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        /// <summary>
        /// Gets the DragPageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragLassoCommand { get; set; }

        private void OnDragLassoCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;

            var newX = Math.Max(0, PageObject.XPosition + e.HorizontalChange);
            newX = Math.Min(newX, parentPage.Width - PageObject.Width);
            var newY = Math.Max(0, PageObject.YPosition + e.VerticalChange);
            newY = Math.Min(newY, parentPage.Height - PageObject.Height);

            var oldXPos = PageObject.XPosition;
            var oldYPos = PageObject.YPosition;
            var xDelta = newX - oldXPos;
            var yDelta = newY - oldYPos;

            var xDiff = Math.Abs(xDelta);
            var yDiff = Math.Abs(yDelta);
            var diff = xDiff + yDiff;
            //if(diff > PageHistory.SAMPLE_RATE)
            //{
            //    var batch = pageObject.ParentPage.History.CurrentHistoryBatch;
            //    if(batch is PageObjectMoveBatchHistoryItem)
            //    {
            //        (batch as PageObjectMoveBatchHistoryItem).AddPositionPointToBatch(pageObject.ID, new Point(newX, newY));
            //    }
            //    else
            //    {
            //        Logger.Instance.WriteToLog("Error: Current Batch not ChangePositionBatch.");
            //        var batchHistoryItem = pageObject.ParentPage.History.EndBatch();
            //        ACLPPageBaseViewModel.AddHistoryItemToPage(pageObject.ParentPage, batchHistoryItem, true);
            //    }
            //}

            PageObject.XPosition = newX;
            PageObject.YPosition = newY;

            //foreach(var pageObject in ContainedPageObjectIDs.Select(parentPage.GetPageObjectByID))
            //{
            //    ChangePageObjectPosition(PageObject, newX, newY, false);
            //}
        }

        /// <summary>
        /// Gets the DragStartPageObjectCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> DragStartLassoCommand { get; set; }

        /// <summary>
        /// Method to invoke when the DragStartPageObjectCommand command is executed.
        /// </summary>
        private void OnDragStartLassoCommandExecute(DragStartedEventArgs e)
        {
            PageObject.ParentPage.History.BeginBatch(new PageObjectsMoveBatchHistoryItem(PageObject.ParentPage,
                                                                                         App.MainWindowViewModel.CurrentUser,
                                                                                         ContainedPageObjectIDs,
                                                                                         new Point(PageObject.XPosition, PageObject.YPosition)));
        }

        /// <summary>
        /// Gets the DragStopPageObjectCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> DragStopLassoCommand { get; set; }

        private void OnDragStopLassoCommandExecute(DragCompletedEventArgs e)
        {
            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if(batch is PageObjectsMoveBatchHistoryItem)
            {
                (batch as PageObjectsMoveBatchHistoryItem).AddPositionPointToBatch(new Point(PageObject.XPosition,
                                                                                           PageObject.YPosition));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved();
        }

        #endregion //Commands
    }
}