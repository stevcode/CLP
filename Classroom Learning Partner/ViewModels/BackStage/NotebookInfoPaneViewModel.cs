using System;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookInfoPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NotebookInfoPaneViewModel()
        {
            Notebook = LoadedNotebookService.CurrentNotebook;
            InitializeCommands();
        }

        private void InitializeCommands() { SaveCurrentNotebookCommand = new Command(OnSaveCurrentNotebookCommandExecute, OnSaveCurrentNotebookCanExecute); }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Model
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Date and Time the <see cref="CLP.Entities.Notebook" /> was last saved.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof(DateTime?));

        /// <summary>
        /// Name of the <see cref="CLP.Entities.Notebook" />.
        /// </summary>
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
        public override string PaneTitleText
        {
            get { return "Notebook Information"; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>Saves the current notebook.</summary>
        public Command SaveCurrentNotebookCommand { get; private set; }

        private void OnSaveCurrentNotebookCommandExecute() { SaveCurrentNotebook(); }

        #endregion //Commands

        private void SaveCurrentNotebook()
        {
            if (LoadedNotebookService == null ||
                LoadedNotebookService.CurrentNotebook == null)
            {
                return;
            }

            PleaseWaitHelper.Show(LoadedNotebookService.SaveCurrentNotebookLocally, null, "Saving Notebook");
        }

        private bool OnSaveCurrentNotebookCanExecute()
        {
            return Notebook != null;
        }
    }
}