using System;
using System.Windows;
using Catel;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPRegionViewModel : APageObjectBaseViewModel
    {
        #region Constructor 

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CLPRegionViewModel" /> class.
        ///// </summary>
        //public CLPRegionViewModel(CLPRegion region)
        //{
        //    PageObject = region;

        //    RemovePageObjectsCommand = new Command(OnRemovePageObjectsCommandExecute);
        //    DuplicateCommand = new Command(OnDuplicateCommandExecute);
        //    UnselectRegionCommand = new Command(OnUnselectRegionCommandExecute);
        //}

        #endregion //Constructor

        public override void ClearAdorners()
        {
            IsAdornerVisible = false;
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        #region Commands

        ///// <summary>
        ///// Removes pageObjects from page when Delete button is pressed.
        ///// </summary>
        //public Command RemovePageObjectsCommand { get; set; }

        //private void OnRemovePageObjectsCommandExecute()
        //{
        //    foreach(ICLPPageObject pageObject in PageObject.GetPageObjectsOverPageObject())
        //    {
        //        ACLPPageBaseViewModel.RemovePageObjectFromPage(pageObject);
        //    }
        //    ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        //}

        ///// <summary>
        ///// Brings up a menu to make multiple copies of page objects in the region
        ///// </summary>
        //public Command DuplicateCommand { get; private set; }

        //private void OnDuplicateCommandExecute()
        //{
        //    var keyPad = new KeypadWindowView("How many copies?", 21)
        //                 {
        //                     Owner = Application.Current.MainWindow,
        //                     WindowStartupLocation = WindowStartupLocation.Manual,
        //                     Top = 100,
        //                     Left = 100
        //                 };
        //    keyPad.ShowDialog();
        //    if(keyPad.DialogResult != true ||
        //       keyPad.NumbersEntered.Text.Length <= 0) { return; }
        //    var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);

        //    double xPosition = 50.0;
        //    double yPosition = YPosition;
        //    const double GAP = 35.0;
        //    if(XPosition + Width * (numberOfCopies + 1) + GAP * numberOfCopies <= PageObject.ParentPage.Width) { xPosition = XPosition + Width + GAP; }
        //    else if(YPosition + 2 * Height < PageObject.ParentPage.Height) { yPosition = YPosition + Height; }
        //    foreach(var lassoedPageObject in PageObject.GetPageObjectsOverPageObject())
        //    {
        //        for(int i = 0; i < numberOfCopies; i++)
        //        {
        //            var duplicatePageObject = lassoedPageObject.Duplicate();
        //            double xOffset = lassoedPageObject.XPosition - XPosition;
        //            double yOffset = lassoedPageObject.YPosition - YPosition;

        //            if(xPosition + Width * (i + 1) + GAP * i <= PageObject.ParentPage.Width)
        //            {
        //                duplicatePageObject.XPosition = xPosition + xOffset + i * (Width + GAP);
        //                duplicatePageObject.YPosition = yPosition + yOffset;
        //            }
        //            else
        //            { ACLPPageObjectBase.ApplyDistinctPosition(duplicatePageObject); }
        //            ACLPPageBaseViewModel.AddPageObjectToPage(PageObject.ParentPage, duplicatePageObject, true);
        //            //TODO: Steve - add MassPageObjectAdd history item and MassPageObjectRemove history item.
        //        }
        //    }
        //}

        /// <summary>
        /// Unselects the region
        /// </summary>
        public Command UnselectRegionCommand { get; private set; }

        private void OnUnselectRegionCommandExecute()
        {
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject, false);
        }

        #endregion //Commands
    }
}