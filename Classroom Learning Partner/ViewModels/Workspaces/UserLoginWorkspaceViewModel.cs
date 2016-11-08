﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;
using NuGet;

//using Microsoft.Ink;

namespace Classroom_Learning_Partner.ViewModels
{
    public class UserLoginWorkspaceViewModel : ViewModelBase
    {
        #region Fields

        private readonly INetworkService _networkService;
        private readonly IDataService _dataService;

        #endregion // Fields

        #region Constructors

        /// <summary>Initializes a new instance of the UserLoginWorkspaceViewModel class.</summary>
        public UserLoginWorkspaceViewModel(INetworkService networkService, IDataService dataService)
        {
            _networkService = networkService;
            _dataService = dataService;

            InitializeCommands();
        }

        #endregion // Constructors

        #region Bindings

        /// <summary>Initially hides student names to avoid accidental clicks.</summary>
        public bool IsAllowLoginPromptActivated
        {
            get { return GetValue<bool>(IsAllowLoginPromptActivatedProperty); }
            set { SetValue(IsAllowLoginPromptActivatedProperty, value); }
        }

        public static readonly PropertyData IsAllowLoginPromptActivatedProperty = RegisterProperty("IsAllowLoginPromptActivated", typeof(bool), true);

        /// <summary>Loaded from the Class Roster</summary>
        public ObservableCollection<Person> AvailableStudents
        {
            get { return GetValue<ObservableCollection<Person>>(AvailableStudentsProperty); }
            set { SetValue(AvailableStudentsProperty, value); }
        }

        public static readonly PropertyData AvailableStudentsProperty = RegisterProperty("AvailableStudents", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        /// <summary>Lock to prevent double taps.</summary>
        public bool IsLoggingIn
        {
            get { return GetValue<bool>(IsLoggingInProperty); }
            set { SetValue(IsLoggingInProperty, value); }
        }

        public static readonly PropertyData IsLoggingInProperty = RegisterProperty("IsLoggingIn", typeof(bool), false);

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            AllowLoginCommand = new Command(OnAllowLoginCommandExecute);
            LogInCommand = new Command<Person>(OnLogInCommandExecute);
        }

        /// <summary>Toggles the safety screen off and allows students to log in.</summary>
        public Command AllowLoginCommand { get; private set; }

        private void OnAllowLoginCommandExecute()
        {
            IsAllowLoginPromptActivated = false;
        }

        /// <summary>Gets the LogInCommand command.</summary>
        public Command<Person> LogInCommand { get; private set; }

