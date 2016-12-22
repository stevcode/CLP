using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NewPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NewPaneViewModel()
        {
            AvailableZipContainerFileNames = _dataService.AvailableZipContainerFileInfos.Select(fi => Path.GetFileNameWithoutExtension(fi.Name)).ToObservableCollection();
            SelectedExistingZipContainerFileName = AvailableZipContainerFileNames.FirstOrDefault();

            InitializeCommands();
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText => "New Notebook";

        /// <summary>Name of the notebook to be created.</summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string), string.Empty);

        /// <summary>File name (without extension) of the ZipContainer currently selected.</summary>
        public string SelectedExistingZipContainerFileName
        {
            get { return GetValue<string>(SelectedExistingZipContainerFileNameProperty); }
            set { SetValue(SelectedExistingZipContainerFileNameProperty, value); }
        }

        public static readonly PropertyData SelectedExistingZipContainerFileNameProperty = RegisterProperty("SelectedExistingZipContainerFileName", typeof(string), string.Empty);

        /// <summary>File name (without extension) for a new ZipContainer to be created.</summary>
        public string NewZipContainerFileName
        {
            get { return GetValue<string>(NewZipContainerFileNameProperty); }
            set { SetValue(NewZipContainerFileNameProperty, value); }
        }

        public static readonly PropertyData NewZipContainerFileNameProperty = RegisterProperty("NewZipContainerFileName", typeof(string), string.Empty);

        /// <summary>List of all the available ZipContainers in the default Cache location.</summary>
        public ObservableCollection<string> AvailableZipContainerFileNames
        {
            get { return GetValue<ObservableCollection<string>>(AvailableZipContainerFileNamesProperty); }
            set { SetValue(AvailableZipContainerFileNamesProperty, value); }
        }

        public static readonly PropertyData AvailableZipContainerFileNamesProperty = RegisterProperty("AvailableZipContainerFileNames",
                                                                                                      typeof(ObservableCollection<string>),
                                                                                                      () => new ObservableCollection<string>());

        /// <summary>File Name to use when accessing the ZipContainer to make the new notebook in.</summary>
        public string ZipContainerFileNameToUse
        {
            get { return GetValue<string>(ZipContainerFileNameToUseProperty); }
            set { SetValue(ZipContainerFileNameToUseProperty, value); }
        }

        public static readonly PropertyData ZipContainerFileNameToUseProperty = RegisterProperty("ZipContainerFileNameToUse", typeof(string), string.Empty);
        
        public string SelectedZipContainerFullFilePath => $"{_dataService.CurrentCacheFolderPath}\\{ZipContainerFileNameToUse}.{AInternalZipEntryFile.CONTAINER_EXTENSION}";

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            CreateNotebookCommand = new Command(OnCreateNotebookCommandExecute, OnCreateNotebookCanExecute);
        }

        /// <summary>Creates a new notebook.</summary>
        public Command CreateNotebookCommand { get; private set; }

        private void OnCreateNotebookCommandExecute()
        {
            if (AvailableZipContainerFileNames.Select(s => s.ToUpper()).Contains(NewZipContainerFileName.ToUpper()))
            {
                MessageBox.Show("The new class roster name cannot be the same as an existing class roster name.");
                return;
            }

            _dataService.CreateAuthorNotebook(NotebookName, SelectedZipContainerFullFilePath);
        }

        private bool OnCreateNotebookCanExecute()
        {
            return !string.IsNullOrWhiteSpace(NotebookName) && !string.IsNullOrWhiteSpace(ZipContainerFileNameToUse);
        }

        #endregion //Commands

        #region Overrides of ViewModelBase

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NewZipContainerFileName))
            {
                var newZipContainerFileName = e.NewValue as string;
                if (string.IsNullOrWhiteSpace(newZipContainerFileName))
                {
                    SelectedExistingZipContainerFileName = AvailableZipContainerFileNames.FirstOrDefault();
                    ZipContainerFileNameToUse = SelectedExistingZipContainerFileName;
                }
                else
                {
                    SelectedExistingZipContainerFileName = null;
                    ZipContainerFileNameToUse = newZipContainerFileName;
                }

                RaisePropertyChanged(nameof(SelectedZipContainerFullFilePath));
            }

            if (e.PropertyName == nameof(SelectedExistingZipContainerFileName))
            {
                var selectedExistingZipContainerFileName = e.NewValue as string;
                if (!string.IsNullOrWhiteSpace(selectedExistingZipContainerFileName))
                {
                    NewZipContainerFileName = null;
                    ZipContainerFileNameToUse = selectedExistingZipContainerFileName;
                }

                RaisePropertyChanged(nameof(SelectedZipContainerFullFilePath));
            }

            base.OnPropertyChanged(e);
        }

        #endregion
    }
}