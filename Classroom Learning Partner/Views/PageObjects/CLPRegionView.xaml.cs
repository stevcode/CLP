using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPRegionView.xaml</summary>
    public partial class CLPRegionView
    {
        public CLPRegionView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();
            if (ViewModel is CLPRegionViewModel)
            {
                (ViewModel as CLPRegionViewModel).IsAdornerVisible = true;
            }
        }
    }
}