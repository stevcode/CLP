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

        public override string Title { get { return "MainWindowVM"; } }

        private void InitializeCommands()
        {
            SetUserModeCommand = new Command<string>(OnSetUserModeCommandExecute);

            ToggleDebugCommand = new Command(OnToggleDebugCommandExecute);
            ToggleExtrasCommand = new Command(OnToggleExtrasCommandExecute);
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
            set { SetValue(IsAuthoringProperty, value); }
        }

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

        #endregion //Bindings

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<Notebook> OpenNotebooks
        {
            get { return GetValue<ObservableCollection<Notebook>>(OpenNotebooksProperty); }
            set { SetValue(OpenNotebooksProperty, value); }
        }

        public static readonly PropertyData OpenNotebooksProperty = RegisterProperty("OpenNotebooks", typeof(ObservableCollection<Notebook>), () => new ObservableCollection<Notebook>());

        #endregion //Properties

        #region Methods

        public void SetWorkspace()
        {
            IsAuthoring = false;
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    Workspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
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
            switch (userMode)
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

        private void OnToggleDebugCommandExecute()
        {
            Ribbon.DebugTabVisibility = Ribbon.DebugTabVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Toggles Extras Tab.
        /// </summary>
        public Command ToggleExtrasCommand { get; private set; }

        private void OnToggleExtrasCommandExecute()
        {
            Ribbon.ExtrasTabVisibility = Ribbon.ExtrasTabVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion //Commands

        #region Static Methods

        public static List<string> AvailableNotebookNames
        {
            get
            {
                // TODO: DATABASE - Attempt to grab names from database
                var directoryInfo = new DirectoryInfo(App.NotebookCacheDirectory);
                return directoryInfo.GetDirectories().Select(directory => directory.Name).ToList();
            }
        }

        public static void CreateNewNotebook()
        {
            var nameChooserLoop = true;

            while(nameChooserLoop)
            {
                var nameChooser = new NotebookNamerWindowView {Owner = Application.Current.MainWindow};
                nameChooser.ShowDialog();
                if(nameChooser.DialogResult == true)
                {
                    var notebookName = nameChooser.NotebookName.Text;
                    // TODO: Steve - sanitize notebook name
                    var folderPath = Path.Combine(App.NotebookCacheDirectory, notebookName);
                    if(!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        var newNotebook = new Notebook {Name = notebookName};
                        var newPage = new CLPPage();
                        newNotebook.AddCLPPageToNotebook(newPage);
                        var filePath = Path.Combine(folderPath, "notebook.xml");
                        newNotebook.ToXML(filePath);

                        App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(newNotebook);
                        App.MainWindowViewModel.IsAuthoring = true;
                        App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;

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

        public static void OpenNotebook(string notebookName, bool forceCache = false, bool forceDatabase = false)
        {

            var filePath = App.LocalCacheDirectory + @"\" + notebookName + @".clp";
            if(!File.Exists(filePath))
            {
                return;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Notebook notebook = null;

            // TODO: Entities
            //try
            //{
            //    notebook = ModelBase.Load<CLPNotebook>(filePath, SerializationMode.Binary);
            //}
            //catch(Exception ex)
            //{
            //    Logger.Instance.WriteToLog("[ERROR] - Notebook could not be loaded: " + ex.Message);
            //}

            //stopWatch.Stop();
            //Logger.Instance.WriteToLog("Time to OPEN notebook (In Seconds): " + stopWatch.ElapsedMilliseconds / 1000.0);

            //if(notebook == null)
            //{
            //    MessageBox.Show("Notebook could not be opened. Check error log.");
            //    return;
            //}


            //var stopWatch2 = new Stopwatch();
            //stopWatch2.Start();

            //notebook.NotebookName = notebookName;
            //App.MainWindowViewModel.CurrentNotebookName = notebookName;

            //foreach(var page in notebook.Pages)
            //{
            //    ACLPPageBase.Deserialize(page);
            //    if(!notebook.Submissions.ContainsKey(page.UniqueID))
            //    {
            //        continue;
            //    }
            //    foreach(var submission in notebook.Submissions[page.UniqueID])
            //    {
            //        ACLPPageBase.Deserialize(submission);
            //    }
            //}

            //stopWatch2.Stop();
            //Logger.Instance.WriteToLog("Time to DESERIALIZE PAGES in notebook (In Seconds): " + stopWatch2.ElapsedMilliseconds / 1000.0);

            //var stopWatch3 = new Stopwatch();
            //stopWatch3.Start();

            //notebook.InitializeAfterDeserialize();

            //stopWatch3.Stop();
            //Logger.Instance.WriteToLog("Time to INITIALIZE notebook (In Seconds): " + stopWatch3.ElapsedMilliseconds / 1000.0);

            //var count = 0;
            //foreach(var otherNotebook in App.MainWindowViewModel.OpenNotebooks.Where(otherNotebook => otherNotebook.UniqueID == notebook.UniqueID && otherNotebook.NotebookName == notebook.NotebookName)) 
            //{
            //    App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(otherNotebook);
            //    count++;
            //    break;
            //}

            //if(count == 0)
            //{
            //    App.MainWindowViewModel.OpenNotebooks.Add(notebook);
            //    if(App.CurrentUserMode == App.UserMode.Instructor ||
            //       App.CurrentUserMode == App.UserMode.Student ||
            //       App.CurrentUserMode == App.UserMode.Projector)
            //    {
            //        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
            //    }
            //}

            //if(notebook.LastSavedTime != null)
            //{
            //    App.MainWindowViewModel.LastSavedTime = notebook.LastSavedTime.ToString("yyyy/MM/dd - HH:mm:ss");
            //}

            //if(App.CurrentUserMode == App.UserMode.Student)
            //{
            //    // _autoSaveTimer.Start();
            //}
        }

        #endregion //Static Methods
    }
}