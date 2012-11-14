﻿using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
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

        public override string Title { get { return "ServerWorkspaceVM"; } }

        public string WorkspaceName
        {
            get { return "ServerWorkspace"; }
        }
    }
}