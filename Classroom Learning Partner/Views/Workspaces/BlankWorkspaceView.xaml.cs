using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Views.Workspaces
{
    /// <summary>
    /// Interaction logic for BlankWorkspaceView.xaml
    /// </summary>
    public partial class BlankWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public BlankWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(BlankWorkspaceViewModel);
        }
    }
}
