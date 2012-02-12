using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Data;
using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class NotebookWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel()
        {
            WorkspaceBackgroundColor = new SolidColorBrush(Colors.AliceBlue);
            LinkedDisplay = new LinkedDisplayViewModel();
            GridDisplay = new GridDisplayViewModel();

            SelectedDisplay = LinkedDisplay;
            LinkedDisplay.IsActive = true;

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                LinkedDisplay.IsOnProjector = true;
            }
            else
            {
                LinkedDisplay.IsOnProjector = false;
            }
            
            GridDisplay.IsActive = false;
            GridDisplay.IsOnProjector = false;
        }

        //Steve - Do I need SideBar prop here?

        //Steve - Do these displays need to be set here, or can I just set SelectedDisplay to new display values when created?
        // Same with BG color, can be XAML?
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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Brush WorkspaceBackgroundColor
        {
            get { return GetValue<Brush>(WorkspaceBackgroundColorProperty); }
            set { SetValue(WorkspaceBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Register the WorkspaceBackgroundColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WorkspaceBackgroundColorProperty = RegisterProperty("WorkspaceBackgroundColor", typeof(Brush));

        public string WorkspaceName
        {
            get { return "NotebookWorkspace"; }
        }
    }
}
