using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StagingPanelViewModel : APanelBaseViewModel
    {
        private static readonly PropertyGroupDescription OwnerFullNameGroup = new PropertyGroupDescription("Owner.FullName");
        private static readonly PropertyGroupDescription PageNumberGroup = new PropertyGroupDescription("PageNumber");

        private static readonly SortDescription OwnerFullNameAscendingSort = new SortDescription("Owner.FullName", ListSortDirection.Ascending);
        private static readonly SortDescription OwnerFullNameDescendingSort = new SortDescription("Owner.FullName", ListSortDirection.Descending);
        private static readonly SortDescription SubmissionTimeAscendingSort = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
        private static readonly SortDescription SubmissionTimeDescendingSort = new SortDescription("SubmissionTime", ListSortDirection.Descending);
        private static readonly SortDescription PageNumberAscendingSort = new SortDescription("PageNumber", ListSortDirection.Ascending);
        private static readonly SortDescription PageNumberDescendingSort = new SortDescription("PageNumber", ListSortDirection.Descending);

        #region Constructor

        public StagingPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            SortedAndGroupedPages.Source = FilteredPages;

            Initialized += StagingPanelViewModel_Initialized;

            RemovePageFromStageCommand = new Command<CLPPage>(OnRemovePageFromStageCommandExecute);
            ClearStageCommand = new Command(OnClearStageCommandExecute);
        }

        void StagingPanelViewModel_Initialized(object sender, EventArgs e) { Length = InitialLength; }

        public override string Title
        {
            get { return "StagingPanelVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// The Model for this ViewModel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Current, selected submission.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Model

        #region Properties

        /// <summary>
        /// Source collection of filtered <see cref="CLPPage" />s used by the SortedAndGroupedPages <see cref="CollectionViewSource" />.
        /// </summary>
        public ObservableCollection<CLPPage> FilteredPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(FilteredPagesProperty); }
            set { SetValue(FilteredPagesProperty, value); }
        }

        public static readonly PropertyData FilteredPagesProperty = RegisterProperty("FilteredPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// <see cref="CLPPage" />s that have been individually added to the Staging Panel.
        /// </summary>
        public ObservableCollection<CLPPage> SingleAddedPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SingleAddedPagesProperty); }
            set { SetValue(SingleAddedPagesProperty, value); }
        }

        public static readonly PropertyData SingleAddedPagesProperty = RegisterProperty("SingleAddedPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// <see cref="CLPPage" />s that have been individually removed from the Staging Panel.
        /// </summary>
        public ObservableCollection<CLPPage> SingleRemovedPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SingleRemovedPagesProperty); }
            set { SetValue(SingleRemovedPagesProperty, value); }
        }

        public static readonly PropertyData SingleRemovedPagesProperty = RegisterProperty("SingleRemovedPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// All active operations the occur on any appended collection of pages.
        /// </summary>
        public IObservable<ObservableCollectionOperation<CLPPage>> AllCollectionOperations { get; set; } 

        /// <summary>
        /// <see cref="IDisposable" /> that holds the current Subscription for AllCollectionOperations.
        /// </summary>
        private IDisposable _stagingPanelSubscription;
        public IDisposable StagingPanelSubscription
        {
            get { return _stagingPanelSubscription; }
            set
            {
                if(_stagingPanelSubscription != null)
                {
                    _stagingPanelSubscription.Dispose();
                }
                _stagingPanelSubscription = value;
            }
        }

        #endregion //Properties

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
            SingleAddedPages.Clear();
            SingleRemovedPages.Clear();
            FilteredPages.Clear();
            StagingPanelSubscription = null;
            AllCollectionOperations = null;
        }

        public void AddPageToStage(CLPPage page)
        {
            SingleAddedPages.Add(page);
        }

        public void RemovePageFromStage(CLPPage page)
        {
            SingleRemovedPages.Add(page);
        }

        public void AppendCollectionOfPagesToStage(ObservableCollection<CLPPage> pages)
        {
            var appendedPagesOperations = pages.ToOperations(x => !SingleRemovedPages.Contains(x));

            AllCollectionOperations = AllCollectionOperations == null ? appendedPagesOperations : AllCollectionOperations.Merge(appendedPagesOperations);

            StagingPanelSubscription = AllCollectionOperations.Distinct().Subscribe(FilteredPages);
        }

        public void AppendCollectionOfPagesToStage(ObservableCollection<CLPPage> pages, Func<CLPPage, bool> filter)
        {
            var appendedPagesOperations = pages.ToOperations(x => !SingleRemovedPages.Contains(x) && filter(x));

            AllCollectionOperations = AllCollectionOperations == null ? appendedPagesOperations : AllCollectionOperations.Merge(appendedPagesOperations);

            StagingPanelSubscription = AllCollectionOperations.Distinct().Subscribe(FilteredPages);
        }

        #endregion //Filters 

        #region Sorts

        public void ApplySortAndGroupByName()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(OwnerFullNameGroup);
            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByPageNumber()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(OwnerFullNameGroup);
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

        #region Shortcuts

        public void AppendSubmissionsForPage(CLPPage page)
        {
            AppendCollectionOfPagesToStage(page.Submissions);

            //TODO: keep CurrentSort and skip this if already sorted that way.
            ApplySortAndGroupByName();
        }

        public void SetSubmissionsForPage(CLPPage page)
        {
            ClearStage();
            AppendSubmissionsForPage(page);
        }

        public void AppendStudentNotebook(Person student)
        {
            foreach(var page in App.MainWindowViewModel.OpenNotebooks.First(x => x.Name == App.MainWindowViewModel.CurrentNotebookName).Pages)
            {
                AppendCollectionOfPagesToStage(page.Submissions, x => x.OwnerID == student.ID);
            }

            //TODO: keep CurrentSort and skip this if already sorted that way.
            ApplySortAndGroupByName();
        }

        public void SetStudentNotebook(Person student)
        {
            ClearStage();
            AppendStudentNotebook(student);
        }

        public void AppendStarredSubmissionsForPage(CLPPage page)
        {
            AppendCollectionOfPagesToStage(page.Submissions, x => x.Tags.FirstOrDefault(t => t is StarredTag && t.Value == StarredTag.AcceptedValues.Starred.ToString()) != null);

            //TODO: keep CurrentSort and skip this if already sorted that way.
            ApplySortAndGroupByName();
        }

        #endregion //Shortcuts

        #endregion //Methods
    }
}
