using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class BinViewModel : APageObjectBaseViewModel
    {
        #region Constructors

        /// <summary>Initializes a new instance of the BinReporterViewModel class.</summary>
        public BinViewModel(Bin bin)
        {
            PageObject = bin;
            InitializeCommands();
            InitializeButtons();
        }

        private void InitializeCommands()
        {
            DuplicateBinCommand = new Command(OnDuplicateBinCommandExecute);
            RemoveBinCommand = new Command(OnRemoveBinCommandExecute);
            EmptyBinCommand = new Command(OnEmptyBinCommandExecute);
        }

        private void InitializeButtons()
        {
            _contextButtons.Clear();

            _contextButtons.Add(new RibbonButton("Delete", "pack://application:,,,/Resources/Images/Delete.png", RemoveBinCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);
            
            _contextButtons.Add(new RibbonButton("Empty Bin", "pack://application:,,,/Resources/Images/Trash32.png", EmptyBinCommand, null, true));
        }

        #endregion //Constructors

        #region Model

        /// <summary>Number of Parts in the Bin.</summary>
        [ViewModelToModel("PageObject")]
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int)); 

        #endregion //Model

        #region Commands

        /// <summary>Copies a Bin and any containing Marks.</summary>
        public Command DuplicateBinCommand { get; private set; }

        private void OnDuplicateBinCommandExecute()
        {
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
            var numberOfBins = Int32.Parse(keyPad.NumbersEntered.Text);

            var binsToAdd = new List<IPageObject>();
            foreach (var index in Enumerable.Range(1, numberOfBins))
            {
                var newBin = PageObject.Duplicate() as Bin;
                if (newBin == null)
                {
                    continue;
                }

                binsToAdd.Add(newBin);
            }

            ApplyDistinctPosition(PageObject.ParentPage, binsToAdd);

            var marksToAdd = new List<IPageObject>();
            foreach (var addedBin in binsToAdd)
            {
                var bin = PageObject as Bin;
                if (bin == null)
                {
                    continue;
                }

                foreach (var mark in bin.AcceptedPageObjects)
                {
                    var newMark = mark.Duplicate();
                    newMark.XPosition = newMark.XPosition + (addedBin.XPosition - bin.XPosition);
                    newMark.YPosition = newMark.YPosition + (addedBin.YPosition - bin.YPosition);
                    marksToAdd.Add(newMark);
                } 
            }

            binsToAdd.AddRange(marksToAdd);

            if (binsToAdd.Count == 1)
            {
                ACLPPageBaseViewModel.AddPageObjectToPage(binsToAdd.First());
            }
            else
            {
                ACLPPageBaseViewModel.AddPageObjectsToPage(PageObject.ParentPage, binsToAdd);
            }
        }

        /// <summary>
        /// Removes a Bin from the page and removed the BinReporter if this Bin is the last on the page.
        /// </summary>
        public Command RemoveBinCommand { get; private set; }

        private void OnRemoveBinCommandExecute()
        {
            var contextRibbon = NotebookWorkspaceViewModel.GetContextRibbon();
            if (contextRibbon != null)
            {
                contextRibbon.Buttons.Clear();
            }

            var page = PageObject.ParentPage;
            if (page == null)
            {
                return;
            }

            var pageObjectsToRemove = new List<IPageObject>
                                   {
                                       PageObject
                                   };

            if (page.PageObjects.OfType<Bin>().Count() <= 1)
            {
                var binReportersOnPage = page.PageObjects.OfType<BinReporter>().ToList();
                pageObjectsToRemove.AddRange(binReportersOnPage);
            }

            ACLPPageBaseViewModel.RemovePageObjectsFromPage(page, pageObjectsToRemove);
        }

        /// <summary>
        /// Deletes all of the Marks that have been accepted by this Bin.
        /// </summary>
        public Command EmptyBinCommand { get; private set; }

        private void OnEmptyBinCommandExecute()
        {
            var bin = PageObject as Bin;
            if (bin == null)
            {
                return;
            }

            var parentPage = bin.ParentPage;
            if (parentPage == null)
            {
                return;
            }

            var marksToDelete = bin.AcceptedPageObjects.OfType<Mark>().Cast<IPageObject>().ToList();
            if (marksToDelete.Any())
            {
                ACLPPageBaseViewModel.RemovePageObjectsFromPage(bin.ParentPage, marksToDelete);
            }
            
            var strokesToTrash = new StrokeCollection();
            foreach (var stroke in bin.AcceptedStrokes.Where(stroke => parentPage.InkStrokes.Contains(stroke)))
            {
                strokesToTrash.Add(stroke);
            }

            if (strokesToTrash.Any())
            {
                parentPage.InkStrokes.Remove(strokesToTrash);
                bin.ChangeAcceptedStrokes(new List<Stroke>(), strokesToTrash);

                ACLPPageBaseViewModel.AddHistoryActionToPage(parentPage, new ObjectsOnPageChangedHistoryAction(parentPage, App.MainWindowViewModel.CurrentUser, new List<Stroke>(), strokesToTrash));
            }
        }

        #endregion //Commands

        #region Static Methods

        public static void AddBinToPage(CLPPage page)
        {
            var pageObjectsToAdd = new List<IPageObject>();
            var bin = new Bin(page);
            ApplyDistinctPosition(bin);
            pageObjectsToAdd.Add(bin);
            if (!page.PageObjects.OfType<BinReporter>().Any())
            {
                var binReporter = new BinReporter(page);
                pageObjectsToAdd.Add(binReporter);
            }

            ACLPPageBaseViewModel.AddPageObjectsToPage(page, pageObjectsToAdd);
        }

        #endregion //Static Methods
    }
}