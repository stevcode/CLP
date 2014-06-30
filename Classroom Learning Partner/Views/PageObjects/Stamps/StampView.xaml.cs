using System;
using System.Windows.Input;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class StampView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StampView" /> class.
        /// </summary>
        public StampView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(StampViewModel); }

        private void StampHandleHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            var clpStampViewModel = ViewModel as StampViewModel;
            if(clpStampViewModel != null)
            {
                clpStampViewModel.StampHandleColor = new SolidColorBrush(Colors.Green);
            }
        }

        private void StampHandleHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            var clpStampViewModel = ViewModel as StampViewModel;
            if(clpStampViewModel != null)
            {
                clpStampViewModel.StampHandleColor = new SolidColorBrush(Colors.Black);
            }
        }
    }
}