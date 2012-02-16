using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPSnapTile : CLPPageObjectBase
    {
        #region Variables

        public const int TILE_HEIGHT = 45;

        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPSnapTile(Point pt, string color)
            : base()
        {
            Position = pt;
            Tiles.Add(color);
            Height = (TILE_HEIGHT) * Tiles.Count;
            Width = TILE_HEIGHT;

            _tiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_tiles_CollectionChanged);
        }

        void _tiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Height = (TILE_HEIGHT) * Tiles.Count;
        }

        #endregion

        #region Properties

        private ObservableCollection<string> _tiles = new ObservableCollection<string>();
        /// <summary>
        /// List of color names, each list item being a tile in the tower.
        /// </summary>
        public ObservableCollection<string> Tiles
        {
            get { return _tiles; }
        }

        

        #endregion
    }
}
