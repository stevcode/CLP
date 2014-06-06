using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for NotebookChooserWorkspaceView.xaml
    /// </summary>
    public partial class NotebookChooserWorkspaceView
    {
        public NotebookChooserWorkspaceView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(NotebookChooserWorkspaceViewModel); }
        private void UIElement_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}