using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StagingPanelViewModel : APanelBaseViewModel
    {
        private static readonly SortDescription OwnerFullNameAscendingSort = new SortDescription("Owner.FullName", ListSortDirection.Ascending);
        private static readonly SortDescription SubmissionTimeAscendingSort = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
        private static readonly SortDescription SubmissionTimeDescendingSort = new SortDescription("SubmissionTime", ListSortDirection.Descending);
        private static readonly SortDescription PageNumberAscendingSort = new SortDescription("PageNumber", ListSortDirection.Ascending);
        private static readonly SortDescription PageNumberDescendingSort = new SortDescription("PageNumber", ListSortDirection.Descending);

        public StagingPanelViewModel(Notebook notebook)
        {
            Initialized += StagingPanelViewModel_Initialized;

            RemovePageFromStageCommand = new Command<CLPPage>(OnRemovePageFromStageCommandExecute);
            ClearStageCommand = new Command(OnClearStageCommandExecute);

           // var pagesSortedByTime = from page in notebook.Pages[0].Submissions

        }

        void StagingPanelViewModel_Initialized(object sender, EventArgs e)
        {
            
        }

        //observablecollection of PageGroup. PageGroup.GroupName = SubmitterName?

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource SortedAndGroupedPages
        {
            get { return GetValue<CollectionViewSource>(SortedAndGroupedPagesProperty); }
            set { SetValue(SortedAndGroupedPagesProperty, value); }
        }

        public static readonly PropertyData SortedAndGroupedPagesProperty = RegisterProperty("SortedAndGroupedPages", typeof(CollectionViewSource), () => new CollectionViewSource());

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Removes given page from the Staging Panel.
        /// </summary>
        public Command<CLPPage> RemovePageFromStageCommand { get; private set; }

        private void OnRemovePageFromStageCommandExecute(CLPPage page) { RemovePageFromStage(page); }

        /// <summary>
        /// Clears the Stage of all filters, sorts, and contents.
        /// </summary>
        public Command ClearStageCommand { get; private set; }

        private void OnClearStageCommandExecute() { ClearStage(); }

        #endregion //Commands

        #region Methods

        #region Filters

        public void ClearStage()
        {
            
        }

        public void AddPageToStage(CLPPage page)
        {
            
        }

        public void RemovePageFromStage(CLPPage page)
        {
            //remove from FilteredPages and add to listoftrashed pages Rx will ignore.
        }

        public void AppendCollectionOfPagesToStage(ObservableCollection<CLPPage> pages)
        {
            
        }

        #endregion //Filters

        #region Sorts

        public void ApplySortAndGroupByName()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByPageNumber()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortByTime()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        #endregion //Sorts

        #endregion //Methods
    }
}
