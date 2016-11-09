using System;
using System.Collections.ObjectModel;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RemainderTiles : APageObjectBase
    {
        public const double TILE_HEIGHT = 61.0;
        public const double NUMBER_OF_TILES_PER_ROW = 5.0;

        #region Constructors

        /// <summary>Initializes <see cref="RemainderTiles" /> from scratch.</summary>
        public RemainderTiles() { }

        /// <summary>Initializes <see cref="RemainderTiles" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="RemainderTiles" /> belongs to.</param>
        /// <param name="divisionTemplate">Associated <see cref="DivisionTemplate" /> the <see cref="RemainderTiles" /> acts against.</param>
        public RemainderTiles(CLPPage parentPage, DivisionTemplate divisionTemplate)
            : base(parentPage)
        {
            Height = Math.Ceiling(divisionTemplate.CurrentRemainder / NUMBER_OF_TILES_PER_ROW) * TILE_HEIGHT;
            Width = NUMBER_OF_TILES_PER_ROW * TILE_HEIGHT;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Colors of each tile</summary>
        public ObservableCollection<string> TileColors
        {
            get { return GetValue<ObservableCollection<string>>(TileColorsProperty); }
            set { SetValue(TileColorsProperty, value); }
        }

        public static readonly PropertyData TileColorsProperty = RegisterProperty("TileColors", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName => $"Remainder Tiles with {TileColors.Count} tiles";

        public override string CodedName => Codings.OBJECT_REMAINDER_TILES;

        public override string CodedID => "A";

        public override int ZIndex => 40;

        public override bool IsBackgroundInteractable => false;

        #endregion //APageObjectBase Overrides
    }
}