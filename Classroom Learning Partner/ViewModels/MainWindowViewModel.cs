using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum ProgramModes
    {
        Teacher,
        Student,
        Projector,
        Database
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        #region Constructor

        /// <summary>Initializes a new instance of the MainWindowViewModel class.</summary>
        public MainWindowViewModel(ProgramModes currentProgramMode)
        {
            _dataService = ServiceLocator.Default.ResolveType<IDataService>();
            CurrentProgramMode = currentProgramMode;

            MajorRibbon = this.CreateViewModel<MajorRibbonViewModel>(null);
            BackStage = this.CreateViewModel<BackStageViewModel>(null);
            Workspace = this.CreateViewModel<BlankWorkspaceViewModel>(null);

            InitializeCommands();

            CurrentUser = Person.Guest;
            IsProjectorFrozen = CurrentProgramMode != ProgramModes.Projector;
            InitializedAsync += MainWindowViewModel_InitializedAsync;
            ClosedAsync += MainWindowViewModel_ClosedAsync;
        }

        #endregion //Constructor

        #region Events

        private Task MainWindowViewModel_InitializedAsync(object sender, EventArgs e)
        {
            _dataService.CurrentNotebookChanged += _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private Task MainWindowViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentNotebookChanged -= _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentNotebookChanged(object sender, EventArgs e)
        {
            Workspace = this.CreateViewModel<BlankWorkspaceViewModel>(null);
            Workspace = this.CreateViewModel<NotebookWorkspaceViewModel>(null);
            CurrentNotebookName = _dataService.CurrentNotebook.Name;
            CurrentUser = _dataService.CurrentNotebook.Owner;
            IsAuthoring = _dataService.CurrentNotebook.OwnerID == Person.Author.ID;
            IsBackStageVisible = false;
        }

        #endregion // Events

        #region Bindings

        /// <summary>Visibility of the top drag bar when program is not minimized.</summary>
        public bool IsDragBarVisible
        {
            get { return GetValue<bool>(IsDragBarVisibleProperty); }
            set { SetValue(IsDragBarVisibleProperty, value); }
        }

        public static readonly PropertyData IsDragBarVisibleProperty = RegisterProperty("IsDragBarVisible", typeof (bool), false);

        /// <summary>Screenshot of the frozen display.</summary>
        public ImageSource FrozenDisplayImageSource
        {
            get { return GetValue<ImageSource>(FrozenDisplayImageSourceProperty); }
            set { SetValue(FrozenDisplayImageSourceProperty, value); }
        }

        public static readonly PropertyData FrozenDisplayImageSourceProperty = RegisterProperty("FrozenDisplayImageSource", typeof (ImageSource));

        /// <summary>The MajorRibbon at the top of the Window.</summary>
        public MajorRibbonViewModel MajorRibbon
        {
            get { return GetValue<MajorRibbonViewModel>(MajorRibbonProperty); }
            set { SetValue(MajorRibbonProperty, value); }
        }

        public static readonly PropertyData MajorRibbonProperty = RegisterProperty("MajorRibbon", typeof (MajorRibbonViewModel));

        /// <summary>The program's BackStage.</summary>
        public BackStageViewModel BackStage
        {
            get { return GetValue<BackStageViewModel>(BackStageProperty); }
            set { SetValue(BackStageProperty, value); }
        }

        public static readonly PropertyData BackStageProperty = RegisterProperty("BackStage", typeof (BackStageViewModel));

        /// <summary>Toggles BackStage Visibility.</summary>
        public bool IsBackStageVisible
        {
            get { return GetValue<bool>(IsBackStageVisibleProperty); }
            set
            {
                SetValue(IsBackStageVisibleProperty, value);
                ACLPPageBaseViewModel.ClearAdorners(_dataService.CurrentPage);
            }
        }

        public static readonly PropertyData IsBackStageVisibleProperty = RegisterProperty("IsBackStageVisible", typeof (bool), false);

        /// <summary>The Workspace of the <see cref="MainWindowViewModel" />.</summary>
        public ViewModelBase Workspace
        {
            get { return GetValue<ViewModelBase>(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public static readonly PropertyData WorkspaceProperty = RegisterProperty("Workspace", typeof (ViewModelBase));

        private Person _tempCurrentUser;

        /// <summary>Gets or sets the Authoring flag.</summary>
        public bool IsAuthoring
        {
            get { return GetValue<bool>(IsAuthoringProperty); }
            set
            {
                if (value != IsAuthoring)
                {
                    if (value)
                    {
                        _tempCurrentUser = CurrentUser;
                        CurrentUser = Person.Author;
                    }
                    else
                    {
                        CurrentUser = _tempCurrentUser;
                    }
                }
                SetValue(IsAuthoringProperty, value);
            }
        }

        public static readonly PropertyData IsAuthoringProperty = RegisterProperty("IsAuthoring", typeof (bool), false);

        /// <summary>Whether or not the Pens Down Screen has been activated.</summary>
        public bool IsPenDownActivated
        {
            get { return GetValue<bool>(IsPenDownActivatedProperty); }
            set
            {
                if (value && _dataService.CurrentNotebook != null)
                {
                    ACLPPageBaseViewModel.ClearAdorners(_dataService.CurrentNotebook.CurrentPage);
                }
                SetValue(IsPenDownActivatedProperty, value);
            }
        }

        public static readonly PropertyData IsPenDownActivatedProperty = RegisterProperty("IsPenDownActivated", typeof (bool), false);

        /// <summary>Whether or not the ConvertingToPDF Screen has been activated.</summary>
        public bool IsConvertingToPDF
        {
            get { return GetValue<bool>(IsConvertingToPDFProperty); }
            set { SetValue(IsConvertingToPDFProperty, value); }
        }

        public static readonly PropertyData IsConvertingToPDFProperty = RegisterProperty("IsConvertingToPDF", typeof (bool), false);

        /// <summary>The page that is currently being converted to PDF.</summary>
        public CLPPage CurrentConvertingPage
        {
            get { return GetValue<CLPPage>(CurrentConvertingPageProperty); }
            set { SetValue(CurrentConvertingPageProperty, value); }
        }

        public static readonly PropertyData CurrentConvertingPageProperty = RegisterProperty("CurrentConvertingPage", typeof (CLPPage));

        /// <summary>Signifies navigation through the notebook is disabled and can only  be ahieved through network commands.</summary>
        public bool IsLinked
        {
            get { return GetValue<bool>(IsLinkedProperty); }
            set { SetValue(IsLinkedProperty, value); }
        }

        public static readonly PropertyData IsLinkedProperty = RegisterProperty("IsLinked", typeof (bool), false);

        /// <summary>Whether or not to mirror the displays to the projector.</summary>
        public bool IsProjectorFrozen
        {
            get { return GetValue<bool>(IsProjectorFrozenProperty); }
            set { SetValue(IsProjectorFrozenProperty, value); }
        }

        public static readonly PropertyData IsProjectorFrozenProperty = RegisterProperty("IsProjectorFrozen", typeof (bool), true);

        #region Global Bindings

        public Visibility TeacherOnlyVisibility
        {
            get { return CurrentProgramMode == ProgramModes.Teacher ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility ProjectorOnlyVisibility
        {
            get { return CurrentProgramMode == ProgramModes.Projector ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility StudentOnlyVisibility
        {
            get { return CurrentProgramMode == ProgramModes.Student ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility NotTeacherVisibility
        {
            get { return CurrentProgramMode == ProgramModes.Teacher ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility NotStudentVisibility
        {
            get { return CurrentProgramMode == ProgramModes.Student ? Visibility.Collapsed : Visibility.Visible; }
        }

        /// <summary>Global UI binding for Handedness</summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof (Handedness), Handedness.Right);

        /// <summary>Name of the currently loaded notebook.</summary>
        public string CurrentNotebookName
        {
            get { return GetValue<string>(CurrentNotebookNameProperty); }
            set { SetValue(CurrentNotebookNameProperty, value); }
        }

        public static readonly PropertyData CurrentNotebookNameProperty = RegisterProperty("CurrentNotebookName", typeof (string), string.Empty);

        /// <summary>Toggles style of the boundary around selected PageObjects to the old version.</summary>
        public bool IsUsingOldPageObjectBoundary
        {
            get { return GetValue<bool>(IsUsingOldPageObjectBoundaryProperty); }
            set { SetValue(IsUsingOldPageObjectBoundaryProperty, value); }
        }

        public static readonly PropertyData IsUsingOldPageObjectBoundaryProperty = RegisterProperty("IsUsingOldPageObjectBoundary", typeof (bool), false);

        /// <summary>SUMMARY</summary>
        public bool CanUseAutoNumberLine
        {
            get { return GetValue<bool>(CanUseAutoNumberLineProperty); }
            set { SetValue(CanUseAutoNumberLineProperty, value); }
        }

        public static readonly PropertyData CanUseAutoNumberLineProperty = RegisterProperty("CanUseAutoNumberLine", typeof (bool), false);

        #endregion //Global Bindings

        #endregion //Bindings

        #region Properties

        /// <summary>Current program mode for CLP.</summary>
        public ProgramModes CurrentProgramMode
        {
            get { return GetValue<ProgramModes>(CurrentProgramModeProperty); }
            set
            {
                SetValue(CurrentProgramModeProperty, value);
                RaisePropertyChanged(nameof(TeacherOnlyVisibility));
                RaisePropertyChanged(nameof(ProjectorOnlyVisibility));
                RaisePropertyChanged(nameof(StudentOnlyVisibility));
                RaisePropertyChanged(nameof(NotTeacherVisibility));
                RaisePropertyChanged(nameof(NotStudentVisibility));
            }
        }

        public static readonly PropertyData CurrentProgramModeProperty = RegisterProperty("CurrentProgramMode", typeof (ProgramModes), ProgramModes.Teacher);

        /// <summary>The <see cref="Person" /> using the program.</summary>
        public Person CurrentUser
        {
            get { return GetValue<Person>(CurrentUserProperty); }
            set { SetValue(CurrentUserProperty, value); }
        }

        public static readonly PropertyData CurrentUserProperty = RegisterProperty("CurrentUser", typeof (Person));

        #endregion //Properties

        #region Methods

        public void SetWorkspace()
        {
            IsAuthoring = false;
            switch (CurrentProgramMode)
            {
                case ProgramModes.Database:
                    break;
                case ProgramModes.Teacher:
                    CurrentUser = Person.Author;
                    break;
                case ProgramModes.Projector:
                    CurrentUser = Person.Author;
                    break;
                case ProgramModes.Student:
                    Workspace = this.CreateViewModel<UserLoginWorkspaceViewModel>(null);
                    break;
            }
        }

        #endregion //Methods

        #region Commands

        private void InitializeCommands()
        {
            SetUserModeCommand = new Command<string>(OnSetUserModeCommandExecute);
            TogglePenDownCommand = new Command(OnTogglePenDownCommandExecute);
            MoveWindowCommand = new Command<MouseButtonEventArgs>(OnMoveWindowCommandExecute);
            ToggleMinimizeStateCommand = new Command(OnToggleMinimizeStateCommandExecute);
            ToggleMaximizeStateCommand = new Command(OnToggleMaximizeStateCommandExecute);
            ExitProgramCommand = new Command(OnExitProgramCommandExecute);
            ToggleAutoSaveCommand = new Command(OnToggleAutoSaveCommandExecute);
        }

        /// <summary>Sets the UserMode of the program.</summary>
        public Command<string> SetUserModeCommand { get; private set; }

        private void OnSetUserModeCommandExecute(string userMode)
        {
            switch (userMode)
            {
                case "TEACHER":
                    CurrentProgramMode = ProgramModes.Teacher;
                    break;
                case "PROJECTOR":
                    CurrentProgramMode = ProgramModes.Projector;
                    break;
                case "STUDENT":
                    CurrentProgramMode = ProgramModes.Student;
                    break;
                default:
                    CurrentProgramMode = ProgramModes.Teacher;
                    break;
            }

            SetWorkspace();
        }

        /// <summary>Toggles the Pen Down screen.</summary>
        public Command TogglePenDownCommand { get; private set; }

        private void OnTogglePenDownCommandExecute() { IsPenDownActivated = !IsPenDownActivated; }

        /// <summary>Moves the CLP Window.</summary>
        public Command<MouseButtonEventArgs> MoveWindowCommand { get; private set; }

        private void OnMoveWindowCommandExecute(MouseButtonEventArgs args)
        {
            var mainWindow = this.GetFirstView() as MainWindowView;
            if (mainWindow == null ||
                Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            mainWindow.DragMove();
        }

        /// <summary>Toggles CLP between minimized and not.</summary>
        public Command ToggleMinimizeStateCommand { get; private set; }

        private void OnToggleMinimizeStateCommandExecute()
        {
            var mainWindow = this.GetFirstView() as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }

            mainWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>Toggles CLP Window State between Maximized and Normal.</summary>
        public Command ToggleMaximizeStateCommand { get; private set; }

        private void OnToggleMaximizeStateCommandExecute()
        {
            var mainWindow = this.GetFirstView() as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }

            IsDragBarVisible = mainWindow.WindowState == WindowState.Maximized;
            mainWindow.WindowState = mainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>Closes CLP</summary>
        public Command ExitProgramCommand { get; private set; }

        private void OnExitProgramCommandExecute()
        {
            var mainWindow = this.GetFirstView() as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.Close();
        }

        /// <summary>Toggles AutoSave On and Off.</summary>
        public Command ToggleAutoSaveCommand { get; private set; }

        private void OnToggleAutoSaveCommandExecute()
        {
            _dataService.IsAutoSaveOn = !_dataService.IsAutoSaveOn;
        }

        #endregion //Commands

        #region Static Methods

        public static void ChangeApplicationMainColor(string hexString) { Application.Current.Resources["DynamicMainColor"] = new BrushConverter().ConvertFrom(hexString); }

        public static void ChangeApplicationMainColor(Color color) { Application.Current.Resources["DynamicMainColor"] = new SolidColorBrush(color); }

        #endregion //Static Methods
    }
}