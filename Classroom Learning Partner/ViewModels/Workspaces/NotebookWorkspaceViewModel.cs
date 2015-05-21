using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Catel.Data;
using Catel.IO;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;
using Brush = System.Windows.Media.Brush;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [InterestedIn(typeof(RibbonViewModel))]
    [InterestedIn(typeof(MajorRibbonViewModel))]
    [InterestedIn(typeof(MainWindowViewModel))]
    public class NotebookWorkspaceViewModel : ViewModelBase
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel(Notebook notebook)
        {
            Notebook = notebook;
            ContextRibbon = new ContextRibbonViewModel();
            AnimationControlRibbon = new AnimationControlRibbonViewModel(notebook);

            //App.CurrentNotebookCacheDirectory = Path.Combine(App.NotebookCacheDirectory, Notebook.Name + ";" + Notebook.ID + ";" + Notebook.Owner.FullName + ";" + Notebook.OwnerID);

            PreviousPageCommand = new Command(OnPreviousPageCommandExecute, OnPreviousPageCanExecute);
            NextPageCommand = new Command(OnNextPageCommandExecute, OnNextPageCanExecute);
            GoToPageCommand = new Command(OnGoToPageCommandExecute);

            InitializePanels(notebook);

            // TODO: Convert this to string, see DisplaysPanelViewModel to pull from CLPBrushes.xaml
            WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
        }

        private void InitializePanels(Notebook notebook)
        {
            SingleDisplay = new SingleDisplayViewModel(notebook);

            StagingPanel = new StagingPanelViewModel(notebook)
            {
                IsVisible = false
            };
            NotebookPagesPanel = new NotebookPagesPanelViewModel(notebook, StagingPanel);
            ProgressPanel = new ProgressPanelViewModel(notebook, StagingPanel);
            if (App.MainWindowViewModel.MajorRibbon.CurrentLeftPanel == Panels.Progress)
            {
                LeftPanel = ProgressPanel;
            }
            else
            {
                LeftPanel = NotebookPagesPanel;
            }

            DisplaysPanel = new DisplaysPanelViewModel(notebook);
            PageInformationPanel = new PageInformationPanelViewModel(notebook);
            RightPanel = DisplaysPanel;

            // TODO: Use StagingPanel instead?
            //if(App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Student)
            //{
            //    SubmissionHistoryPanel = new SubmissionHistoryPanelViewModel(notebook);
            //    BottomPanel = SubmissionHistoryPanel;
            //}

            if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Projector)
            {
                NotebookPagesPanel.IsVisible = false;
            }
        }

        public override string Title { get { return "NotebookWorkspaceVM"; } }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Model
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// A property mapped to a property on the Model Notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Color of Workspace Background.
        /// </summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        /// <summary>
        /// The Context Ribbon.
        /// </summary>
        public ContextRibbonViewModel ContextRibbon
        {
            get { return GetValue<ContextRibbonViewModel>(ContextRibbonProperty); }
            set { SetValue(ContextRibbonProperty, value); }
        }

        public static readonly PropertyData ContextRibbonProperty = RegisterProperty("ContextRibbon", typeof(ContextRibbonViewModel));

        /// <summary>
        /// The Animation Control Ribbon.
        /// </summary>
        public AnimationControlRibbonViewModel AnimationControlRibbon
        {
            get { return GetValue<AnimationControlRibbonViewModel>(AnimationControlRibbonProperty); }
            set { SetValue(AnimationControlRibbonProperty, value); }
        }

        public static readonly PropertyData AnimationControlRibbonProperty = RegisterProperty("AnimationControlRibbon", typeof(AnimationControlRibbonViewModel));

        #region Displays

        /// <summary>
        /// The Single Display.
        /// </summary>
        public SingleDisplayViewModel SingleDisplay
        {
            get { return GetValue<SingleDisplayViewModel>(SingleDisplayProperty); }
            set { SetValue(SingleDisplayProperty, value); }
        }

        public static readonly PropertyData SingleDisplayProperty = RegisterProperty("SingleDisplay", typeof(SingleDisplayViewModel));

        /// <summary>
        /// The Currently Selected Display.
        /// </summary>
        public IDisplay CurrentDisplay
        {
            get { return GetValue<IDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(IDisplay));

        #endregion //Displays

        #region Panels

        /// <summary>
        /// Right side Panel.
        /// </summary>
        public IPanel RightPanel
        {
            get { return GetValue<IPanel>(RightPanelProperty); }
            set { SetValue(RightPanelProperty, value); }
        }

        public static readonly PropertyData RightPanelProperty = RegisterProperty("RightPanel", typeof(IPanel));

        /// <summary>
        /// Left side Panel.
        /// </summary>
        public IPanel LeftPanel
        {
            get { return GetValue<IPanel>(LeftPanelProperty); }
            set { SetValue(LeftPanelProperty, value); }
        }

        public static readonly PropertyData LeftPanelProperty = RegisterProperty("LeftPanel", typeof(IPanel));

        /// <summary>
        /// Bottom Panel.
        /// </summary>
        public IPanel BottomPanel
        {
            get { return GetValue<IPanel>(BottomPanelProperty); }
            set { SetValue(BottomPanelProperty, value); }
        }

        public static readonly PropertyData BottomPanelProperty = RegisterProperty("BottomPanel", typeof(IPanel));

        /// <summary>
        /// NotebookPagesPanel.
        /// </summary>
        public NotebookPagesPanelViewModel NotebookPagesPanel
        {
            get { return GetValue<NotebookPagesPanelViewModel>(NotebookPagesPanelProperty); }
            set { SetValue(NotebookPagesPanelProperty, value); }
        }

        public static readonly PropertyData NotebookPagesPanelProperty = RegisterProperty("NotebookPagesPanel", typeof(NotebookPagesPanelViewModel));

        /// <summary>
        /// ProgressPanel.
        /// </summary>
        public ProgressPanelViewModel ProgressPanel
        {
            get { return GetValue<ProgressPanelViewModel>(ProgressPanelProperty); }
            set { SetValue(ProgressPanelProperty, value); }
        }

        public static readonly PropertyData ProgressPanelProperty = RegisterProperty("ProgressPanel", typeof(ProgressPanelViewModel));

        /// <summary>
        /// DisplaysPanel.
        /// </summary>
        public DisplaysPanelViewModel DisplaysPanel
        {
            get { return GetValue<DisplaysPanelViewModel>(DisplaysPanelProperty); }
            set { SetValue(DisplaysPanelProperty, value); }
        }

        public static readonly PropertyData DisplaysPanelProperty = RegisterProperty("DisplaysPanel", typeof(DisplaysPanelViewModel));

        /// <summary>
        /// PageInformationPanel.
        /// </summary>
        public PageInformationPanelViewModel PageInformationPanel
        {
            get { return GetValue<PageInformationPanelViewModel>(PageInformationPanelProperty); }
            set { SetValue(PageInformationPanelProperty, value); }
        }

        public static readonly PropertyData PageInformationPanelProperty = RegisterProperty("PageInformationPanel", typeof(PageInformationPanelViewModel));

        /// <summary>
        /// SubmissionHistoryPanel.
        /// </summary>
        // TODO: Replace with StagingPanel?
        public SubmissionHistoryPanelViewModel SubmissionHistoryPanel
        {
            get { return GetValue<SubmissionHistoryPanelViewModel>(SubmissionHistoryPanelProperty); }
            set { SetValue(SubmissionHistoryPanelProperty, value); }
        }

        public static readonly PropertyData SubmissionHistoryPanelProperty = RegisterProperty("SubmissionHistoryPanel", typeof(SubmissionHistoryPanelViewModel));

        /// <summary>
        /// Staging Panel for submissions
        /// </summary>
        public StagingPanelViewModel StagingPanel
        {
            get { return GetValue<StagingPanelViewModel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(StagingPanelViewModel));

        #endregion //Panels

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Navigates to previous page in the notebook.
        /// </summary>
        public Command PreviousPageCommand { get; private set; }

        private void OnPreviousPageCommandExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if (panel == null || currentPage == null)
            {
                return;
            }

            var index = Notebook.Pages.IndexOf(currentPage);

            if (index > 0)
            {
                var page = panel.Notebook.Pages[index - 1];
                panel.CurrentPage = page;
            }
        }

        private bool OnPreviousPageCanExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if (panel == null || currentPage == null)
            {
                return false;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);
            return index > 0;
        }

        /// <summary>
        /// Navigates to the next page in the notebook.
        /// </summary>
        public Command NextPageCommand { get; private set; }

        private void OnNextPageCommandExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if (panel == null || currentPage == null)
            {
                return;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);
            if (index < panel.Notebook.Pages.Count - 1)
            {
                var page = panel.Notebook.Pages[index + 1];
                panel.CurrentPage = page;
            }
        }

        private bool OnNextPageCanExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if (panel == null || currentPage == null)
            {
                return false;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);
            return index < panel.Notebook.Pages.Count - 1;
        }

        /// <summary>
        /// Launched keypad to allow a jump to a specific page.
        /// </summary>
        public Command GoToPageCommand { get; private set; }

        private void OnGoToPageCommandExecute()
        {
            if (Notebook == null)
            {
                MessageBox.Show("No notebook loaded.");
                return;
            }

            var keyPad = new KeypadWindowView("Go To Page", 999)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var newPageIndex = Int32.Parse(keyPad.NumbersEntered.Text);

            var newPage = Notebook.Pages.FirstOrDefault(x => x.PageNumber == newPageIndex);

            if (newPage == null)
            {
                MessageBox.Show("No page with that page number loaded.");
                return;
            }

            Notebook.CurrentPage = newPage;
        }

        #endregion //Commands

        #region Methods

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (viewModel == null)
            {
                return;
            }

            if (viewModel is MajorRibbonViewModel)
            {
                var ribbon = viewModel as MajorRibbonViewModel;

                if (propertyName == "CurrentLeftPanel")
                {
                    switch (ribbon.CurrentLeftPanel)
                    {
                        case Panels.NotebookPages:
                            LeftPanel = NotebookPagesPanel;
                            LeftPanel.IsVisible = true;
                            break;
                        case Panels.Progress:
                            LeftPanel = ProgressPanel;
                            LeftPanel.IsVisible = true;
                            break;
                        default:
                            LeftPanel.IsVisible = false;
                            break;
                    }
                }

                if (propertyName == "CurrentRightPanel")
                {
                    switch (ribbon.CurrentRightPanel)
                    {
                        case Panels.Displays:
                            RightPanel = DisplaysPanel;
                            RightPanel.IsVisible = true;
                            break;
                        case Panels.PageInformation:
                            RightPanel = PageInformationPanel;
                            RightPanel.IsVisible = true;
                            break;
                        default:
                            RightPanel.IsVisible = false;
                            break;
                    }
                }
            }

            if (viewModel is MainWindowViewModel)
            {
                var mainWindow = viewModel as MainWindowViewModel;
                if (propertyName == "IsAuthoring")
                {
                    CurrentDisplay = null;
                    if (mainWindow.IsAuthoring)
                    {
                        WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
                    }
                    else
                    {
                        WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
                    }
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }

        #endregion //Methods

        #region Static Methods

        public static ContextRibbonViewModel GetContextRibbon()
        {
            if (App.MainWindowViewModel == null)
            {
                return null;
            }

            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.ContextRibbon;
        }

        public static AnimationControlRibbonViewModel GetAnimationControlRibbon()
        {
            if (App.MainWindowViewModel == null)
            {
                return null;
            }

            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            return notebookWorkspaceViewModel == null ? null : notebookWorkspaceViewModel.AnimationControlRibbon;
        }

        #endregion //Static Methods
    }
}