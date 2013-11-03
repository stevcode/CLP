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

            double xPosition = 50.0;
            double yPosition = YPosition;
            if(XPosition + Width * (numberOfCopies + 1) < PageObject.ParentPage.PageWidth)
            {
                xPosition = XPosition + Width;
            }
            else if(YPosition + 2 * Height < PageObject.ParentPage.PageHeight)
            {
                yPosition = YPosition + Height;
            }
            foreach(var lassoedPageObject in PageObject.GetPageObjectsOverPageObject())
            {
                for(int i = 0; i < numberOfCopies; i++)
                {
                    var duplicatePageObject = lassoedPageObject.Duplicate();
                    double xOffset = lassoedPageObject.XPosition - XPosition;
                    double yOffset = lassoedPageObject.YPosition - YPosition;

                    if(xPosition + (i + 2) * Width <= PageObject.ParentPage.PageWidth)
                    {
                        duplicatePageObject.XPosition = xPosition + xOffset + i * Width;
                        duplicatePageObject.YPosition = yPosition + yOffset;
                    }
                    else
                    {
                        ACLPPageObjectBase.ApplyDistinctPosition(duplicatePageObject);
                    }
                    ACLPPageBaseViewModel.AddPageObjectToPage(PageObject.ParentPage, duplicatePageObject, true);
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