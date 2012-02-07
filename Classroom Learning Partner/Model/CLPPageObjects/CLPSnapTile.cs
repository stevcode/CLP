using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPSnapTile : CLPPageObjectBase
    {
        public const int TILE_HEIGHT = 50;

        public CLPSnapTile()
            : this(new Point(10, 10), null, null)
        {
        }

        public CLPSnapTile(Point pt)
            : this(pt, null, null)
        {
        }

        public CLPSnapTile(Point pt, CLPSnapTile nextTile, CLPSnapTile prevTile)
            : base()
        {
            Height = TILE_HEIGHT;
            Width = TILE_HEIGHT;
            base.Position = pt;
            NextTile = nextTile;
            PrevTile = prevTile;
        }

        public CLPSnapTile NextTile { get; set; }
        public CLPSnapTile PrevTile { get; set; }

    }
}
