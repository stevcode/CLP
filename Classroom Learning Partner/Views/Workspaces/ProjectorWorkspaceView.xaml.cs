using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Views.Workspaces
{
    /// <summary>
    /// Interaction logic for ProjectorWorkspaceView.xaml
    /// </summary>
    public partial class ProjectorWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public ProjectorWorkspaceView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(ProjectorWorkspaceViewModel);
        }
    }
}
