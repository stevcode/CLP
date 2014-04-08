using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ProgressPanelView.xaml
    /// </summary>
    public partial class ProgressPanelView
    {
        public ProgressPanelView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(ProgressPanelViewModel); }
    }
}