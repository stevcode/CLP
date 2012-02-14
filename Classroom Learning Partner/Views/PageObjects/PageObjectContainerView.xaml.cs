using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows;
using System.Windows.Input;
using System;
using Catel.Windows.Controls;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for PageObjectContainerView.xaml
    /// </summary>
    public partial class PageObjectContainerView : UserControl<PageObjectContainerViewModel>
    {
        public PageObjectContainerView()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            bool isStampedObject = false;
            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPBlankStampViewModel)
            //{
            //    if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPBlankStampViewModel).IsAnchored)
            //    {
            //        isStampedObject = true;
            //    }
            //}

            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPImageStampViewModel)
            //{
            //    if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPImageStampViewModel).IsAnchored)
            //    {
            //        isStampedObject = true;
            //    }
            //}

            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileContainerViewModel)
            //{
            //    isStampedObject = true;
            //}

            if (App.MainWindowViewModel.IsAuthoring || isStampedObject)
            {
                //CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
            }
        }

        private bool isDragging = false;
        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            //bool isStampedObject = false;
            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPBlankStampViewModel)
            //{
            //    if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPBlankStampViewModel).IsAnchored)
            //    {
            //        isStampedObject = true;
            //    }
            //}

            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPImageStampViewModel)
            //{
            //    if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPImageStampViewModel).IsAnchored)
            //    {
            //        isStampedObject = true;
            //    }
            //}

            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileContainerViewModel)
            //{
            //    isStampedObject = true;
            //}


            //if (App.MainWindowViewModel.IsAuthoring || isStampedObject)
            //{
            //    App.MainWindowViewModel.CanSendToTeacher = true;
            //    double x = pageObjectContainerViewModel.Position.X + e.HorizontalChange;
            //    double y = pageObjectContainerViewModel.Position.Y + e.VerticalChange;
            //    if (x < 0)
            //    {
            //        x = 0;
            //    }
            //    if (y < 0)
            //    {
            //        y = 0;
            //    }
            //    if (x > 1056 - pageObjectContainerViewModel.Width)
            //    {
            //        x = 1056 - pageObjectContainerViewModel.Width;
            //    }
            //    if (y > 816 - pageObjectContainerViewModel.Height)
            //    {
            //        y = 816 - pageObjectContainerViewModel.Height;
            //    }

            //    Point pt = new Point(x, y);
            //    isDragging = true;
            //    //CLPService.ChangePageObjectPosition(pageObjectContainerViewModel, pt);
            //}
        }

        private void ResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (App.MainWindowViewModel.IsAuthoring)
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

                //CLPService.ChangePageObjectDimensions(pageObjectContainerViewModel, newHeight, newWidth);
            }
        }

        private void dragButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            bool isStampedObject = false;
            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPBlankStampViewModel)
            //{
            //    if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPBlankStampViewModel).IsAnchored)
            //    {
            //        isStampedObject = true;
            //    }
            //}

            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPImageStampViewModel)
            //{
            //    if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPImageStampViewModel).IsAnchored)
            //    {
            //        isStampedObject = true;
            //    }
            //}

            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileContainerViewModel)
            //{
            //    isStampedObject = true;
            //}


            if (isDragging)
            {
	            if (App.MainWindowViewModel.IsAuthoring || isStampedObject)
	            {
                    //if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileContainerViewModel)
                    //{
                    //    CLPSnapTileContainerViewModel tile = pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileContainerViewModel;
                    //    //Steve - re-write for pageObjectContainer revamp
                    //    //foreach (var pageObject in pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.PageObjects)
                    //    //{
                    //    //    if (pageObject.PageObjectViewModel is CLPSnapTileViewModel)
                    //    //    {

                    //    //        CLPSnapTileViewModel otherTile = pageObject.PageObjectViewModel as CLPSnapTileViewModel;
                    //    //        if (tile.PageObject.UniqueID != otherTile.PageObject.UniqueID)
                    //    //        {
                    //    //            Console.WriteLine("x: " + otherTile.PageObject.Position.X.ToString());
                    //    //            Console.WriteLine("y: " + otherTile.PageObject.Position.Y.ToString());
                    //    //            double deltaX = Math.Abs(pageObjectContainerViewModel.Position.X - otherTile.PageObject.Position.X);
                    //    //            double deltaY = Math.Abs(pageObjectContainerViewModel.Position.Y - otherTile.PageObject.Position.Y);
                    //    //            if (deltaX < 50 && deltaY < 60)
                    //    //            {
                    //    //                foreach (var tileColor in tile.Tiles)
                    //    //                {
                    //    //                    otherTile.Tiles.Add(tileColor);
                    //    //                }

                    //    //                pageObject.Height = CLPSnapTile.TILE_HEIGHT * otherTile.Tiles.Count;

                    //    //                CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
                    //    //                break;
                    //    //            }
                    //    //        }
                    //    //    }
                    //    //}
                    //}
	            }
            }

            isDragging = false;
        }

    }
}
