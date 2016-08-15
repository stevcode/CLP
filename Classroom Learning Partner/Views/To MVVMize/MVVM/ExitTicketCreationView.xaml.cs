using System.Windows.Input;
using Catel.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ExitTicketCreationView.xaml
    /// </summary>
    public partial class ExitTicketCreationView : DataWindow
    {
        public ExitTicketCreationView(ExitTicketCreationViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }

        private void ItemsControl_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}