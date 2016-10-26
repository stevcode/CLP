using System;
using System.Drawing;
using System.Linq;
using Catel.IO;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OptionsPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OptionsPaneViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            GenerateRandomMainColorCommand = new Command(OnGenerateRandomMainColorCommandExecute);
            ClearHistoryCommand = new Command(OnClearHistoryCommandExecute);
            GenerateCommand = new Command(OnGenerateCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Options"; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>Sets the DynamicMainColor of the program to a random color.</summary>
        public Command GenerateRandomMainColorCommand { get; private set; }

        private void OnGenerateRandomMainColorCommandExecute()
        {
            var randomGen = new Random();
            var names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            var randomColorName = names[randomGen.Next(names.Length)];
            var color = Color.FromKnownColor(randomColorName);
            MainWindowViewModel.ChangeApplicationMainColor(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        #endregion //Commands

        /// <summary>SUMMARY</summary>
        public Command ClearHistoryCommand { get; private set; }

        private void OnClearHistoryCommandExecute()
        {
            PleaseWaitHelper.Show(() =>
            {
                foreach (var page in _dataService.CurrentNotebookInfo.Notebook.Pages)
                {
                    page.History.ClearNonAnimationHistory();
                }
            },
                                  null,
                                  "Clearing History");
        }

        /// <summary>SUMMARY</summary>
        public Command GenerateCommand { get; private set; }

        private void OnGenerateCommandExecute()
        {
            // HACK: This is very hardcoded.
            if (_dataService == null ||
                _dataService.CurrentCacheInfo == null ||
                _dataService.CurrentNotebookInfo == null ||
                _dataService.CurrentNotebookInfo.Notebook == null)
            {
                return;
            }

            var classInfoPath = Path.Combine(_dataService.CurrentCacheInfo.ClassesFolderPath, "classInfo;KK;S1nEmeKiYkSuPPo3t2nWXQ.xml");
            var classInfo = ClassInformation.LoadFromXML(classInfoPath);
            if (classInfo == null)
            {
                return;
            }

            var teacher = classInfo.Teacher;
            var copiedNotebookT = _dataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(teacher);
            copiedNotebookT.CurrentPage = copiedNotebookT.Pages.FirstOrDefault();
            var notebookCompositeT = NotebookNameComposite.ParseNotebook(copiedNotebookT);
            var notebookPathT = Path.Combine(_dataService.CurrentCacheInfo.NotebooksFolderPath, notebookCompositeT.ToFolderName());
            var notebookInfoT = new NotebookInfo(_dataService.CurrentCacheInfo, notebookPathT)
            {
                Notebook = copiedNotebookT
            };
            PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(notebookInfoT, true), null, "Saving Notebook for " + teacher.FullName);

            foreach (var person in classInfo.StudentList)
            {
                var copiedNotebook = _dataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(person);
                copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();
                var notebookComposite = NotebookNameComposite.ParseNotebook(copiedNotebook);
                var notebookPath = Path.Combine(_dataService.CurrentCacheInfo.NotebooksFolderPath, notebookComposite.ToFolderName());
                var notebookInfo = new NotebookInfo(_dataService.CurrentCacheInfo, notebookPath)
                {
                    Notebook = copiedNotebook
                };
                PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(notebookInfo, true), null, "Saving Notebook for " + person.FullName);
            }
        }
    }
}