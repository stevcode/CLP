﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
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

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
        {
            InitializeCommands();
            TitleBarText = CLP_TEXT;
            CurrentUser = Person.Guest;
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
            set
            {
                if(value)
                {
                    ACLPPageBaseViewModel.ClearAdorners(RibbonViewModel.CurrentPage);
                }
                SetValue(IsPenDownActivatedProperty, value);
            }
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
        /// ImagePool for the current CLP instance, populated by all open notebooks.
        /// </summary>
        public Dictionary<string, BitmapImage> ImagePool
        {
            get { return GetValue<Dictionary<string, BitmapImage>>(ImagePoolProperty); }
            set { SetValue(ImagePoolProperty, value); }
        }

        public static readonly PropertyData ImagePoolProperty = RegisterProperty("ImagePool", typeof(Dictionary<string, BitmapImage>), () => new Dictionary<string, BitmapImage>());

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
                return directoryInfo.GetDirectories().Select(directory => directory.Name).OrderBy(x => x).ToList();
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
            App.MainWindowViewModel.IsAuthoring = false;

            var nameChooser = new NotebookNamerWindowView
                              {
                                  Owner = Application.Current.MainWindow
                              };
            nameChooser.ShowDialog();
            if(nameChooser.DialogResult != true)
            {
                return;
            }

            // TODO: Steve - sanitize notebook name
            var notebookName = nameChooser.NotebookName.Text;
            var newNotebook = new Notebook(notebookName, Person.Author);

            var newPage = new CLPPage(Person.Author);
            newNotebook.AddCLPPageToNotebook(newPage);

            var folderName = newNotebook.Name + ";" + newNotebook.ID + ";" + newNotebook.Owner.FullName + ";" + newNotebook.OwnerID;
            var folderPath = Path.Combine(App.NotebookCacheDirectory, folderName);
            if(Directory.Exists(folderPath))
            {
                return;
            }
            
            // TODO: Reimplement when autosave returns
            //SaveNotebook(newNotebook);

            App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(newNotebook);
            App.MainWindowViewModel.IsAuthoring = true;
            App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;
            App.MainWindowViewModel.CurrentNotebookName = notebookName;
        }

        public static void OpenNotebook(string notebookFolderName, bool forceCache = false, bool forceDatabase = false)
        {
            //TODO: find way to bypass this if partial notebook is currently open and you try to open full notebook (or vis versa).
            foreach(var otherNotebook in
                    App.MainWindowViewModel.OpenNotebooks.Where(otherNotebook => otherNotebook.ID == notebookFolderName.Split(';')[1] && 
                                                                                 otherNotebook.OwnerID == notebookFolderName.Split(';')[2]))
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

            var notebook = Notebook.OpenNotebook(folderPath);
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
            if(notebook.OwnerID == Person.Author.ID)
            {
                App.MainWindowViewModel.IsAuthoring = true;
            }
        }

        public static void SaveNotebook(Notebook notebook, bool isFullSaveForced = false)
        {
            var folderPath = Path.Combine(App.NotebookCacheDirectory, notebook.Name + ";" + notebook.ID + ";" + notebook.Owner.FullName + ";" + notebook.OwnerID);

            //////////////////

            //foreach(var page in notebook.Pages)
            //{
            //    foreach(var pageObject in page.PageObjects)
            //    {
            //        pageObject.CreatorID = Person.Author.ID;
            //        pageObject.ParentPage = page;
            //        pageObject.ID = new Guid(pageObject.ID).ToCompactID();
            //    }
            //}

            /// ////////////////

            //if(isFullSaveForced)
            //{
            //    folderPath += " - FORCED";
            //}
            
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            notebook.SaveNotebook(folderPath, isFullSaveForced);

            switch(App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    break;
                case App.UserMode.Projector:
                    break;
                case App.UserMode.Student:
                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    notebook.SaveSubmissions(submissionsPath);
                    break;
            }
            
            if(notebook.LastSavedDate != null)
            {
                App.MainWindowViewModel.LastSavedTime = notebook.LastSavedDate.Value.ToString("yyyy/MM/dd - HH:mm:ss");
            }
        }

        public static void OpenClassPeriod()
        {
            var classPeriodFilePaths = Directory.GetFiles(App.ClassCacheDirectory);
            foreach(var classPeriodFilePath in classPeriodFilePaths)
            {
                var classFileName = Path.GetFileNameWithoutExtension(classPeriodFilePath);
                var classInfo = classFileName.Split(';');
                if(classInfo.Length != 3 ||
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
                var now = DateTime.Now;
                var timeSpan = now - dateTime;
                var threeHours = new TimeSpan(3, 0, 0);
                if(timeSpan >= threeHours)
                {
                    continue;
                }
                var classPeriod = ClassPeriod.OpenClassPeriod(classPeriodFilePath);
                if(classPeriod == null)
                {
                    continue;
                }
                App.MainWindowViewModel.CurrentClassPeriod = classPeriod;
                break;
            }

            if(App.MainWindowViewModel.CurrentClassPeriod == null)
            {
                MessageBox.Show("ERROR: Could not find ClassPeriod.");
                return;
            }

            var notebookFolderPath = GetNotebookFolderPathByCompositeID(App.MainWindowViewModel.CurrentClassPeriod.NotebookID, Person.Author.ID);
            if(notebookFolderPath == null)
            {
                MessageBox.Show("ERROR: Could not find Notebook for latest ClassPeriod.");
                return;
            }

            if(!Directory.Exists(notebookFolderPath))
            {
                MessageBox.Show("Notebook doesn't exist");
                return;
            }

            var notebook = Notebook.OpenPartialNotebook(notebookFolderPath, App.MainWindowViewModel.CurrentClassPeriod.PageIDs, new List<string>());

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
            notebook.CurrentPage = notebook.Pages.First();
            App.MainWindowViewModel.OpenNotebooks.Add(notebook);

            var copiedNotebook = notebook.CopyForNewOwner(App.MainWindowViewModel.CurrentUser);
            var notebookToUse = copiedNotebook;

            var storedNotebookFolderName = copiedNotebook.Name + ";" + copiedNotebook.ID + ";" + copiedNotebook.Owner.FullName + ";" + copiedNotebook.OwnerID;
            var storedNotebookFolderPath = Path.Combine(App.NotebookCacheDirectory, storedNotebookFolderName);
            if(Directory.Exists(storedNotebookFolderPath))
            {
                var pageIDs = App.MainWindowViewModel.CurrentClassPeriod.PageIDs;
                var storedNotebook = Notebook.OpenPartialNotebook(storedNotebookFolderPath, pageIDs, new List<string>());
                if(storedNotebook != null)
                {
                    var loadedPageIDs = storedNotebook.Pages.Select(page => page.ID).ToList();
                    foreach(var page in copiedNotebook.Pages.Where(page => !loadedPageIDs.Contains(page.ID)))
                    {
                        storedNotebook.Pages.Add(page);
                    }
                    var orderedPages = storedNotebook.Pages.OrderBy(x => x.PageNumber).ToList();
                    storedNotebook.Pages = new ObservableCollection<CLPPage>(orderedPages);
                    notebookToUse = storedNotebook;
                }
            }

            foreach(var page in notebookToUse.Pages)
            {
                foreach(var notebookName in AvailableLocalNotebookNames)
                {
                    var notebookInfo = notebookName.Split(';');
                    if(notebookInfo.Length != 4 ||
                       notebookInfo[3] == Person.Author.ID ||
                       notebookInfo[3] == Person.Emily.ID ||
                       notebookInfo[3] == Person.EmilyProjector.ID)
                    {
                        continue;
                    }

                    var folderPath = Path.Combine(App.NotebookCacheDirectory, notebookName);
                    if(!Directory.Exists(folderPath))
                    {
                        continue;
                    }

                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    if(!Directory.Exists(submissionsPath))
                    {
                        continue;
                    }

                    var submissionPaths = Directory.EnumerateFiles(submissionsPath, "*.xml");

                    foreach(var submissionPath in submissionPaths)
                    {
                        var submissionFileName = Path.GetFileNameWithoutExtension(submissionPath);
                        var submissionInfo = submissionFileName.Split(';');
                        if(submissionInfo.Length == 5 &&
                           submissionInfo[2] == page.ID &&
                           submissionInfo[4] != "0")
                        {
                            var submission = Load<CLPPage>(submissionPath, SerializationMode.Xml);
                            page.Submissions.Add(submission);
                        }
                    }
                }
            }
            App.MainWindowViewModel.OpenNotebooks.Add(notebookToUse);
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebookToUse);
            App.MainWindowViewModel.AvailableUsers = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
        }

        public static string GetNotebookFolderPathByCompositeID(string id, string ownerID)
        {
            var notebookFolderPaths = Directory.GetDirectories(App.NotebookCacheDirectory);
            return (from notebookFolderPath in notebookFolderPaths
                    let notebookInfo = notebookFolderPath.Split(';')
                    where notebookInfo.Length == 4
                    let notebookID = notebookInfo[1]
                    let notebookOwnerID = notebookInfo[3]
                    where notebookID == id && notebookOwnerID == ownerID
                    select notebookFolderPath).FirstOrDefault();
        }

        #endregion //Static Methods

        #region Temp Methods

        public static void ConvertStudentIDsToCompactIDs()
        {
            var classPeriod = ClassSubject.OpenClassSubject(@"C:\Users\Steve\Desktop\CacheT\Classes\ClassSubject;AAAAABERAAAAAAAAAAAAAQ.xml");
            foreach(var student in classPeriod.StudentList)
            {
                student.ID = new Guid(student.ID).ToCompactID();
            }
            File.Delete(@"C:\Users\Steve\Desktop\CacheT\Classes\ClassSubject;AAAAABERAAAAAAAAAAAAAQ.xml");
            classPeriod.SaveClassSubject(@"C:\Users\Steve\Desktop\CacheT\Classes\ClassSubject;AAAAABERAAAAAAAAAAAAAQ.xml");
        }

        public static void GenerateSubmissionsFromOriginals()
        {
            foreach(var notebookName in AvailableLocalNotebookNames)
            {
                var notebookInfo = notebookName.Split(';');
                if(notebookInfo.Length != 4 ||
                   notebookInfo[2] == Person.Author.ID ||
                   notebookInfo[2] == Person.Emily.ID ||
                   notebookInfo[2] == Person.EmilyProjector.ID)
                {
                    continue;
                }

                var folderPath = Path.Combine(App.NotebookCacheDirectory, notebookName);
                if(!Directory.Exists(folderPath))
                {
                    MessageBox.Show("Notebook doesn't exist");
                    return;
                }

                var notebook = Notebook.OpenNotebook(folderPath);

                if(notebook == null)
                {
                    MessageBox.Show("Notebook could not be opened. Check error log.");
                    return;
                }

                var submissionFolderPath = Path.Combine(folderPath, "Pages", "Submissions");
                if(!Directory.Exists(submissionFolderPath))
                {
                    Directory.CreateDirectory(submissionFolderPath);
                }

                var submissions = new List<CLPPage>();
                foreach(var page in notebook.Pages)
                {
                    page.VersionIndex = 1;
                    page.LastVersionIndex = 1;
                    page.SubmissionTime = notebook.LastSavedDate;
                    foreach(var pageObject in page.PageObjects)
                    {
                        if(pageObject.OwnerID == page.OwnerID)
                        {
                            pageObject.VersionIndex = 1;
                            pageObject.LastVersionIndex = 1;
                        }

                        pageObject.ParentPageVersionIndex = 1;
                    }
                    foreach(var serializedStroke in page.SerializedStrokes)
                    {
                        if(serializedStroke.PersonID != page.OwnerID)
                        {
                            continue;
                        }
                        serializedStroke.VersionIndex = 1;
                    }

                    var submissionFilePath = Catel.IO.Path.Combine(submissionFolderPath, "Page;" + page.PageNumber + ";" + page.ID + ";" + page.OwnerID + ";" + page.VersionIndex + ".xml");
                    page.ToXML(submissionFilePath, false);
                }
            }
        }

        #endregion //Temp Methods
    }
}