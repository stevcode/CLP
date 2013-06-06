using System.Timers;
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
            _divisorHideTimer.Elapsed += _divisorHideTimer_Elapsed;
            _divisorHideTimer.Interval = 1500;
        }

        private void ArrayBottomHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            _divisorHideTimer.Stop();
            var clpArrayViewModel = ViewModel as CLPArrayViewModel; 
            if(clpArrayViewModel != null && clpArrayViewModel.IsDivisionBehaviorOn && !clpArrayViewModel.IsDefaultAdornerVisible)
            {
                double newPos = e.GetPosition(TopGrid).X - CLPArray.LargeLabelLength;
                if(newPos > CLPArray.SmallLabelLength * .75 && newPos < clpArrayViewModel.ArrayWidth - CLPArray.SmallLabelLength * .75)
                {
                    clpArrayViewModel.BottomArrowPosition = newPos;
                }
                clpArrayViewModel.IsBottomAdornerVisible = true;
                clpArrayViewModel.IsRightAdornerVisible = false;
                CLPPageViewModel.ClearAdorners(clpArrayViewModel.PageObject.ParentPage);
                clpArrayViewModel.IsAdornerVisible = true;
            }
        }

        private void ArrayRightHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            _divisorHideTimer.Stop();
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel != null && clpArrayViewModel.IsDivisionBehaviorOn && !clpArrayViewModel.IsDefaultAdornerVisible)
            {
                double newPos = e.GetPosition(TopGrid).Y - CLPArray.LargeLabelLength;
                if(newPos > CLPArray.SmallLabelLength * .75 && newPos < clpArrayViewModel.ArrayHeight - CLPArray.SmallLabelLength * .75)
                {
                    clpArrayViewModel.RightArrowPosition = newPos;
                }
                clpArrayViewModel.IsBottomAdornerVisible = false;
                clpArrayViewModel.IsRightAdornerVisible = true;
                CLPPageViewModel.ClearAdorners(clpArrayViewModel.PageObject.ParentPage);
                clpArrayViewModel.IsAdornerVisible = true;
            }
        }

        private void DivisorButton_Leave(object sender, MouseEventArgs e)
        {
            _overDivisionAdorner = false;
            _divisorHideTimer.Start();
        }

        private Timer _divisorHideTimer = new Timer();

        void _divisorHideTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _divisorHideTimer.Stop();
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel != null && !_overDivisionAdorner && !clpArrayViewModel.IsDefaultAdornerVisible)
            {
                clpArrayViewModel.IsBottomAdornerVisible = false;
                clpArrayViewModel.IsRightAdornerVisible = false;
                clpArrayViewModel.IsAdornerVisible = false;
            }
        }

        private void ArrayDivisionHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel != null && !clpArrayViewModel.IsDefaultAdornerVisible)
            {
                _divisorHideTimer.Start();
            }
        }

        private bool _overDivisionAdorner;
        private void DivisorButton_Enter(object sender, MouseEventArgs e)
        {
            _overDivisionAdorner = true;
            _divisorHideTimer.Stop();
        }
    }
}
