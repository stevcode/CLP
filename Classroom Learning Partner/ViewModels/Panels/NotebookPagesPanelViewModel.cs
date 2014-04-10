using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookPagesPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelViewModel" /> class.
        /// </summary>
        public NotebookPagesPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            CurrentPage = notebook.SingleDisplay.CurrentPage;
            Initialized += NotebookPagesPanelViewModel_Initialized;
            
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ShowSubmissionsCommand = new Command<CLPPage>(OnShowSubmissionsCommandExecute);
        }

        void NotebookPagesPanelViewModel_Initialized(object sender, System.EventArgs e)
        {
            Length = InitialLength;
        }

        public override string Title
        {
            get { return "NotebookPagesPanelVM"; }
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
        /// Pages of the Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Current, selected page in the notebook.
        /// </summary>
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page) { SetCurrentPage(page); }

        /// <summary>
        /// Shows the submissions for the selected page.
        /// </summary>
        public Command<CLPPage> ShowSubmissionsCommand { get; private set; }

        private void OnShowSubmissionsCommandExecute(CLPPage page)
        {
            // TODO: Entities, convert to StagingPanel
            //var submissionsPanel = LinkedPanel as SubmissionsPanelViewModel;
            //if(submissionsPanel == null)
            //{
            //    return;
            //}

            //submissionsPanel.IsVisible = true;

            //if(_currentDisplaySubmissionsPageID == page.ID)
            //{
            //    return;
            //}

            //_currentDisplaySubmissionsPageID = page.ID;
            //submissionsPanel.SubmissionPages = Notebook.Submissions[_currentDisplaySubmissionsPageID];
        }

        private string _currentDisplaySubmissionsPageID;

        #endregion //Commands

        #region Methods

        public void SetCurrentPage(CLPPage page)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);
            }

            // TODO: Entities, StagingPanel
            //var submissionsPanel = LinkedPanel as SubmissionsPanelViewModel;
            //if(submissionsPanel != null)
            //{
            //    submissionsPanel.CurrentPage = null;
            //}

            //var historyPanel = GetSubmissionHistoryPanelViewModel();
            //if(historyPanel != null)
            //{
            //    historyPanel.CurrentPage = null;
            //    historyPanel.IsSubmissionHistoryVisible = false;
            //}
        }

        #endregion //Methods

        #region Static Methods

        public static CLPPage GetCurrentPage()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return null;
            }
            var singleDisplay = notebookWorkspaceViewModel.CurrentDisplay as SingleDisplay;
            return singleDisplay == null ? null : singleDisplay.CurrentPage;
        }

        public static NotebookPagesPanelViewModel GetNotebookPagesPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.NotebookPagesPanel;
        }

        public static SubmissionHistoryPanelViewModel GetSubmissionHistoryPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.SubmissionHistoryPanel;
        }

        #endregion //Methods
    }
}