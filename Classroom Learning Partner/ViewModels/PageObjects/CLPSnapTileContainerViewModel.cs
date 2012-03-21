using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Collections.ObjectModel;
using Catel.MVVM;
using Catel.Data;
using System.Collections.Specialized;
using System;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
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

            
            for (int i = tile.NumberOfTiles - 1; i >= 0; i--)
            {
                Tiles.Add("SpringGreen");
            }
            //Tiles.CollectionChanged += new NotifyCollectionChangedEventHandler(Tiles_CollectionChanged);
            SnapCommand = new Command(OnSnapCommandExecute);

            
        }

        public override string Title { get { return "SnapTileContainerVM"; } }

        void Tiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Height = CLPSnapTileContainer.TILE_HEIGHT * Tiles.Count;
            (PageObject as CLPSnapTileContainer).NumberOfTiles = Tiles.Count;
        }

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
        public static readonly PropertyData NumberOfTilesProperty = RegisterProperty("NumberOfTiles", typeof(int),0,(sender, e) => ((CLPSnapTileContainerViewModel)sender).OnNumberOfTilesChanged());

        /// <summary>
        /// Called when the name property has changed.
        /// </summary>
        private void OnNumberOfTilesChanged()
        {
            int diff = NumberOfTiles - Tiles.Count;

            if (diff > 0)
            {
                for (int i = diff - 1; i >= 0; i--)
                {
                    Tiles.Add("SpringGreen");
                }
            }
            else
            {
                diff *= -1;
                for (int i = diff - 1; i >= 0; i--)
                {
                    Tiles.RemoveAt(Tiles.Count - 1);
                }
            }

            Tiles.Add("SpringGreen");
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

                                otherTile.NumberOfTiles += NumberOfTiles;

                                //container.Height = (CLPSnapTileContainer.TILE_HEIGHT) * otherTile.Tiles.Count;
                                //CLPHistoryItem item = new CLPHistoryItem("STACK_TILE");
                                //container.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(otherTile.PageObject, item);
                                //item.OldValue = oldCount.ToString();
                                //item.NewValue = otherTile.Tiles.Count.ToString();

                                //CLPSnapTile t = container.PageObjectViewModel.PageViewModel.HistoryVM.ObjectReferences[item.ObjectID] as CLPSnapTile;


                                CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
                                break;
                            }
                            else if (deltaYTopSnap < 55)
                            {
                                //int oldCount = tile.Tiles.Count;

                                //for (int i = otherTile.NumberOfTiles - 1; i >= 0; i--)
                                //{
                                //    Tiles.Add("SpringGreen");
                                //}

                                NumberOfTiles += otherTile.NumberOfTiles;

                                //pageObjectContainerViewModel.Height = (CLPSnapTile.TILE_HEIGHT) * tile.Tiles.Count;
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
