using System;
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
    }
}