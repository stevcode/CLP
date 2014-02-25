using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookPagesPanelViewModel : ViewModelBase, IPanel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelViewModel"/> class.
        /// </summary>
        public NotebookPagesPanelViewModel(CLPNotebook notebook)
        {
            Notebook = notebook;
            CurrentPage = notebook.MirrorDisplay.CurrentPage;
            LinkedPanel = new SubmissionsPanelViewModel(notebook);
            PanelWidth = InitialWidth;

            SetCurrentPageCommand = new Command<ICLPPage>(OnSetCurrentPageCommandExecute);
            ShowSubmissionsCommand = new Command<ICLPPage>(OnShowSubmissionsCommandExecute);
            PanelResizeDragCommand = new Command<DragDeltaEventArgs>(OnPanelResizeDragCommandExecute);
            IsVisible = true;
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "NotebookPagesPanelVM"; } }

        #region Model

        /// <summary>
        /// Notebook associated with the panel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        /// <summary>
        /// Pages of the Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<ICLPPage> Pages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<ICLPPage>));

        #endregion //Model

        #region IPanel Members

        public string PanelName
        {
            get
            {
                return "NotebookPagesPanel";
            }
        }

        /// <summary>
        /// Whether the Panel is pinned to the same Z-Index as the Workspace.
        /// </summary>
        public bool IsPinned
        {
            get { return GetValue<bool>(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        public static readonly PropertyData IsPinnedProperty = RegisterProperty("IsPinned", typeof(bool), true);

        /// <summary>
        /// Visibility of Panel, True for Visible, False for Collapsed.
        /// </summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof(bool), true);

        /// <summary>
        /// Can the Panel be resized.
        /// </summary>
        public bool IsResizable
        {
            get { return GetValue<bool>(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public static readonly PropertyData IsResizableProperty = RegisterProperty("IsResizable", typeof(bool), true);

        /// <summary>
        /// Initial Width of the Panel, before any resizing.
        /// </summary>
        public double InitialWidth
        {
            get { return 250; }
        }

        public double PanelWidth
        {
            get { return GetValue<double>(PanelWidthProperty); }
            set { SetValue(PanelWidthProperty, value); }
        }

        public static readonly PropertyData PanelWidthProperty = RegisterProperty("PanelWidth", typeof(double), 250);

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocation Location
        {
            get { return GetValue<PanelLocation>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocation), PanelLocation.Left);

        /// <summary>
        /// A Linked IPanel if more than one IPanel is to be used in the same Location.
        /// </summary>
        public IPanel LinkedPanel
        {
            get { return GetValue<IPanel>(LinkedPanelProperty); }
            set { SetValue(LinkedPanelProperty, value); }
        }

        public static readonly PropertyData LinkedPanelProperty = RegisterProperty("LinkedPanel", typeof(IPanel));

        #endregion

        #region Bindings

        /// <summary>
        /// Current, selected page in the notebook.
        /// </summary>
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage));

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Resizes the panel.
        /// </summary>
        public Command<DragDeltaEventArgs> PanelResizeDragCommand { get; private set; }

        private void OnPanelResizeDragCommandExecute(DragDeltaEventArgs e)
        {
            var newWidth = PanelWidth + e.HorizontalChange;
            if(newWidth < 50) { newWidth = 50; }
            if(newWidth > Application.Current.MainWindow.ActualWidth - 100) { newWidth = Application.Current.MainWindow.ActualWidth - 100; }
            PanelWidth = newWidth;
        }

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<ICLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(ICLPPage page)
        {
            SetCurrentPage(page);
        }

        /// <summary>
        /// Shows the submissions for the selected page.
        /// </summary>
        public Command<ICLPPage> ShowSubmissionsCommand { get; private set; }

        private void OnShowSubmissionsCommandExecute(ICLPPage page)
        {
            var submissionsPanel = LinkedPanel as SubmissionsPanelViewModel;
            if(submissionsPanel == null)
            {
                return;
            }

            submissionsPanel.IsVisible = true;

            if(_currentDisplaySubmissionsPageID == page.UniqueID)
            {
                return;
            }

            _currentDisplaySubmissionsPageID = page.UniqueID;
            submissionsPanel.SubmissionPages = Notebook.Submissions[_currentDisplaySubmissionsPageID];
        }

        private string _currentDisplaySubmissionsPageID;

        #endregion //Commands

        #region Methods

        public void SetCurrentPage(ICLPPage page)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.SelectedDisplay.AddPageToDisplay(page);
            }

            var submissionsPanel = LinkedPanel as SubmissionsPanelViewModel;
            if(submissionsPanel != null)
            {
                submissionsPanel.CurrentPage = null;
            }

            var historyPanel = GetSubmissionHistoryPanelViewModel();
            if(historyPanel != null)
            {
                historyPanel.CurrentPage = null;
                historyPanel.IsSubmissionHistoryVisible = false;
            }
        }

        #endregion //Methods

        #region Static Methods

        public static ICLPPage GetCurrentPage()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return null;
            }
            var mirrorDisplay = notebookWorkspaceViewModel.SelectedDisplay as CLPMirrorDisplay;
            return mirrorDisplay == null ? null : mirrorDisplay.CurrentPage;
        }

        public static NotebookPagesPanelViewModel GetNotebookPagesPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.NotebookPagesPanel;
        }

        public static SubmissionsPanelViewModel GetSubmissionsPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.NotebookPagesPanel.LinkedPanel as SubmissionsPanelViewModel;
        }

        public static SubmissionHistoryPanelViewModel GetSubmissionHistoryPanelViewModel()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.SubmissionHistoryPanel;
        }

        #endregion //Methods
    }
}
