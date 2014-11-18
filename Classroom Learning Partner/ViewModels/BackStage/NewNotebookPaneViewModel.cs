using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using Path = Catel.IO.Path;

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
            CreateClassSubjectCommand = new Command(OnCreateClassSubjectCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "New Notebook"; }
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

        public static readonly PropertyData SelectedCacheDirectoryProperty = RegisterProperty("SelectedCacheDirectory",
                                                                                              typeof (string),
                                                                                              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Cache"));

        /// <summary>Notebook Name to use on creation.</summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof (string), string.Empty);

        /// <summary>Curriculum for the new notebook.</summary>
        public string NotebookCurriculum
        {
            get { return GetValue<string>(NotebookCurriculumProperty); }
            set { SetValue(NotebookCurriculumProperty, value); }
        }

        public static readonly PropertyData NotebookCurriculumProperty = RegisterProperty("NotebookCurriculum", typeof (string), string.Empty);

        #endregion //Bindings

        #region Commands

        /// <summary>Creates a new notebook.</summary>
        public Command CreateNotebookCommand { get; private set; }

        private void OnCreateNotebookCommandExecute()
        {
            var previousLocalCacheDirectory = LoadedNotebookService.CurrentLocalCacheDirectory;
            LoadedNotebookService.CurrentLocalCacheDirectory = SelectedCacheDirectory;
            var notebookName = NotebookName;
            var newNotebook = new Notebook(notebookName, Person.Author)
                              {
                                  Curriculum = NotebookCurriculum
                              };

            var newPage = new CLPPage(Person.Author);
            newNotebook.AddCLPPageToNotebook(newPage);

            var folderName = NotebookNameComposite.ParseNotebookToNameComposite(newNotebook).ToFolderName();
            var folderPath = Path.Combine(LoadedNotebookService.CurrentNotebookCacheDirectory, folderName);
            if (Directory.Exists(folderPath))
            {
                LoadedNotebookService.CurrentLocalCacheDirectory = previousLocalCacheDirectory;
                return;
            }

            // TODO: Reimplement when autosave returns
            //SaveNotebook(newNotebook);

            LoadedNotebookService.OpenNotebooks.Add(newNotebook);
            LoadedNotebookService.CurrentNotebook = newNotebook;

            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(newNotebook);
            App.MainWindowViewModel.IsAuthoring = true;
            App.MainWindowViewModel.IsBackStageVisible = false;
        }

        private bool OnCreateNotebookCanExecute() { return NotebookName != string.Empty; }

        /// <summary>SUMMARY</summary>
        public Command CreateClassSubjectCommand { get; private set; }

        private void OnCreateClassSubjectCommandExecute()
        {
            var classSubject = new ClassSubject();
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
            var classesFolderPath = Path.Combine(SelectedCacheDirectory, "Classes");
            if (!Directory.Exists(classesFolderPath))
            {
                Directory.CreateDirectory(classesFolderPath);
            }
            classSubject.SaveClassSubject(classesFolderPath);
        }

        #endregion //Commands
    }
}