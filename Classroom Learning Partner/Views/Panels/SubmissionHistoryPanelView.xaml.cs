using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for SubmissionHistoryPanelView.xaml</summary>
    public partial class SubmissionHistoryPanelView
    {
        public SubmissionHistoryPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        private void UIElement_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}