using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for GridDisplayView.xaml</summary>
    public partial class GridDisplayView
    {
        public GridDisplayView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (GridDisplayViewModel); }
        private void ItemsControl_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}