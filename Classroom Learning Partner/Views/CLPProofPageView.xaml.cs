﻿using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPProofPageView.xaml
    /// </summary>
    public partial class CLPProofPageView
    {
        public CLPProofPageView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPProofPageViewModel);
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is CLPProofPageViewModel)
            {
                (ViewModel as CLPProofPageViewModel).TopCanvas = TopCanvas;
                (ViewModel as CLPProofPageViewModel).IsPagePreview = false;
            }
            
            base.OnViewModelChanged();
        }
    }
}
