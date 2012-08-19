using Catel.MVVM;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using Catel.Data;
using System;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookChooserWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the NotebookChooserWorkspaceViewModel class.
        /// </summary>
        public NotebookChooserWorkspaceViewModel() : base()
        {
            SelectNotebookCommand = new Command<string>(OnSelectNotebookCommandExecute);

            NotebookNames = new ObservableCollection<string>();
            CLPServiceAgent.Instance.GetNotebookNames(this);
        }

        public override string Title { get { return "NotebookChooserWorkspaceVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> NotebookNames
        {
            get { return GetValue<ObservableCollection<string>>(NotebookNamesProperty); }
            set { SetValue(NotebookNamesProperty, value); }
        }

        /// <summary>
        /// Register the NotebookNames property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookNamesProperty = RegisterProperty("NotebookNames", typeof(ObservableCollection<string>));
        
        /// <summary>
        /// Gets the SelectNotebookCommand command.
        /// </summary>
        public Command<string> SelectNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SelectNotebookCommand command is executed.
        /// </summary>
        private void OnSelectNotebookCommandExecute(string notebookName)
        {
            Catel.Windows.PleaseWaitHelper.Show(() =>
            CLPServiceAgent.Instance.OpenNotebook(notebookName), null, "Loading Notebook", 0.0 / 0.0);
        }

        public string WorkspaceName
        {
            get { return "NotebookChooserWorkspace"; }
        }
    }
}