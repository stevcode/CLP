namespace Classroom_Learning_Partner.Views.PageObjects
{
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels.PageObjects;
    using Classroom_Learning_Partner.Model;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using System.Windows;
    using System.Windows.Input;
    using System;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for CLPStampView.xaml.
    /// </summary>
    public partial class CLPStampView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStampView"/> class.
        /// </summary>
        public CLPStampView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStampViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            CLPServiceAgent.Instance.RemovePageObjectFromPage(stamp.PageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);

            double x = stamp.Position.X + e.HorizontalChange;
            double y = stamp.Position.Y + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > 1056 - stamp.Width)
            {
                x = 1056 - stamp.Width;
            }
            if (y > 816 - stamp.Height)
            {
                y = 816 - stamp.Height;
            }

            Point pt = new Point(x, y);
            CLPServiceAgent.Instance.ChangePageObjectPosition(stamp.PageObject, pt);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPStampViewModel stamp = (this.DataContext as CLPStampViewModel);
            double newHeight = stamp.Height + e.VerticalChange;
            double newWidth = stamp.Width + e.HorizontalChange;
            if (newHeight < 10)
            {
                newHeight = 10;
            }
            if (newWidth < 10)
            {
                newWidth = 10;
            }
            if (newHeight + stamp.Position.Y > 816)
            {
                newHeight = stamp.Height;
            }
            if (newWidth + stamp.Position.X > 1056)
            {
                newWidth = stamp.Width;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(stamp.PageObject, newHeight, newWidth);
        }

        private void PageObjectHitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Green);
        }

        private void PageObjectHitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Black);
        }

        private void PageObjectHitBox_StylusEnter(object sender, StylusEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Green);
        }

        private void PageObjectHitBox_StylusLeave(object sender, StylusEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Black);
        }

    }
}
