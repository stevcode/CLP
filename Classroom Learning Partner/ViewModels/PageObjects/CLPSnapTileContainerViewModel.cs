using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPSnapTileContainerViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPSnapTileViewModel class.
        /// </summary>
        public CLPSnapTileContainerViewModel(CLPSnapTileContainer tile, CLPPageViewModel pageViewModel)
            : base(pageViewModel)
        {
            PageObject = tile;
            foreach (var tileColor in tile.Tiles)
            {
                Tiles.Add(tileColor);
            }

            _tiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_tiles_CollectionChanged);
        }

        void _tiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Height = CLPSnapTileContainer.TILE_HEIGHT * Tiles.Count;
        }

        private ObservableCollection<string> _tiles = new ObservableCollection<string>();
        /// <summary>
        /// List of color names, each list item being a tile in the tower.
        /// </summary>
        public ObservableCollection<string> Tiles
        {
            get { return _tiles; }
        }


    }
}
