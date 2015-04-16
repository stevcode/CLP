using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for TemporaryBoundaryView.xaml</summary>
    public partial class TemporaryBoundaryView
    {
        public TemporaryBoundaryView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (LassoRegionViewModel); }
    }
}