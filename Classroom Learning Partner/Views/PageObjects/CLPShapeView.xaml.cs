using System.Windows;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPSquareShapeView.xaml.
    /// </summary>
    public partial class CLPShapeView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPSquareShapeView"/> class.
        /// </summary>
        public CLPShapeView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPShapeViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            CLPShapeViewModel shape = (this.DataContext as CLPShapeViewModel);
            CLPServiceAgent.Instance.RemovePageObjectFromPage(shape.PageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPShapeViewModel shape = (this.DataContext as CLPShapeViewModel);

            double x = shape.Position.X + e.HorizontalChange;
            double y = shape.Position.Y + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > 1056 - shape.Width)
            {
                x = 1056 - shape.Width;
            }
            if (y > 816 - shape.Height)
            {
                y = 816 - shape.Height;
            }

            Point pt = new Point(x, y);
            CLPServiceAgent.Instance.ChangePageObjectPosition(shape.PageObject, pt);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CLPShapeViewModel shape = (this.DataContext as CLPShapeViewModel);

            double newHeight = shape.Height + e.VerticalChange;
            double newWidth = shape.Width + e.HorizontalChange;
            if (newHeight < 10)
            {
                newHeight = 10;
            }
            if (newWidth < 10)
            {
                newWidth = 10;
            }
            if (newHeight + shape.Position.Y > 816)
            {
                newHeight = shape.Height;
            }
            if (newWidth + shape.Position.X > 1056)
            {
                newWidth = shape.Width;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(shape.PageObject, newHeight, newWidth);
        }
    }
}
