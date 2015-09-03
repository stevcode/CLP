﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookPagesPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelViewModel" /> class.
        /// </summary>
        public NotebookPagesPanelViewModel(Notebook notebook, StagingPanelViewModel stagingPanel)
        {
            Notebook = notebook;
            Initialized += NotebookPagesPanelViewModel_Initialized;
            
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ShowSubmissionsCommand = new Command<CLPPage>(OnShowSubmissionsCommandExecute);
            AddPageToStageCommand = new Command<CLPPage>(OnAddPageToStageCommandExecute);
            
            StagingPanel = stagingPanel;
            SubmissionHistoryPanel = new SubmissionHistoryPanelViewModel(notebook);
        }

        void NotebookPagesPanelViewModel_Initialized(object sender, EventArgs e)
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
        public StagingPanelViewModel StagingPanel
        {
            get { return GetValue<StagingPanelViewModel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(StagingPanelViewModel));

        /// <summary>
        /// Submissions History Panel for the submissions of a student's page.
        /// </summary>
        public SubmissionHistoryPanelViewModel SubmissionHistoryPanel
        {
            get { return GetValue<SubmissionHistoryPanelViewModel>(SubmissionHistoryPanelProperty); }
            set { SetValue(SubmissionHistoryPanelProperty, value); }
        }

        public static readonly PropertyData SubmissionHistoryPanelProperty = RegisterProperty("SubmissionHistoryPanel", typeof(SubmissionHistoryPanelViewModel));

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
            StagingPanel.IsVisible = true;
            StagingPanel.SetSubmissionsForPage(page);
            StagingPanel.LastFilteredPage = page;
            StagingPanel.StudentsWithNoSubmissions = StagingPanel.GetStudentsWithNoSubmissions();
        }

        /// <summary>
        /// Adds individual page to the Staging Panel
        /// </summary>
        public Command<CLPPage> AddPageToStageCommand { get; private set; }

        private void OnAddPageToStageCommandExecute(CLPPage page)
        {
            AdvancedStagingView dialog = new AdvancedStagingView();
            dialog.ShowDialog();
            if(dialog.DialogResult == false)
            {
                return;
            }
            switch(dialog.StagingType)
            {
                case AdvancedStagingView.StagingTypes.Starred:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendStarredSubmissionsForPage(page);
                    break;
                case AdvancedStagingView.StagingTypes.Correct:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendCollectionOfPagesToStage(page.Submissions,
                        x => x.Tags.FirstOrDefault(t => t is CorrectnessTag && (t as CorrectnessTag).Correctness == Correctness.Correct) != null);

                    //TODO: keep CurrentSort and skip this if already sorted that way.
                    StagingPanel.ApplySortAndGroupByName();
                    break;
                case AdvancedStagingView.StagingTypes.Incorrect:
                    StagingPanel.IsVisible = true;
                    StagingPanel.AppendCollectionOfPagesToStage(page.Submissions,
                        x => x.Tags.FirstOrDefault(t => t is CorrectnessTag && (t as CorrectnessTag).Correctness == Correctness.Incorrect) != null);

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

        #endregion //Methods

        #region Static Methods

        public static CLPPage GetCurrentPage()
        {
            if (App.MainWindowViewModel == null)
            {
                return null;
            }

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