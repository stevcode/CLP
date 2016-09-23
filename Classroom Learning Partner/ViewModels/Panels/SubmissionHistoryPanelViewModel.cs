using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SubmissionHistoryPanelViewModel : APanelBaseViewModel
    {
        private readonly IDataService _dataService;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionHistoryPanelViewModel" /> class.
        /// </summary>
        public SubmissionHistoryPanelViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Notebook = _dataService.CurrentNotebook;
            InitializedAsync += SubmissionHistoryPanelViewModel_InitializedAsync;
            ClosedAsync += SubmissionHistoryPanelViewModel_ClosedAsync;

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
        }

        private Task SubmissionHistoryPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            _dataService.CurrentNotebookChanged += _dataService_CurrentNotebookChanged;

            Length = InitialLength;
            IsVisible = false;

            return TaskHelper.Completed;
        }

        private Task SubmissionHistoryPanelViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentNotebookChanged -= _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentNotebookChanged(object sender, EventArgs e)
        {
            Notebook = _dataService.CurrentNotebook;
        }

        /// <summary>
        /// Initial Length of the Panel, before any resizing.
        /// </summary>
        public override double InitialLength
        {
            get { return 300.0; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Notebook associated with the panel.
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

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage), propertyChangedEventHandler:CurrentPageChangedEventHandler);

        private static void CurrentPageChangedEventHandler(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            if(!advancedPropertyChangedEventArgs.IsNewValueMeaningful)
            {
                return;
            }

            var submissionHistoryPanel = sender as SubmissionHistoryPanelViewModel;
            var currentPage = advancedPropertyChangedEventArgs.NewValue as CLPPage;
            if(submissionHistoryPanel == null || 
               currentPage == null ||
               currentPage.SubmissionType != SubmissionTypes.Unsubmitted)
            {
                return;
            }

            submissionHistoryPanel.OriginPage = currentPage;
            submissionHistoryPanel.SubmissionPages = currentPage.Submissions;

            submissionHistoryPanel.RaisePropertyChanged("IsSelectedANonSubmission");
        }

        #endregion //Model
        
        #region Bindings

        public bool IsSelectedANonSubmission
        {
            get { return CurrentPage?.VersionIndex == 0; }
        }

        /// <summary>
        /// The live version of the <see cref="CLPPage" /> as it exists in the Notebook Pages Panel.
        /// </summary>
        public CLPPage OriginPage
        {
            get { return GetValue<CLPPage>(OriginPageProperty); }
            set { SetValue(OriginPageProperty, value); }
        }

        public static readonly PropertyData OriginPageProperty = RegisterProperty("OriginPage", typeof(CLPPage));

        /// <summary>
        /// All the submissions for the desired page.
        /// </summary>
        public ObservableCollection<CLPPage> SubmissionPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionPagesProperty); }
            set { SetValue(SubmissionPagesProperty, value); }
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
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
                RaisePropertyChanged("IsSelectedANonSubmission");
                return;
            }

            notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);

            RaisePropertyChanged("IsSelectedANonSubmission");
        }

        #endregion //Commands
    }
}