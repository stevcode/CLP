using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Collections.ObjectModel;
using System.IO;
using System;
using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class UserLoginWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the UserLoginWorkspaceViewModel class.
        /// </summary>
        public UserLoginWorkspaceViewModel()
        {
            CLPService = new CLPServiceAgent();
            //CLPService.ChooseNotebook(this);



            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNames.txt";

            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                string name;
                while (!((name = reader.ReadLine()) == null))
                {
                    UserNames.Add(name);
                }
            }
        }

        private ICLPServiceAgent CLPService { get; set; }

        #region Bindings

        private ObservableCollection<string> _userNames = new ObservableCollection<string>();
        public ObservableCollection<string> UserNames
        {
            get
            {
                return _userNames;
            }
        }

        #endregion //Bindings

        private RelayCommand<string> _logInCommand;

        /// <summary>
        /// Gets the SetPenCommand.
        /// </summary>
        public RelayCommand<string> LogInCommand
        {
            get
            {
                return _logInCommand
                    ?? (_logInCommand = new RelayCommand<string>(
                                          (userName) =>
                                          {
                                              App.Peer.UserName = userName;
                                              
                                              App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                                          }));
            }
        }
    }
}