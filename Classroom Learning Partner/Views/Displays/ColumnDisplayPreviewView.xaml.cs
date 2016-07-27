using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for GridDisplayView.xaml</summary>
    public partial class ColumnDisplayPreviewView
    {
        public ColumnDisplayPreviewView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelChanged()
        {
            var viewModel = ViewModel as ColumnDisplayViewModel;
            if (viewModel != null)
            {
                viewModel.IsDisplayPreview = true;
            }

            base.OnViewModelChanged();
        }
    }
}