using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPSnapTileView.xaml
    /// </summary>
    public partial class CLPSnapTileContainerView : Catel.Windows.Controls.UserControl
    {
        public CLPSnapTileContainerView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPSnapTileContainerViewModel);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPSnapTileContainerViewModel).PageObject;

            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.RemovePageObjectFromPage(pageObject);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPSnapTileContainerViewModel).PageObject;

            double x = pageObject.XPosition + e.HorizontalChange;
            double y = pageObject.YPosition + e.VerticalChange;
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
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.ChangePageObjectPosition(pageObject, pt);
        }

        private void removeTileButton_Click(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as CLPSnapTileContainerViewModel).Tiles.Count > 1)
            {
                (this.DataContext as CLPSnapTileContainerViewModel).NumberOfTiles--;
                //add the snapTileRemoveTile history item here
                CLPHistory pageHistory = Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.GetPageFromID((this.DataContext as CLPSnapTileContainerViewModel).PageObject.PageID).PageHistory;
                if (!pageHistory.IgnoreHistory)
                {
                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileRemoveTile, (this.DataContext as CLPSnapTileContainerViewModel).PageObject.UniqueID, null, null);
                    pageHistory.HistoryItems.Add(item);
                }
            }
        }

        private void duplicateTileButton_Click(object sender, RoutedEventArgs e)
        {
            ICLPPageObject pageObject = (this.DataContext as CLPSnapTileContainerViewModel).PageObject;

            CLPSnapTileContainer newSnapTile = pageObject.Duplicate() as CLPSnapTileContainer;
            double x = newSnapTile.XPosition + 80;
            double y = newSnapTile.YPosition;
            if (x > 1056 - pageObject.Width)
            {
                /* Want some distinguishable change in location. 
                 * Check to see if on the edge already or near the edge.
                 * If on the edge, also move down if possible.
                 */
                if (newSnapTile.XPosition == 1056 - pageObject.Width)
                {
                    y = newSnapTile.YPosition + 20;
                    if (y > 816 - pageObject.Height)
                    {
                        y = 816 - pageObject.Height;
                    }
                }
                x = 1056 - pageObject.Width;
            }

            newSnapTile.XPosition = x;
            newSnapTile.YPosition = y;

            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.AddPageObjectToPage(pageObject.PageID, newSnapTile);
        }
    }
}
