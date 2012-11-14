﻿using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for ProjectorWorkspaceView.xaml
    /// </summary>
    public partial class ProjectorWorkspaceView : Catel.Windows.Controls.UserControl
    {
        public ProjectorWorkspaceView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(ProjectorWorkspaceViewModel);
        }
    }
}
