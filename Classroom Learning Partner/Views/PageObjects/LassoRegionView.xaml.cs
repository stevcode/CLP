using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPRegionView.xaml</summary>
    public partial class LassoRegionView
    {
        public LassoRegionView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (LassoRegionViewModel); }

        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();
            if (ViewModel is LassoRegionViewModel)
            {
                (ViewModel as LassoRegionViewModel).IsAdornerVisible = true;
            }
        }
    }
}