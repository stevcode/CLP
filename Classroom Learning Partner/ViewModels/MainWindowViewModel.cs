using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum ProgramModes
    {
        Author,
        Instructor,
        Student,
        Projector,
        Database
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private const string CLP_TEXT = "Classroom Learning Partner";

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
        {
            InitializeCommands();
            TitleBarText = CLP_TEXT;
        }

        public override string Title
        {
            get { return "MainWindowVM"; }
        }

        private void InitializeCommands()
        {
            SetUserModeCommand = new Command<string>(OnSetUserModeCommandExecute);

            ToggleDebugCommand = new Command(OnToggleDebugCommandExecute);
            ToggleExtrasCommand = new Command(OnToggleExtrasCommandExecute);
            TogglePenDownCommand = new Command(OnTogglePenDownCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>
        /// Gets or sets the Title Bar text of the window.
        /// </summary>
        public string TitleBarText
        {
            get { return GetValue<string>(TitleBarTextProperty); }
            set { SetValue(TitleBarTextProperty, value); }
        }

        public static readonly PropertyData TitleBarTextProperty = RegisterProperty("TitleBarText", typeof(string));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public RibbonViewModel Ribbon
        {
            get { return GetValue<RibbonViewModel>(RibbonProperty); }
            set { SetValue(RibbonProperty, value); }
        }

        public static readonly PropertyData RibbonProperty = RegisterProperty("Ribbon", typeof(RibbonViewModel), new RibbonViewModel());

        /// <summary>
        /// The Workspace of the <see cref="MainWindowViewModel" />.
        /// </summary>
        public ViewModelBase Workspace
        {
            get { return GetValue<ViewModelBase>(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public static readonly PropertyData WorkspaceProperty = RegisterProperty("Workspace", typeof(ViewModelBase), new BlankWorkspaceViewModel());

        /// <summary>
        /// Gets or sets the Authoring flag.
        /// </summary>
        public bool IsAuthoring
        {
            get { return GetValue<bool>(IsAuthoringProperty); }
            set
            {
                if(value != IsAuthoring)
                {
                    if(value)
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

        private Person _tempCurrentUser;

        public static readonly PropertyData IsAuthoringProperty = RegisterProperty("IsAuthoring", typeof(bool), false);

        #region Status Bar Bindings

        /// <summary>
        /// The name of the current open Notebook.
        /// </summary>
        public string CurrentNotebookName
        {
            get { return GetValue<string>(CurrentNotebookNameProperty); }
            set { SetValue(CurrentNotebookNameProperty, value); }
        }

        public static readonly PropertyData CurrentNotebookNameProperty = RegisterProperty("CurrentNotebookName", typeof(string), string.Empty);

        /// <summary>
        /// Shows the last time the notebook was saved during the current session.
        /// </summary>
        public string LastSavedTime
        {
            get { return GetValue<string>(LastSavedTimeProperty); }
            set { SetValue(LastSavedTimeProperty, value); }
        }

        public static readonly PropertyData LastSavedTimeProperty = RegisterProperty("LastSavedTime", typeof(string), string.Empty);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string OnlineStatus
        {
            get { return GetValue<string>(OnlineStatusProperty); }
            set { SetValue(OnlineStatusProperty, value); }
        }

        public static readonly PropertyData OnlineStatusProperty = RegisterProperty("OnlineStatus", typeof(string), "DISCONNECTED");

        #endregion //Status Bar Bindings

        /// <summary>
        /// Whether or not the Pens Down Screen has been activated.
        /// </summary>
        public bool IsPenDownActivated
        {
            get { return GetValue<bool>(IsPenDownActivatedProperty); }
            set { SetValue(IsPenDownActivatedProperty, value); }
        }

        public static readonly PropertyData IsPenDownActivatedProperty = RegisterProperty("IsPenDownActivated", typeof(bool), false);

        /// <summary>
        /// SUMMARY
        /// </summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        /// <summary>
        /// Signifies navigation through the notebook is disabled and can only  be ahieved through network commands.
        /// </summary>
        public bool IsLinked
        {
            get { return GetValue<bool>(IsLinkedProperty); }
            set { SetValue(IsLinkedProperty, value); }
        }

        public static readonly PropertyData IsLinkedProperty = RegisterProperty("IsLinked", typeof(bool), false);

        #endregion //Bindings

        #region Properties

        /// <summary>
        /// The <see cref="Person" /> using the program.
        /// </summary>
        public Person CurrentUser
        {
            get { return GetValue<Person>(CurrentUserProperty); }
            set { SetValue(CurrentUserProperty, value); }
        }

        public static readonly PropertyData CurrentUserProperty = RegisterProperty("CurrentUser", typeof(Person));

        /// <summary>
        /// List of all available <see cref="Person" />s.
        /// </summary>
        public ObservableCollection<Person> AvailableUsers
        {
            get { return GetValue<ObservableCollection<Person>>(AvailableUsersProperty); }
            set { SetValue(AvailableUsersProperty, value); }
        }

        public static readonly PropertyData AvailableUsersProperty = RegisterProperty("AvailableUsers", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<Notebook> OpenNotebooks
        {
            get { return GetValue<ObservableCollection<Notebook>>(OpenNotebooksProperty); }
            set { SetValue(OpenNotebooksProperty, value); }
        }

        public static readonly PropertyData OpenNotebooksProperty = RegisterProperty("OpenNotebooks", typeof(ObservableCollection<Notebook>), () => new ObservableCollection<Notebook>());

        /// <summary>
        /// The current <see cref="ClassPeriod" /> the program will attemp to use to load the day's <see cref="CLPPage" />s.
        /// </summary>
        public ClassPeriod CurrentClassPeriod
        {
            get { return GetValue<ClassPeriod>(CurrentClassPeriodProperty); }
            set { SetValue(CurrentClassPeriodProperty, value); }
        }

        public static readonly PropertyData CurrentClassPeriodProperty = RegisterProperty("CurrentClassPeriod", typeof(ClassPeriod));

        #endregion //Properties

        #region Methods

        public void SetWorkspace()
        {
            IsAuthoring = false;
            switch(App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    //TODO: Remove after database established
                    CurrentUser = Person.Emily;
                    Workspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    //TODO: Remove after database established
                    CurrentUser = Person.EmilyProjector;
                    Workspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Student:
                    Workspace = new UserLoginWorkspaceViewModel();
                    break;
            }
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Sets the UserMode of the program.
        /// </summary>
        public Command<string> SetUserModeCommand { get; private set; }

        private void OnSetUserModeCommandExecute(string userMode)
        {
            switch(userMode)
            {
                case "INSTRUCTOR":
                    App.CurrentUserMode = App.UserMode.Instructor;
                    break;
                case "PROJECTOR":
                    App.CurrentUserMode = App.UserMode.Projector;
                    break;
                case "STUDENT":
                    App.CurrentUserMode = App.UserMode.Student;
                    break;
                default:
                    App.CurrentUserMode = App.UserMode.Instructor;
                    break;
            }

            SetWorkspace();
        }

        /// <summary>
        /// Toggles Debug Tab.
        /// </summary>
        public Command ToggleDebugCommand { get; private set; }

        private void OnToggleDebugCommandExecute() { Ribbon.DebugTabVisibility = Ribbon.DebugTabVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed; }

        /// <summary>
        /// Toggles Extras Tab.
        /// </summary>
        public Command ToggleExtrasCommand { get; private set; }

        private void OnToggleExtrasCommandExecute() { Ribbon.ExtrasTabVisibility = Ribbon.ExtrasTabVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed; }

        /// <summary>
        /// Toggles the Pen Down screen.
        /// </summary>
        public Command TogglePenDownCommand { get; private set; }

        private void OnTogglePenDownCommandExecute() { IsPenDownActivated = !IsPenDownActivated; }

        #endregion //Commands

        #region Static Methods

        public static List<string> AvailableLocalNotebookNames
        {
            get
            {
                var directoryInfo = new DirectoryInfo(App.NotebookCacheDirectory);
                return directoryInfo.GetDirectories().Select(directory => directory.Name).ToList();
            }
        }

        public static List<string> AvailableDatabaseNotebookNames
        {
            get
            {
                // TODO: DATABASE - Attempt to grab names from database
                return new List<string>();
            }
        }

        public static void CreateNewNotebook()
        {
            var nameChooserLoop = true;

            while(nameChooserLoop)
            {
                var nameChooser = new NotebookNamerWindowView
                                  {
                                      Owner = Application.Current.MainWindow
                                  };
                nameChooser.ShowDialog();
                if(nameChooser.DialogResult == true)
                {
                    // TODO: Steve - sanitize notebook name
                    var notebookName = nameChooser.NotebookName.Text;
                    var newNotebook = new Notebook
                                          {
                                              Name = notebookName
                                          };
                    var newPage = new CLPPage();
                    newNotebook.AddCLPPageToNotebook(newPage);

                    var folderName = newNotebook.Name + ";" + newNotebook.ID;
                    var folderPath = Path.Combine(App.NotebookCacheDirectory, folderName);
                    if(!Directory.Exists(folderPath))
                    {
                        SaveNotebook(newNotebook);

                        App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(newNotebook);
                        App.MainWindowViewModel.IsAuthoring = true;
                        App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;
                        App.MainWindowViewModel.CurrentNotebookName = notebookName;

                        nameChooserLoop = false;
                    }
                    else
                    {
                        MessageBox.Show("A Notebook with that name already exists. Please choose a different name.");
                    }
                }
                else
                {
                    nameChooserLoop = false;
                }
            }
        }

        public static void OpenNotebook(string notebookFolderName, bool forceCache = false, bool forceDatabase = false)
        {
            foreach(var otherNotebook in App.MainWindowViewModel.OpenNotebooks.Where(otherNotebook => otherNotebook.ID == notebookFolderName.Split(';')[1]))
            {
                App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(otherNotebook);
                return;
            }

            var folderPath = Path.Combine(App.NotebookCacheDirectory, notebookFolderName);
            if(!Directory.Exists(folderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var notebook = Notebook.OpenNotebook(folderPath);

            stopWatch.Stop();
            Logger.Instance.WriteToLog("Time to OPEN notebook (In Seconds): " + stopWatch.ElapsedMilliseconds / 1000.0);

            if(notebook == null)
            {
                MessageBox.Show("Notebook could not be opened. Check error log.");
                return;
            }

            App.MainWindowViewModel.CurrentNotebookName = notebook.Name;
            if(notebook.LastSavedDate != null)
            {
                App.MainWindowViewModel.LastSavedTime = notebook.LastSavedDate.Value.ToString("yyyy/MM/dd - HH:mm:ss");
            }

            App.MainWindowViewModel.OpenNotebooks.Add(notebook);
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
        }

        public static void SaveNotebook(Notebook notebook, bool isFullSaveForced = false)
        {
            var folderPath = Path.Combine(App.NotebookCacheDirectory, notebook.Name);

            //if(isFullSaveForced)
            //{
            //    folderPath += " - FORCED";
            //}

            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            notebook.SaveNotebook(folderPath, isFullSaveForced);
            if(notebook.LastSavedDate != null)
            {
                App.MainWindowViewModel.LastSavedTime = notebook.LastSavedDate.Value.ToString("yyyy/MM/dd - HH:mm:ss");
            }
        }

        #endregion //Static Methods

        #region Temp Methods

        public static void CreateStudentListSeed()
        {
            var namesList = new List<string>
                            {
                                "Ivanoshka",
                                "Adam",
                                "Serenity",
                                "Sarisha",
                                "Selma",
                                "Fadi",
                                "Keaton",
                                "Renata",
                                "Deven",
                                "Ruggero",
                                "Isabel",
                                "Trishyn",
                                "Miles",
                                "Satyri",
                                "Julio",
                                "Tyler",
                                "Katherine",
                                "Antinoe"
                            };

            foreach(var name in namesList)
            {
                var student = new Person
                              {
                                  FullName = name,
                                  IsStudent = true
                              };
                ClassSubject.EmilyClass.StudentList.Add(student);
            }

            ClassSubject.EmilyClass.SaveClassSubject(App.ClassCacheDirectory);
        }

        #endregion //Temp Methods
    }
}