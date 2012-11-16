using System;
using System.Windows;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for LinkedDisplayView.xaml
    /// </summary>
    public partial class LinkedDisplayView : Catel.Windows.Controls.UserControl
    {
        public LinkedDisplayView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(LinkedDisplayViewModel);
        }

        //AspectRatio = w / h
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            (ViewModel as LinkedDisplayViewModel).DisplayWidthHeight = new Tuple<double,double>(ActualWidth,ActualHeight);

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
