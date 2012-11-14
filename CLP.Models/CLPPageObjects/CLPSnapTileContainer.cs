using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using Catel.Data;
using System.Collections.ObjectModel;

namespace CLP.Models
{
    [Serializable]
    public class CLPSnapTileContainer : CLPPageObjectBase
    {
        #region Variables

        public const int TILE_HEIGHT = 45;

        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPSnapTileContainer(Point pt, CLPPage page)
            : base(page)
        {
            XPosition = pt.X;
            YPosition = pt.Y;
            NumberOfTiles = 1;
            Height = TILE_HEIGHT * NumberOfTiles;
            Width = TILE_HEIGHT;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPSnapTileContainer(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int NumberOfTiles
        {
            get { return GetValue<int>(NumberOfTilesProperty); }
            set { SetValue(NumberOfTilesProperty, value); }
        }

        /// <summary>
        /// Register the NumberOfTiles property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfTilesProperty = RegisterProperty("NumberOfTiles", typeof(int), 1);

        #endregion

        #region Methods

        public override string PageObjectType
        {
            get { return "CLPSnapTileContainer"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPSnapTileContainer newSnapTile = this.Clone() as CLPSnapTileContainer;
            newSnapTile.UniqueID = Guid.NewGuid().ToString();

            return newSnapTile;
        }

        #endregion        
    }
}
