using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SubmissionHistoryPanelViewModel : APanelBaseViewModel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelViewModel"/> class.
        /// </summary>
        public SubmissionHistoryPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            Location = PanelLocations.Bottom;

            TogglePanelCommand = new Command<RoutedEventArgs>(OnTogglePanelCommandExecute);
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "SubmissionHistoryPanelVM"; } }

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
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #region Bindings

        /// <summary>
        /// All the submissions for the desired page.
        /// </summary>
        public ObservableCollection<CLPPage> SubmissionPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionPagesProperty); }
            set { SetValue(SubmissionPagesProperty, value); }
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// Whether or not the submission history listbox is visible.
        /// </summary>
        public bool IsSubmissionHistoryVisible
        {
            get { return GetValue<bool>(IsSubmissionHistoryVisibleProperty); }
            set { SetValue(IsSubmissionHistoryVisibleProperty, value); }
        }

        public static readonly PropertyData IsSubmissionHistoryVisibleProperty = RegisterProperty("IsSubmissionHistoryVisible", typeof(bool), false);

        #endregion //Bindings

        #region Commands

        private string _currentNotebookPageID = string.Empty;

        /// <summary>
        /// Toggles the listbox visibility.
        /// </summary>
        public Command<RoutedEventArgs> TogglePanelCommand { get; private set; }

        private void OnTogglePanelCommandExecute(RoutedEventArgs e)
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var notebookPagesPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            var toggleButton = e.Source as ToggleButton;
            if(toggleButton == null)
            {
                return;
            }
            if((toggleButton.IsChecked != null && !(bool)toggleButton.IsChecked) || currentPage == null || notebookPagesPanel == null)
            {
                IsSubmissionHistoryVisible = false;
                return;
            }

            if(currentPage.ID != _currentNotebookPageID)
            {
                _currentNotebookPageID = currentPage.ID;
                // TODO: Entities
               // SubmissionPages = notebookPagesPanel.Notebook.Submissions[_currentNotebookPageID];
            }

            IsSubmissionHistoryVisible = true;
        }       

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);
            }
        }

        #endregion //Commands
    }
}
