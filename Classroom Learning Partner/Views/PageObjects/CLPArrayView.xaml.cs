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
        private const double MINIMUM_DIVISION_ADORNER_GAP = 30.0;
        private readonly Timer _divisorHideTimer = new Timer();

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPArrayView"/> class.
        /// </summary>
        public CLPArrayView()
        {
            InitializeComponent();
            _divisorHideTimer.Elapsed += _divisorHideTimer_Elapsed;
            _divisorHideTimer.Interval = 1500;
        }

        private void ArrayTopHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            _divisorHideTimer.Stop();
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel == null ||
               !clpArrayViewModel.IsDivisionBehaviorOn ||
               clpArrayViewModel.IsDefaultAdornerVisible)
            {
                return;
            }
            var clpArray = clpArrayViewModel.PageObject as CLPArray;
            if(clpArray != null)
            {
                clpArrayViewModel.TopArrowPosition = e.GetPosition(TopGrid).X - clpArray.LargeLabelLength;
            }
            if(clpArrayViewModel.TopArrowPosition < MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.TopArrowPosition = MINIMUM_DIVISION_ADORNER_GAP;
            }
            if(clpArrayViewModel.TopArrowPosition > clpArrayViewModel.ArrayWidth - MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.TopArrowPosition = clpArrayViewModel.ArrayWidth - MINIMUM_DIVISION_ADORNER_GAP;
            }
            clpArrayViewModel.IsTopAdornerVisible = true;
            clpArrayViewModel.IsLeftAdornerVisible = false;
            ACLPPageBaseViewModel.ClearAdorners(clpArrayViewModel.PageObject.ParentPage);
            clpArrayViewModel.IsAdornerVisible = true;
        }

        private void ArrayLeftHitBox_MouseMove(object sender, MouseEventArgs e)
        {
            _divisorHideTimer.Stop();
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel == null ||
               !clpArrayViewModel.IsDivisionBehaviorOn ||
               clpArrayViewModel.IsDefaultAdornerVisible)
            {
                return;
            }
            var clpArray = clpArrayViewModel.PageObject as CLPArray;
            if(clpArray != null)
            {
                clpArrayViewModel.LeftArrowPosition = e.GetPosition(TopGrid).Y - clpArray.LargeLabelLength;
            }
            if(clpArrayViewModel.LeftArrowPosition < MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.LeftArrowPosition = MINIMUM_DIVISION_ADORNER_GAP;
            }
            if(clpArrayViewModel.LeftArrowPosition > clpArrayViewModel.ArrayHeight - MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.LeftArrowPosition = clpArrayViewModel.ArrayHeight - MINIMUM_DIVISION_ADORNER_GAP;
            }
            clpArrayViewModel.IsTopAdornerVisible = false;
            clpArrayViewModel.IsLeftAdornerVisible = true;
            ACLPPageBaseViewModel.ClearAdorners(clpArrayViewModel.PageObject.ParentPage);
            clpArrayViewModel.IsAdornerVisible = true;
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

        private void DivisorButton_Leave(object sender, MouseEventArgs e)
        {
            _overDivisionAdorner = false;
            _divisorHideTimer.Start();
        }

        private void LeftDivisorButton_Mouse(object sender, MouseEventArgs e)
        {
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel == null ||
               !clpArrayViewModel.IsDivisionBehaviorOn ||
               clpArrayViewModel.IsDefaultAdornerVisible)
            {
                return;
            }
            var clpArray = clpArrayViewModel.PageObject as CLPArray;
            if(clpArray != null)
            {
                clpArrayViewModel.LeftArrowPosition = e.GetPosition(TopGrid).Y - clpArray.LargeLabelLength;
            }
            if(clpArrayViewModel.LeftArrowPosition < MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.LeftArrowPosition = MINIMUM_DIVISION_ADORNER_GAP;
            }
            if(clpArrayViewModel.LeftArrowPosition > clpArrayViewModel.ArrayHeight - MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.LeftArrowPosition = clpArrayViewModel.ArrayHeight - MINIMUM_DIVISION_ADORNER_GAP;
            }
        }

        private void TopDivisorButton_Mouse(object sender, MouseEventArgs e)
        {
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel == null ||
               !clpArrayViewModel.IsDivisionBehaviorOn ||
               clpArrayViewModel.IsDefaultAdornerVisible)
            {
                return;
            }
            var clpArray = clpArrayViewModel.PageObject as CLPArray;
            if(clpArray != null)
            {
                clpArrayViewModel.TopArrowPosition = e.GetPosition(TopGrid).X - clpArray.LargeLabelLength;
            }
            if(clpArrayViewModel.TopArrowPosition < MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.TopArrowPosition = MINIMUM_DIVISION_ADORNER_GAP;
            }
            if(clpArrayViewModel.TopArrowPosition > clpArrayViewModel.ArrayWidth - MINIMUM_DIVISION_ADORNER_GAP)
            {
                clpArrayViewModel.TopArrowPosition = clpArrayViewModel.ArrayWidth - MINIMUM_DIVISION_ADORNER_GAP;
            }
        }

        void _divisorHideTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _divisorHideTimer.Stop();
            var clpArrayViewModel = ViewModel as CLPArrayViewModel;
            if(clpArrayViewModel == null ||
               _overDivisionAdorner ||
               clpArrayViewModel.IsDefaultAdornerVisible)
            {
                return;
            }
            clpArrayViewModel.IsTopAdornerVisible = false;
            clpArrayViewModel.IsLeftAdornerVisible = false;
            clpArrayViewModel.IsAdornerVisible = false;
        }
    }
}
