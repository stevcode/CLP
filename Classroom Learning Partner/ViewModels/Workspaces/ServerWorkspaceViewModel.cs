using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    public class ServerWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ServerWorkspaceViewModel class.
        /// </summary>
        public ServerWorkspaceViewModel()
            : base()
        {
        }

        public string WorkspaceName
        {
            get { return "ServerWorkspace"; }
        }
    }
}