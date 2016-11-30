﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum SortAndGroupTypes
    {
        StudentName,
        PageNumber,
        SubmissionTime,
        Starred,
        HadHelp,
        Correctness,
        TroubleWithFactorPairs,
        TroubleWithRemainders,
        TroubleWithDivision,
        DivisionTemplateStrategy
    }

    public class StagingPanelViewModel : APanelBaseViewModel
    {
        private static readonly PropertyGroupDescription OwnerFullNameGroup = new PropertyGroupDescription("Owner.FullName");
        private static readonly PropertyGroupDescription PageNumberGroup = new PropertyGroupDescription("PageNumber");
        private static readonly PropertyGroupDescription StarredGroup = new PropertyGroupDescription("IsStarred");
        private static readonly PropertyGroupDescription HadHelpGroup = new PropertyGroupDescription("HadHelp");
        private static readonly PropertyGroupDescription CorrectnessGroup = new PropertyGroupDescription("Correctness");
        private static readonly PropertyGroupDescription TroubleWithFactorPairsGroup = new PropertyGroupDescription("TroubleWithFactorPairs");
        private static readonly PropertyGroupDescription TroubleWithRemaindersGroup = new PropertyGroupDescription("TroubleWithRemainders");
        private static readonly PropertyGroupDescription TroubleWithDivisionGroup = new PropertyGroupDescription("TroubleWithDivision");
        private static readonly PropertyGroupDescription IncorrectArrayCreationGroup = new PropertyGroupDescription("DivisionTemplateIncorrectArrayCreation");
        private static readonly PropertyGroupDescription DivisionTemplateStrategyGroup = new PropertyGroupDescription("DivisionTemplateStrategy");

        private static readonly SortDescription OwnerFullNameAscendingSort = new SortDescription("Owner.FullName", ListSortDirection.Ascending);
        private static readonly SortDescription OwnerFullNameDescendingSort = new SortDescription("Owner.FullName", ListSortDirection.Descending);
        private static readonly SortDescription PageNumberAscendingSort = new SortDescription("PageNumber", ListSortDirection.Ascending);
        private static readonly SortDescription PageNumberDescendingSort = new SortDescription("PageNumber", ListSortDirection.Descending);
        private static readonly SortDescription SubmissionTimeAscendingSort = new SortDescription("SubmissionTime", ListSortDirection.Ascending);
        private static readonly SortDescription SubmissionTimeDescendingSort = new SortDescription("SubmissionTime", ListSortDirection.Descending);
        private static readonly SortDescription StarredAscendingSort = new SortDescription("IsStarred", ListSortDirection.Ascending);
        private static readonly SortDescription StarredDescendingSort = new SortDescription("IsStarred", ListSortDirection.Descending);
        private static readonly SortDescription HadHelpAscendingSort = new SortDescription("HadHelp", ListSortDirection.Ascending);
        private static readonly SortDescription HadHelpDescendingSort = new SortDescription("HadHelp", ListSortDirection.Descending);
        private static readonly SortDescription CorrectnessAscendingSort = new SortDescription("Correctness", ListSortDirection.Ascending);
        private static readonly SortDescription CorrectnessDescendingSort = new SortDescription("Correctness", ListSortDirection.Descending);
        private static readonly SortDescription TroubleWithFactorPairsAscendingSort = new SortDescription("TroubleWithFactorPairs", ListSortDirection.Ascending);
        private static readonly SortDescription TroubleWithFactorPairsDescendingSort = new SortDescription("TroubleWithFactorPairs", ListSortDirection.Descending);
        private static readonly SortDescription TroubleWithRemaindersAscendingSort = new SortDescription("TroubleWithRemainders", ListSortDirection.Ascending);
        private static readonly SortDescription TroubleWithRemaindersDescendingSort = new SortDescription("TroubleWithRemainders", ListSortDirection.Descending);
        private static readonly SortDescription TroubleWithDivisionAscendingSort = new SortDescription("TroubleWithDivision", ListSortDirection.Ascending);
        private static readonly SortDescription TroubleWithDivisionDescendingSort = new SortDescription("TroubleWithDivision", ListSortDirection.Descending);
        private static readonly SortDescription IncorrectArrayCreationAscendingSort = new SortDescription("DivisionTemplateIncorrectArrayCreation", ListSortDirection.Ascending);
        private static readonly SortDescription IncorrectArrayCreationDescendingSort = new SortDescription("DivisionTemplateIncorrectArrayCreation", ListSortDirection.Descending);
        private static readonly SortDescription DivisionTemplateStrategyAscendingSort = new SortDescription("DivisionTemplateStrategy", ListSortDirection.Ascending);
        private static readonly SortDescription DivisionTemplateStrategyDescendingSort = new SortDescription("DivisionTemplateStrategy", ListSortDirection.Descending);

        private readonly INotebookService _notebookService;

        #region Constructor

        public StagingPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            _notebookService = DependencyResolver.Resolve<INotebookService>();

            SortedAndGroupedPages.Source = FilteredPages;

            Initialized += StagingPanelViewModel_Initialized;

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            RemovePageFromStageCommand = new Command<CLPPage>(OnRemovePageFromStageCommandExecute);
            ClearStageCommand = new Command(OnClearStageCommandExecute);
            ToggleNoSubmissionsCommand = new Command<RoutedEventArgs>(OnToggleNoSubmissionsCommandExecute);

         //   AppendCollectionOfPagesToStage(SingleAddedPages);
         //   FilterCollectionOfPagesFromStage(SingleRemovedPages);
            CurrentSortAndGroupType = SortAndGroupTypes.PageNumber;
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
        /// Signifies the visibilty of manual tag icons.
        /// </summary>
        public bool IsToggleIconButtonChecked
        {
            get { return GetValue<bool>(IsToggleIconButtonCheckedProperty); }
            set { SetValue(IsToggleIconButtonCheckedProperty, value); }
        }

        public static readonly PropertyData IsToggleIconButtonCheckedProperty = RegisterProperty("IsToggleIconButtonChecked", typeof (bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CollectionViewSource SortedAndGroupedPages
        {
            get { return GetValue<CollectionViewSource>(SortedAndGroupedPagesProperty); }
            set { SetValue(SortedAndGroupedPagesProperty, value); }
        }

        public static readonly PropertyData SortedAndGroupedPagesProperty = RegisterProperty("SortedAndGroupedPages", typeof(CollectionViewSource), () => new CollectionViewSource());

        /// <summary>
        /// Current Sort and Group method applied.
        /// </summary>
        public SortAndGroupTypes CurrentSortAndGroupType
        {
            get { return GetValue<SortAndGroupTypes>(CurrentSortAndGroupTypeProperty); }
            set
            {
                SetValue(CurrentSortAndGroupTypeProperty, value);
                ApplySortAndGroup();
            }
        }

        public static readonly PropertyData CurrentSortAndGroupTypeProperty = RegisterProperty("CurrentSortAndGroupType", typeof(SortAndGroupTypes));

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page) { SetCurrentPage(page); }

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

        /// <summary>
        /// Toggles the panel that shows the student names who haven't submitted.
        /// </summary>
        public Command<RoutedEventArgs> ToggleNoSubmissionsCommand { get; private set; }

        private void OnToggleNoSubmissionsCommandExecute(RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            if(toggleButton == null)
            {
                return;
            }
            if(toggleButton.IsChecked != null &&
               !(bool)toggleButton.IsChecked)
            {
                return;
            }

            StudentsWithNoSubmissions = GetStudentsWithNoSubmissions();
        }

        /// <summary>
        /// List of student names who don't have submissions for the CurrentPage.
        /// </summary>
        public ObservableCollection<string> StudentsWithNoSubmissions
        {
            get { return GetValue<ObservableCollection<string>>(StudentsWithNoSubmissionsProperty); }
            set { SetValue(StudentsWithNoSubmissionsProperty, value); }
        }

        public static readonly PropertyData StudentsWithNoSubmissionsProperty = RegisterProperty("StudentsWithNoSubmissions",
                                                                                                 typeof(ObservableCollection<string>),
                                                                                                 () => new ObservableCollection<string>());

        /// <summary>
        /// Whether the panel showing students with no submissions is visible.
        /// </summary>
        public bool IsStudentsWithNoSubmissionsVisible
        {
            get { return GetValue<bool>(IsStudentsWithNoSubmissionsVisibleProperty); }
            set { SetValue(IsStudentsWithNoSubmissionsVisibleProperty, value); }
        }

        public static readonly PropertyData IsStudentsWithNoSubmissionsVisibleProperty = RegisterProperty("IsStudentsWithNoSubmissionsVisible", typeof(bool), false);

        /// <summary>
        /// SUMMARY
        /// </summary>
        public CLPPage LastFilteredPage
        {
            get { return GetValue<CLPPage>(LastFilteredPageProperty); }
            set { SetValue(LastFilteredPageProperty, value); }
        }

        public static readonly PropertyData LastFilteredPageProperty = RegisterProperty("LastFilteredPage", typeof(CLPPage));

        public ObservableCollection<string> GetStudentsWithNoSubmissions()
        {
            var userNames = new ObservableCollection<string>();

            foreach(var availableUser in App.MainWindowViewModel.AvailableUsers)
            {
                userNames.Add(availableUser.FullName);
            }

            foreach(var p in LastFilteredPage.Submissions.Where(p => userNames.Contains(p.Owner.FullName))) 
            {
                userNames.Remove(p.Owner.FullName);
            }
            return userNames;
        }

        #endregion //Commands

        #region Methods

        public void SetCurrentPage(CLPPage page)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            if(notebookWorkspaceViewModel.CurrentDisplay == null)
            {
                //Take thumbnail of page before navigating away from it.
                ACLPPageBaseViewModel.TakePageThumbnail(CurrentPage);
                CurrentPage = page;
                return;
            }

            notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);
        }

        #region Filters

        public void ClearStage()
        {
            SingleAddedPages.Clear();
            SingleRemovedPages.Clear();
            FilteredPages.Clear();
            StagingPanelSubscription = null;
            AllCollectionOperations = null;
            AppendCollectionOfPagesToStage(SingleAddedPages);
            FilterCollectionOfPagesFromStage(SingleRemovedPages);
        }

        public void AddPageToStage(CLPPage page)
        {
            if(SingleRemovedPages.Contains(page))
            {
                SingleRemovedPages.Remove(page);
            }
            
            SingleAddedPages.Add(page);
        }

        public void RemovePageFromStage(CLPPage page)
        {
            if(SingleAddedPages.Contains(page))
            {
                SingleAddedPages.Remove(page);
            }

            SingleRemovedPages.Add(page);
        }

        public void AppendCollectionOfPagesToStage(ObservableCollection<CLPPage> pages, bool includeRemovesAndClears = true)
        {
            FilteredPages.Clear();
            var appendedPagesOperations = pages.ToOperations(x => !SingleRemovedPages.Contains(x), includeRemovesAndClears);

            AllCollectionOperations = AllCollectionOperations == null ? appendedPagesOperations : AllCollectionOperations.Merge(appendedPagesOperations);

            StagingPanelSubscription = AllCollectionOperations.Distinct().Subscribe(FilteredPages);
        }

        public void AppendCollectionOfPagesToStage(ObservableCollection<CLPPage> pages, Func<CLPPage, bool> filter, bool includeRemovesAndClears = true)
        {
            FilteredPages.Clear();
            var appendedPagesOperations = pages.ToOperations(x => !SingleRemovedPages.Contains(x) && filter(x), includeRemovesAndClears);

            AllCollectionOperations = AllCollectionOperations == null ? appendedPagesOperations : AllCollectionOperations.Merge(appendedPagesOperations);

            StagingPanelSubscription = AllCollectionOperations.Distinct().Subscribe(FilteredPages);
        }

        public void FilterCollectionOfPagesFromStage(ObservableCollection<CLPPage> pages, bool includeRemovesAndClears = true)
        {
            var appendedPagesOperations = pages.ToOppositeOperations(includeRemovesAndClears);

            AllCollectionOperations = AllCollectionOperations == null ? appendedPagesOperations : AllCollectionOperations.Merge(appendedPagesOperations);

            StagingPanelSubscription = AllCollectionOperations.Distinct().Subscribe(FilteredPages);
        }

        #endregion //Filters 

        #region Sorts

        public void ApplySortAndGroup()
        {
            switch(CurrentSortAndGroupType)
            {
                case SortAndGroupTypes.StudentName:
                    ApplySortAndGroupByName();
                    break;
                case SortAndGroupTypes.PageNumber:
                    ApplySortAndGroupByPageNumber();
                    break;
                case SortAndGroupTypes.SubmissionTime:
                    ApplySortByTime();
                    break;
                case SortAndGroupTypes.Starred:
                    ApplySortAndGroupByStarred();
                    break;
                case SortAndGroupTypes.HadHelp:
                    ApplySortAndGroupByHadHelp();
                    break;
                case SortAndGroupTypes.Correctness:
                    ApplySortAndGroupByCorrectness();
                    break;
                case SortAndGroupTypes.TroubleWithFactorPairs:
                    ApplySortAndGroupByTroubleWithFactorPairs();
                    break;
                case SortAndGroupTypes.TroubleWithRemainders:
                    ApplySortAndGroupByTroubleWithRemainders();
                    break;
                case SortAndGroupTypes.TroubleWithDivision:
                    ApplySortAndGroupByTroubleWithDivision();
                    break;
                case SortAndGroupTypes.DivisionTemplateStrategy:
                    ApplySortAndGroupByDivisionTemplateStrategy();
                    break;
                default:
                    ApplySortAndGroupByName();
                    break;
            }
        }

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

        public void ApplySortAndGroupByStarred()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(StarredGroup);
            SortedAndGroupedPages.SortDescriptions.Add(StarredAscendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByHadHelp()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(HadHelpGroup);
            SortedAndGroupedPages.SortDescriptions.Add(HadHelpAscendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByCorrectness()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(CorrectnessGroup);
            SortedAndGroupedPages.SortDescriptions.Add(CorrectnessAscendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByTroubleWithFactorPairs()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(TroubleWithFactorPairsGroup);
            SortedAndGroupedPages.SortDescriptions.Add(TroubleWithFactorPairsDescendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByTroubleWithRemainders()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(TroubleWithRemaindersGroup);
            SortedAndGroupedPages.SortDescriptions.Add(TroubleWithRemaindersDescendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByTroubleWithDivision()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(TroubleWithDivisionGroup);
            SortedAndGroupedPages.SortDescriptions.Add(TroubleWithDivisionDescendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        public void ApplySortAndGroupByDivisionTemplateStrategy()
        {
            SortedAndGroupedPages.GroupDescriptions.Clear();
            SortedAndGroupedPages.SortDescriptions.Clear();

            SortedAndGroupedPages.GroupDescriptions.Add(DivisionTemplateStrategyGroup);
            SortedAndGroupedPages.SortDescriptions.Add(DivisionTemplateStrategyAscendingSort);

            SortedAndGroupedPages.GroupDescriptions.Add(PageNumberGroup);
            SortedAndGroupedPages.SortDescriptions.Add(PageNumberAscendingSort);

            SortedAndGroupedPages.SortDescriptions.Add(OwnerFullNameAscendingSort);
            SortedAndGroupedPages.SortDescriptions.Add(SubmissionTimeAscendingSort);
        }

        #endregion //Sorts

        #region Shortcuts

        public void AppendSubmissionsForPage(CLPPage page)
        {
            AppendCollectionOfPagesToStage(page.Submissions);

            if(CurrentSortAndGroupType != SortAndGroupTypes.StudentName)
            {
                CurrentSortAndGroupType = SortAndGroupTypes.StudentName;
            }
        }

        public void SetSubmissionsForPage(CLPPage page)
        {
            ClearStage();
            AppendSubmissionsForPage(page);
        }

        public void AppendStudentNotebook(Person student)
        {
            foreach (var page in _notebookService.CurrentNotebook.Pages)
            {
                AppendCollectionOfPagesToStage(page.Submissions, x => x.OwnerID == student.ID);
            }

            if(CurrentSortAndGroupType != SortAndGroupTypes.PageNumber)
            {
                CurrentSortAndGroupType = SortAndGroupTypes.PageNumber;
            }
        }

        public void SetStudentNotebook(Person student)
        {
            ClearStage();
            AppendStudentNotebook(student);
        }

        public void AppendStarredSubmissionsForPage(CLPPage page)
        {
            AppendCollectionOfPagesToStage(page.Submissions, x => x.Tags.FirstOrDefault(t => t is StarredTag && (t as StarredTag).Value == StarredTag.AcceptedValues.Starred) != null);

            if(CurrentSortAndGroupType != SortAndGroupTypes.Starred)
            {
                CurrentSortAndGroupType = SortAndGroupTypes.Starred;
            }
        }

        #endregion //Shortcuts

        #endregion //Methods
    }
}
