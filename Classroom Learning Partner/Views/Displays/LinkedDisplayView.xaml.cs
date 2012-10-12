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

        [ViewToViewModel(MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        public Tuple<double, double> DisplayWidthHeight
        {
            get { return (Tuple<double, double>)GetValue(DisplayWidthHeightProperty); }
            set { SetValue(DisplayWidthHeightProperty, value); }
        }
        // Using a DependencyProperty as the backing store
        // for MyDependencyProperty. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayWidthHeightProperty =
            DependencyProperty.Register("DisplayWidthHeight",
            typeof(Tuple<double, double>), typeof(LinkedDisplayView), new UIPropertyMetadata(new Tuple<double, double>(0.0, 0.0)));

        //ar = w / h
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            DisplayWidthHeight = new Tuple<double,double>(ActualWidth,ActualHeight);

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
