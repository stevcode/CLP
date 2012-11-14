using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for NotebookChooserWorkspaceView.xaml
    /// </summary>
    public partial class NotebookChooserWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public NotebookChooserWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookChooserWorkspaceViewModel);
        }
    }
}
