using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for PageObjectContainerView.xaml
    /// </summary>
    public partial class PageObjectContainerView : UserControl
    {
        public PageObjectContainerView()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);
            AppMessages.RequestCurrentDisplayedPage.Send((callbackMessage) =>
            {
                callbackMessage.PageObjectContainerViewModels.Remove(pageObjectContainerViewModel);
            });
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);
            double x = pageObjectContainerViewModel.Position.X + e.HorizontalChange;
            double y = pageObjectContainerViewModel.Position.Y + e.VerticalChange;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x > 816 - pageObjectContainerViewModel.Width)
            {
                x = 816 - pageObjectContainerViewModel.Width;
            }
            if (y > 1056 - pageObjectContainerViewModel.Height)
            {
                y = 1056 - pageObjectContainerViewModel.Height;
            }
            pageObjectContainerViewModel.Position = new Point(x, y);
        }

      
    }
}
