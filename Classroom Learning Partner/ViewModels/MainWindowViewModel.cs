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

        private void SetWorkspace()
        {
            IsAuthoring = false;

            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    App.MainWindowViewModel.Workspace = new ServerWorkspaceViewModel();
                    break;
                case App.UserMode.Instructor:
                    App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    App.MainWindowViewModel.Workspace = new ProjectorWorkspaceViewModel();
                    break;
                case App.UserMode.Student:
                    App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel();
                    break;
            }
        }

        #endregion //Methods

        #region Commands

        private RelayCommand _setInstructorCommand;

        /// <summary>
        /// Gets the SetInstructorCommand.
        /// </summary>
        public RelayCommand SetInstructorCommand
        {
            get
            {
                return _setInstructorCommand
                    ?? (_setInstructorCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.CurrentUserMode = App.UserMode.Instructor;
                                              CLPService.SetWorkspace();
                                          }));
            }
        }

        private RelayCommand _setStudentCommand;

        /// <summary>
        /// Gets the SetStudentCommand.
        /// </summary>
        public RelayCommand SetStudentCommand
        {
            get
            {
                return _setStudentCommand
                    ?? (_setStudentCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.CurrentUserMode = App.UserMode.Student;
                                              CLPService.SetWorkspace();
                                          }));
            }
        }

        private RelayCommand _setProjectorCommand;

        /// <summary>
        /// Gets the SetProjectorCommand.
        /// </summary>
        public RelayCommand SetProjectorCommand
        {
            get
            {
                return _setProjectorCommand
                    ?? (_setProjectorCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.CurrentUserMode = App.UserMode.Projector;
                                              CLPService.SetWorkspace();
                                          }));
            }
        }

        private RelayCommand _setServerCommand;

        /// <summary>
        /// Gets the SetServerCommand.
        /// </summary>
        public RelayCommand SetServerCommand
        {
            get
            {
                return _setServerCommand
                    ?? (_setServerCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.CurrentUserMode = App.UserMode.Server;
                                              CLPService.SetWorkspace();
                                          }));
            }
        }

        #endregion //Commands
        #endregion //NonRibbon Items
    }
}