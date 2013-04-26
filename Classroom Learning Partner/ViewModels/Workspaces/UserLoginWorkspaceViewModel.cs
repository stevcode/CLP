﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Catel.Data;
using Catel.MVVM;

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
            App.Network.CurrentUser.FullName = userName.Split(new char[] { ',' })[0];
            App.Network.CurrentUser.GroupName = userName.Split(new char[] { ',' })[1];
            App.Network.CurrentGroup.GroupName = userName.Split(new char[] { ',' })[1];

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                int i = App.Network.DiscoveredInstructors.Addresses.Count();
                while(App.Network.DiscoveredInstructors.Addresses.Count() < 1 || App.Network.CurrentUser.CurrentMachineAddress == null || App.Network.InstructorProxy == null)
                {
                    Thread.Sleep(1000);
                }

                if(App.Network.InstructorProxy != null)
                {
                    try
                    {
                        App.Network.InstructorProxy.StudentLogin(App.Network.CurrentUser);
                        App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " + App.Network.CurrentUser.FullName;
                    }
                    catch(System.Exception)
                    {
                        Logger.Instance.WriteToLog("Problem Logging In as " + App.Network.CurrentUser.FullName);
                    }
                }
                else
                {
                    Console.WriteLine("Instructor NOT Available");
                }
            }).Start();

            App.MainWindowViewModel.SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
        }

        public string WorkspaceName
        {
            get { return "UserLoginWorkspace"; }
        }
    }
}