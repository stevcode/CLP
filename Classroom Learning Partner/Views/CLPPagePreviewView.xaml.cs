using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPagePreviewView : UserControl<CLPPageViewModel>
    {
        public CLPPagePreviewView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }
    }
}
