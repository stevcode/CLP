using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;
using Microsoft.Ink;

namespace Classroom_Learning_Partner.ViewModels
{
    public class UserLoginWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the UserLoginWorkspaceViewModel class.
        /// </summary>
        public UserLoginWorkspaceViewModel()
        {
            AllowLoginCommand = new Command(OnAllowLoginCommandExecute);
            LogInCommand = new Command<Person>(OnLogInCommandExecute);

            // TODO: DATABASE - inject IPersonService that can grab the available student names?
        }

        public override string Title
        {
            get { return "UserLoginWorkspaceVM"; }
        }

        /// <summary>
        /// Initially hides student names to avoid accidental clicks.
        /// </summary>
        public bool IsAllowLoginPromptActivated
        {
            get { return GetValue<bool>(IsAllowLoginPromptActivatedProperty); }
            set { SetValue(IsAllowLoginPromptActivatedProperty, value); }
        }

        public static readonly PropertyData IsAllowLoginPromptActivatedProperty = RegisterProperty("IsAllowLoginPromptActivated", typeof(bool), true);

        /// <summary>
        /// Toggles the safety screen off and allows students to log in.
        /// </summary>
        public Command AllowLoginCommand { get; private set; }

        private void OnAllowLoginCommandExecute() { IsAllowLoginPromptActivated = false; }

        /// <summary>
        /// Gets the LogInCommand command.
        /// </summary>
        public Command<Person> LogInCommand { get; private set; }

        private bool _isLoggingIn;
        private void OnLogInCommandExecute(Person user)
        {
            if(_isLoggingIn)
            {
                return;
            }

            App.MainWindowViewModel.CurrentUser = user;
            _isLoggingIn = true;
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
                                   if(String.IsNullOrEmpty(zippedNotebook))
                                   {
                                       Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                              (DispatcherOperationCallback)delegate
                                                                                                           {
                                                                                                               App.MainWindowViewModel.CurrentUser.IsConnected = true;
                                                                                                               App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                                                                                                               App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " +
                                                                                                                                                      App.MainWindowViewModel.CurrentUser.FullName;

                                                                                                               return null;
                                                                                                           },
                                                                              null);
                                       return;
                                   }

                                   var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
                                   var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;
                                   if(notebook == null)
                                   {
                                       Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                              (DispatcherOperationCallback)delegate
                                                                                                           {
                                                                                                               App.MainWindowViewModel.CurrentUser.IsConnected = true;
                                                                                                               App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                                                                                                               App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " +
                                                                                                                                                      App.MainWindowViewModel.CurrentUser.FullName;

                                                                                                               return null;
                                                                                                           },
                                                                              null);
                                       return;
                                   }

                                   notebook.CurrentPage = notebook.Pages.First();
                                   foreach(var page in notebook.Pages)
                                   {
                                       page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                                       page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
                                   }
                                   App.ResetCache();

                                   var imageHashIDs = notebook.ImagePoolHashIDs;
                                   if(Directory.Exists(App.ImageCacheDirectory))
                                   {
                                       var localImageFilePaths = Directory.EnumerateFiles(App.ImageCacheDirectory);
                                       foreach(var localImageFilePath in localImageFilePaths)
                                       {
                                           var imageHashID = Path.GetFileNameWithoutExtension(localImageFilePath);
                                           if(imageHashIDs.Contains(imageHashID))
                                           {
                                               imageHashIDs.Remove(imageHashID);
                                           }
                                       }
                                   }
                                   var imageList = App.Network.InstructorProxy.SendImages(imageHashIDs);
                                   foreach(var byteSource in imageList)
                                   {
                                       var imagePath = Path.Combine(App.ImageCacheDirectory, byteSource.Key);
                                       File.WriteAllBytes(imagePath, byteSource.Value);
                                   }

                                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                              (DispatcherOperationCallback)delegate
                                                                                                           {
                                                                                                               App.MainWindowViewModel.CurrentUser.IsConnected = true;
                                                                                                               App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                                                                                                               App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
                                                                                                               App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " +
                                                                                                                                                      App.MainWindowViewModel.CurrentUser.FullName;

                                                                                                               return null;
                                                                                                           },
                                                                              null);
                                   _isLoggingIn = false;
                               }
                               catch(Exception)
                               {
                                   Logger.Instance.WriteToLog("Problem Logging In as " + App.MainWindowViewModel.CurrentUser.FullName);
                                   _isLoggingIn = false;
                               }
                           }
                           else
                           {
                               Console.WriteLine("Instructor NOT Available");
                               _isLoggingIn = false;
                           }
                       }).Start();
        }

        private async void LogUserIn(Person user)
        {
            
        }
    }
}