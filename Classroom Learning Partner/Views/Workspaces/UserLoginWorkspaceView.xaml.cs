using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Views.Workspaces
{
    /// <summary>
    /// Interaction logic for UserLoginWorkspaceView.xaml
    /// </summary>
    public partial class UserLoginWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public UserLoginWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(UserLoginWorkspaceViewModel);
        }
    }
}
