﻿using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.Resources;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using System.Windows;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPSnapTileView.xaml
    /// </summary>
    public partial class CLPSnapTileContainerView : Catel.Windows.Controls.UserControl
    {
        public CLPSnapTileContainerView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPSnapTileContainerViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPSnapTileContainerViewModel).PageObject;

            CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPSnapTileContainerViewModel).PageObject;

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

        private void removeTileButton_Click(object sender, RoutedEventArgs e)
        {
            CLPSnapTileContainer snapTileContainer = (this.DataContext as CLPSnapTileContainerViewModel).PageObject as CLPSnapTileContainer;

            if (snapTileContainer.Tiles.Count > 1)
            {
                snapTileContainer.Tiles.RemoveAt(snapTileContainer.Tiles.Count - 1);
                //add the snapTileRemoveTile history item here
                CLPHistory pageHistory = CLPServiceAgent.Instance.GetPageFromID(snapTileContainer.PageID).PageHistory;
                if (!pageHistory.IgnoreHistory)
                {
                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileRemoveTile, snapTileContainer.UniqueID, null, null);
                    pageHistory.HistoryItems.Add(item);
                }
            }
        }

        private void duplicateTileButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPSnapTileContainerViewModel).PageObject;

            CLPSnapTileContainer newSnapTile = pageObject.Duplicate() as CLPSnapTileContainer;
            double x = newSnapTile.Position.X + 80;
            double y = newSnapTile.Position.Y;
            if (x > 1056 - pageObject.Width)
            {
                /* Want some distinguishable change in location. 
                 * Check to see if on the edge already or near the edge.
                 * If on the edge, also move down if possible.
                 */
                if (newSnapTile.Position.X == 1056 - pageObject.Width)
                {
                    y = newSnapTile.Position.Y + 20;
                    if (y > 816 - pageObject.Height)
                    {
                        y = 816 - pageObject.Height;
                    }
                }
                x = 1056 - pageObject.Width;
            }

            newSnapTile.Position = new Point(x, y);
            CLPServiceAgent.Instance.AddPageObjectToPage(pageObject.PageID, newSnapTile);
        }

        private void dragButton_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CLPSnapTileContainer currentTile = (this.DataContext as CLPSnapTileContainerViewModel).PageObject as CLPSnapTileContainer;

            CLPPage currentPage = CLPServiceAgent.Instance.GetPageFromID(currentTile.PageID);

            foreach (var pageObject in currentPage.PageObjects)
            {
                if (pageObject is CLPSnapTileContainer)
                {

                    CLPSnapTileContainer otherTile = pageObject as CLPSnapTileContainer;
                    if (currentTile.UniqueID != otherTile.UniqueID)
                    {
                        double deltaX = Math.Abs(currentTile.Position.X - otherTile.Position.X);
                        double deltaYBottomSnap = Math.Abs(currentTile.Position.Y - (otherTile.Position.Y + otherTile.Height));
                        double deltaYTopSnap = Math.Abs(otherTile.Position.Y - (currentTile.Position.Y + currentTile.Height));
                        if (deltaX < 50)
                        {
                            if (deltaYBottomSnap < 55)
                            {
                                //int oldCount = otherTile.Tiles.Count;
                                foreach (var tileColor in currentTile.Tiles)
                                {
                                    otherTile.Tiles.Add(tileColor);
                                }

                                //container.Height = (CLPSnapTileContainer.TILE_HEIGHT) * otherTile.Tiles.Count; -> don't do this
                                //CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                //container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(otherTile.PageObject, item);
                                //item.OldValue = oldCount.ToString(); -> old num of tiles
                                //item.NewValue = otherTile.Tiles.Count.ToString(); -> current num of tiles

                                //CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;


                                CLPServiceAgent.Instance.RemovePageObjectFromPage(currentTile);
                                break;
                            }
                            else if (deltaYTopSnap < 55)
                            {
                                //int oldCount = tile.Tiles.Count;
                                foreach (var tileColor in otherTile.Tiles)
                                {
                                    currentTile.Tiles.Add(tileColor);
                                }
                                //pageObjectContainerViewModel.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count; -> ignore
                                //container.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count;
                                //CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                //container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(tile.PageObject, item);
                                //item.OldValue = oldCount.ToString(); 
                                //item.NewValue = tile.Tiles.Count.ToString();
                                //CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;



                                CLPServiceAgent.Instance.RemovePageObjectFromPage(otherTile);
                                break;
                            }


                        }
                    }
                }
            }
        }
    }
}
