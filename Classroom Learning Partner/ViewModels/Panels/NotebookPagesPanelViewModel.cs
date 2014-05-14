using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Path = Catel.IO.Path;
using Catel.MVVM;
using CLP.Entities;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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
            Initialized += NotebookPagesPanelViewModel_Initialized;
            
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ShowSubmissionsCommand = new Command<CLPPage>(OnShowSubmissionsCommandExecute);

            StagingPanel = new SubmissionsPanelViewModel(notebook)
                           {
                               IsVisible = false
                           };
        }

        void NotebookPagesPanelViewModel_Initialized(object sender, System.EventArgs e)
        {
            Length = InitialLength;
        }

        public override string Title
        {
            get { return "NotebookPagesPanelVM"; }
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
        /// Current, selected page in the notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

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
        /// Staging Panel for submissions
        /// </summary>
        public IPanel StagingPanel
        {
            get { return GetValue<IPanel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(IPanel)); 

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
            var submissionsPanel = StagingPanel as SubmissionsPanelViewModel;
            if(submissionsPanel == null)
            {
                return;
            }

            submissionsPanel.IsVisible = true;

            submissionsPanel.SubmissionPages = page.Submissions;
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
                // save a thumbnail of page being navigated away from
                var pageViewModel = CLPServiceAgent.Instance.GetViewModelsFromModel(CurrentPage).First(x => (x is CLPPageViewModel) && !(x as CLPPageViewModel).IsPagePreview);
                UIElement pageView = (UIElement)CLPServiceAgent.Instance.GetViewFromViewModel(pageViewModel);
                var thumbnail = CLPServiceAgent.Instance.GetJpgImage(pageView, 1.0, 100, true);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                bitmapImage.StreamSource = new MemoryStream(thumbnail);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                CurrentPage.PageThumbnail = bitmapImage;

                // actually set current page
                CurrentPage = page;
                return;
            }

            notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);

            // TODO: Entities, History of submissions
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

            return notebookWorkspaceViewModel.CurrentDisplay == null ? notebookWorkspaceViewModel.Notebook.CurrentPage : null;
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