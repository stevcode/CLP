using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for UserLoginWorkspaceView.xaml</summary>
    public partial class UserLoginWorkspaceView
    {
        public UserLoginWorkspaceView()
        {
            InitializeComponent();
        }

        private void UIElement_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}