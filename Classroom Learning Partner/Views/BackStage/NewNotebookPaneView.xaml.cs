using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for NewNotebookPaneView.xaml</summary>
    public partial class NewNotebookPaneView
    {
        public NewNotebookPaneView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof (NewNotebookPaneViewModel); }

        private void CachesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}