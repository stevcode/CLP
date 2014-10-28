using System;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using Catel.IO;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NewNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NewNotebookPaneViewModel()
        {
            InitializeCommands();
            AvailableCacheNames.AddRange(LoadedNotebookService.AvailableLocalCacheNames);
            SelectedCacheName = AvailableCacheNames.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            CreateNotebookCommand = new Command(OnCreateNotebookCommandExecute, OnCreateNotebookCanExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "New Notebook"; }
        }

        /// <summary>
        /// List of available Cache names.
        /// </summary>
        public ObservableCollection<string> AvailableCacheNames
        {
            get { return GetValue<ObservableCollection<string>>(AvailableCacheNamesProperty); }
            set { SetValue(AvailableCacheNamesProperty, value); }
        }

        public static readonly PropertyData AvailableCacheNamesProperty = RegisterProperty("AvailableCacheNames", typeof (ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// Selected Cache Name.
        /// </summary>
        public string SelectedCacheName
        {
            get { return GetValue<string>(SelectedCacheNameProperty); }
            set
            {
                SetValue(SelectedCacheNameProperty, value);
                if (value != null)
                {
                    SelectedCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), value);
                }
            }
        }

        public static readonly PropertyData SelectedCacheNameProperty = RegisterProperty("SelectedCacheName", typeof (string), string.Empty);

        /// <summary>
        /// Path of the Selected Cache's Directory.
        /// </summary>
        public string SelectedCacheDirectory
        {
            get { return GetValue<string>(SelectedCacheDirectoryProperty); }
            set { SetValue(SelectedCacheDirectoryProperty, value); }
        }

        public static readonly PropertyData SelectedCacheDirectoryProperty = RegisterProperty("SelectedCacheDirectory", typeof (string), string.Empty);

        /// <summary>
        /// Notebook Name to use on creation.
        /// </summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof (string), string.Empty);

        /// <summary>
        /// Curriculum for the new notebook.
        /// </summary>
        public string NotebookCurriculum
        {
            get { return GetValue<string>(NotebookCurriculumProperty); }
            set { SetValue(NotebookCurriculumProperty, value); }
        }

        public static readonly PropertyData NotebookCurriculumProperty = RegisterProperty("NotebookCurriculum", typeof (string), string.Empty);

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Creates a new notebook.
        /// </summary>
        public Command CreateNotebookCommand { get; private set; }

        private void OnCreateNotebookCommandExecute()
        {
            
        }

        private bool OnCreateNotebookCanExecute()
        {
            return NotebookName != string.Empty;
        }

        #endregion //Commands
    }
}