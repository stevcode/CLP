using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ProjectorWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProjectorWorkspaceViewModel class.
        /// </summary>
        public ProjectorWorkspaceViewModel()
            : base()
        {   
            
            LinkedDisplay = new LinkedDisplayViewModel(new CLPPage());
            GridDisplay = new GridDisplayViewModel();
            SelectedDisplay = LinkedDisplay;
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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public LinkedDisplayViewModel LinkedDisplay
        {
            get { return GetValue<LinkedDisplayViewModel>(LinkedDisplayProperty); }
            set { SetValue(LinkedDisplayProperty, value); }
        }

        /// <summary>
        /// Register the LinkedDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData LinkedDisplayProperty = RegisterProperty("LinkedDisplay", typeof(LinkedDisplayViewModel));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public GridDisplayViewModel GridDisplay
        {
            get { return GetValue<GridDisplayViewModel>(GridDisplayProperty); }
            set { SetValue(GridDisplayProperty, value); }
        }

        /// <summary>
        /// Register the GridDisplay property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GridDisplayProperty = RegisterProperty("GridDisplay", typeof(GridDisplayViewModel));

        public string WorkspaceName
        {
            get { return "ProjectorWorkspace"; }
        }
    }
}