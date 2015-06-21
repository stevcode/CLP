using System.Collections.Generic;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
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
        }

        private void InitializeButtons()
        {
            _contextButtons.Clear();

            _contextButtons.Add(new RibbonButton("Delete", "pack://application:,,,/Images/Delete.png", RemoveBinCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Create Copies", "pack://application:,,,/Images/AddToDisplay.png", DuplicateBinCommand, null, true));
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

        private void OnDuplicateBinCommandExecute() { }

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

        #endregion //Commands

        #region Static Methods

        public static void AddBinToPage(CLPPage page)
        {
            var pageObjectsToAdd = new List<IPageObject>();
            var bin = new Bin(page);
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