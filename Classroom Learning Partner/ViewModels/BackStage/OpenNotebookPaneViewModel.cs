using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OpenNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OpenNotebookPaneViewModel(IDataService dataService, IRoleService roleService)
            : base(dataService, roleService)
        {
            AvailableZipContainerFileNames = _dataService.AvailableZipContainerFileInfos.Select(fi => Path.GetFileNameWithoutExtension(fi.Name)).ToObservableCollection();
            SelectedExistingZipContainerFileName = AvailableZipContainerFileNames.FirstOrDefault();

            InitializeCommands();
        }

        #endregion //Constructor

        #region Model

        /// <summary>Loaded ClassRoster of the SelectedExistingZipContainerFileName.</summary>
        [Model(SupportIEditableObject = false)]
        public ClassRoster SelectedClassRoster
        {
            get { return GetValue<ClassRoster>(SelectedClassRosterProperty); }
            set { SetValue(SelectedClassRosterProperty, value); }
        }

        public static readonly PropertyData SelectedClassRosterProperty = RegisterProperty("SelectedClassRoster", typeof(ClassRoster));

        /// <summary>List of all the internal and connected NotebookSets available to this class.</summary>
        [ViewModelToModel("SelectedClassRoster")]
        public ObservableCollection<NotebookSet> ListOfNotebookSets
        {
            get { return GetValue<ObservableCollection<NotebookSet>>(ListOfNotebookSetsProperty); }
            set { SetValue(ListOfNotebookSetsProperty, value); }
        }

        public static readonly PropertyData ListOfNotebookSetsProperty = RegisterProperty("ListOfNotebookSets", typeof(ObservableCollection<NotebookSet>), () => new ObservableCollection<NotebookSet>());

        #endregion // Model

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText => "Open Notebook";

        /// <summary>File name (without extension) of the ZipContainer currently selected.</summary>
        public string SelectedExistingZipContainerFileName
        {
            get { return GetValue<string>(SelectedExistingZipContainerFileNameProperty); }
            set { SetValue(SelectedExistingZipContainerFileNameProperty, value); }
        }

        public static readonly PropertyData SelectedExistingZipContainerFileNameProperty = RegisterProperty("SelectedExistingZipContainerFileName", typeof(string), string.Empty);

        /// <summary>List of all the available ZipContainers in the default Cache location.</summary>
        public ObservableCollection<string> AvailableZipContainerFileNames
        {
            get { return GetValue<ObservableCollection<string>>(AvailableZipContainerFileNamesProperty); }
            set { SetValue(AvailableZipContainerFileNamesProperty, value); }
        }

        public static readonly PropertyData AvailableZipContainerFileNamesProperty = RegisterProperty("AvailableZipContainerFileNames",
                                                                                                      typeof(ObservableCollection<string>),
                                                                                                      () => new ObservableCollection<string>());

        /// <summary>The selected Notebook Set in the class.</summary>
        public NotebookSet SelectedNotebookSet
        {
            get { return GetValue<NotebookSet>(SelectedNotebookSetProperty); }
            set { SetValue(SelectedNotebookSetProperty, value); }
        }

        public static readonly PropertyData SelectedNotebookSetProperty = RegisterProperty("SelectedNotebookSet", typeof(NotebookSet));

        /// <summary>Loaded Notebooks from the selected NotebookSet.</summary>
        public ObservableCollection<Notebook> NotebooksInSelectedNotebookSet
        {
            get { return GetValue<ObservableCollection<Notebook>>(NotebooksInSelectedNotebookSetProperty); }
            set { SetValue(NotebooksInSelectedNotebookSetProperty, value); }
        }

        public static readonly PropertyData NotebooksInSelectedNotebookSetProperty = RegisterProperty("NotebooksInSelectedNotebookSet", typeof(ObservableCollection<Notebook>), () => new ObservableCollection<Notebook>());

        /// <summary>Model of this ViewModel.</summary>
        public Notebook SelectedNotebook
        {
            get { return GetValue<Notebook>(SelectedNotebookProperty); }
            set { SetValue(SelectedNotebookProperty, value); }
        }

        public static readonly PropertyData SelectedNotebookProperty = RegisterProperty("SelectedNotebook", typeof(Notebook));


        /// <summary>Toggles the loading of submissions when opening a notebook.</summary>
        public bool IsIncludeSubmissionsChecked
        {
            get { return GetValue<bool>(IsIncludeSubmissionsCheckedProperty); }
            set { SetValue(IsIncludeSubmissionsCheckedProperty, value); }
        }

        public static readonly PropertyData IsIncludeSubmissionsCheckedProperty = RegisterProperty("IsIncludeSubmissionsChecked", typeof(bool), true);

        public string SelectedZipContainerFullFilePath => $"{_dataService.CurrentCacheFolderPath}\\{SelectedExistingZipContainerFileName}.{AInternalZipEntryFile.CONTAINER_EXTENSION}";

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute, OnOpenNotebookCanExecute);
            OpenPageRangeCommand = new Command(OnOpenPageRangeCommandExecute, OnOpenNotebookCanExecute);
            OpenSessionCommand = new TaskCommand(OnOpenSessionCommandExecuteAsync, OnOpenNotebookCanExecute);
            OpenCacheFolderCommand = new Command(OnOpenCacheFolderCommandExecute);
        }

        /// <summary>Opens selected notebook.</summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {
            _dataService.LoadNotebook(SelectedNotebook, new List<int>(), IsIncludeSubmissionsChecked);

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

        /// <summary>Opens a range of pages in a notebook.</summary>
        public Command OpenPageRangeCommand { get; private set; }

        private void OnOpenPageRangeCommandExecute()
        {
            var textInputViewModel = new TextInputViewModel();
            var textInputView = new TextInputView(textInputViewModel);
            textInputView.ShowDialog();

            if (textInputView.DialogResult == null ||
                textInputView.DialogResult != true ||
                string.IsNullOrEmpty(textInputViewModel.InputText))
            {
                return;
            }

            var pageNumbersToOpen = RangeHelper.ParseStringToIntNumbers(textInputViewModel.InputText).ToList();
            if (!pageNumbersToOpen.Any())
            {
                return;
            }

            _dataService.LoadNotebook(SelectedNotebook, pageNumbersToOpen, IsIncludeSubmissionsChecked);

        }

        private bool OnOpenNotebookCanExecute()
        {
            return SelectedNotebook != null;
        }

        /// <summary>Opens a session.</summary>
        public TaskCommand OpenSessionCommand { get; private set; }

        private async Task OnOpenSessionCommandExecuteAsync()
        {
            var viewModel = this.CreateViewModel<SessionsViewModel>(SelectedNotebook);
            viewModel.IsOpening = true;
            if (!(await viewModel.ShowWindowAsDialogAsync() ?? false))
            {
                return;
            }

            var session = viewModel.CurrentSession;
            var pageNumbersToOpen = RangeHelper.ParseStringToIntNumbers(session.PageNumbers).ToList();
            if (!pageNumbersToOpen.Any())
            {
                return;
            }

            var startingPageID = session.StartingPageID;

            _dataService.LoadNotebook(SelectedNotebook, pageNumbersToOpen, IsIncludeSubmissionsChecked, startingPageID);
        }

        /// <summary>Opens the cache folder in Windows Exploerer.</summary>
        public Command OpenCacheFolderCommand { get; private set; }

        private void OnOpenCacheFolderCommandExecute()
        {
            var cacheFolder = _dataService.CurrentCacheFolderPath;
            const string EXPLORER_PROCESS_NAME = "explorer";

            Process.Start(EXPLORER_PROCESS_NAME, cacheFolder);
        }

        #endregion //Commands

        #region Overrides of ViewModelBase

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedExistingZipContainerFileName))
            {
                var selectedExistingZipContainerFileName = e.NewValue as string;
                if (!string.IsNullOrWhiteSpace(selectedExistingZipContainerFileName))
                {
                    var classRoster = DataService.LoadClassRosterFromCLPContainer(SelectedZipContainerFullFilePath);
                    if (classRoster == null)
                    {
                        MessageBox.Show("Problem loading the selected class.");
                        return;
                    }

                    SelectedClassRoster = classRoster;
                    SelectedNotebookSet = SelectedClassRoster.ListOfNotebookSets.FirstOrDefault();
                }
            }

            if (e.PropertyName == nameof(SelectedNotebookSet))
            {
                var selectedNotebookSet = e.NewValue as NotebookSet;
                if (selectedNotebookSet != null)
                {
                    // TODO: Use notebookSet.NotebookID/.IsConnected to search connected containers
                    var notebooks = DataService.LoadAllNotebooksFromCLPContainer(SelectedZipContainerFullFilePath);
                    NotebooksInSelectedNotebookSet = notebooks.Where(n => n.ID == selectedNotebookSet.NotebookID).OrderBy(n => n.OwnerID == Person.AUTHOR_ID ? 0 : 1).ThenBy(n => !n.Owner.IsStudent ? 0 : 1).ThenBy(n => n.Owner.FullName).ToObservableCollection();
                    SelectedNotebook = NotebooksInSelectedNotebookSet.FirstOrDefault();
                }
            }

            base.OnPropertyChanged(e);
        }

        #endregion
    }
}