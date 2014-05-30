using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StagingPanelViewModel : APanelBaseViewModel
    {
        private static SortDescription _ownerFullNameAscendingSort = new SortDescription("Owner.FullName", ListSortDirection.Ascending);
        private static SortDescription _submissionTimeDescendingSort = new SortDescription("SubmissionTime", ListSortDirection.Descending);

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource SortedAndFilteredPages
        {
            get { return GetValue<CollectionViewSource>(SortedAndFilteredPagesProperty); }
            set { SetValue(SortedAndFilteredPagesProperty, value); }
        }

        public static readonly PropertyData SortedAndFilteredPagesProperty = RegisterProperty("SortedAndFilteredPages", typeof(CollectionViewSource), () => new CollectionViewSource());

        public void AddPageToStage(CLPPage page)
        {
            
        }

        /// <summary>
        /// Removes given page from the Staging Panel.
        /// </summary>
        public Command<CLPPage> RemovePageFromStageCommand { get; private set; }

        private void OnRemovePageFromStageCommandExecute(CLPPage page)
        {
            //remove from FilteredPages and add to listoftrashed pages Rx will ignore.
        }

        /// <summary>
        /// Clears the Stage of all filters, sorts, and contents.
        /// </summary>
        public Command ClearStageCommand { get; private set; }

        private void OnClearStageCommandExecute()
        {
            
        }
    }
}
