﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum NavigationPanes
    {
        Info,
        New,
        Open,
        Export,
        Options
    }

    public class BackStageViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow => App.MainWindowViewModel;

        private readonly IRoleService _roleService;

        #region Constructor

        public BackStageViewModel(IRoleService roleService)
        {
            Argument.IsNotNull(() => roleService);

            _roleService = roleService;

            InitializeEventSubscriptions();
            InitializeCommands();
        }

        #endregion // Constructor

        #region Events

        private void InitializeEventSubscriptions()
        {
            _roleService.RoleChanged += _roleService_RoleChanged;
        }

        private void _roleService_RoleChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(ResearcherOrTeacherVisibility));
        }

        #endregion // Events

        #region ViewModelBase Overrides

        protected override async Task OnClosingAsync()
        {
            _roleService.RoleChanged -= _roleService_RoleChanged;
            await base.OnClosingAsync();
        }

        #endregion // ViewModelBase Overrides

        #region Bindings

        /// <summary>Title Text for the currnet navigation pane.</summary>
        public string PaneTitleText
        {
            get { return GetValue<string>(PaneTitleTextProperty); }
            set { SetValue(PaneTitleTextProperty, value); }
        }

        public static readonly PropertyData PaneTitleTextProperty = RegisterProperty("PaneTitleText", typeof (string), string.Empty);

        /// <summary>Currently Displayed Navigation Pane.</summary>
        public NavigationPanes CurrentNavigationPane
        {
            get { return GetValue<NavigationPanes>(CurrentNavigationPaneProperty); }
            set
            {
                SetValue(CurrentNavigationPaneProperty, value);
                SetBackStagePane();
            }
        }

        public static readonly PropertyData CurrentNavigationPaneProperty = RegisterProperty("CurrentNavigationPane",
                                                                                             typeof (NavigationPanes),
                                                                                             NavigationPanes.Info);

        /// <summary>Pane currently displayed.</summary>
        public APaneBaseViewModel DisplayedPane
        {
            get { return GetValue<APaneBaseViewModel>(DisplayedPaneProperty); }
            set { SetValue(DisplayedPaneProperty, value); }
        }

        public static readonly PropertyData DisplayedPaneProperty = RegisterProperty("DisplayedPane", typeof (APaneBaseViewModel));

        public Visibility ResearcherOrTeacherVisibility => _roleService.Role == ProgramRoles.Researcher || _roleService.Role == ProgramRoles.Teacher
                                                               ? Visibility.Visible
                                                               : Visibility.Collapsed;

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            HideBackStageCommand = new Command(OnHideBackStageCommandExecute);
            MoveWindowCommand = new Command<MouseButtonEventArgs>(OnMoveWindowCommandExecute);
            ToggleMinimizeStateCommand = new Command(OnToggleMinimizeStateCommandExecute);
            ToggleMaximizeStateCommand = new Command(OnToggleMaximizeStateCommandExecute);
            ExitProgramCommand = new Command(OnExitProgramCommandExecute);
        }

        /// <summary>Hides the BackStage.</summary>
        public Command HideBackStageCommand { get; private set; }

        private void OnHideBackStageCommandExecute() { MainWindow.IsBackStageVisible = false; }

        /// <summary>
        /// Moves the CLP Window.
        /// </summary>
        public Command<MouseButtonEventArgs> MoveWindowCommand { get; private set; }

        private void OnMoveWindowCommandExecute(MouseButtonEventArgs args)
        {
            var mainWindow = App.MainWindowViewModel.GetFirstView() as MainWindowView;
            if (mainWindow == null ||
                Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            mainWindow.DragMove();
        }

        /// <summary>
        /// Toggles CLP between minimized and not.
        /// </summary>
        public Command ToggleMinimizeStateCommand { get; private set; }

        private void OnToggleMinimizeStateCommandExecute()
        {
            var mainWindow = App.MainWindowViewModel.GetFirstView() as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }

            mainWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Toggles CLP Window State between Maximized and Normal.
        /// </summary>
        public Command ToggleMaximizeStateCommand { get; private set; }

        private void OnToggleMaximizeStateCommandExecute()
        {
            var mainWindow = App.MainWindowViewModel.GetFirstView() as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }

            App.MainWindowViewModel.IsDragBarVisible = mainWindow.WindowState == WindowState.Maximized;
            mainWindow.WindowState = mainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// Closes CLP
        /// </summary>
        public Command ExitProgramCommand { get; private set; }

        private void OnExitProgramCommandExecute()
        {
            var mainWindow = App.MainWindowViewModel.GetFirstView() as MainWindowView;
            mainWindow?.Close();
        }

        #endregion //Commands

        #region Methods

        private void SetBackStagePane()
        {
            var dependencyResolver = this.GetDependencyResolver();
            var viewModelFactory = dependencyResolver.Resolve<IViewModelFactory>();

            switch (CurrentNavigationPane)
            {
                case NavigationPanes.Info:
                    DisplayedPane = viewModelFactory.CreateViewModel<NotebookInfoPaneViewModel>(null, null);
                    break;
                case NavigationPanes.New:
                    DisplayedPane = viewModelFactory.CreateViewModel<NewPaneViewModel>(null, null);
                    break;
                case NavigationPanes.Open:
                    DisplayedPane = viewModelFactory.CreateViewModel<OpenNotebookPaneViewModel>(null, null);
                    break;
                case NavigationPanes.Export:
                    DisplayedPane = viewModelFactory.CreateViewModel<ExportPaneViewModel>(null, null);
                    break;
                case NavigationPanes.Options:
                    DisplayedPane = viewModelFactory.CreateViewModel<OptionsPaneViewModel>(null, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            PaneTitleText = DisplayedPane.PaneTitleText;
        }

        #endregion //Methods
    }
}