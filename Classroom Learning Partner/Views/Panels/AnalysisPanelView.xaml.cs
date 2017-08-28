using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    public partial class AnalysisPanelView
    {
        public AnalysisPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        private void TagsListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}