namespace Classroom_Learning_Partner.Views
{
    using System;
    using System.Windows;
    using Catel.MVVM.Views;
    using Catel.Windows.Controls;

    /// <summary>
    /// Interaction logic for CLPArrayView.xaml.
    /// </summary>
    public partial class CLPArrayView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPArrayView"/> class.
        /// </summary>
        public CLPArrayView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RightArrowPositionProperty =
            DependencyProperty.Register("RightArrowPosition", typeof(double),
            typeof(CLPArrayView));

        [ViewToViewModel("RightArrowPosition", MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        public double RightArrowPosition
        {
            get { return (double)GetValue(RightArrowPositionProperty); }
            set { SetValue(RightArrowPositionProperty, value); }
        }

        public static readonly DependencyProperty BottomArrowPositionProperty =
            DependencyProperty.Register("BottomArrowPosition", typeof(double),
            typeof(CLPArrayView));

        [ViewToViewModel("BottomArrowPosition", MappingType=ViewToViewModelMappingType.ViewToViewModel)]
        public double BottomArrowPosition
        {
            get { return (double)GetValue(BottomArrowPositionProperty); }
            set { SetValue(BottomArrowPositionProperty, value); }
        }

        private void ArrayBottomHitBox_MouseMove_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BottomArrowPosition = e.GetPosition(TopGrid).X;
        }

        private void ArrayRightHitBox_MouseMove_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RightArrowPosition = e.GetPosition(TopGrid).Y;
        }

    }


}
