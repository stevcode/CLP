using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPSnapTileViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPSnapTileViewModel class.
        /// </summary>
        public CLPSnapTileViewModel(CLPSnapTile tile, CLPPageViewModel pageViewModel)
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
            Height = (CLPSnapTile.TILE_HEIGHT) * Tiles.Count;
            try
            {
                if (e.NewItems != null)
                {
                    foreach (var color in e.NewItems)
                    {
                        (PageObject as CLPSnapTile).Tiles.Add(color as string);
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (var color in e.OldItems)
                    {
                        (PageObject as CLPSnapTile).Tiles.RemoveAt((PageObject as CLPSnapTile).Tiles.Count - 1);
                    }
                }
            }
            catch (ArgumentOutOfRangeException arg)
            {
                Logger.Instance.WriteToLog("Argument out of range when snapping tiles.");
            }

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
