using Classroom_Learning_Partner.ViewModels.Workspaces;
using System;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            CLPService = new CLPServiceAgent();
            TitleBarText = "Disconnected";
        }

        private ICLPServiceAgent CLPService { get; set; }

        #region Bindings

        /// <summary>
        /// The <see cref="TitleBarText" /> property's name.
        /// </summary>
        public const string TitleBarTextPropertyName = "TitleBarText";

        private string _titleBarText = "";

        /// <summary>
        /// Sets and gets the TitleBarText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TitleBarText
        {
            get
            {
                return _titleBarText;
            }

            set
            {
                if (_titleBarText == value)
                {
                    return;
                }

                _titleBarText = clpText + value;
                RaisePropertyChanged(TitleBarTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Workspace" /> property's name.
        /// </summary>
        public const string WorkspacePropertyName = "Workspace";

        private ViewModelBase _workspace = new BlankWorkspaceViewModel();

        /// <summary>
        /// Sets and gets the Workspace property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ViewModelBase Workspace
        {
            get
            {
                return _workspace;
            }

            set
            {
                if (_workspace == value)
                {
                    return;
                }
                if (_workspace != null && _workspace is IDisposable)
                {
                    (_workspace as IDisposable).Dispose();
                }
                
                _workspace = value;
                RaisePropertyChanged(WorkspacePropertyName);
            }
        }


        private RibbonViewModel _ribbon = new RibbonViewModel();
        public RibbonViewModel Ribbon
        {
            get
            {
                return _ribbon;
            }
        }

        #endregion //Bindings

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

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}