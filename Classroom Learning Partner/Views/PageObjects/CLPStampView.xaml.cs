namespace Classroom_Learning_Partner.Views.PageObjects
{
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Classroom_Learning_Partner.Model;
    using Classroom_Learning_Partner.ViewModels.PageObjects;
    using Catel.Windows.Controls;

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

        // Highlight handle when mouse is hovering over it
        private void Thumb_MouseOver(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Green);
        }

        // Turn handle back to normal state when mouse leaves the bounds
        private void Thumb_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Polygon).Fill = new SolidColorBrush(Colors.Black);
        }

    }
}
