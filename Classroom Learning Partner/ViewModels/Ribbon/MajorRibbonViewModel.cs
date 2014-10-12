using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MajorRibbonViewModel : ViewModelBase
    {
        public MajorRibbonViewModel() { InitializeCommands(); }

        private void InitializeCommands()
        {
            ShowBackStageCommand = new Command(OnShowBackStageCommandExecute);
        }

        

        #region Commands

        /// <summary>
        /// Brings up the BackStage.
        /// </summary>
        public Command ShowBackStageCommand { get; private set; }

        private void OnShowBackStageCommandExecute()
        {

        }

        #endregion //Commands
    }
}