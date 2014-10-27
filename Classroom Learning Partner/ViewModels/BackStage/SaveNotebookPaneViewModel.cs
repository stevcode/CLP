namespace Classroom_Learning_Partner.ViewModels
{
    public class SaveNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public SaveNotebookPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands() { }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Save Notebook"; }
        }

        #endregion //Bindings

        #region Commands

        #endregion //Commands
    }
}