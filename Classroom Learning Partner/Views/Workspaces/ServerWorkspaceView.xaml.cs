using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Views.Workspaces
{
    /// <summary>
    /// Interaction logic for ServerWorkspaceView.xaml
    /// </summary>
    public partial class ServerWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public ServerWorkspaceView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(ServerWorkspaceViewModel);
        }
    }
}
