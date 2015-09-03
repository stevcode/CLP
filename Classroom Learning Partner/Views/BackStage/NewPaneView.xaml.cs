using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for NewPaneView.xaml</summary>
    public partial class NewPaneView
    {
        public NewPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (NewPaneViewModel); }

        private void CachesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}