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
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPImageStampView.xaml
    /// </summary>
    public partial class CLPStampView : UserControl
    {
        public CLPStampView()
        {
            InitializeComponent();

            CLPService = new CLPServiceAgent();

            adornedControl.IsMouseOverShowEnabled = false;

            this.Loaded += new RoutedEventHandler(CLPStampView_Loaded);
        }

        private bool isOnPreview = false;

        private CLPStampViewModel stampViewModel;
        void CLPStampView_Loaded(object sender, RoutedEventArgs e)
        {
            stampViewModel = this.DataContext as CLPStampViewModel;
            if (stampViewModel.IsAnchored)
            {
                adornedControl.ShowAdorner();
            }

            PageObjectContainerView pageObjectContainerView = UIHelper.TryFindParent<PageObjectContainerView>(adornedControl);
            CLPPagePreviewView preview = UIHelper.TryFindParent<CLPPagePreviewView>(pageObjectContainerView);
            if (preview != null)
            {
                isOnPreview = true;
            }
        }

        private ICLPServiceAgent CLPService { get; set; }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (!isOnPreview)
            {
                if (!(this.DataContext as CLPStampViewModel).PageViewModel.Page.IsSubmission)
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
                    if (x > 1056 - pageObjectContainerViewModel.Width)
                    {
                        x = 1056 - pageObjectContainerViewModel.Width;
                    }
                    if (y > 816 - pageObjectContainerViewModel.Height)
                    {
                        y = 816 - pageObjectContainerViewModel.Height;
                    }

                    Point pt = new Point(x, y);
                    CLPService.ChangePageObjectPosition(pageObjectContainerViewModel, pt);
                }
            }
        }

        private void Thumb_MouseOver(object sender, MouseEventArgs e)
        {
            if (!isOnPreview)
            {
                if (!(this.DataContext as CLPStampViewModel).PageViewModel.Page.IsSubmission)
                {
                    poly.Fill = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private void Thumb_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isOnPreview)
            {
                if (!(this.DataContext as CLPStampViewModel).PageViewModel.Page.IsSubmission)
                {
                    poly.Fill = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private Point oldPosition;
        private void Thumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right && !isOnPreview)
            {
                if (!(this.DataContext as CLPStampViewModel).PageViewModel.Page.IsSubmission)
                {
                    poly.Fill = new SolidColorBrush(Colors.Green);

                    PageObjectContainerView pageObjectContainerView = UIHelper.TryFindParent<PageObjectContainerView>(adornedControl);
                    PageObjectContainerViewModel pageObjectContainerViewModel = pageObjectContainerView.DataContext as PageObjectContainerViewModel;
                    oldPosition = pageObjectContainerViewModel.Position;

                    CLPStamp stamp = stampViewModel.PageObject.Copy() as CLPStamp;
                    CLPService.AddPageObjectToPage(stamp);

                    stampViewModel.IsAnchored = false;
                    //make serviceagent call here to change model and database
                    (stampViewModel.PageObject as CLPStamp).IsAnchored = false;
                    stampViewModel.ScribblesToStrokePaths();
                }
            }
        }

        private void Thumb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right && !isOnPreview)
            {
                if (!(this.DataContext as CLPStampViewModel).PageViewModel.Page.IsSubmission)
                {
                    poly.Fill = new SolidColorBrush(Colors.Black);

                    PageObjectContainerView pageObjectContainerView = UIHelper.TryFindParent<PageObjectContainerView>(adornedControl);
                    PageObjectContainerViewModel pageObjectContainerViewModel = pageObjectContainerView.DataContext as PageObjectContainerViewModel;
                    Point newPosition = pageObjectContainerViewModel.Position;

                    double deltaX = Math.Abs(newPosition.X - oldPosition.X);
                    double deltaY = Math.Abs(newPosition.Y - oldPosition.Y);
                    //change these to be past the height/width of the container
                    if (deltaX > 50 || deltaY > 50)
                    {
                        stampViewModel.IsAnchored = false;
                        //make serviceagent call here to change model and database
                        (stampViewModel.PageObject as CLPStamp).IsAnchored = false;

                        adornedControl.HideAdorner();

                        if (App.Peer.Channel != null && App.CurrentUserMode == App.UserMode.Instructor)
                        {
                            string stringPageObject = ObjectSerializer.ToString(pageObjectContainerViewModel.PageObjectViewModel.PageObject);
                            App.Peer.Channel.AddPageObjectToPage(pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.Page.UniqueID, stringPageObject);
                        }
                    }
                    else
                    {
                        CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
                    }
                }
            }
        }
    }
}
