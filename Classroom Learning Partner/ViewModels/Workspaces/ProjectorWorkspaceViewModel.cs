using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class ProjectorWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProjectorWorkspaceViewModel class.
        /// </summary>
        public ProjectorWorkspaceViewModel()
            : base()
        {
            LinkedDisplay = new LinkedDisplayViewModel();
            GridDisplay = new GridDisplayViewModel();

            SelectedDisplay = LinkedDisplay;
            LinkedDisplay.IsActive = true;
            LinkedDisplay.IsOnProjector = true;
            GridDisplay.IsActive = false;
            GridDisplay.IsOnProjector = false;
        }


        //Steve - Do these displays need to be set here, or can I just set SelectedDisplay to new display values when created?
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public LinkedDisplayViewModel LinkedDisplay
        {
            get { return GetValue<LinkedDisplayViewModel>(LinkedDisplayProperty); }
            private set { SetValue(LinkedDisplayProperty, value); }
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