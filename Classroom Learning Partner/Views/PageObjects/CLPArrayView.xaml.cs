using System.Windows;
using System.Windows.Input;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using System;

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
                double newPos = e.GetPosition(TopGrid).X - CLPArray.LargeLabelLength;
                if(newPos > CLPArray.SmallLabelLength * .75 && newPos < clpArrayViewModel.ArrayWidth - CLPArray.SmallLabelLength * .75)
                {
                    clpArrayViewModel.BottomArrowPosition = newPos;
                }
            }
        }

        private void ArrayRightHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if (clpArrayViewModel != null)
            {
                double newPos = e.GetPosition(TopGrid).Y - CLPArray.LargeLabelLength;
                if(newPos > CLPArray.SmallLabelLength * .75 && newPos < clpArrayViewModel.ArrayHeight - CLPArray.SmallLabelLength * .75)
                {
                    clpArrayViewModel.RightArrowPosition = newPos;
                }
            }
        }
    }
}
