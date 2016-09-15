using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for NewPaneView.xaml</summary>
    public partial class NewPaneView
    {
        public NewPaneView()
        {
            InitializeComponent();
        }

        private void ListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}