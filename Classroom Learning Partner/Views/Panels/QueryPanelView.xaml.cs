using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    public partial class QueryPanelView
    {
        public QueryPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        private void PagesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}