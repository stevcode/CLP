﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookChooserWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookChooserWorkspaceViewModel class.
        /// </summary>
        public NotebookChooserWorkspaceViewModel()
        {
            SelectNotebookCommand = new Command<string>(OnSelectNotebookCommandExecute);

            CLPServiceAgent.Instance.GetNotebookNames(this);
        }

        public override string Title
        {
            get { return "NotebookChooserWorkspaceVM"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> NotebookNames
        {
            get { return GetValue<ObservableCollection<string>>(NotebookNamesProperty); }
            set { SetValue(NotebookNamesProperty, value); }
        }

        public static readonly PropertyData NotebookNamesProperty = RegisterProperty("NotebookNames", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// Gets the SelectNotebookCommand command.
        /// </summary>
        public Command<string> SelectNotebookCommand { get; private set; }

        private void OnSelectNotebookCommandExecute(string notebookName)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            PleaseWaitHelper.Show(() => CLPServiceAgent.Instance.OpenNotebook(notebookName), null, "Loading Notebook");
            stopWatch.Stop();
            Logger.Instance.WriteToLog("Time to LOAD notebook (In Seconds): " + stopWatch.ElapsedMilliseconds / 1000.0);
        }
    }
}