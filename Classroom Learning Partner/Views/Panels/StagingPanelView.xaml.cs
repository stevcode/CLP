using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for StagingPanelView.xaml
    /// </summary>
    public partial class StagingPanelView
    {
        public StagingPanelView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(StagingPanelViewModel); }
    }
}