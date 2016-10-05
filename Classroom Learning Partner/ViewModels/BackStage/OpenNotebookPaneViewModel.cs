using System.Collections.ObjectModel;
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

        public OpenNotebookPaneViewModel()
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

        /// <summary>Name of the subject being taught.</summary>
        [ViewModelToModel("SelectedClassRoster")]
        public string SubjectName
        {
            get { return GetValue<string>(SubjectNameProperty); }
            set { SetValue(SubjectNameProperty, value); }
        }

        public static readonly PropertyData SubjectNameProperty = RegisterProperty("SubjectName", typeof(string));

        /// <summary>Grade Level of the subject being taught.</summary>
        [ViewModelToModel("SelectedClassRoster")]
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof(string));

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
        }

        /// <summary>Opens selected notebook.</summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {


            _dataService.LoadAllNotebookPages(SelectedNotebook);

            //PleaseWaitHelper.Show(() => _dataService.OpenNotebook(SelectedNotebook), null, "Loading Notebook");
            //var pageIDs = DataService.GetAllPageIDsInNotebook(SelectedNotebook);
            //PleaseWaitHelper.Show(() =>
            //                      {
            //                          _dataService.LoadPages(SelectedNotebook, pageIDs, true);
            //                          _dataService.LoadLocalSubmissions(SelectedNotebook, pageIDs, true);
            //                          if ((App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher || App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Projector) &&
            //                              IsIncludeSubmissionsChecked &&
            //                              SelectedNotebook.NameComposite.OwnerTypeTag == "T")
            //                          {
            //                              Parallel.ForEach(AvailableNotebooks,
            //                                               notebookInfo =>
            //                                               {
            //                                                   if (notebookInfo.NameComposite.OwnerTypeTag == "A" ||
            //                                                       notebookInfo.NameComposite.OwnerTypeTag == "T" ||
            //                                                       notebookInfo == SelectedNotebook)
            //                                                   {
            //                                                       return;
            //                                                   }

            //                                                   _dataService.OpenNotebook(notebookInfo, false, false);
            //                                                   _dataService.LoadPages(notebookInfo, pageIDs, true);
            //                                                   _dataService.LoadLocalSubmissions(notebookInfo, pageIDs, true);
            //                                               });
            //                          }
            //                      },
            //                      null,
            //                      "Loading Pages");

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
            //var textInputViewModel = new TextInputViewModel();
            //var textInputView = new TextInputView(textInputViewModel);
            //textInputView.ShowDialog();

            //if (textInputView.DialogResult == null ||
            //    textInputView.DialogResult != true ||
            //    string.IsNullOrEmpty(textInputViewModel.InputText))
            //{
            //    return;
            //}

            //var pageNumbersToOpen = RangeHelper.ParseStringToIntNumbers(textInputViewModel.InputText);
            //if (!pageNumbersToOpen.Any())
            //{
            //    return;
            //}

            //PleaseWaitHelper.Show(() => _dataService.OpenNotebook(SelectedNotebook), null, "Loading Notebook");
            //var pageIDs = DataService.GetPageIDsFromPageNumbers(SelectedNotebook, pageNumbersToOpen);
            //PleaseWaitHelper.Show(() =>
            //                      {
            //                          _dataService.LoadPages(SelectedNotebook, pageIDs, false);
            //                          _dataService.LoadLocalSubmissions(SelectedNotebook, pageIDs, false);
            //                          if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher &&
            //                              IsIncludeSubmissionsChecked &&
            //                              SelectedNotebook.NameComposite.OwnerTypeTag == "T")
            //                          {
            //                              Parallel.ForEach(AvailableNotebooks,
            //                                               notebookInfo =>
            //                                               {
            //                                                   if (notebookInfo.NameComposite.OwnerTypeTag == "A" ||
            //                                                       notebookInfo.NameComposite.OwnerTypeTag == "T" ||
            //                                                       notebookInfo == SelectedNotebook)
            //                                                   {
            //                                                       return;
            //                                                   }

            //                                                   _dataService.OpenNotebook(notebookInfo, false, false);
            //                                                   _dataService.LoadPages(notebookInfo, pageIDs, true);
            //                                                   _dataService.LoadLocalSubmissions(notebookInfo, pageIDs, true);
            //                                               });
            //                          }
            //                      },
            //                      null,
            //                      "Loading Pages");
        }

        private bool OnOpenNotebookCanExecute()
        {
            return SelectedNotebook != null;
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
                    var classRoster = DataService.LoadClassRosterFromZipContainer(SelectedZipContainerFullFilePath);
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
                    var notebooks = DataService.LoadAllNotebooksFromZipContainer(SelectedZipContainerFullFilePath);
                    NotebooksInSelectedNotebookSet = notebooks.ToObservableCollection();
                    SelectedNotebook = NotebooksInSelectedNotebookSet.FirstOrDefault();
                }
            }

            base.OnPropertyChanged(e);
        }

        #endregion
    }
}