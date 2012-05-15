﻿using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Views.Workspaces
{
    /// <summary>
    /// Interaction logic for NotebookChooserWorkspaceView.xaml
    /// </summary>
    public partial class NotebookChooserWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public NotebookChooserWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookChooserWorkspaceViewModel);
        }
    }
}
