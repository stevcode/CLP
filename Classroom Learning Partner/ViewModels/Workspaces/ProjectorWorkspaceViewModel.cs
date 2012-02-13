using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Data;
using System;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    public class ProjectorWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProjectorWorkspaceViewModel class.
        /// </summary>
        public ProjectorWorkspaceViewModel()
            : base()
        {
            Console.WriteLine(Title + " created");
        }

        public override string Title { get { return "ProjectorWorkspaceVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public IDisplayViewModel SelectedDisplay
        {
            get { return GetValue<IDisplayViewModel>(SelectedDisplayProperty); }
            set { SetValue(SelectedDisplayProperty, value); }
        }

        /// <summary>
        /// Register the SelectedDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedDisplayProperty = RegisterProperty("SelectedDisplay", typeof(IDisplayViewModel));

        public string WorkspaceName
        {
            get { return "ProjectorWorkspace"; }
        }
    }
}