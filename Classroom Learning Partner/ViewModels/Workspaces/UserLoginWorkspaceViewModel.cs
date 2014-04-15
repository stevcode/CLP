using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class UserLoginWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the UserLoginWorkspaceViewModel class.
        /// </summary>
        public UserLoginWorkspaceViewModel()
        {
            LogInCommand = new Command<Person>(OnLogInCommandExecute);

            // TODO: DATABASE - inject IPersonService that can grab the available student names?
        }

        public override string Title
        {
            get { return "UserLoginWorkspaceVM"; }
        }

        /// <summary>
        /// Gets the LogInCommand command.
        /// </summary>
        public Command<Person> LogInCommand { get; private set; }

        private void OnLogInCommandExecute(Person user)
        {
            App.MainWindowViewModel.CurrentUser = user;

            new Thread(() =>
                       {
                           Thread.CurrentThread.IsBackground = true;
                           while(!App.Network.DiscoveredInstructors.Addresses.Any() ||
                                 App.Network.CurrentMachineAddress == null ||
                                 App.Network.InstructorProxy == null)
                           {
                               Thread.Sleep(1000);
                           }

                           if(App.Network.InstructorProxy != null)
                           {
                               try
                               {
                                   var zippedNotebook = App.Network.InstructorProxy.StudentLogin(App.MainWindowViewModel.CurrentUser.ID,
                                                                                                 App.Network.CurrentMachineName,
                                                                                                 App.Network.CurrentMachineAddress);
                                   var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
                                   var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;
                                   if(notebook == null)
                                   {
                                       Logger.Instance.WriteToLog("Failed to load notebook.");
                                       return;
                                   }
                                   notebook.CurrentPage = notebook.Pages.First();

                                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                              (DispatcherOperationCallback)delegate
                                                                                                           {
                                                                                                               App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                                                                                                               App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
                                                                                                               App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " +
                                                                                                                                                      App.MainWindowViewModel.CurrentUser.FullName;

                                                                                                               return null;
                                                                                                           },
                                                                              null);
                               }
                               catch(Exception)
                               {
                                   Logger.Instance.WriteToLog("Problem Logging In as " + App.MainWindowViewModel.CurrentUser.FullName);
                               }
                           }
                           else
                           {
                               Console.WriteLine("Instructor NOT Available");
                           }
                       }).Start();
        }
    }
}