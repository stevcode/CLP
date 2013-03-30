namespace Classroom_Learning_Partner.Views
{
    using System;
    using System.Windows;
    using Catel.MVVM.Views;
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels;

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


        private void ArrayBottomHitBox_MouseMove_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (ViewModel as CLPArrayViewModel).BottomArrowPosition = e.GetPosition(TopGrid).X;
        }

        private void ArrayRightHitBox_MouseMove_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (ViewModel as CLPArrayViewModel).RightArrowPosition = e.GetPosition(TopGrid).Y;
        }

    }


}
