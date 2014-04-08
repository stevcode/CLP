using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SubmissionHistoryPanelViewModel : ViewModelBase, IPanel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelViewModel"/> class.
        /// </summary>
        public SubmissionHistoryPanelViewModel(CLPNotebook notebook)
        {
            Notebook = notebook;

            TogglePanelCommand = new Command<RoutedEventArgs>(OnTogglePanelCommandExecute);
            SetCurrentPageCommand = new Command<ICLPPage>(OnSetCurrentPageCommandExecute);
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
        public CLPNotebook Notebook
        {
            get { return GetValue<CLPNotebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(CLPNotebook));

        #region IPanel Members

        public string PanelName
        {
            get
            {
                return "SubmissionHistoryPanel";
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
        public double InitialWidth { get { return 250; } }

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocation Location
        {
            get { return GetValue<PanelLocation>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocation), PanelLocation.Bottom);

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
        /// Current, selected submission.
        /// </summary>
        public ICLPPage CurrentPage
        {
            get { return GetValue<ICLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(ICLPPage));

        /// <summary>
        /// All the submissions for the desired page.
        /// </summary>
        public ObservableCollection<ICLPPage> SubmissionPages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(SubmissionPagesProperty); }
            set { SetValue(SubmissionPagesProperty, value); }
        }

        public static readonly PropertyData SubmissionPagesProperty = RegisterProperty("SubmissionPages", typeof(ObservableCollection<ICLPPage>), () => new ObservableCollection<ICLPPage>());

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

            if(currentPage.UniqueID != _currentNotebookPageID)
            {
                _currentNotebookPageID = currentPage.UniqueID;
                SubmissionPages = notebookPagesPanel.Notebook.Submissions[_currentNotebookPageID];
            }

            IsSubmissionHistoryVisible = true;
        }       

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<ICLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(ICLPPage page)
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
