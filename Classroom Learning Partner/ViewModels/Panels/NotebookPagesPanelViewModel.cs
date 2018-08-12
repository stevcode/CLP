using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookPagesPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly IRoleService _roleService;

        #region Constructor

        public NotebookPagesPanelViewModel(StagingPanelViewModel stagingPanel, IDataService dataService, IRoleService roleService)
        {
            Argument.IsNotNull(() => dataService);
            Argument.IsNotNull(() => roleService);

            _dataService = dataService;
            _roleService = roleService;

            Notebook = _dataService.CurrentNotebook;
            
            InitializeEventSubscriptions();
            InitializeCommands();

            StagingPanel = stagingPanel;

            var dependencyResolver = this.GetDependencyResolver();
            var viewModelFactory = dependencyResolver.Resolve<IViewModelFactory>();
            SubmissionHistoryPanel = viewModelFactory.CreateViewModel<SubmissionHistoryPanelViewModel>(null);
        }

        /// <summary>Initial Length of the Panel, before any resizing.</summary>
        public override double InitialLength => 300.0;

        #endregion //Constructor

        #region Events

        private void InitializeEventSubscriptions()
        {
            InitializedAsync += NotebookPagesPanelViewModel_InitializedAsync;
            _roleService.RoleChanged += _roleService_RoleChanged;
        }

        private Task NotebookPagesPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            Length = InitialLength;

            return TaskHelper.Completed;
        }

        private void _roleService_RoleChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(ResearcherOrTeacherVisibility));
            RaisePropertyChanged(nameof(StudentOrProjectorVisibility));
            RaisePropertyChanged(nameof(IsStudentRole));
            RaisePropertyChanged(nameof(IsResearcherOrTeacherRole));
        }

        #endregion // Events

        #region ViewModelBase Overrides

        protected override async Task OnClosingAsync()
        {
            InitializedAsync -= NotebookPagesPanelViewModel_InitializedAsync;
            _roleService.RoleChanged -= _roleService_RoleChanged;
            await base.OnClosingAsync();
        }

        #endregion // ViewModelBase Overrides

        #region Model

        /// <summary>Notebook associated with the panel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get => GetValue<Notebook>(NotebookProperty);
            private set => SetValue(NotebookProperty, value);
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>Current, selected page in the notebook.</summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get => GetValue<CLPPage>(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        /// <summary>Pages of the Notebook.</summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<CLPPage> Pages
        {
            get => GetValue<ObservableCollection<CLPPage>>(PagesProperty);
            set => SetValue(PagesProperty, value);
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        #region Bindings

        /// <summary>Staging Panel for submissions</summary>
        public StagingPanelViewModel StagingPanel
        {
            get => GetValue<StagingPanelViewModel>(StagingPanelProperty);
            set => SetValue(StagingPanelProperty, value);
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(StagingPanelViewModel));

        /// <summary>Submissions History Panel for the submissions of a student's page.</summary>
        public SubmissionHistoryPanelViewModel SubmissionHistoryPanel
        {
            get => GetValue<SubmissionHistoryPanelViewModel>(SubmissionHistoryPanelProperty);
            set => SetValue(SubmissionHistoryPanelProperty, value);
        }

        public static readonly PropertyData SubmissionHistoryPanelProperty = RegisterProperty("SubmissionHistoryPanel", typeof(SubmissionHistoryPanelViewModel));

        #region Visibilities

        public Visibility ResearcherOrTeacherVisibility => IsResearcherOrTeacherRole ? Visibility.Visible : Visibility.Collapsed;

        public Visibility StudentOrProjectorVisibility => _roleService.Role == ProgramRoles.Student || _roleService.Role == ProgramRoles.Projector
                                                              ? Visibility.Visible
                                                              : Visibility.Collapsed;

        public bool IsStudentRole => _roleService.Role == ProgramRoles.Student;

        public bool IsResearcherOrTeacherRole => _roleService.Role == ProgramRoles.Researcher || _roleService.Role == ProgramRoles.Teacher;

        #endregion // Visibilities

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ShowSubmissionsCommand = new Command<CLPPage>(OnShowSubmissionsCommandExecute);
            AddPageToStageCommand = new Command<CLPPage>(OnAddPageToStageCommandExecute);
        }

        /// <summary>Sets the current selected page in the listbox.</summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            _dataService.AddPageToCurrentDisplay(page);
        }

        /// <summary>Shows the submissions for the selected page.</summary>
        public Command<CLPPage> ShowSubmissionsCommand { get; private set; }

        private void OnShowSubmissionsCommandExecute(CLPPage page)
        {
            StagingPanel.IsVisible = true;
            StagingPanel.SetSubmissionsForPage(page);
            StagingPanel.LastFilteredPage = page;
            StagingPanel.StudentsWithNoSubmissions = StagingPanel.GetStudentsWithNoSubmissions();
        }

        /// <summary>Adds individual page to the Staging Panel</summary>
        public Command<CLPPage> AddPageToStageCommand { get; private set; }

        private void OnAddPageToStageCommandExecute(CLPPage page)
        {
            var dialog = new AdvancedStagingView();
            dialog.ShowDialog();
            if (dialog.DialogResult == false)
            {
                return;
            }
            switch (dialog.StagingType)
            {
                case AdvancedStagingView.StagingTypes.Starred:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendStarredSubmissionsForPage(page);
                    break;
                case AdvancedStagingView.StagingTypes.Correct:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendCollectionOfPagesToStage(page.Submissions,
                                                                x => x.Tags.FirstOrDefault(t => t is CorrectnessSummaryTag &&
                                                                                                ((CorrectnessSummaryTag)t).OverallCorrectness == Correctness.Correct) !=
                                                                     null);

                    //TODO: keep CurrentSort and skip this if already sorted that way.
                    StagingPanel.ApplySortAndGroupByName();
                    break;
                case AdvancedStagingView.StagingTypes.Incorrect:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendCollectionOfPagesToStage(page.Submissions,
                                                                x => x.Tags.FirstOrDefault(t => t is CorrectnessSummaryTag &&
                                                                                                ((CorrectnessSummaryTag)t).OverallCorrectness == Correctness.Incorrect) !=
                                                                     null);

                    //TODO: keep CurrentSort and skip this if already sorted that way.
                    StagingPanel.ApplySortAndGroupByName();
                    break;
                case AdvancedStagingView.StagingTypes.All:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendCollectionOfPagesToStage(page.Submissions);
                    //TODO: keep CurrentSort and skip this if already sorted that way.
                    StagingPanel.ApplySortAndGroupByName();
                    break;
                case AdvancedStagingView.StagingTypes.Teacher:
                    StagingPanel.AddPageToStage(page);
                    break;
                case AdvancedStagingView.StagingTypes.None:
                default:
                    break;
            }
        }

        #endregion //Commands
    }
}