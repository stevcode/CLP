﻿using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for SubmissionHistoryPanelView.xaml
    /// </summary>
    public partial class SubmissionHistoryPanelView
    {
        public SubmissionHistoryPanelView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(SubmissionHistoryPanelViewModel);
        }
    }
}
