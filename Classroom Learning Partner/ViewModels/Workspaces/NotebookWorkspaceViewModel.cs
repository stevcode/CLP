using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookWorkspaceViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IWindowManagerService _windowManagerService;

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="NotebookWorkspaceViewModel" /> class.</summary>
        public NotebookWorkspaceViewModel(IDataService dataService, IWindowManagerService windowManagerService)
        {
            _dataService = dataService;
            _windowManagerService = windowManagerService;

            Notebook = _dataService.CurrentNotebook;

            ContextRibbon = this.CreateViewModel<ContextRibbonViewModel>(null);
            AnimationControlRibbon = this.CreateViewModel<AnimationControlRibbonViewModel>(_dataService.CurrentNotebook);

            InitializeCommands();

            InitializePanels(Notebook);

            if (Notebook.Owner.ID == Person.AUTHOR_ID)
            {
                WorkspaceBackgroundColor = new SolidColorBrush(Colors.Salmon);
            }
            else
            {
                // TODO: Convert this to string, see DisplaysPanelViewModel to pull from CLPBrushes.xaml
                WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
            }

            InitializedAsync += NotebookWorkspaceViewModel_InitializedAsync;
            ClosedAsync += NotebookWorkspaceViewModel_ClosedAsync;
        }

        private void InitializePanels(Notebook notebook)
        {
            var dependencyResolver = this.GetDependencyResolver();
            var viewModelFactory = dependencyResolver.Resolve<IViewModelFactory>();

            SingleDisplay = viewModelFactory.CreateViewModel<SingleDisplayViewModel>(notebook);

            StagingPanel = viewModelFactory.CreateViewModel<StagingPanelViewModel>(notebook);
            StagingPanel.IsVisible = false;

            NotebookPagesPanel = viewModelFactory.CreateViewModel<NotebookPagesPanelViewModel>(StagingPanel);
            ProgressPanel = viewModelFactory.CreateViewModel<ProgressPanelViewModel>(StagingPanel);
            if (App.MainWindowViewModel.MajorRibbon.CurrentLeftPanel == Panels.Progress)
            {
                LeftPanel = ProgressPanel;
            }
            else
            {
                LeftPanel = NotebookPagesPanel;
            }

            DisplaysPanel = viewModelFactory.CreateViewModel<DisplaysPanelViewModel>(null);
            PageInformationPanel = viewModelFactory.CreateViewModel<PageInformationPanelViewModel>(notebook);
            PageInformationPanel.IsVisible = _windowManagerService.IsPageInformationPanelVisible;

            RightPanel = DisplaysPanel;

            if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Projector)
            {
                NotebookPagesPanel.IsVisible = false;
            }
        }

        #endregion //Constructor

        #region Events

        private Task NotebookWorkspaceViewModel_InitializedAsync(object sender, EventArgs e)
        {
            _dataService.CurrentDisplayChanged += _dataService_CurrentDisplayChanged;
            _windowManagerService.PageInformationPanelVisibleChanged += _windowManagerService_PageInformationPanelVisibleChanged;

            return TaskHelper.Completed;
        }

        private Task NotebookWorkspaceViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentDisplayChanged -= _dataService_CurrentDisplayChanged;
            _windowManagerService.PageInformationPanelVisibleChanged -= _windowManagerService_PageInformationPanelVisibleChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentDisplayChanged(object sender, EventArgs e)
        {
            CurrentDisplay = _dataService.CurrentDisplay;
        }

        private void _windowManagerService_PageInformationPanelVisibleChanged(object sender, EventArgs e)
        {
            if (PageInformationPanel == null)
            {
                return;
            }

            if (RightPanel != PageInformationPanel)
            {
                RightPanel = PageInformationPanel;
            }

            PageInformationPanel.IsVisible = _windowManagerService.IsPageInformationPanelVisible;
        }

        #endregion // Events

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>Auto-Mapped property of the Notebook Model.</summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        /// <summary>Auto-Mapped property of the Notebook Model.</summary>
        [ViewModelToModel("Notebook")]
        public IDisplay CurrentDisplay
        {
            get { return GetValue<IDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(IDisplay));

        #endregion // Model

        #region Bindings

        /// <summary>Color of Workspace Background.</summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        #region Ribbons

        /// <summary>The Context Ribbon.</summary>
        public ContextRibbonViewModel ContextRibbon
        {
            get { return GetValue<ContextRibbonViewModel>(ContextRibbonProperty); }
            set { SetValue(ContextRibbonProperty, value); }
        }

        public static readonly PropertyData ContextRibbonProperty = RegisterProperty("ContextRibbon", typeof(ContextRibbonViewModel));

        /// <summary>The Animation Control Ribbon.</summary>
        public AnimationControlRibbonViewModel AnimationControlRibbon
        {
            get { return GetValue<AnimationControlRibbonViewModel>(AnimationControlRibbonProperty); }
            set { SetValue(AnimationControlRibbonProperty, value); }
        }

        public static readonly PropertyData AnimationControlRibbonProperty = RegisterProperty("AnimationControlRibbon", typeof(AnimationControlRibbonViewModel));

        #endregion // Ribbons

        #region Displays

        /// <summary>The Single Display.</summary>
        public SingleDisplayViewModel SingleDisplay
        {
            get { return GetValue<SingleDisplayViewModel>(SingleDisplayProperty); }
            set { SetValue(SingleDisplayProperty, value); }
        }

        public static readonly PropertyData SingleDisplayProperty = RegisterProperty("SingleDisplay", typeof(SingleDisplayViewModel));

        #endregion //Displays

        #region Panels

        /// <summary>Right side Panel.</summary>
        public IPanel RightPanel
        {
            get { return GetValue<IPanel>(RightPanelProperty); }
            set { SetValue(RightPanelProperty, value); }
        }

        public static readonly PropertyData RightPanelProperty = RegisterProperty("RightPanel", typeof(IPanel));

        /// <summary>Left side Panel.</summary>
        public IPanel LeftPanel
        {
            get { return GetValue<IPanel>(LeftPanelProperty); }
            set { SetValue(LeftPanelProperty, value); }
        }

        public static readonly PropertyData LeftPanelProperty = RegisterProperty("LeftPanel", typeof(IPanel));

        /// <summary>NotebookPagesPanel.</summary>
        public NotebookPagesPanelViewModel NotebookPagesPanel
        {
            get { return GetValue<NotebookPagesPanelViewModel>(NotebookPagesPanelProperty); }
            set { SetValue(NotebookPagesPanelProperty, value); }
        }

        public static readonly PropertyData NotebookPagesPanelProperty = RegisterProperty("NotebookPagesPanel", typeof(NotebookPagesPanelViewModel));

        /// <summary>ProgressPanel.</summary>
        public ProgressPanelViewModel ProgressPanel
        {
            get { return GetValue<ProgressPanelViewModel>(ProgressPanelProperty); }
            set { SetValue(ProgressPanelProperty, value); }
        }

        public static readonly PropertyData ProgressPanelProperty = RegisterProperty("ProgressPanel", typeof(ProgressPanelViewModel));

        /// <summary>DisplaysPanel.</summary>
        public DisplaysPanelViewModel DisplaysPanel
        {
            get { return GetValue<DisplaysPanelViewModel>(DisplaysPanelProperty); }
            set { SetValue(DisplaysPanelProperty, value); }
        }

        public static readonly PropertyData DisplaysPanelProperty = RegisterProperty("DisplaysPanel", typeof(DisplaysPanelViewModel));

        /// <summary>PageInformationPanel.</summary>
        public PageInformationPanelViewModel PageInformationPanel
        {
            get { return GetValue<PageInformationPanelViewModel>(PageInformationPanelProperty); }
            set { SetValue(PageInformationPanelProperty, value); }
        }

        public static readonly PropertyData PageInformationPanelProperty = RegisterProperty("PageInformationPanel", typeof(PageInformationPanelViewModel));

        /// <summary>SubmissionHistoryPanel.</summary>
        // TODO: Replace with StagingPanel?
        public SubmissionHistoryPanelViewModel SubmissionHistoryPanel
        {
            get { return GetValue<SubmissionHistoryPanelViewModel>(SubmissionHistoryPanelProperty); }
            set { SetValue(SubmissionHistoryPanelProperty, value); }
        }

        public static readonly PropertyData SubmissionHistoryPanelProperty = RegisterProperty("SubmissionHistoryPanel", typeof(SubmissionHistoryPanelViewModel));

        /// <summary>Staging Panel for submissions</summary>
        public StagingPanelViewModel StagingPanel
        {
            get { return GetValue<StagingPanelViewModel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(StagingPanelViewModel));

        #endregion //Panels

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            ResetDemoCommand = new Command(OnResetDemoCommandExecute);
            PreviousPageCommand = new Command(OnPreviousPageCommandExecute, OnPreviousPageCanExecute);
            NextPageCommand = new Command(OnNextPageCommandExecute, OnNextPageCanExecute);
            GoToPageCommand = new Command(OnGoToPageCommandExecute, OnGoToPageCanExecute);
        }

        public readonly List<CLPPage> PagesAddedThisSession = new List<CLPPage>();

        /// <summary>Resets Notebook Pages to initial Demo state.</summary>
        public Command ResetDemoCommand { get; private set; }

        private void OnResetDemoCommandExecute()
        {
            //if (MessageBox.Show("Are you sure you want to completely reset the notebook?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            //{
            //    return;
            //}

            //foreach (var page in PagesAddedThisSession)
            //{
            //    if (Notebook.Pages.Contains(page))
            //    {
            //        Notebook.Pages.Remove(page);
            //    }
            //}

            //PagesAddedThisSession.Clear();

            //foreach (var page in Notebook.Pages)
            //{
            //    var pageObjectsToDelete = page.PageObjects.Where(p => p.CreatorID == App.MainWindowViewModel.CurrentUser.ID).ToList();
            //    foreach (var pageObject in pageObjectsToDelete)
            //    {
            //        page.PageObjects.Remove(pageObject);
            //    }

            //    var strokesToDelete = page.InkStrokes.Where(s => s.GetStrokeOwnerID() == App.MainWindowViewModel.CurrentUser.ID).ToList();
            //    foreach (var stroke in strokesToDelete)
            //    {
            //        if (page.InkStrokes.Contains(stroke))
            //        {
            //            page.InkStrokes.Remove(stroke);
            //        }
            //    }

            //    page.History.ClearHistory();
            //    page.History.SemanticEvents.Clear();

            //    var existingTags = page.Tags.Where(t => t.Category != Category.Definition).ToList();
            //    foreach (var tempArraySkipCountingTag in existingTags)
            //    {
            //        page.RemoveTag(tempArraySkipCountingTag);
            //    }
            //}

            //ACLPPageBaseViewModel.ClearAdorners(Notebook.CurrentPage);
            //var newPage = Notebook.Pages.FirstOrDefault();
            //if (newPage == null)
            //{
            //    return;
            //}

            //_dataService.SetCurrentPage(newPage);
        }

        /// <summary>Navigates to previous page in the notebook.</summary>
        public Command PreviousPageCommand { get; private set; }

        private void OnPreviousPageCommandExecute()
        {
            // TODO: Take into account Teacher build when in Staging Panel.
            if (Notebook == null ||
                CurrentPage == null)
            {
                return;
            }

            var index = Notebook.Pages.IndexOf(CurrentPage);
            if (index <= 0)
            {
                return;
            }

            ACLPPageBaseViewModel.ClearAdorners(CurrentPage);
            var page = Notebook.Pages[index - 1];
            _dataService.AddPageToCurrentDisplay(page);
        }

        private bool OnPreviousPageCanExecute()
        {
            if (Notebook == null ||
                CurrentPage == null)
            {
                return false;
            }

            var index = Notebook.Pages.IndexOf(CurrentPage);
            return index > 0;
        }

        /// <summary>Navigates to the next page in the notebook.</summary>
        public Command NextPageCommand { get; private set; }

        private void OnNextPageCommandExecute()
        {
            // TODO: Take into account Teacher build when in Staging Panel.
            if (Notebook == null ||
                CurrentPage == null)
            {
                return;
            }

            var index = Notebook.Pages.IndexOf(CurrentPage);
            if (index >= Notebook.Pages.Count - 1)
            {
                return;
            }

            ACLPPageBaseViewModel.ClearAdorners(CurrentPage);
            var page = Notebook.Pages[index + 1];
            _dataService.AddPageToCurrentDisplay(page);
        }

        private bool OnNextPageCanExecute()
        {
            if (Notebook == null ||
                CurrentPage == null)
            {
                return false;
            }

            var index = Notebook.Pages.IndexOf(CurrentPage);
            return index < Notebook.Pages.Count - 1;
        }

        /// <summary>Launched keypad to allow a jump to a specific page.</summary>
        public Command GoToPageCommand { get; private set; }

        private void OnGoToPageCommandExecute()
        {
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

            var newPageIndex = int.Parse(keyPad.NumbersEntered.Text);

            var newPage = Notebook.Pages.FirstOrDefault(x => x.PageNumber == newPageIndex);
            if (newPage == null)
            {
                MessageBox.Show("No page with that page number loaded.");
                return;
            }

            _dataService.AddPageToCurrentDisplay(newPage);
        }

        private bool OnGoToPageCanExecute()
        {
            if (Notebook == null)
            {
                return false;
            }

            return Notebook.Pages.Count > 1;
        }

        #endregion //Commands

        #region Methods

        // HACK: Needs a WindowManager Service. From InterestedIn attribute following MajorRibbonViewModel and MainWindowViewModel
        protected void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
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