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
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using Classroom_Learning_Partner.Model.CLPPageObjects;

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
            CLPService = new CLPServiceAgent();
        }

        private ICLPServiceAgent CLPService { get; set; }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);
            CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
        }

        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
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

        private void ResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);
            double newHeight = pageObjectContainerViewModel.Height + e.VerticalChange;
            double newWidth = pageObjectContainerViewModel.Width + e.HorizontalChange;
            if (newHeight < 10)
            {
                newHeight = 10;
            }
            if (newWidth < 10)
            {
                newWidth = 10;
            }
            if (newHeight + pageObjectContainerViewModel.Position.Y > 816)
            {
                newHeight = pageObjectContainerViewModel.Height;
            }
            if (newWidth + pageObjectContainerViewModel.Position.X > 1056)
            {
                newWidth = pageObjectContainerViewModel.Width;
            }

            CLPService.ChangePageObjectDimensions(pageObjectContainerViewModel, newHeight, newWidth);
        }

        private void dragButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            {
                CLPSnapTileViewModel tile = pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel;
                foreach (var container in pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.PageObjectContainerViewModels)
                {
                    if (container.PageObjectViewModel is CLPSnapTileViewModel)
                    {
                        
                        CLPSnapTileViewModel otherTile = container.PageObjectViewModel as CLPSnapTileViewModel;
                        if (tile.PageObject.UniqueID != otherTile.PageObject.UniqueID)
                        {
                            Console.WriteLine("x: " + otherTile.PageObject.Position.X.ToString());
                            Console.WriteLine("y: " + otherTile.PageObject.Position.Y.ToString());
                            double deltaX = Math.Abs(pageObjectContainerViewModel.Position.X - otherTile.PageObject.Position.X);
                            double deltaY = Math.Abs(pageObjectContainerViewModel.Position.Y - otherTile.PageObject.Position.Y);
                            if (deltaX < 50 && deltaY < 60)
                            {
                                otherTile.NextTile = tile;
                                tile.PrevTile = otherTile;

                                Point pt = new Point(otherTile.PageObject.Position.X, otherTile.PageObject.Position.Y + CLPSnapTile.TILE_HEIGHT);
                                CLPService.ChangePageObjectPosition(pageObjectContainerViewModel, pt);
                                break;
                            }
                        }
                        
                    }
                }
                
            }
        }

      
    }
}
