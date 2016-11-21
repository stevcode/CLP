using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
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

            InitializeButtons();
        }

        public override void ClearAdorners()
        {
            IsAdornerVisible = false;
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        private void InitializeButtons()
        {
            _contextButtons.Clear();

            _contextButtons.Add(new RibbonButton("Delete Lassoed Objects", "pack://application:,,,/Resources/Images/Delete.png", RemovePageObjectsCommand, null, true));
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

            if (region.LassoedStrokes.Any())
            {
                ACLPPageBaseViewModel.RemoveStrokes(region.ParentPage, region.LassoedStrokes);
            }

            if (region.LassoedPageObjects.Any())
            {
                var pageObjectsToRemove = region.LassoedPageObjects.ToList();

                ACLPPageBaseViewModel.RemovePageObjectsFromPage(region.ParentPage, pageObjectsToRemove);
            }

            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        /// <summary>Brings up a menu to make multiple copies of page objects in the region</summary>
        public Command DuplicateCommand { get; private set; }

        private void OnDuplicateCommandExecute()
        {
            var lassoRegion = PageObject as LassoRegion;
            if (lassoRegion == null)
            {
                return;
            }

            var keyPad = new KeypadWindowView("How many copies?", 21)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);

            var initialXPosition = XPosition + Width + 10.0;
            var initialYPosition = YPosition;

            var pageObjectCopiesToAdd = new List<IPageObject>();
            for (var i = 0; i < numberOfCopies; i++)
            {
                foreach (var pageObject in lassoRegion.LassoedPageObjects.Where(x => !APageObjectBase.IsPageObjectAnAcceptedPageObject(x)))
                {
                    var newPageObject = pageObject.Duplicate();
                    newPageObject.XPosition = initialXPosition;
                    newPageObject.YPosition = initialYPosition;
                    if (initialXPosition + 2 * newPageObject.Width + 5 < PageObject.ParentPage.Width)
                    {
                        initialXPosition += newPageObject.Width + 5;
                    }
                    else if (initialYPosition + 2 * newPageObject.Height + 5 < PageObject.ParentPage.Height)
                    {
                        initialXPosition = 25;
                        initialYPosition += newPageObject.Height + 5;
                    }
                    pageObjectCopiesToAdd.Add(newPageObject);

                    var acceptor = pageObject as IPageObjectAccepter;
                    var newAcceptor = newPageObject as IPageObjectAccepter;
                    if (acceptor != null &&
                        newAcceptor != null)
                    {
                        newAcceptor.AcceptedPageObjectIDs.Clear();
                        newAcceptor.AcceptedPageObjects.Clear();
                        foreach (var innerPageObject in acceptor.AcceptedPageObjects)
                        {
                            var newInnerPageObject = innerPageObject.Duplicate() as IPageObjectAccepter;
                            if (newInnerPageObject == null)
                            {
                                continue;
                            }
                            newInnerPageObject.XPosition = newPageObject.XPosition + (innerPageObject.XPosition - pageObject.XPosition);
                            newInnerPageObject.YPosition = newPageObject.YPosition + (innerPageObject.YPosition - pageObject.YPosition);
                            newAcceptor.AcceptedPageObjectIDs.Add(newInnerPageObject.ID);
                            newAcceptor.AcceptedPageObjects.Add(newInnerPageObject);
                            pageObjectCopiesToAdd.Add(newInnerPageObject);
                        }
                    }
                }
            }

            ACLPPageBaseViewModel.AddPageObjectsToPage(lassoRegion.ParentPage, pageObjectCopiesToAdd);
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

            var pageObjectIDs = region.LassoedPageObjects.ToDictionary(p => p.ID, p => new Point(p.XPosition - XPosition, p.YPosition - YPosition));
            var strokeIDs = region.LassoedStrokes.ToDictionary(s => s.GetStrokeID(), s => new Point(s.GetBounds().X - XPosition, s.GetBounds().Y - YPosition));

            PageObject.ParentPage.History.BeginBatch(new ObjectsMovedBatchHistoryAction(PageObject.ParentPage,
                                                                                      App.MainWindowViewModel.CurrentUser,
                                                                                      pageObjectIDs,
                                                                                      strokeIDs,
                                                                                      new Point(PageObject.XPosition, PageObject.YPosition)));
        }

        /// <summary>Gets the DragPageObjectCommand command.</summary>
        public Command<DragDeltaEventArgs> DragLassoCommand { get; set; }

        private void OnDragLassoCommandExecute(DragDeltaEventArgs e)
        {
            var oldXPos = XPosition;
            var oldYPos = YPosition;
            var parentPage = PageObject.ParentPage;

            //BUG: http://stackoverflow.com/questions/26298398/wpf-thumb-drag-behaviour-wrong#comment49592425_26298398
            var newX = Math.Max(0, XPosition + e.HorizontalChange);
            newX = Math.Min(newX, parentPage.Width - Width);
            
            var newY = Math.Max(0, YPosition + e.VerticalChange);
            newY = Math.Min(newY, parentPage.Height - Height);

            var xDelta = newX - oldXPos;
            var yDelta = newY - oldYPos;

            var xDiff = Math.Abs(xDelta);
            var yDiff = Math.Abs(yDelta);
            var diff = xDiff + yDiff;
            if (diff > PageHistory.SAMPLE_RATE)
            {
                var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
                if (batch is ObjectsMovedBatchHistoryAction)
                {
                    ((ObjectsMovedBatchHistoryAction)batch).AddPositionPointToBatch(new Point(newX, newY));
                }
                else
                {
                    Logger.Instance.WriteToLog("Error: Current Batch not ChangePositionBatch.");
                    var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
                    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
                }
            }

            XPosition = newX;
            YPosition = newY;
            PageObject.OnMoving(oldXPos, oldYPos);
        }

        /// <summary>Gets the DragStopPageObjectCommand command.</summary>
        public Command<DragCompletedEventArgs> DragStopLassoCommand { get; set; }

        private void OnDragStopLassoCommandExecute(DragCompletedEventArgs e)
        {
            var initialX = XPosition;
            var initialY = YPosition;

            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is ObjectsMovedBatchHistoryAction)
            {
                ((ObjectsMovedBatchHistoryAction)batch).AddPositionPointToBatch(new Point(PageObject.XPosition, PageObject.YPosition));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved(initialX, initialY);
        }

        #endregion //Commands
    }
}