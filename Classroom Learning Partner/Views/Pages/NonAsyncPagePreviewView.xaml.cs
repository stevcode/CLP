﻿using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class NonAsyncPagePreviewView
    {
        public NonAsyncPagePreviewView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(CLPPageViewModel); }
    }
}
