using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Collections.ObjectModel;
using Catel.MVVM;
using Catel.Data;
using System.Collections.Specialized;
using System;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPSnapTileContainerViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPSnapTileViewModel class.
        /// </summary>
        public CLPSnapTileContainerViewModel(CLPSnapTileContainer tile)
            : base()
        {
            Tiles = new ObservableCollection<string>();
            PageObject = tile;

            SnapCommand = new Command(OnSnapCommandExecute);   
        }

        public override string Title { get { return "SnapTileContainerVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> Tiles
        {
            get { return GetValue<ObservableCollection<string>>(TilesProperty); }
            set { SetValue(TilesProperty, value); }
        }

        /// <summary>
        /// Register the Tiles property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TilesProperty = RegisterProperty("Tiles", typeof(ObservableCollection<string>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int NumberOfTiles
        {
            get { return GetValue<int>(NumberOfTilesProperty); }
            set { SetValue(NumberOfTilesProperty, value); }
        }

        /// <summary>
        /// Register the NumberOfTiles property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfTilesProperty = RegisterProperty("NumberOfTiles", typeof(int), 0, (sender, e) => ((CLPSnapTileContainerViewModel)sender).OnNumberOfTilesChanged());

        /// <summary>
        /// Called when the name property has changed.
        /// </summary>
        private void OnNumberOfTilesChanged()
        {
            //Claire, HistoryItems stuff here
            CLPPage currentPage = CLPServiceAgent.Instance.GetPageFromID(PageObject.PageID);
            int diff = NumberOfTiles - Tiles.Count;

            if (diff > 0)
            {
                for (int i = diff - 1; i >= 0; i--)
                {
                    Tiles.Add("SpringGreen");
                    //if (!currentPage.PageHistory.IgnoreHistory)
                    //{
                    //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileSnap, (PageObject as CLPSnapTileContainer).UniqueID, null, null);
                    //    currentPage.PageHistory.HistoryItems.Add(item);
                    //}
                }
            }
            else
            {
                diff *= -1;
                for (int i = diff - 1; i >= 0; i--)
                {
                    Tiles.RemoveAt(Tiles.Count - 1);
                    //if (!currentPage.PageHistory.IgnoreHistory)
                    //{
                    //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileRemoveTile, (PageObject as CLPSnapTileContainer).UniqueID, null, null);
                    //    currentPage.PageHistory.HistoryItems.Add(item);
                    //}
                }
            }

            Height = CLPSnapTileContainer.TILE_HEIGHT * NumberOfTiles;
        }

        /// <summary>
        /// Gets the SnapCommand command.
        /// </summary>
        public Command SnapCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SnapCommand command is executed.
        /// </summary>
        private void OnSnapCommandExecute()
        {

            CLPPage currentPage = CLPServiceAgent.Instance.GetPageFromID(PageObject.PageID);

            foreach (var pageObject in currentPage.PageObjects)
            {
                if (pageObject is CLPSnapTileContainer)
                {

                    CLPSnapTileContainer otherTile = pageObject as CLPSnapTileContainer;
                    if (PageObject.UniqueID != otherTile.UniqueID)
                    {
                        double deltaX = Math.Abs(PageObject.Position.X - otherTile.Position.X);
                        double deltaYBottomSnap = Math.Abs(PageObject.Position.Y - (otherTile.Position.Y + otherTile.Height));
                        double deltaYTopSnap = Math.Abs(otherTile.Position.Y - (PageObject.Position.Y + PageObject.Height));
                        if (deltaX < 50)
                        {
                            if (deltaYBottomSnap < 55)
                            {
                                //int count = 0;
                                //foreach (var tileColor in Tiles)
                                //{
                                //    count++;
                                //}
                                int oldCount = otherTile.NumberOfTiles;
                                otherTile.NumberOfTiles += NumberOfTiles;
                                CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
                                if (!currentPage.PageHistory.IgnoreHistory)
                                {
                                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileSnap, otherTile.UniqueID, oldCount.ToString(), otherTile.NumberOfTiles.ToString());
                                    //currentPage.PageHistory.HistoryItems.Add(item);
                                    String ID = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage.Page.PageHistory.UniqueID;
                                    CLPHistory.AddToHistoryItems(item, new Guid(ID));
                                }
                                //container.Height = (CLPSnapTileContainer.TILE_HEIGHT) * otherTile.Tiles.Count;
                                //CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                //container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(otherTile.PageObject, item);
                                //item.OldValue = oldCount.ToString();
                                //item.NewValue = otherTile.Tiles.Count.ToString();

                                //CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;
                                
                                //if (!currentPage.PageHistory.IgnoreHistory)
                                //{
                                //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileSnap, otherTile.UniqueID, oldCount.ToString(), otherTile.NumberOfTiles.ToString());
                                //    currentPage.PageHistory.HistoryItems.Add(item);
                                //}
                                
                                //for (int i = 0; i < (pageObject as CLPSnapTileContainer).NumberOfTiles; i++)
                                //{
                                //    (pageObject as CLPSnapTileContainer).NumberOfTiles--;
                                //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileRemoveTile, (pageObject as CLPSnapTileContainer).UniqueID, null, null);
                                //    currentPage.PageHistory.HistoryItems.Add(item);
                                //}
                                break;
                            }
                            else if (deltaYTopSnap < 55)
                            {
                                int oldCount = NumberOfTiles;
                                NumberOfTiles += otherTile.NumberOfTiles;

                                //does it really matter if we technically add to the top or bottom?
                                //int oldCount = otherTile.NumberOfTiles;
                                //otherTile.NumberOfTiles += NumberOfTiles;
                                
                                CLPServiceAgent.Instance.RemovePageObjectFromPage(otherTile);
                                if (!currentPage.PageHistory.IgnoreHistory)
                                {
                                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileSnap, PageObject.UniqueID, oldCount.ToString(), NumberOfTiles.ToString());
                                    //currentPage.PageHistory.HistoryItems.Add(item);
                                    String ID = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage.Page.PageHistory.UniqueID;
                                    CLPHistory.AddToHistoryItems(item, new Guid(ID));
                                }
                                //pageObjectContainerViewModel.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count;
                                //container.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count;
                                //CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                //container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(tile.PageObject, item);
                                //item.OldValue = oldCount.ToString();
                                //item.NewValue = tile.Tiles.Count.ToString();
                                //CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;
                                //if (!currentPage.PageHistory.IgnoreHistory )
                                //{
                                //    //CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileSnap, (pageObject as CLPSnapTileContainer).UniqueID, oldCount.ToString(), NumberOfTiles.ToString());
                                //    //currentPage.PageHistory.HistoryItems.Add(item);

                                   
                                //}

                                
                                //for (int i = 0; i < otherTile.NumberOfTiles; i++)
                                //{
                                //    otherTile.NumberOfTiles--;
                                //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.SnapTileRemoveTile, otherTile.UniqueID, null, null);
                                //    currentPage.PageHistory.HistoryItems.Add(item);
                                //}
                                //if (otherTile.NumberOfTiles == 1)
                                //{
                                //    CLPServiceAgent.Instance.RemovePageObjectFromPage(otherTile);
                                //}
                                break;
                            }


                        }
                    }
                }
            }
        }

    }
}
