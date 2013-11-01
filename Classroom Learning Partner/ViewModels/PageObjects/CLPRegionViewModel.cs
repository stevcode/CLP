using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPRegionViewModel : ACLPPageObjectBaseViewModel
    {

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPRegionViewModel"/> class.
        /// </summary>
        public CLPRegionViewModel(CLPRegion region)
        {
            PageObject = region;

            RemovePageObjectsCommand = new Command(OnRemovePageObjectsCommandExecute);
            DragPageObjectsCommand = new Command<DragDeltaEventArgs>(OnDragPageObjectsCommandExecute);
            DragStartPageObjectsCommand = new Command<DragStartedEventArgs>(OnDragStartPageObjectsCommandExecute);
            DragStopPageObjectsCommand = new Command<DragCompletedEventArgs>(OnDragStopPageObjectsCommandExecute);
            DuplicateCommand = new Command(OnDuplicateCommandExecute);
            UnselectRegionCommand = new Command(OnUnselectRegionCommandExecute);
        }

        #endregion //Constructor

        #region Commands

        /// <summary>
        /// Removes pageObjects from page when Delete button is pressed.
        /// </summary>
        public Command RemovePageObjectsCommand
        {
            get;
            set;
        }

        private void OnRemovePageObjectsCommandExecute()
        {
            foreach(ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
            {
                ACLPPageBaseViewModel.RemovePageObjectFromPage(pageObject);
            }
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Gets the DragPageObjectsCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> DragPageObjectsCommand
        {
            get;
            set;
        }

        private void OnDragPageObjectsCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;

            foreach(ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
            {
                var newX = PageObject.XPosition + e.HorizontalChange;
                var newY = PageObject.YPosition + e.VerticalChange;
                if(newX < 0)
                {
                    newX = 0;
                }
                if(newY < 0)
                {
                    newY = 0;
                }
                if(newX > parentPage.PageWidth - PageObject.Width)
                {
                    newX = parentPage.PageWidth - PageObject.Width;
                }
                if(newY > parentPage.PageHeight - PageObject.Height)
                {
                    newY = parentPage.PageHeight - PageObject.Height;
                }

                ChangePageObjectPosition(PageObject, newX, newY);
            }
        }

        /// <summary>
        /// Gets the DragStartPageObjectsCommand command.
        /// </summary>
        public Command<DragStartedEventArgs> DragStartPageObjectsCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Method to invoke when the DragStartPageObjectsCommand command is executed.
        /// </summary>
        private void OnDragStartPageObjectsCommandExecute(DragStartedEventArgs e)
        {
            foreach(ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
            {
                PageObject.ParentPage.PageHistory.BeginBatch(new CLPHistoryPageObjectMoveBatch(PageObject.ParentPage,
                                                                                           PageObject.UniqueID,
                                                                                           new Point(PageObject.XPosition,
                                                                                                     PageObject.YPosition)));
            }
        }

        /// <summary>
        /// Gets the DragStopPageObjectsCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> DragStopPageObjectsCommand
        {
            get;
            set;
        }

        private void OnDragStopPageObjectsCommandExecute(DragCompletedEventArgs e)
        {
            foreach(ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
            {
                var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
                if(batch is CLPHistoryPageObjectMoveBatch)
                {
                    (batch as CLPHistoryPageObjectMoveBatch).AddPositionPointToBatch(PageObject.UniqueID,
                                                                                     new Point(PageObject.XPosition,
                                                                                               PageObject.YPosition));
                }
                var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
                PageObject.OnMoved();
            }

            //Remove region after operation
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Brings up a menu to make multiple copies of page objects in the region
        /// </summary>
        public Command DuplicateCommand
        {
            get;
            private set;
        }

        private void OnDuplicateCommandExecute()
        {
            var keyPad = new KeypadWindowView("How many copies?", 21)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Top = 100,
                Left = 100
            };
            keyPad.ShowDialog();
            if(keyPad.DialogResult != true ||
               keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);

            bool isHorizontallyAligned = (Width / PageObject.ParentPage.PageWidth < Height / PageObject.ParentPage.PageHeight);
            foreach(var lassoedPageObject in PageObject.GetPageObjectsOverPageObject())
            {
                for(int i = 0; i < numberOfCopies; i++)
                {
                    var duplicatePageObject = lassoedPageObject.Duplicate();
                    if(isHorizontallyAligned)
                    {
                        duplicatePageObject.XPosition += duplicatePageObject.Width + 10;
                    }
                    else
                    {
                        duplicatePageObject.YPosition += duplicatePageObject.Height + 10;
                    }
                    ACLPPageObjectBase.ApplyDistinctPosition(duplicatePageObject);
                    ACLPPageBaseViewModel.AddPageObjectToPage(PageObject.ParentPage, duplicatePageObject, false);
                    //TODO: Steve - add MassPageObjectAdd history item and MassPageObjectRemove history item.
                }
            }
        }

        /// <summary>
        /// Unselects the region
        /// </summary>
        public Command UnselectRegionCommand
        {
            get;
            private set;
        }

        private void OnUnselectRegionCommandExecute()
        {
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        }

        #endregion //Commands

    }
}