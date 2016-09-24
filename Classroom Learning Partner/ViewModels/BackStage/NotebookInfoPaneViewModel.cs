using System;
using Catel.Data;
using Catel.MVVM;
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
        }

        /// <summary>Saves the current notebook.</summary>
        public Command SaveCurrentNotebookCommand { get; private set; }

        private void OnSaveCurrentNotebookCommandExecute()
        {
            SaveCurrentNotebook();
        }

        private bool OnSaveCurrentNotebookCanExecute()
        {
            return Notebook != null;
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

        #endregion // Methods
    }
}