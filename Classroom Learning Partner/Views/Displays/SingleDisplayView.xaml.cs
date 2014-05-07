using System;
using System.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for SingleDisplayView.xaml
    /// </summary>
    public partial class SingleDisplayView
    {
        public SingleDisplayView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType() { return typeof(SingleDisplayViewModel); }

        //AspectRatio = w / h
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var singleDisplayViewModel = ViewModel as SingleDisplayViewModel;
            if(singleDisplayViewModel != null)
            {
                singleDisplayViewModel.DisplayWidthHeight = new Tuple<double, double>(ActualWidth, ActualHeight);
            }

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}