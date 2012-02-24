using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using Catel.Data;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPSnapTileContainer : CLPPageObjectBase
    {
        #region Variables

        public const int TILE_HEIGHT = 50;

        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPSnapTileContainer(Point pt, string color)
            : base()
        {
            Position = pt;
            Tiles = new ObservableCollection<string>();
            Tiles.Add(color);
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
        /// List of color names, each list item being a tile in the tower.
        /// </summary>
        public ObservableCollection<string> Tiles
        {
            get { return GetValue<ObservableCollection<string>>(TilesProperty); }
            private set { SetValue(TilesProperty, value); }
        }

        /// <summary>
        /// Register the Tiles property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TilesProperty = RegisterProperty("Tiles", typeof(ObservableCollection<string>), new ObservableCollection<string>());

        #endregion

        #region Methods

        /// <summary>
        /// Validates the fields.
        /// </summary>
        protected override void ValidateFields()
        {
            // TODO: Implement any field validation of this object. Simply set any error by using the SetFieldError method
        }

        /// <summary>
        /// Validates the business rules.
        /// </summary>
        protected override void ValidateBusinessRules()
        {
            // TODO: Implement any business rules of this object. Simply set any error by using the SetBusinessRuleError method
        }

        public override string PageObjectType
        {
            get { return "CLPSnapTileContainer"; }
        }

        public override CLPPageObjectBase Duplicate()
        {
            CLPSnapTileContainer newSnapTile = this.Clone() as CLPSnapTileContainer;
            newSnapTile.UniqueID = Guid.NewGuid().ToString();

            return newSnapTile;
        }

        #endregion        
    }
}
