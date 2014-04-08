﻿using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for NotebookWorkspaceView.xaml.
    /// </summary>
    public partial class NotebookWorkspaceView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceView" /> class.
        /// </summary>
        public NotebookWorkspaceView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(NotebookWorkspaceViewModel); }
    }
}