﻿using System;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPPagePreviewView.xaml
    /// </summary>
    public partial class CLPPagePreviewView
    {
        public CLPPagePreviewView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(ACLPPageBaseViewModel);
        }

        protected override Type GetViewModelType(object dataContext)
        {
            if(dataContext is CLPPage)
                return typeof(CLPPageViewModel);
            if(dataContext is CLPAnimationPage)
                return typeof(CLPAnimationPageViewModel);
          
            return null;
        }
    }
}