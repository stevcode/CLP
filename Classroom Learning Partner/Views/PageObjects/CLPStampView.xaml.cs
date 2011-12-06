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
using Classroom_Learning_Partner.Views.Modal_Windows;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Resources;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPStampView.xaml
    /// </summary>
    public partial class CLPStampView : UserControl
    {
        public CLPStampView()
        {
            InitializeComponent();
            CLPService = new CLPServiceAgent();
            
            adornedControl.IsMouseOverShowEnabled = false;

            //if ((this.DataContext as CLPStampViewModel).IsAnchored)
            //{
                adornedControl.ShowAdorner();
            //}
            
        }

        private ICLPServiceAgent CLPService { get; set; }

        private void PartsButton_Click(object sender, RoutedEventArgs e)
        {
            KeypadWindowView keyPad = new KeypadWindowView();
            keyPad.ShowDialog();
            if (keyPad.DialogResult == true) {
                Button partsBtn = sender as Button;
                partsBtn.Content = keyPad.Parts;
            }
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            CLPStampViewModel stampViewModel = (this.DataContext as CLPStampViewModel);
            PageObjectContainerView pageObjectContainerView = UIHelper.TryFindParent<PageObjectContainerView>(adornedControl);
            PageObjectContainerViewModel pageObjectContainerViewModel = pageObjectContainerView.DataContext as PageObjectContainerViewModel;

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

            Point pt = new Point(x, y);
            CLPService.ChangePageObjectPosition(pageObjectContainerViewModel, pt);
        }
        
    }
}
