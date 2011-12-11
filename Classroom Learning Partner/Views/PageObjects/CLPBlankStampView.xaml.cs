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
using Classroom_Learning_Partner.Resources;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Classroom_Learning_Partner.Views.Modal_Windows;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.PageObjects;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPBlankStampView.xaml
    /// </summary>
    public partial class CLPBlankStampView : UserControl
    {
        public CLPBlankStampView()
        {
            InitializeComponent();

            CLPService = new CLPServiceAgent();

            adornedControl.IsMouseOverShowEnabled = false;

            this.Loaded += new RoutedEventHandler(CLPStampView_Loaded);  
        }

        private CLPBlankStampViewModel stampViewModel;
        void CLPStampView_Loaded(object sender, RoutedEventArgs e)
        {
            stampViewModel = this.DataContext as CLPBlankStampViewModel;
            if (stampViewModel.IsAnchored)
            {
                adornedControl.ShowAdorner();
            }
        }

        private ICLPServiceAgent CLPService { get; set; }

        //datacontext from model to assign initial
        private bool isPartsAssignedByTeacher = false;

        private void PartsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPartsAssignedByTeacher || App.IsAuthoring)
            {
                KeypadWindowView keyPad = new KeypadWindowView();
                keyPad.ShowDialog();
                if (keyPad.DialogResult == true)
                {
                    Button partsBtn = sender as Button;
                    partsBtn.Content = keyPad.Parts;
                }

                if (App.IsAuthoring)
                {
                    isPartsAssignedByTeacher = true;
                }
            }

        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
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


        private Point oldPosition;
        private void Thumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageObjectContainerView pageObjectContainerView = UIHelper.TryFindParent<PageObjectContainerView>(adornedControl);
            PageObjectContainerViewModel pageObjectContainerViewModel = pageObjectContainerView.DataContext as PageObjectContainerViewModel;
            oldPosition = pageObjectContainerViewModel.Position;

            CLPBlankStamp stamp = stampViewModel.PageObject.Copy() as CLPBlankStamp;
            CLPService.AddPageObjectToPage(stamp);

            stampViewModel.IsAnchored = false;
            //make serviceagent call here to change model and database
            (stampViewModel.PageObject as CLPBlankStamp).IsAnchored = false;
            stampViewModel.ScribblesToStrokePaths();
            
        }

        private void Thumb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PageObjectContainerView pageObjectContainerView = UIHelper.TryFindParent<PageObjectContainerView>(adornedControl);
            PageObjectContainerViewModel pageObjectContainerViewModel = pageObjectContainerView.DataContext as PageObjectContainerViewModel;
            Point newPosition = pageObjectContainerViewModel.Position;

            double deltaX = Math.Abs(newPosition.X - oldPosition.X);
            double deltaY = Math.Abs(newPosition.Y - oldPosition.Y);
            //change these to be past the height/width of the container
            if (deltaX > 50 || deltaY > 50)
            {
                adornedControl.HideAdorner();
            }
            else
            {
                CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
            }
        }
    }
}
