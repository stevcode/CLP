using Catel.MVVM;
using System;

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
            Console.WriteLine(Title + " created");
        }

        public override string Title { get { return "ServerWorkspaceVM"; } }

        public string WorkspaceName
        {
            get { return "ServerWorkspace"; }
        }
    }
}