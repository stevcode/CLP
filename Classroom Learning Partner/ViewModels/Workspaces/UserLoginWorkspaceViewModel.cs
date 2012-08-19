using Catel.MVVM;
using System;
using System.IO;
using System.Collections.ObjectModel;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels
{
    public class UserLoginWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the UserLoginWorkspaceViewModel class.
        /// </summary>
        public UserLoginWorkspaceViewModel()
            : base()
        {
            LogInCommand = new Command<string>(OnLogInCommandExecute);

            UserNames = new ObservableCollection<string>();
            //Steve - move to CLPService and grab from database
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNames.txt";

            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                string name;
                while (!((name = reader.ReadLine()) == null))
                {
                    UserNames.Add(name);
                }
                reader.Dispose();
            }
            else
            {
                for(int i = 1; i < 26; i++)
                {
                    UserNames.Add("Guest " + i.ToString());
                }
            }
        }

        public override string Title { get { return "UserLoginWorkspaceVM"; } }

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> UserNames
        {
            get { return GetValue<ObservableCollection<string>>(UserNamesProperty); }
            private set { SetValue(UserNamesProperty, value); }
        }

        /// <summary>
        /// Register the UserNames property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UserNamesProperty = RegisterProperty("UserNames", typeof(ObservableCollection<string>));

        #endregion //Bindings

        /// <summary>
        /// Gets the LogInCommand command.
        /// </summary>
        public Command<string> LogInCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the LogInCommand command is executed.
        /// </summary>
        private void OnLogInCommandExecute(string userName)
        {
            App.Peer.UserName = userName;
            App.MainWindowViewModel.SetTitleBarText("");
            App.MainWindowViewModel.SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
        }

        public string WorkspaceName
        {
            get { return "UserLoginWorkspace"; }
        }
    }
}