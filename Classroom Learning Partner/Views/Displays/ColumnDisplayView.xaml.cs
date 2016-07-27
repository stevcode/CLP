using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ColumnDisplayView.xaml</summary>
    public partial class ColumnDisplayView
    {
        public ColumnDisplayView()
        {
            InitializeComponent();
        }

        private void ItemsControl_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}