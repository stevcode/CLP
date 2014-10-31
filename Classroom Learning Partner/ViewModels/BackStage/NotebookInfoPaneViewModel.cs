using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookInfoPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NotebookInfoPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands() { SaveCurrentNotebookCommand = new Command(OnSaveCurrentNotebookCommandExecute, OnSaveCurrentNotebookCanExecute); }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Notebook Information"; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Saves the current notebook.
        /// </summary>
        public Command SaveCurrentNotebookCommand { get; private set; }

        private void OnSaveCurrentNotebookCommandExecute() { SaveCurrentNotebook(); }

        #endregion //Commands

        private void SaveCurrentNotebook()
        {
            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService == null || notebookService.CurrentNotebook == null)
            {
                return;
            }

            Catel.Windows.PleaseWaitHelper.Show(notebookService.SaveCurrentNotebook, null, "Saving Notebook");
        }

        private bool OnSaveCurrentNotebookCanExecute()
        {
            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService == null)
            {
                return false;
            }

            return notebookService.CurrentNotebook != null;
        }
    }
}