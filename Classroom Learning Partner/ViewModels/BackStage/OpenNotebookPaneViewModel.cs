namespace Classroom_Learning_Partner.ViewModels
{
    public class OpenNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OpenNotebookPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands() { }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Open Notebook"; }
        }

        #endregion //Bindings

        #region Commands

        #endregion //Commands
    }
}