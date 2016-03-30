using System;
using System.Linq;
using Catel.Data;
using Catel.IO;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookInfoPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NotebookInfoPaneViewModel()
        {
            Notebook = DataService.CurrentNotebook;
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            SaveCurrentNotebookCommand = new Command(OnSaveCurrentNotebookCommandExecute, OnSaveCurrentNotebookCanExecute);
            SaveNotebookForStudentCommand = new Command(OnSaveNotebookForStudentCommandExecute, OnSaveNotebookForStudentCanExecute);
            ForceSaveCurrentNotebookCommand = new Command(OnForceSaveCurrentNotebookCommandExecute, OnSaveCurrentNotebookCanExecute);
            ClearPagesNonAnimationHistoryCommand = new Command(OnClearPagesNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            GenerateStudentNotebooksCommand = new Command(OnGenerateStudentNotebooksCommandExecute);
        }

        #endregion //Constructor

        #region Model

        /// <summary>Model</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof (Notebook));

        /// <summary>Date and Time the <see cref="CLP.Entities.Notebook" /> was last saved.</summary>
        [ViewModelToModel("Notebook")]
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof (DateTime?));

        /// <summary>Name of the <see cref="CLP.Entities.Notebook" />.</summary>
        [ViewModelToModel("Notebook")]
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof (string));

        #endregion //Model

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Notebook Information"; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>Saves the current notebook.</summary>
        public Command SaveCurrentNotebookCommand { get; private set; }

        private void OnSaveCurrentNotebookCommandExecute() { SaveCurrentNotebook(); }

        /// <summary>Saves the current notebook.</summary>
        public Command ForceSaveCurrentNotebookCommand { get; private set; }

        private void OnForceSaveCurrentNotebookCommandExecute() { SaveCurrentNotebook(true); }

        #endregion //Commands

        private void SaveCurrentNotebook(bool isForceSave = false)
        {
            if (DataService == null ||
                DataService.CurrentNotebook == null)
            {
                return;
            }

            PleaseWaitHelper.Show(() => DataService.SaveNotebookLocally(DataService.CurrentNotebookInfo, isForceSave), null, "Saving Notebook");

            //PleaseWaitHelper.Show(
            //                      () =>
            //                      LoadedNotebookService.SaveNotebookLocally(LoadedNotebookService.CurrentNotebook,
            //                                                                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
            //                      null,
            //                      "Exporting Notebook");

            //if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Student ||
            //    App.Network.InstructorProxy == null)
            //{
            //    return;
            //}

            //var zippedPages = string.Empty;
            //try
            //{
            //    var sPages = ObjectSerializer.ToString(LoadedNotebookService.CurrentNotebook.Pages.ToList());
            //    zippedPages = CLPServiceAgent.Instance.Zip(sPages);
            //}
            //catch (Exception)
            //{
            //    Logger.Instance.WriteToLog("Failed to zip pages for collection.");
            //    return;
            //}

            //if (string.IsNullOrEmpty(zippedPages))
            //{
            //    Logger.Instance.WriteToLog("Failed to zip pages for collection.");
            //    return;
            //}

            //PleaseWaitHelper.Show(
            //                      () =>
            //                      App.Network.InstructorProxy.AddSerializedPages(zippedPages, LoadedNotebookService.CurrentNotebook.ID),
            //                      null,
            //                      "Collecting Notebook");
        }

        private bool OnSaveCurrentNotebookCanExecute() { return Notebook != null; }

        public Command SaveNotebookForStudentCommand { get; private set; }

        private void OnSaveNotebookForStudentCommandExecute()
        {
            if (DataService == null ||
                DataService.CurrentCacheInfo == null ||
                DataService.CurrentNotebookInfo == null ||
                DataService.CurrentNotebookInfo.Notebook == null)
            {
                return;
            }

            var textInputViewModel = new TextInputViewModel
                                     {
                                         TextPrompt = "Student Name: "
                                     };
            var textInputView = new TextInputView(textInputViewModel);
            textInputView.ShowDialog();

            if (textInputView.DialogResult == null ||
                textInputView.DialogResult != true ||
                string.IsNullOrEmpty(textInputViewModel.InputText))
            {
                return;
            }

            var person = new Person
                         {
                             IsStudent = true,
                             FullName = textInputViewModel.InputText
                         };

            var copiedNotebook = DataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(person);
            copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();
            var notebookComposite = NotebookNameComposite.ParseNotebook(copiedNotebook);
            var notebookPath = Path.Combine(DataService.CurrentCacheInfo.NotebooksFolderPath, notebookComposite.ToFolderName());
            var notebookInfo = new NotebookInfo(DataService.CurrentCacheInfo, notebookPath)
                               {
                                   Notebook = copiedNotebook
                               };
            PleaseWaitHelper.Show(() => DataService.SaveNotebookLocally(notebookInfo, true), null, "Saving Notebook");
            DataService.SetCurrentNotebook(notebookInfo);
        }

        private bool OnSaveNotebookForStudentCanExecute()
        {
            return Notebook != null; 
        }

        /// <summary>Completely clears all non-animation histories for regular pages in a notebook.</summary>
        public Command ClearPagesNonAnimationHistoryCommand { get; private set; }

        private void OnClearPagesNonAnimationHistoryCommandExecute()
        {
            PleaseWaitHelper.Show(() =>
                                  {
                                      foreach (var page in Notebook.Pages)
                                      {
                                          page.History.ClearNonAnimationHistory();
                                      }
                                  },
                                  null,
                                  "Clearing History");
        }

        private bool OnClearHistoryCommandCanExecute() { return Notebook != null; }

        /// <summary>SUMMARY</summary>
        public Command GenerateStudentNotebooksCommand { get; private set; }

        private void OnGenerateStudentNotebooksCommandExecute()
        {
            // HACK: This is very hardcoded.
            if (DataService == null ||
                DataService.CurrentCacheInfo == null ||
                DataService.CurrentNotebookInfo == null ||
                DataService.CurrentNotebookInfo.Notebook == null)
            {
                return;
            }

            var classInfoPath = Path.Combine(DataService.CurrentCacheInfo.ClassesFolderPath, "classInfo;KK;S1nEmeKiYkSuPPo3t2nWXQ.xml");
            var classInfo = ClassInformation.LoadFromXML(classInfoPath);
            if (classInfo == null)
            {
                return;
            }

            var teacher = classInfo.Teacher;
            var copiedNotebookT = DataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(teacher);
            copiedNotebookT.CurrentPage = copiedNotebookT.Pages.FirstOrDefault();
            var notebookCompositeT = NotebookNameComposite.ParseNotebook(copiedNotebookT);
            var notebookPathT = Path.Combine(DataService.CurrentCacheInfo.NotebooksFolderPath, notebookCompositeT.ToFolderName());
            var notebookInfoT = new NotebookInfo(DataService.CurrentCacheInfo, notebookPathT)
                                {
                                    Notebook = copiedNotebookT
                                };
            PleaseWaitHelper.Show(() => DataService.SaveNotebookLocally(notebookInfoT, true), null, "Saving Notebook for " + teacher.FullName);

            foreach (var person in classInfo.StudentList)
            {
                var copiedNotebook = DataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(person);
                copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();
                var notebookComposite = NotebookNameComposite.ParseNotebook(copiedNotebook);
                var notebookPath = Path.Combine(DataService.CurrentCacheInfo.NotebooksFolderPath, notebookComposite.ToFolderName());
                var notebookInfo = new NotebookInfo(DataService.CurrentCacheInfo, notebookPath)
                {
                    Notebook = copiedNotebook
                };
                PleaseWaitHelper.Show(() => DataService.SaveNotebookLocally(notebookInfo, true), null, "Saving Notebook for " + person.FullName);
            }
        }
    }
}