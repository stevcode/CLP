﻿using System;
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
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPStampViewModel).IsAnchored)
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
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPStampViewModel).IsAnchored)
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
            if (pageObjectContainerViewModel.PageObjectViewModel is CLPStampViewModel)
            {
                if (!(pageObjectContainerViewModel.PageObjectViewModel as CLPStampViewModel).IsAnchored)
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
                                    

	                                double deltaX = Math.Abs(pageObjectContainerViewModel.Position.X - otherTile.PageObject.Position.X);
	                                double deltaYBottomSnap = Math.Abs(pageObjectContainerViewModel.Position.Y - (container.Position.Y + container.Height));
                                    double deltaYTopSnap = Math.Abs(container.Position.Y - (pageObjectContainerViewModel.Position.Y + pageObjectContainerViewModel.Height));
	                                if (deltaX < 50)
	                                {
                                        if (deltaYBottomSnap < 55)
                                        {
                                            int oldCount = otherTile.Tiles.Count;
                                            foreach (var tileColor in tile.Tiles)
                                            {
                                                otherTile.Tiles.Add(tileColor);
                                            }

                                            container.Height = (CLPSnapTile.TILE_HEIGHT) * otherTile.Tiles.Count;
                                            CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                            container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(otherTile.PageObject, item);
                                            item.OldValue = oldCount.ToString();
                                            item.NewValue = otherTile.Tiles.Count.ToString();


                                            //Claire - this adds tile to the actual page object, essentially  doubling the amount of tiles we add
                                            // to this tile, once above, to do the official add, and once again here. I've commented it out for now.
                                            // same for the identical action during the top snap action
                                            CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;
                                            foreach (var tileColor in tile.Tiles)
                                            {
                                            //    t.Tiles.Add("SpringGreen");
                                            }
                                         
                                            CLPService.RemovePageObjectFromPage(pageObjectContainerViewModel);
                                            break;
                                        }
                                        else if (deltaYTopSnap < 55)
                                        {
                                            int oldCount = tile.Tiles.Count;
                                            foreach (var tileColor in otherTile.Tiles)
                                            {
                                                tile.Tiles.Add(tileColor);
                                            }
                                            pageObjectContainerViewModel.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count;
                                            container.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count;
                                            CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                            container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(tile.PageObject, item);
                                            item.OldValue = oldCount.ToString();
                                            item.NewValue = tile.Tiles.Count.ToString();
                                            CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;
                                            
                                            foreach (var tileColor in otherTile.Tiles)
                                            {
                                             //   t.Tiles.Add("SpringGreen");
                                            }
                                             
                                            CLPService.RemovePageObjectFromPage(container);
                                            break;
                                        }
                                        
	                                    
	                                }
	                            }
	                        }
	                    }
	                }
	            }
            }

            isDragging = false;
        }

        private void removeTileButton_Click(object sender, RoutedEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            bool isStampedObject = false;

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            {
                isStampedObject = true;
            }

            if (App.IsAuthoring || isStampedObject)
            {

                if ((pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel).Tiles.Count > 1)
                {
                    (pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel).Tiles.RemoveAt((pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel).Tiles.Count - 1);
                    pageObjectContainerViewModel.Height = CLPSnapTile.TILE_HEIGHT * (pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel).Tiles.Count;
                }
            }
        }

        private void duplicateTileButton_Click(object sender, RoutedEventArgs e)
        {
            PageObjectContainerViewModel pageObjectContainerViewModel = (this.DataContext as PageObjectContainerViewModel);

            bool isStampedObject = false;

            if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            {
                isStampedObject = true;
            }

            if (App.IsAuthoring || isStampedObject)
            {
                CLPSnapTile newSnapTile = ((pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel).PageObject as CLPSnapTile).Copy() as CLPSnapTile;
                newSnapTile.Position = new Point(newSnapTile.Position.X + 80, newSnapTile.Position.Y);
                CLPService.AddPageObjectToPage(newSnapTile);
            }
        }

    }
}