        private void OnLogInCommandExecute(Person user)
        {
            if (IsLoggingIn)
            {
                return;
            }
            
            IsLoggingIn = true;
            App.MainWindowViewModel.CurrentUser = user;
            _networkService.CurrentUser = user;
            // TODO: DataService for current user?
            if (_networkService.CurrentMachineAddress == null)
            {
                MessageBox.Show("Error logging in. Cannot find this machine's address.");
                return;
            }

            if (_networkService.InstructorProxy == null)
            {
                IsLoggingIn = false;
                return;
            }

            try
            {
                var connectionMessage = _networkService.InstructorProxy.StudentLogin(_networkService.CurrentUser.FullName,
                                                                                     _networkService.CurrentUser.ID,
                                                                                     _networkService.CurrentMachineName,
                                                                                     _networkService.CurrentMachineAddress);

                if (connectionMessage == InstructorService.MESSAGE_NO_DATA_SERVICE)
                {
                    IsLoggingIn = false;
                    return;
                }

                if (connectionMessage != InstructorService.MESSAGE_SUCCESSFUL_STUDENT_LOG_IN)
                {
                    MessageBox.Show($"Someone else is already logged in with this name from machine {connectionMessage}. Make sure you are logging in as the correct person.",
                                    "Attempted Incorrect Login",
                                    MessageBoxButton.OK);
                    IsLoggingIn = false;
                    return;
                }

                var notebookJson = _networkService.InstructorProxy.GetStudentNotebookJson(_networkService.CurrentUser.ID);
                switch (notebookJson)
                {
                    case InstructorService.MESSAGE_NO_DATA_SERVICE:
                        IsLoggingIn = false;
                        return;
                    case InstructorService.MESSAGE_STUDENT_NOT_IN_ROSTER:
                        IsLoggingIn = false;
                        return;
                    case InstructorService.MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER:
                        IsLoggingIn = false;
                        return;
                }

                if (string.IsNullOrWhiteSpace(notebookJson))
                {
                    IsLoggingIn = false;
                    return;
                }

                var notebook = AEntityBase.FromJsonString<Notebook>(notebookJson);
                if (notebook == null)
                {
                    IsLoggingIn = false;
                    return;
                }

                var pagesJson = _networkService.InstructorProxy.GetStudentNotebookPagesJson(_networkService.CurrentUser.ID);
                if (!pagesJson.Any())
                {
                    IsLoggingIn = false;
                    return;
                }

                var pages = pagesJson.Select(AEntityBase.FromJsonString<CLPPage>).OrderBy(p => p.PageNumber).ToList();

                var submissionsJson = _networkService.InstructorProxy.GetStudentPageSubmissionsJson(_networkService.CurrentUser.ID);
                if (!submissionsJson.Any())
                {
                    IsLoggingIn = false;
                    return;
                }

                foreach (var submissionJson in submissionsJson)
                {
                    var submission = AEntityBase.FromJsonString<CLPPage>(submissionJson);
                    var page = pages.FirstOrDefault(p => p.ID == submission.ID);
                    if (page != null)
                    {
                        page.Submissions.Add(submission);
                    }
                }

                notebook.Pages.AddRange(pages);

                UIHelper.RunOnUI(() => _dataService.SetCurrentNotebook(notebook));

                // TODO: Successfully connected, now download notebook, pages, and submissions.
                //var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
                //var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;
                //if (notebook == null)
                //{
                //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                //                                               (DispatcherOperationCallback)delegate
                //                                                                            {
                //                                                                                App.MainWindowViewModel.CurrentUser.IsConnected = true;
                //                                                                                App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " +
                //                                                                                                                       App.MainWindowViewModel.CurrentUser.FullName;

                //                                                                                return null;
                //                                                                            },
                //                                               null);
                //    return;
                //}

                //notebook.CurrentPage = notebook.Pages.First();
                //foreach (var page in notebook.Pages)
                //{
                //    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                //    page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
                //}
                //MainWindowViewModel.ResetCache();

                //var imageHashIDs = notebook.ImagePoolHashIDs;
                //if (Directory.Exists(MainWindowViewModel.ImageCacheDirectory))
                //{
                //    var localImageFilePaths = Directory.EnumerateFiles(MainWindowViewModel.ImageCacheDirectory);
                //    foreach (var localImageFilePath in localImageFilePaths)
                //    {
                //        var imageHashID = Path.GetFileNameWithoutExtension(localImageFilePath);
                //        if (imageHashIDs.Contains(imageHashID))
                //        {
                //            imageHashIDs.Remove(imageHashID);
                //        }
                //    }
                //}
                //var imageList = App.Network.InstructorProxy.SendImages(imageHashIDs);
                //foreach (var byteSource in imageList)
                //{
                //    var imagePath = Path.Combine(MainWindowViewModel.ImageCacheDirectory, byteSource.Key);
                //    File.WriteAllBytes(imagePath, byteSource.Value);
                //}

                //App.MainWindowViewModel.CurrentUser.IsConnected = true;
                //App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                //App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);
                //App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " + App.MainWindowViewModel.CurrentUser.FullName;
                IsLoggingIn = false;
            }
            catch (Exception)
            {
                IsLoggingIn = false;
            }
        }

        #endregion // Commands
    }
}