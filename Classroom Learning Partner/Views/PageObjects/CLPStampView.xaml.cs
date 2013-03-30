using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class CLPStampView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampView"/> class.
        /// </summary>
        public CLPStampView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStampViewModel);
        }

        private void StampHandleHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            var clpStampViewModel = ViewModel as CLPStampViewModel;
            if(clpStampViewModel != null)
            {
                clpStampViewModel.StampHandleColor = new SolidColorBrush(Colors.Green);
            }
        }

        private void StampHandleHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            var clpStampViewModel = ViewModel as CLPStampViewModel;
            if(clpStampViewModel != null)
            {
                clpStampViewModel.StampHandleColor = new SolidColorBrush(Colors.Black);
            }
        }

        private void AdornerClose_Click(object sender, RoutedEventArgs e)
        {
            var clpStampViewModel = ViewModel as CLPStampViewModel;
            if(clpStampViewModel != null)
            {
                CLPServiceAgent.Instance.RemovePageObjectFromPage(clpStampViewModel.PageObject);
            }
        }
    }
}
