using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for OpenNotebookPaneView.xaml</summary>
    public partial class OpenNotebookPaneView
    {
        public OpenNotebookPaneView()
        {
            InitializeComponent();
        }

        private void ListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}