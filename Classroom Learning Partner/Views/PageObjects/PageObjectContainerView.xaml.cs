using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows;
using System.Windows.Input;
using System;
using System.Windows.Controls.Primitives;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for PageObjectContainerView.xaml
    /// </summary>
    public partial class PageObjectContainerView : Catel.Windows.Controls.UserControl
    {
        public PageObjectContainerView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPPageObjectBaseViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as ICLPPageObject);

            if (App.MainWindowViewModel.IsAuthoring)
            {
                CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
            }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as ICLPPageObject);

            if (App.MainWindowViewModel.IsAuthoring)
            {
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

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as ICLPPageObject);

            if (App.MainWindowViewModel.IsAuthoring)
            {
                double newHeight = pageObject.Height + e.VerticalChange;
                double newWidth = pageObject.Width + e.HorizontalChange;
                if (newHeight < 10)
                {
                    newHeight = 10;
                }
                if (newWidth < 10)
                {
                    newWidth = 10;
                }
                if (newHeight + pageObject.Position.Y > 816)
                {
                    newHeight = pageObject.Height;
                }
                if (newWidth + pageObject.Position.X > 1056)
                {
                    newWidth = pageObject.Width;
                }

                CLPServiceAgent.Instance.ChangePageObjectDimensions(pageObject, newHeight, newWidth);
            }
        }
    }
}
