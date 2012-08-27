using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Windows.Controls;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
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

            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPStrokePathContainerViewModel).PageObject;

            double x = pageObject.XPosition + e.HorizontalChange;
            double y = pageObject.YPosition + e.VerticalChange;
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
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pt);
        }
    }
}
