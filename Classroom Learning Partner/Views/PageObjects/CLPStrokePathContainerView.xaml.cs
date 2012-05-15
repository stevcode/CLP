namespace Classroom_Learning_Partner.Views.PageObjects
{
    using Catel.Windows.Controls;
    using Classroom_Learning_Partner.ViewModels.PageObjects;
    using System.Windows;
    using Classroom_Learning_Partner.Model;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Interaction logic for CLPStrokePathContainerView.xaml.
    /// </summary>
    public partial class CLPStrokePathContainerView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStrokePathContainerView"/> class.
        /// </summary>
        public CLPStrokePathContainerView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPStrokePathContainerViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPStrokePathContainerViewModel).PageObject;

            CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPStrokePathContainerViewModel).PageObject;

            double x = pageObject.Position.X + e.HorizontalChange;
            double y = pageObject.Position.Y + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > 1056 - pageObject.Width)
            {
                x = 1056 - pageObject.Width;
            }
            if (y > 816 - pageObject.Height)
            {
                y = 816 - pageObject.Height;
            }

            Point pt = new Point(x, y);
            CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pt);
        }
    }
}
