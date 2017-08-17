using System;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookInfoPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NotebookInfoPaneViewModel()
        {
            Notebook = _dataService.CurrentNotebook;
            InitializeCommands();
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

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>Date and Time the <see cref="CLP.Entities.Notebook" /> was last saved.</summary>
        [ViewModelToModel("Notebook")]
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof(DateTime?));

        /// <summary>Name of the <see cref="CLP.Entities.Notebook" />.</summary>
        [ViewModelToModel("Notebook")]
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string));

        #endregion //Model

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText => "Notebook Information";

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            SaveCurrentNotebookCommand = new Command(OnSaveCurrentNotebookCommandExecute, OnSaveCurrentNotebookCanExecute);
            EditClassCommand = new TaskCommand(OnEditClassCommandExecuteAsync, OnSaveCurrentNotebookCanExecute);
            EditSessionsCommand = new TaskCommand(OnEditSessionsCommandExecuteAsync, OnSaveCurrentNotebookCanExecute);
            GenerateClassNotebooksCommand = new Command(OnGenerateClassNotebooksCommandExecute, OnGenerateClassNotebooksCanExecute);
        }

        /// <summary>Saves the current notebook.</summary>
        public Command SaveCurrentNotebookCommand { get; private set; }

        private void OnSaveCurrentNotebookCommandExecute()
        {
            _dataService.SaveLocal();
            SaveCurrentNotebook();
        }

        private bool OnSaveCurrentNotebookCanExecute()
        {
            return Notebook != null;
        }

        /// <summary>Edits the currently loaded ClassRoster.</summary>
        public TaskCommand EditClassCommand { get; private set; }

        private async Task OnEditClassCommandExecuteAsync()
        {
            if (_dataService.CurrentClassRoster == null)
            {
                return;
            }

            var viewModel = new ClassRosterViewModel(_dataService.CurrentClassRoster);
            await viewModel.ShowWindowAsDialogAsync();
            DataService.SaveClassRoster(_dataService.CurrentClassRoster);
        }

        /// <summary>Opens a list of sessions in the class.</summary>
        public TaskCommand EditSessionsCommand { get; private set; }

        private async Task OnEditSessionsCommandExecuteAsync()
        {
            var viewModel = this.CreateViewModel<SessionsViewModel>(null);
            await viewModel.ShowWindowAsDialogAsync();
        }

        /// <summary>Generates class notebooks from the authored version.</summary>
        public Command GenerateClassNotebooksCommand { get; private set; }

        private void OnGenerateClassNotebooksCommandExecute()
        {
            var authorNotebook = Notebook;
            var classRoster = _dataService.CurrentClassRoster;
            _dataService.GenerateClassNotebooks(authorNotebook, classRoster);
        }

        private bool OnGenerateClassNotebooksCanExecute()
        {
            // TODO: Test to see if all students/teachers in the Class have generated notebooks.
            return Notebook?.Owner?.ID == Person.Author.ID;
        }

        #endregion //Commands

        #region Methods

        private void SaveCurrentNotebook(bool isForceSave = false)
        {
            //if (_dataService == null ||
            //    _dataService.CurrentNotebook == null)
            //{
            //    return;
            //}

            //if (_dataService.CurrentNotebook.OwnerID == Person.Author.ID)
            //{
            //    isForceSave = true;
            //}

            //PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(_dataService.CurrentNotebookInfo, isForceSave), null, "Saving Notebook");

            //PleaseWaitHelper.Show(
            //                      () =>
            //                      LoadedNotebookService.SaveNotebookLocally(LoadedNotebookService.CurrentNotebook,
            //                                                                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
            //                      null,
            //                      "Exporting Notebook");

            //if (App.MainWindowViewModel.CurrentProgramMode != ProgramRoles.Student ||
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
            //    CLogger.AppendToLog("Failed to zip pages for collection.");
            //    return;
            //}

            //if (string.IsNullOrEmpty(zippedPages))
            //{
            //    CLogger.AppendToLog("Failed to zip pages for collection.");
            //    return;
            //}

            //PleaseWaitHelper.Show(
            //                      () =>
            //                      App.Network.InstructorProxy.AddSerializedPages(zippedPages, LoadedNotebookService.CurrentNotebook.ID),
            //                      null,
            //                      "Collecting Notebook");
        }

        #endregion // Methods
    }
}