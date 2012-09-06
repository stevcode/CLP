using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
//using Classroom_Learning_Partner.Resources;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using Microsoft.Windows.Controls.Ribbon;
using System.Security.Cryptography;

namespace Classroom_Learning_Partner.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
            : base()
        {
            SetTitleBarText("Starting Up");
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

        //Sets the text in the title bar of the window, endText can add optional information
        public void SetTitleBarText(string endText)
        {
            string isOnline = "Disconnected";
            string userName = "";
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    userName = "ServerMode";
                    break;
                case App.UserMode.Instructor:
                    userName = "InstructorMode";
                    break;
                case App.UserMode.Projector:
                    userName = "ProjectorMode";
                    break;
                case App.UserMode.Student:
                    userName = "No One";
                    break;
                default:
                    break;
            }
            
            if (App.Peer != null)
            {
                if (App.Peer.OnlineStatusHandler != null)
                {
                    if (App.Peer.OnlineStatusHandler.IsOnline)
                    {
                        isOnline = "Connected";
                    }
                }

                userName = App.Peer.UserName;
            }

            TitleBarText = clpText + "Logged In As: " + userName + ", Connection Status: " + isOnline + " | " + endText;
        }

        public void SetWorkspace()
        {
            IsAuthoring = false;
            IsMinimized = false;
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
                    IsMinimized = true;
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
        
    }
}