using System.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayView : Catel.Windows.Controls.UserControl
    {
        public GridDisplayView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(GridDisplayViewModel);
        }
    }
}
