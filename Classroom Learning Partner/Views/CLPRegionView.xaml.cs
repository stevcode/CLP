﻿using System.Timers;
using System.Windows.Input;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using System;


namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPRegionView.xaml
    /// </summary>
    public partial class CLPRegionView
    {
        public CLPRegionView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPRegionViewModel);
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is CLPRegionViewModel)
            {
                (ViewModel as CLPRegionViewModel).IsAdornerVisible = true;
            }

            base.OnViewModelChanged();
        }
    }
}
