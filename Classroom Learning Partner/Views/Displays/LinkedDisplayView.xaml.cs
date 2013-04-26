using System;
using System.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for LinkedDisplayView.xaml
    /// </summary>
    public partial class LinkedDisplayView
    {
        public LinkedDisplayView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(LinkedDisplayViewModel);
        }

        //AspectRatio = w / h
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var linkedDisplayViewModel = ViewModel as LinkedDisplayViewModel;
            if (linkedDisplayViewModel != null)
            {
                linkedDisplayViewModel.DisplayWidthHeight = new Tuple<double, double>(ActualWidth, ActualHeight);
            }

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
