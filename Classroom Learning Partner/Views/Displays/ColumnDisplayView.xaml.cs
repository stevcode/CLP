using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ColumnDisplayView.xaml</summary>
    public partial class ColumnDisplayView
    {
        public ColumnDisplayView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (ColumnDisplayViewModel); }
        private void ItemsControl_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}