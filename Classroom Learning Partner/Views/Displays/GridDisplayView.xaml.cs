using Classroom_Learning_Partner.ViewModels.Displays;

namespace Classroom_Learning_Partner.Views.Displays
{
    /// <summary>
    /// Interaction logic for GridDisplayView.xaml
    /// </summary>
    public partial class GridDisplayView : Catel.Windows.Controls.UserControl
    {
        public GridDisplayView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(GridDisplayViewModel);
        }
    }
}
