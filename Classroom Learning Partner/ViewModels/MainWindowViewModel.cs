using System;
using System.Collections.ObjectModel;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner";

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
        {
            TitleBarText = clpText;
            InitializeCommands();
            IsAuthoring = false;
            
            OpenNotebooks = new ObservableCollection<CLPNotebook>();
        }

        private void InitializeCommands()
        {
            SetInstructorCommand = new Command(OnSetInstructorCommandExecute);
            SetStudentCommand = new Command(OnSetStudentCommandExecute);
            SetProjectorCommand = new Command(OnSetProjectorCommandExecute);
            SetServerCommand = new Command(OnSetServerCommandExecute);

            ToggleDebugCommand = new Command(OnToggleDebugCommandExecute);
            ToggleExtrasCommand = new Command(OnToggleExtrasCommandExecute);
        }

        public override string Title { get { return "MainWindowVM"; } }

        #region Bindings

        /// <summary>
        /// Gets or sets the Title Bar text of the window.
        /// </summary>
        public string TitleBarText
        {
            get { return GetValue<string>(TitleBarTextProperty); }
            private set { SetValue(TitleBarTextProperty, value); }
        }

        /// <summary>
        /// Register the TitleBarText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TitleBarTextProperty = RegisterProperty("TitleBarText", typeof(string));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public RibbonViewModel Ribbon
        {
            get { return GetValue<RibbonViewModel>(RibbonProperty); }
            set { SetValue(RibbonProperty, value); }
        }

        /// <summary>
        /// Register the Ribbon property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RibbonProperty = RegisterProperty("Ribbon", typeof(RibbonViewModel), new RibbonViewModel());

        /// <summary>
        /// Gets or sets the current Workspace.
        /// </summary>
        public IWorkspaceViewModel SelectedWorkspace
        {
            get { return GetValue<IWorkspaceViewModel>(SelectedWorkspaceProperty); }
            set { SetValue(SelectedWorkspaceProperty, value); }
        }

        /// <summary>
        /// Register the SelectedWorkspace property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedWorkspaceProperty = RegisterProperty("SelectedWorkspace", typeof(IWorkspaceViewModel));

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
        /// Number of pages in the current notebook.
        /// </summary>
        public int NotebookPageCount
        {
            get { return GetValue<int>(NotebookPageCountProperty); }
            set { SetValue(NotebookPageCountProperty, value); }
        }

        public static readonly PropertyData NotebookPageCountProperty = RegisterProperty("NotebookPageCount", typeof(int));

        /// <summary>
        /// Index of the currently displayed page.
        /// </summary>
        public int CurrentPageIndex
        {
            get { return GetValue<int>(CurrentPageIndexProperty); }
            set { SetValue(CurrentPageIndexProperty, value); }
        }

        public static readonly PropertyData CurrentPageIndexProperty = RegisterProperty("CurrentPageIndex", typeof(int));

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
        public ObservableCollection<CLPNotebook> OpenNotebooks
        {
            get { return GetValue<ObservableCollection<CLPNotebook>>(OpenNotebooksProperty); }
            private set { SetValue(OpenNotebooksProperty, value); }
        }

        /// <summary>
        /// Register the OpenNotebooks property so it is known in the class.
        /// </summary>
        public static readonly PropertyData OpenNotebooksProperty = RegisterProperty("OpenNotebooks", typeof(ObservableCollection<CLPNotebook>));

        /// <summary>
        /// Gets or sets the Authoring flag.
        /// </summary>
        public bool IsAuthoring
        {
            get { return GetValue<bool>(IsAuthoringProperty); }
            set { SetValue(IsAuthoringProperty, value); }
        }

        /// <summary>
        /// Register the IsAuthoring property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAuthoringProperty = RegisterProperty("IsAuthoring", typeof(bool));

        #endregion //Properties

        #region Methods

        //Sets the text in the title bar of the window to inform the name of the open Notebook.
        public void SetTitleBarText(string notebookName)
        {
            if(notebookName == null || notebookName == "")
            {
                TitleBarText = clpText;
            }
            else
            {
                TitleBarText = notebookName + " - " + clpText;
            }
        }

        public void SetWorkspace()
        {
            IsAuthoring = false;
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Student:
                    SelectedWorkspace = new UserLoginWorkspaceViewModel();
                    break;
            }
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the SetInstructorCommand command.
        /// </summary>
        public Command SetInstructorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetInstructorCommand command is executed.
        /// </summary>
        private void OnSetInstructorCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Instructor;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetStudentCommand command.
        /// </summary>
        public Command SetStudentCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetStudentCommand command is executed.
        /// </summary>
        private void OnSetStudentCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Student;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetProjectorCommand command.
        /// </summary>
        public Command SetProjectorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetProjectorCommand command is executed.
        /// </summary>
        private void OnSetProjectorCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Projector;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetServerCommand command.
        /// </summary>
        public Command SetServerCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetServerCommand command is executed.
        /// </summary>
        private void OnSetServerCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Server;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the ToggleDebugCommand command.
        /// </summary>
        public Command ToggleDebugCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ToggleDebugCommand command is executed.
        /// </summary>
        private void OnToggleDebugCommandExecute()
        {
            Ribbon.DebugTabVisibility = Ribbon.DebugTabVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets the ToggleExtrasCommand command.
        /// </summary>
        public Command ToggleExtrasCommand { get; private set; }

        private void OnToggleExtrasCommandExecute()
        {
            Ribbon.ExtrasTabVisibility = Ribbon.ExtrasTabVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion //Commands
    }
}