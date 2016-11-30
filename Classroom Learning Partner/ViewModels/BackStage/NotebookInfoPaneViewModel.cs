﻿using System;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;
using CLP.Entities.Ann;

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

        private void InitializeCommands()
        {
            SaveCurrentNotebookCommand = new Command(OnSaveCurrentNotebookCommandExecute, OnSaveCurrentNotebookCanExecute);
            ClearPagesNonAnimationHistoryCommand = new Command(OnClearPagesNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);
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

        /// <summary>Date and Time the <see cref="CLP.Entities.Ann.Notebook" /> was last saved.</summary>
        [ViewModelToModel("Notebook")]
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof (DateTime?));

        /// <summary>Name of the <see cref="CLP.Entities.Ann.Notebook" />.</summary>
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

        #endregion //Commands

        private void SaveCurrentNotebook()
        {
            if (LoadedNotebookService == null ||
                LoadedNotebookService.CurrentNotebook == null)
            {
                return;
            }

            PleaseWaitHelper.Show(LoadedNotebookService.SaveCurrentNotebookLocally, null, "Saving Notebook");

            PleaseWaitHelper.Show(
                                  () =>
                                  LoadedNotebookService.SaveNotebookLocally(LoadedNotebookService.CurrentNotebook,
                                                                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                                  null,
                                  "Exporting Notebook");

            if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Student ||
                App.Network.InstructorProxy == null)
            {
                return;
            }

            var zippedPages = string.Empty;
            try
            {
                var sPages = ObjectSerializer.ToString(LoadedNotebookService.CurrentNotebook.Pages.ToList());
                zippedPages = CLPServiceAgent.Instance.Zip(sPages);
            }
            catch (Exception)
            {
                Logger.Instance.WriteToLog("Failed to zip pages for collection.");
                return;
            }

            if (string.IsNullOrEmpty(zippedPages))
            {
                Logger.Instance.WriteToLog("Failed to zip pages for collection.");
                return;
            }

            PleaseWaitHelper.Show(
                                  () =>
                                  App.Network.InstructorProxy.AddSerializedPages(zippedPages, LoadedNotebookService.CurrentNotebook.ID),
                                  null,
                                  "Collecting Notebook");
        }

        private bool OnSaveCurrentNotebookCanExecute() { return Notebook != null; }

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
    }
}