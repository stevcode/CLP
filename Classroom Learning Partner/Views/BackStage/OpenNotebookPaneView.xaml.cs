using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for OpenNotebookPaneView.xaml</summary>
    public partial class OpenNotebookPaneView
    {
        public OpenNotebookPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (OpenNotebookPaneViewModel); }

        private void ListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}