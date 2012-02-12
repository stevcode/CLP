using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    public class BlankWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the BlankWorkspaceViewModel class.
        /// </summary>
        public BlankWorkspaceViewModel() : base()
        {
        }

        public string WorkspaceName
        {
            get { return "BlankWorkspace"; }
        }
    }
}