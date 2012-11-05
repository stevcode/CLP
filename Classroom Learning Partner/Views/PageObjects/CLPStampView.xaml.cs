using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

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

        private void PageObjectHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Green);
            //adornerCanvas.Visibility = Visibility.Hidden;
        }

        private void PageObjectHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Black);
        }

        private void StampObject_MouseMove(object sender, MouseEventArgs e)
        {
            CLPStampViewModel stamp = (this.ViewModel as CLPStampViewModel);
            if (!stamp.PageObject.IsBackground)
            {
                ////VisualTreeHelper.HitTest(StampObject, new HitTestFilterCallback(HitFilter), new HitTestResultCallback(HitResult), new PointHitTestParameters(e.GetPosition(StampObject)));
            }
            else if (stamp.PageObject.IsBackground && !App.MainWindowViewModel.IsAuthoring)
            {
                ////adornerCanvas.Visibility = Visibility.Hidden;
            }
        }
    }
}
