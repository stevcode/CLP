using Classroom_Learning_Partner.ViewModels.Displays;
using Catel.Windows.Controls;

namespace Classroom_Learning_Partner.Views.Displays
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayView : UserControl<GridDisplayViewModel>
    {
        public GridDisplayView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }
    }
}
