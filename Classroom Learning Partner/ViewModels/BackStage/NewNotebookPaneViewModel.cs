namespace Classroom_Learning_Partner.ViewModels
{
    public class NewNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NewNotebookPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands() { }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "New Notebook"; }
        }

        #endregion //Bindings

        #region Commands

        #endregion //Commands
    }
}