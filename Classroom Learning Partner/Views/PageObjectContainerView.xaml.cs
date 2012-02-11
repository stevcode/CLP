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

            bool isStampedObject = false;
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPBlankStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPBlankStampViewModel).IsAnchored)
                {
                    isStampedObject = true;
                }
            }

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPImageStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPImageStampViewModel).IsAnchored)
                {
                    isStampedObject = true;
                }
            }

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            {
                isStampedObject = true;
            }

            if (App.IsAuthoring || isStampedObject)
            {
                CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
            }
        }

        private bool isDragging = false;
        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            bool isStampedObject = false;
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPBlankStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPBlankStampViewModel).IsAnchored)
                {
                    isStampedObject = true;
                }
            }

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPImageStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPImageStampViewModel).IsAnchored)
                {
                    isStampedObject = true;
                }
            }

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            {
                isStampedObject = true;
            }


            if (App.IsAuthoring || isStampedObject)
            {
                App.MainWindowViewModel.Ribbon.CanSendToTeacher = true;
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
                isDragging = true;
                CLPService.ChangePageObjectPosition(pageObjectContainerViewModel, pt);
            }
        }

        private void ResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (App.IsAuthoring)
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
        }

        private void dragButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            bool isStampedObject = false;
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPBlankStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPBlankStampViewModel).IsAnchored)
                {
                    isStampedObject = true;
                }
            }

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPImageStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPImageStampViewModel).IsAnchored)
                {
                    isStampedObject = true;
                }
            }

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            {
                isStampedObject = true;
            }


            if (isDragging)
            {
	            if (App.IsAuthoring || isStampedObject)
	            {
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
                                        foreach (var tileColor in tile.Tiles)
                                        {
                                            otherTile.Tiles.Add(tileColor);
                                        }

                                        container.Height = CLPSnapTile.TILE_HEIGHT * otherTile.Tiles.Count;

                                        CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
	                                    break;
	                                }
	                            }
	                        }
	                    }
	                }
	            }
            }

            isDragging = false;
        }

    }
}
