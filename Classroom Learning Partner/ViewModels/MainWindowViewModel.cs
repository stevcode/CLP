using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum ProgramModes
    {
        Author,
        Teacher,
        Student,
        Projector,
        Database
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private const string CLP_TEXT = "Classroom Learning Partner";

        #region Constructor

        /// <summary>Initializes a new instance of the MainWindowViewModel class.</summary>
        public MainWindowViewModel(ProgramModes currentProgramMode)
        {
            CurrentProgramMode = currentProgramMode;

            InitializeCommands();

            TitleBarText = CLP_TEXT;
            CurrentUser = Person.Guest;
            IsProjectorFrozen = CurrentProgramMode != ProgramModes.Projector;
        }

        public override string Title
        {
            get { return "MainWindowVM"; }
        }

        private void InitializeCommands()
        {
            SetUserModeCommand = new Command<string>(OnSetUserModeCommandExecute);
            TogglePenDownCommand = new Command(OnTogglePenDownCommandExecute);
            MoveWindowCommand = new Command<MouseButtonEventArgs>(OnMoveWindowCommandExecute);
            ToggleMinimizeStateCommand = new Command(OnToggleMinimizeStateCommandExecute);
            ToggleMaximizeStateCommand = new Command(OnToggleMaximizeStateCommandExecute);
            ExitProgramCommand = new Command(OnExitProgramCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>
        /// Visibility of the top drag bar when program is not minimized.
        /// </summary>
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

        /// <summary>Gets or sets the Title Bar text of the window.</summary>
        public string TitleBarText
        {
            get { return GetValue<string>(TitleBarTextProperty); }
            set { SetValue(TitleBarTextProperty, value); }
        }

        public static readonly PropertyData TitleBarTextProperty = RegisterProperty("TitleBarText", typeof (string));

        /// <summary>Gets or sets the property value.</summary>
        public RibbonViewModel Ribbon
        {
            get { return GetValue<RibbonViewModel>(RibbonProperty); }
            set { SetValue(RibbonProperty, value); }
        }

        public static readonly PropertyData RibbonProperty = RegisterProperty("Ribbon", typeof (RibbonViewModel), new RibbonViewModel());

        /// <summary>The MajorRibbon at the top of the Window.</summary>
        public MajorRibbonViewModel MajorRibbon
        {
            get { return GetValue<MajorRibbonViewModel>(MajorRibbonProperty); }
            set { SetValue(MajorRibbonProperty, value); }
        }

        public static readonly PropertyData MajorRibbonProperty = RegisterProperty("MajorRibbon",
                                                                                   typeof (MajorRibbonViewModel),
                                                                                   new MajorRibbonViewModel());

        /// <summary>The program's BackStage.</summary>
        public BackStageViewModel BackStage
        {
            get { return GetValue<BackStageViewModel>(BackStageProperty); }
            set { SetValue(BackStageProperty, value); }
        }

        public static readonly PropertyData BackStageProperty = RegisterProperty("BackStage", typeof (BackStageViewModel), new BackStageViewModel());

        /// <summary>Toggles BackStage Visibility.</summary>
        public bool IsBackStageVisible
        {
            get { return GetValue<bool>(IsBackStageVisibleProperty); }
            set
            {
                SetValue(IsBackStageVisibleProperty, value);
                ACLPPageBaseViewModel.ClearAdorners(NotebookPagesPanelViewModel.GetCurrentPage());
            }
        }

        public static readonly PropertyData IsBackStageVisibleProperty = RegisterProperty("IsBackStageVisible", typeof (bool), false);

        /// <summary>The Workspace of the <see cref="MainWindowViewModel" />.</summary>
        public ViewModelBase Workspace
        {
            get { return GetValue<ViewModelBase>(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public static readonly PropertyData WorkspaceProperty = RegisterProperty("Workspace", typeof (ViewModelBase), new BlankWorkspaceViewModel());

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

        #region Status Bar Bindings

        /// <summary>Gets or sets the property value.</summary>
        public string OnlineStatus
        {
            get { return GetValue<string>(OnlineStatusProperty); }
            set { SetValue(OnlineStatusProperty, value); }
        }

        public static readonly PropertyData OnlineStatusProperty = RegisterProperty("OnlineStatus", typeof (string), "DISCONNECTED");

        #endregion //Status Bar Bindings

        /// <summary>Whether or not the Pens Down Screen has been activated.</summary>
        public bool IsPenDownActivated
        {
            get { return GetValue<bool>(IsPenDownActivatedProperty); }
            set
            {
                if (value)
                {
                    ACLPPageBaseViewModel.ClearAdorners(RibbonViewModel.CurrentPage);
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

        /// <summary>SUMMARY</summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof (Handedness), Handedness.Right);

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

        #endregion //Global Bindings

        #endregion //Bindings

        #region Static Properties

        private static string _localCacheDirectory;

        public static string LocalCacheDirectory
        {
            get
            {
                return _localCacheDirectory;
            }

            set { _localCacheDirectory = value; }
        }

        public static string ClassCacheDirectory
        {
            get
            {
                var path = Path.Combine(LocalCacheDirectory, "Classes");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static string NotebookCacheDirectory
        {
            get
            {
                var path = Path.Combine(LocalCacheDirectory, "Notebooks");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static List<string> AvailableLocalNotebookNames
        {
            get
            {
                var directoryInfo = new DirectoryInfo(NotebookCacheDirectory);
                return directoryInfo.GetDirectories().Select(directory => directory.Name).OrderBy(x => x).ToList();
            }
        }

        #endregion //Static Properties

        #region Properties

        /// <summary>Current program mode for CLP.</summary>
        public ProgramModes CurrentProgramMode
        {
            get { return GetValue<ProgramModes>(CurrentProgramModeProperty); }
            set
            {
                SetValue(CurrentProgramModeProperty, value);
                RaisePropertyChanged("TeacherOnlyVisibility");
                RaisePropertyChanged("ProjectorOnlyVisibility");
                RaisePropertyChanged("StudentOnlyVisibility");
                RaisePropertyChanged("NotTeacherVisibility");
                RaisePropertyChanged("NotStudentVisibility");
            }
        }

        public static readonly PropertyData CurrentProgramModeProperty = RegisterProperty("CurrentProgramMode",
                                                                                          typeof (ProgramModes),
                                                                                          ProgramModes.Teacher);

        /// <summary>ImagePool for the current CLP instance, populated by all open notebooks.</summary>
        public Dictionary<string, BitmapImage> ImagePool
        {
            get { return GetValue<Dictionary<string, BitmapImage>>(ImagePoolProperty); }
            set { SetValue(ImagePoolProperty, value); }
        }

        public static readonly PropertyData ImagePoolProperty = RegisterProperty("ImagePool",
                                                                                 typeof (Dictionary<string, BitmapImage>),
                                                                                 () => new Dictionary<string, BitmapImage>());

        /// <summary>The <see cref="Person" /> using the program.</summary>
        public Person CurrentUser
        {
            get { return GetValue<Person>(CurrentUserProperty); }
            set { SetValue(CurrentUserProperty, value); }
        }

        public static readonly PropertyData CurrentUserProperty = RegisterProperty("CurrentUser", typeof (Person));

        /// <summary>List of all available <see cref="Person" />s.</summary>
        public ObservableCollection<Person> AvailableUsers
        {
            get { return GetValue<ObservableCollection<Person>>(AvailableUsersProperty); }
            set { SetValue(AvailableUsersProperty, value); }
        }

        public static readonly PropertyData AvailableUsersProperty = RegisterProperty("AvailableUsers",
                                                                                      typeof (ObservableCollection<Person>),
                                                                                      () => new ObservableCollection<Person>());

        /// <summary>Gets or sets the property value.</summary>
        public ObservableCollection<Notebook> OpenNotebooks
        {
            get { return GetValue<ObservableCollection<Notebook>>(OpenNotebooksProperty); }
            set { SetValue(OpenNotebooksProperty, value); }
        }

        public static readonly PropertyData OpenNotebooksProperty = RegisterProperty("OpenNotebooks",
                                                                                     typeof (ObservableCollection<Notebook>),
                                                                                     () => new ObservableCollection<Notebook>());

        /// <summary>The current <see cref="ClassPeriod" /> the program will attemp to use to load the day's <see cref="CLPPage" />s.</summary>
        public ClassPeriod CurrentClassPeriod
        {
            get { return GetValue<ClassPeriod>(CurrentClassPeriodProperty); }
            set { SetValue(CurrentClassPeriodProperty, value); }
        }

        public static readonly PropertyData CurrentClassPeriodProperty = RegisterProperty("CurrentClassPeriod", typeof (ClassPeriod));

        #endregion //Properties

        #region Methods

        public void SetWorkspace()
        {
            IsAuthoring = false;
            switch (CurrentProgramMode)
            {
                case ProgramModes.Author:
                case ProgramModes.Database:
                    break;
                case ProgramModes.Teacher:
                    //TODO: Remove after database established
                    CurrentUser = Person.Author;
                    break;
                case ProgramModes.Projector:
                    //TODO: Remove after database established
                    CurrentUser = Person.Author;
                    break;
                case ProgramModes.Student:
                    Workspace = new UserLoginWorkspaceViewModel();
                    break;
            }
        }

        #endregion //Methods

        #region Commands

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

        /// <summary>
        /// Moves the CLP Window.
        /// </summary>
        public Command<MouseButtonEventArgs> MoveWindowCommand { get; private set; }

        private void OnMoveWindowCommandExecute(MouseButtonEventArgs args)
        {
            var mainWindow = CLPServiceAgent.Instance.GetViewFromViewModel(App.MainWindowViewModel) as MainWindowView;
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
            var mainWindow = CLPServiceAgent.Instance.GetViewFromViewModel(App.MainWindowViewModel) as MainWindowView;
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
            var mainWindow = CLPServiceAgent.Instance.GetViewFromViewModel(App.MainWindowViewModel) as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }

            IsDragBarVisible = mainWindow.WindowState == WindowState.Maximized;
            mainWindow.WindowState = mainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// Closes CLP
        /// </summary>
        public Command ExitProgramCommand { get; private set; }

        private void OnExitProgramCommandExecute()
        {
            var mainWindow = CLPServiceAgent.Instance.GetViewFromViewModel(App.MainWindowViewModel) as MainWindowView;
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.Close();
        }

        #endregion //Commands

        #region Static Methods

        public static void ChangeApplicationMainColor(string hexString)
        {
            Application.Current.Resources["DynamicMainColor"] = new BrushConverter().ConvertFrom(hexString);
        }

        public static void ChangeApplicationMainColor(Color color) { Application.Current.Resources["DynamicMainColor"] = new SolidColorBrush(color); }

        public static void ResetCache()
        {
            if (!Directory.Exists(NotebookCacheDirectory))
            {
                return;
            }

            var archiveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CacheArchive");
            var now = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            var newCacheDirectory = Path.Combine(archiveDirectory, "Cache-" + now);
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
            Directory.Move(NotebookCacheDirectory, newCacheDirectory);
        }

        public static void OpenClassPeriod()
        {
            var classPeriodFilePaths = Directory.GetFiles(ClassCacheDirectory);
            string closestClassPeriodFilePath = null;
            var closestTimeSpan = TimeSpan.MaxValue;
            var now = DateTime.Now;
            foreach (var classPeriodFilePath in classPeriodFilePaths)
            {
                var classFileName = Path.GetFileNameWithoutExtension(classPeriodFilePath);
                var classInfo = classFileName.Split(';');
                if (classInfo.Length != 3 ||
                    classInfo[0] != "period")
                {
                    continue;
                }
                var time = classInfo[1];
                var timeParts = time.Split('.');
                var year = Int32.Parse(timeParts[0]);
                var month = Int32.Parse(timeParts[1]);
                var day = Int32.Parse(timeParts[2]);
                var hour = Int32.Parse(timeParts[3]);
                var minute = Int32.Parse(timeParts[4]);
                var dateTime = new DateTime(year, month, day, hour, minute, 0);

                var timeSpan = now - dateTime;
                if (timeSpan.Duration() >= closestTimeSpan.Duration())
                {
                    continue;
                }
                closestTimeSpan = timeSpan;
                closestClassPeriodFilePath = classPeriodFilePath;
            }

            if (String.IsNullOrEmpty(closestClassPeriodFilePath))
            {
                MessageBox.Show("ERROR: Could not find ClassPeriod .xml file.");
                return;
            }

            var classPeriod = ClassPeriod.LoadLocalClassPeriod(closestClassPeriodFilePath);
            if (classPeriod == null)
            {
                MessageBox.Show("ERROR: Could not open ClassPeriod.");
                return;
            }
            App.MainWindowViewModel.CurrentClassPeriod = classPeriod;

            var notebookFolderPath = GetNotebookFolderPathByCompositeID(App.MainWindowViewModel.CurrentClassPeriod.NotebookID, Person.Author.ID);
            if (notebookFolderPath == null)
            {
                MessageBox.Show("ERROR: Could not find Notebook for latest ClassPeriod.");
                return;
            }

            if (!Directory.Exists(notebookFolderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            var notebook = Notebook.OpenPartialNotebook(notebookFolderPath, App.MainWindowViewModel.CurrentClassPeriod.PageIDs, new List<string>());

            if (notebook == null)
            {
                MessageBox.Show("Notebook could not be opened. Check error log.");
                return;
            }

            App.MainWindowViewModel.CurrentUser = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.Teacher;

            notebook.CurrentPage = notebook.Pages.FirstOrDefault();
            App.MainWindowViewModel.OpenNotebooks.Add(notebook);

            var copiedNotebook = notebook.CopyForNewOwner(App.MainWindowViewModel.CurrentUser);
            var notebookToUse = copiedNotebook;

            var storedNotebookFolderName = copiedNotebook.Name + ";" + copiedNotebook.ID + ";" + copiedNotebook.Owner.FullName + ";" +
                                           copiedNotebook.OwnerID + ";T";
            var storedNotebookFolderPath = Path.Combine(NotebookCacheDirectory, storedNotebookFolderName);
            if (Directory.Exists(storedNotebookFolderPath))
            {
                var pageIDs = App.MainWindowViewModel.CurrentClassPeriod.PageIDs;
                var storedNotebook = Notebook.OpenPartialNotebook(storedNotebookFolderPath, pageIDs, new List<string>());
                if (storedNotebook != null)
                {
                    var loadedPageIDs = storedNotebook.Pages.Select(page => page.ID).ToList();
                    foreach (var page in copiedNotebook.Pages.Where(page => !loadedPageIDs.Contains(page.ID)))
                    {
                        storedNotebook.Pages.Add(page);
                    }
                    var orderedPages = storedNotebook.Pages.OrderBy(x => x.PageNumber).ToList();
                    storedNotebook.Pages = new ObservableCollection<CLPPage>(orderedPages);
                    notebookToUse = storedNotebook;
                }
            }

            foreach (var page in notebookToUse.Pages)
            {
                foreach (var notebookName in AvailableLocalNotebookNames)
                {
                    var notebookInfo = notebookName.Split(';');
                    if ((notebookInfo.Length != 4 && notebookInfo.Length != 5) ||
                        notebookInfo[3] == Person.Author.ID ||
                        notebookInfo[3] == App.MainWindowViewModel.CurrentUser.ID)
                    {
                        continue;
                    }

                    var folderPath = Path.Combine(NotebookCacheDirectory, notebookName);
                    if (!Directory.Exists(folderPath))
                    {
                        continue;
                    }

                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    if (!Directory.Exists(submissionsPath))
                    {
                        continue;
                    }

                    var submissionPaths = Directory.EnumerateFiles(submissionsPath, "*.xml");

                    foreach (var submissionPath in submissionPaths)
                    {
                        var submissionFileName = Path.GetFileNameWithoutExtension(submissionPath);
                        var submissionInfo = submissionFileName.Split(';');
                        if (submissionInfo.Length == 5 &&
                            submissionInfo[2] == page.ID &&
                            submissionInfo[3] == page.DifferentiationLevel &&
                            submissionInfo[4] != "0")
                        {
                            var submission = Load<CLPPage>(submissionPath, SerializationMode.Xml);
                            page.Submissions.Add(submission);
                        }
                    }
                }
            }
            notebookToUse.CurrentPage = notebookToUse.Pages.FirstOrDefault();

            //HACK: To load gridDisplays after all submissions are loaded
            foreach (var gridDisplay in notebookToUse.Displays)
            {
                foreach (var compositePageID in gridDisplay.CompositePageIDs)
                {
                    var compositeSections = compositePageID.Split(';');
                    var id = compositeSections[0];
                    var ownerID = compositeSections[1];
                    var differentiationlevel = compositeSections[2];
                    var versionIndex = Convert.ToUInt32(compositeSections[3]);

                    var page = notebookToUse.GetPageByCompositeKeys(id, ownerID, differentiationlevel, versionIndex);
                    if (page == null)
                    {
                        continue;
                    }

                    gridDisplay.Pages.Add(page);
                }
            }

            App.MainWindowViewModel.OpenNotebooks.Add(notebookToUse);
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebookToUse);
            App.MainWindowViewModel.AvailableUsers = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        public static void ViewAllWork()
        {
            //TODO: This is very hacky.
            var classPeriodFilePaths = Directory.GetFiles(ClassCacheDirectory);
            string closestClassPeriodFilePath = null;
            var closestTimeSpan = TimeSpan.MaxValue;
            var now = DateTime.Now;
            foreach (var classPeriodFilePath in classPeriodFilePaths)
            {
                var classFileName = Path.GetFileNameWithoutExtension(classPeriodFilePath);
                var classInfo = classFileName.Split(';');
                if (classInfo.Length != 3 ||
                    classInfo[0] != "period")
                {
                    continue;
                }
                var time = classInfo[1];
                var timeParts = time.Split('.');
                var year = Int32.Parse(timeParts[0]);
                var month = Int32.Parse(timeParts[1]);
                var day = Int32.Parse(timeParts[2]);
                var hour = Int32.Parse(timeParts[3]);
                var minute = Int32.Parse(timeParts[4]);
                var dateTime = new DateTime(year, month, day, hour, minute, 0);

                var timeSpan = now - dateTime;
                if (timeSpan.Duration() >= closestTimeSpan.Duration())
                {
                    continue;
                }
                closestTimeSpan = timeSpan;
                closestClassPeriodFilePath = classPeriodFilePath;
            }

            if (String.IsNullOrEmpty(closestClassPeriodFilePath))
            {
                MessageBox.Show("ERROR: Could not find ClassPeriod.");
                return;
            }

            var classPeriod = ClassPeriod.LoadLocalClassPeriod(closestClassPeriodFilePath);
            if (classPeriod == null)
            {
                MessageBox.Show("ERROR: Could not open ClassPeriod.");
                return;
            }

            App.MainWindowViewModel.CurrentClassPeriod = classPeriod;

            var notebookFolderPath = GetNotebookFolderPathByCompositeID(App.MainWindowViewModel.CurrentClassPeriod.NotebookID, Person.Author.ID);
            if (notebookFolderPath == null)
            {
                MessageBox.Show("ERROR: Could not find Notebook for latest ClassPeriod.");
                return;
            }

            if (!Directory.Exists(notebookFolderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            App.MainWindowViewModel.CurrentClassPeriod.PageIDs.Clear();
            var notebookPagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
            var pageAndHistoryFilePaths = Directory.EnumerateFiles(notebookPagesFolderPath, "*.xml");
            foreach (var pageAndHistoryFilePath in pageAndHistoryFilePaths)
            {
                var pageAndHistoryFileName = Path.GetFileNameWithoutExtension(pageAndHistoryFilePath);
                var pageAndHistoryInfo = pageAndHistoryFileName.Split(';');
                if (pageAndHistoryInfo.Length != 5 ||
                    pageAndHistoryInfo[0] != "p")
                {
                    continue;
                }
                if (pageAndHistoryInfo[4] != "0")
                {
                    continue;
                }

                App.MainWindowViewModel.CurrentClassPeriod.PageIDs.Add(pageAndHistoryInfo[2]);
            }

            var notebook = Notebook.OpenPartialNotebook(notebookFolderPath, App.MainWindowViewModel.CurrentClassPeriod.PageIDs, new List<string>());

            if (notebook == null)
            {
                MessageBox.Show("Notebook could not be opened. Check error log.");
                return;
            }

            App.MainWindowViewModel.CurrentUser = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.Teacher;

            notebook.CurrentPage = notebook.Pages.First();
            App.MainWindowViewModel.OpenNotebooks.Add(notebook);

            var copiedNotebook = notebook.CopyForNewOwner(App.MainWindowViewModel.CurrentUser);
            var notebookToUse = copiedNotebook;

            var storedNotebookFolderName = copiedNotebook.Name + ";" + copiedNotebook.ID + ";" + copiedNotebook.Owner.FullName + ";" +
                                           copiedNotebook.OwnerID;
            var storedNotebookFolderPath = Path.Combine(NotebookCacheDirectory, storedNotebookFolderName);
            if (Directory.Exists(storedNotebookFolderPath))
            {
                var pageIDs = App.MainWindowViewModel.CurrentClassPeriod.PageIDs;
                var storedNotebook = Notebook.OpenPartialNotebook(storedNotebookFolderPath, pageIDs, new List<string>());
                if (storedNotebook != null)
                {
                    var loadedPageIDs = storedNotebook.Pages.Select(page => page.ID).ToList();
                    foreach (var page in copiedNotebook.Pages.Where(page => !loadedPageIDs.Contains(page.ID)))
                    {
                        storedNotebook.Pages.Add(page);
                    }
                    var orderedPages = storedNotebook.Pages.OrderBy(x => x.PageNumber).ToList();
                    storedNotebook.Pages = new ObservableCollection<CLPPage>(orderedPages);
                    notebookToUse = storedNotebook;
                }
            }

            foreach (var page in notebookToUse.Pages)
            {
                foreach (var notebookName in AvailableLocalNotebookNames)
                {
                    var notebookInfo = notebookName.Split(';');
                    if (notebookInfo.Length != 4 ||
                        notebookInfo[3] == Person.Author.ID ||
                        notebookInfo[3] == App.MainWindowViewModel.CurrentUser.ID)
                    {
                        continue;
                    }

                    var folderPath = Path.Combine(NotebookCacheDirectory, notebookName);
                    if (!Directory.Exists(folderPath))
                    {
                        continue;
                    }

                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    if (!Directory.Exists(submissionsPath))
                    {
                        continue;
                    }

                    var submissionPaths = Directory.EnumerateFiles(submissionsPath, "*.xml");

                    foreach (var submissionPath in submissionPaths)
                    {
                        var submissionFileName = Path.GetFileNameWithoutExtension(submissionPath);
                        var submissionInfo = submissionFileName.Split(';');
                        if (submissionInfo.Length == 5 &&
                            submissionInfo[2] == page.ID &&
                            submissionInfo[3] == page.DifferentiationLevel &&
                            submissionInfo[4] != "0")
                        {
                            var submission = Load<CLPPage>(submissionPath, SerializationMode.Xml);
                            page.Submissions.Add(submission);
                        }
                    }
                }
            }
            notebookToUse.CurrentPage = notebookToUse.Pages.FirstOrDefault();
            App.MainWindowViewModel.OpenNotebooks.Add(notebookToUse);
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebookToUse);
            App.MainWindowViewModel.AvailableUsers = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
        }

        public static string GetNotebookFolderPathByCompositeID(string id, string ownerID)
        {
            var notebookFolderPaths = Directory.GetDirectories(NotebookCacheDirectory);
            return (from notebookFolderPath in notebookFolderPaths
                    let notebookInfo = notebookFolderPath.Split(';')
                    where notebookInfo.Length == 4 || notebookInfo.Length == 5
                    let notebookID = notebookInfo[1]
                    let notebookOwnerID = notebookInfo[3]
                    where notebookID == id && notebookOwnerID == ownerID
                    select notebookFolderPath).FirstOrDefault();
        }

        #endregion //Static Methods
    }
}