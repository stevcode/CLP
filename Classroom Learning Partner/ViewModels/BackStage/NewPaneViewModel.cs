using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NewPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NewPaneViewModel()
        {
            InitializeCommands();
            AvailableCaches.AddRange(DataService.AvailableCaches);
            SelectedCache = AvailableCaches.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            CreateNotebookCommand = new Command(OnCreateNotebookCommandExecute, OnCreateNotebookCanExecute);
            CreateClassSubjectCommand = new Command(OnCreateClassSubjectCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "New Notebook"; }
        }

        #region Cache Bindings

        /// <summary>Manually typed Cache Name for creating a new Cache.</summary>
        public string TypedCacheName
        {
            get { return GetValue<string>(TypedCacheNameProperty); }
            set { SetValue(TypedCacheNameProperty, value); }
        }

        public static readonly PropertyData TypedCacheNameProperty = RegisterProperty("TypedCacheName", typeof(string), string.Empty);

        /// <summary>List of available Caches.</summary>
        public ObservableCollection<CacheInfo> AvailableCaches
        {
            get { return GetValue<ObservableCollection<CacheInfo>>(AvailableCachesProperty); }
            set { SetValue(AvailableCachesProperty, value); }
        }

        public static readonly PropertyData AvailableCachesProperty = RegisterProperty("AvailableCaches",
                                                                                       typeof(ObservableCollection<CacheInfo>),
                                                                                       () => new ObservableCollection<CacheInfo>());

        /// <summary>Selected Cache.</summary>
        public CacheInfo SelectedCache
        {
            get { return GetValue<CacheInfo>(SelectedCacheProperty); }
            set { SetValue(SelectedCacheProperty, value); }
        }

        public static readonly PropertyData SelectedCacheProperty = RegisterProperty("SelectedCache", typeof(CacheInfo));

        #endregion //Cache Bindings

        #region Notebook Bindings

        /// <summary>Notebook Name to use on creation.</summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string), string.Empty);

        /// <summary>Curriculum for the new notebook.</summary>
        public string NotebookCurriculum
        {
            get { return GetValue<string>(NotebookCurriculumProperty); }
            set { SetValue(NotebookCurriculumProperty, value); }
        }

        public static readonly PropertyData NotebookCurriculumProperty = RegisterProperty("NotebookCurriculum", typeof(string), string.Empty);

        #endregion //Notebook Bindings

        #endregion //Bindings

        #region Commands

        /// <summary>Creates a new notebook.</summary>
        public Command CreateNotebookCommand { get; private set; }

        private void OnCreateNotebookCommandExecute()
        {
            if (string.IsNullOrEmpty(TypedCacheName) ||
                string.IsNullOrWhiteSpace(TypedCacheName))
            {
                DataService.CurrentCacheInfo = SelectedCache;
            }
            else
            {
                var newCache = DataService.CreateNewCache(TypedCacheName);
                if (newCache == null)
                {
                    MessageBox.Show("A folder with that name already exists.");
                    return;
                }
            }

            var newNotebook = DataService.CreateNewNotebook(NotebookName, NotebookCurriculum);
            if (newNotebook == null)
            {
                MessageBox.Show("Something went wrong. The notebook you tried to create already exists in this cache.");
            }
        }

        private bool OnCreateNotebookCanExecute() { return NotebookName != string.Empty; }

        /// <summary>Creates a new ClassInformation.</summary>
        public Command CreateClassSubjectCommand { get; private set; }

        private void OnCreateClassSubjectCommandExecute()
        {
            var classSubject = new ClassInformation();
            var classSubjectCreationViewModel = new ClassSubjectCreationViewModel(classSubject);
            var classSubjectCreationView = new ClassSubjectCreationView(classSubjectCreationViewModel);
            classSubjectCreationView.ShowDialog();

            if (classSubjectCreationView.DialogResult == null ||
                classSubjectCreationView.DialogResult != true)
            {
                return;
            }

            foreach (var group in classSubjectCreationViewModel.GroupCreationViewModel.Groups)
            {
                foreach (var student in group.Members)
                {
                    if (classSubjectCreationViewModel.GroupCreationViewModel.GroupType == "Temp")
                    {
                        student.TempDifferentiationGroup = group.Label;
                    }
                    else
                    {
                        student.CurrentDifferentiationGroup = group.Label;
                    }
                }
            }

            foreach (var group in classSubjectCreationViewModel.TempGroupCreationViewModel.Groups)
            {
                foreach (var student in group.Members)
                {
                    if (classSubjectCreationViewModel.TempGroupCreationViewModel.GroupType == "Temp")
                    {
                        student.TempDifferentiationGroup = group.Label;
                    }
                    else
                    {
                        student.CurrentDifferentiationGroup = group.Label;
                    }
                }
            }

            classSubject.Projector = classSubject.Teacher;
            //var classesFolderPath = Path.Combine(SelectedCacheDirectory, "Classes");
            //if (!Directory.Exists(classesFolderPath))
            //{
            //    Directory.CreateDirectory(classesFolderPath);
            //}
            //ClassInformation.SaveClassSubject(classesFolderPath);
        }

        #endregion //Commands
    }
}