using System;
using System.Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for MirrorDisplayView.xaml
    /// </summary>
    public partial class MirrorDisplayView
    {
        public MirrorDisplayView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(MirrorDisplayViewModel);
        }

        //AspectRatio = w / h
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var mirrorDisplayViewModel = ViewModel as MirrorDisplayViewModel;
            if(mirrorDisplayViewModel != null)
            {
                mirrorDisplayViewModel.DisplayWidthHeight = new Tuple<double, double>(ActualWidth, ActualHeight);
            }

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
