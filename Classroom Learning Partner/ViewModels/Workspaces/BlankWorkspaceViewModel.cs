using Catel.MVVM;
using System;

namespace Classroom_Learning_Partner.ViewModels
{
    public class BlankWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the BlankWorkspaceViewModel class.
        /// </summary>
        public BlankWorkspaceViewModel() : base()
        {
        }

        public override string Title { get { return "BlankWorkspaceVM"; } }

        public string WorkspaceName
        {
            get { return "BlankWorkspace"; }
        }
    }
}