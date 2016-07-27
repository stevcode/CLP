using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for GridDisplayView.xaml</summary>
    public partial class GridDisplayView
    {
        public GridDisplayView()
        {
            InitializeComponent();
        }

        private void ItemsControl_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}