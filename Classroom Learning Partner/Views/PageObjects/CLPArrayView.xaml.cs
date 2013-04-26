using System.Windows;
using System.Windows.Input;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPArrayView.xaml.
    /// </summary>
    public partial class CLPArrayView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPArrayView"/> class.
        /// </summary>
        public CLPArrayView()
        {
            InitializeComponent();
        }

        private void ArrayBottomHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel != null)
            {
                clpArrayViewModel.BottomArrowPosition = e.GetPosition(TopGrid).X - CLPArray.LargeLabelLength;
            }
        }

        private void ArrayRightHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if (clpArrayViewModel != null)
            {
                clpArrayViewModel.RightArrowPosition = e.GetPosition(TopGrid).Y - CLPArray.LargeLabelLength;
            }
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("CLICK!");
        }
    }
}
