using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for PageInformationPanelView.xaml</summary>
    public partial class PageInformationPanelView
    {
        public PageInformationPanelView()
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