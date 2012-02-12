using System;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
        {
            CLPService = new CLPServiceAgent();
            SetTitleBarText("Starting Up");
            IsAuthoring = false;


            SetInstructorCommand = new Command(OnSetInstructorCommandExecute);
            SetStudentCommand = new Command(OnSetStudentCommandExecute);
            SetProjectorCommand = new Command(OnSetProjectorCommandExecute);
            SetServerCommand = new Command(OnSetServerCommandExecute);
        }

        private ICLPServiceAgent CLPService { get; set; }

        #region NonRibbon Items

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

        #endregion //Bindings

        #region Properties

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

        //Sets the text in the title bar of the window, endText can add optional information
        public void SetTitleBarText(string endText)
        {
            string isOnline = "Disconnected";
            if (App.Peer.OnlineStatusHandler.IsOnline)
            {
                isOnline = "Connected";
            }
            TitleBarText = clpText + "Logged In As: " + App.Peer.UserName + " Connection Status: " + isOnline + " " + endText;
        }

        public void SetWorkspace()
        {
            IsAuthoring = false;

            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    SelectedWorkspace = new ServerWorkspaceViewModel();
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

        #endregion //Commands

        #endregion //NonRibbon Items

        #region Ribbon



        #endregion //Ribbon
    }
}