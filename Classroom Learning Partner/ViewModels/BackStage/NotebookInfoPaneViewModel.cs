namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookInfoPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NotebookInfoPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands() { }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Notebook Information"; }
        }

        #endregion //Bindings

        #region Commands

        #endregion //Commands
    }
}