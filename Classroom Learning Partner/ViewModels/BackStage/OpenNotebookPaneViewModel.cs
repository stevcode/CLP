using System;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using Catel.IO;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OpenNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OpenNotebookPaneViewModel()
        {
            InitializeCommands();
            AvailableCacheNames.AddRange(LoadedNotebookService.AvailableLocalCacheNames);
            SelectedCacheName = AvailableCacheNames.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute, OnOpenNotebookCanExecute);
            StartClassPeriodCommand = new Command(OnStartClassPeriodCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Open Notebook"; }
        }

        /// <summary>List of available Cache names.</summary>
        public ObservableCollection<string> AvailableCacheNames
        {
            get { return GetValue<ObservableCollection<string>>(AvailableCacheNamesProperty); }
            set { SetValue(AvailableCacheNamesProperty, value); }
        }

        public static readonly PropertyData AvailableCacheNamesProperty = RegisterProperty("AvailableCacheNames",
                                                                                           typeof (ObservableCollection<string>),
                                                                                           () => new ObservableCollection<string>());

        /// <summary>Selected Cache Name.</summary>
        public string SelectedCacheName
        {
            get { return GetValue<string>(SelectedCacheNameProperty); }
            set
            {
                SetValue(SelectedCacheNameProperty, value);
                if (value != null)
                {
                    SelectedCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), value);
                    AvailableNotebooks.Clear();
                    AvailableNotebooks.AddRange(NotebookService.GetAvailableNotebookNameCompositesInCache(SelectedCacheDirectory));
                    SelectedNotebook = AvailableNotebooks.FirstOrDefault();
                }
            }
        }

        public static readonly PropertyData SelectedCacheNameProperty = RegisterProperty("SelectedCacheName", typeof (string), string.Empty);

        /// <summary>Path of the Selected Cache's Directory.</summary>
        public string SelectedCacheDirectory
        {
            get { return GetValue<string>(SelectedCacheDirectoryProperty); }
            set { SetValue(SelectedCacheDirectoryProperty, value); }
        }

        public static readonly PropertyData SelectedCacheDirectoryProperty = RegisterProperty("SelectedCacheDirectory", typeof (string), string.Empty);

        /// <summary>Available notebooks in the currently selected Cache.</summary>
        public ObservableCollection<NotebookNameComposite> AvailableNotebooks
        {
            get { return GetValue<ObservableCollection<NotebookNameComposite>>(AvailableNotebooksProperty); }
            set { SetValue(AvailableNotebooksProperty, value); }
        }

        public static readonly PropertyData AvailableNotebooksProperty = RegisterProperty("AvailableNotebooks",
                                                                                          typeof (ObservableCollection<NotebookNameComposite>),
                                                                                          () => new ObservableCollection<NotebookNameComposite>());

        /// <summary>Currently selected Notebook.</summary>
        public NotebookNameComposite SelectedNotebook
        {
            get { return GetValue<NotebookNameComposite>(SelectedNotebookProperty); }
            set { SetValue(SelectedNotebookProperty, value); }
        }

        public static readonly PropertyData SelectedNotebookProperty = RegisterProperty("SelectedNotebook", typeof (NotebookNameComposite));

        #endregion //Bindings

        #region Commands

        /// <summary>Opens selected notebook.</summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {
            PleaseWaitHelper.Show(() => LoadedNotebookService.OpenLocalNotebook(SelectedNotebook, SelectedCacheDirectory), null, "Loading Notebook");

            //if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Student)
            //{
            //    return;
            //}

            //if (App.Network.InstructorProxy == null)
            //{
            //    return;
            //}

            //var connectionString = App.Network.InstructorProxy.StudentLogin(App.MainWindowViewModel.CurrentUser.FullName,
            //                                         App.MainWindowViewModel.CurrentUser.ID,
            //                                         App.Network.CurrentMachineName,
            //                                         App.Network.CurrentMachineAddress);

            //if (connectionString == "connected")
            //{
            //    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.LoggedIn;
            //}
        }

        private bool OnOpenNotebookCanExecute() { return SelectedNotebook != null; }

        /// <summary>Starts the closest <see cref="ClassPeriod" />.</summary>
        public Command StartClassPeriodCommand { get; private set; }

        private void OnStartClassPeriodCommandExecute()
        {
            LoadedNotebookService.StartSoonestClassPeriod(SelectedCacheDirectory);
            //    LoadedNotebookService.StartLocalClassPeriod(, SelectedCacheDirectory);
        }

        #endregion //Commands
    }
}