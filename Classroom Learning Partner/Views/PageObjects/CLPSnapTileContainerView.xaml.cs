using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.Resources;
using Classroom_Learning_Partner.ViewModels;
using System.Windows;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System;

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
            if ((this.DataContext as CLPSnapTileContainerViewModel).Tiles.Count > 1)
            {
                (this.DataContext as CLPSnapTileContainerViewModel).NumberOfTiles--;
                //add the snapTileRemoveTile history item here
                CLPHistory pageHistory = CLPServiceAgent.Instance.GetPageFromID((this.DataContext as CLPSnapTileContainerViewModel).PageObject.PageID).PageHistory;
                if (!pageHistory.IgnoreHistory)
                {
                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileRemoveTile, (this.DataContext as CLPSnapTileContainerViewModel).PageObject.UniqueID, null, null);
                    //pageHistory.HistoryItems.Add(item);
                    String ID = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage.Page.PageHistory.UniqueID;
                    CLPHistory.AddToHistoryItems(item, new Guid(ID));
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
    }
}
