namespace Classroom_Learning_Partner.ViewModels
{
    public class ExportPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public ExportPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands() { }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Export"; }
        }

        #endregion //Bindings

        #region Commands

        #endregion //Commands
    }
}