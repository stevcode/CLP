using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RemainderTiles : APageObjectBase
    {
        public const double TILE_HEIGHT = 61.0;
        public const double NUMBER_OF_TILES_PER_ROW = 5.0;

        #region Constructors

        /// <summary>
        /// Initializes <see cref="RemainderTiles" /> from scratch.
        /// </summary>
        public RemainderTiles() { }

        /// <summary>
        /// Initializes <see cref="RemainderTiles" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="RemainderTiles" /> belongs to.</param>
        /// <param name="divisionTemplate">Associated <see cref="FuzzyFactorCard" /> the <see cref="RemainderTiles" /> acts against.</param>
        public RemainderTiles(CLPPage parentPage, FuzzyFactorCard divisionTemplate)
            : base(parentPage)
        {
            Height = Math.Ceiling(divisionTemplate.CurrentRemainder / NUMBER_OF_TILES_PER_ROW) * TILE_HEIGHT;
            Width = NUMBER_OF_TILES_PER_ROW * TILE_HEIGHT;
        }

        /// <summary>
        /// Initializes <see cref="RemainderTiles" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public RemainderTiles(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Colors of each tile
        /// </summary>
        public ObservableCollection<string> TileColors
        {
            get { return GetValue<ObservableCollection<string>>(TileColorsProperty); }
            set { SetValue(TileColorsProperty, value); }
        }

        public static readonly PropertyData TileColorsProperty = RegisterProperty("TileColors", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return "Remainder Tiles"; }
        }

        public override int ZIndex
        {
            get { return 40; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public override IPageObject Duplicate()
        {
            var newRemainderTiles = Clone() as RemainderTiles;
            if (newRemainderTiles == null)
            {
                return null;
            }
            newRemainderTiles.CreationDate = DateTime.Now;
            newRemainderTiles.ID = Guid.NewGuid().ToCompactID();
            newRemainderTiles.VersionIndex = 0;
            newRemainderTiles.LastVersionIndex = null;
            newRemainderTiles.ParentPage = ParentPage;

            return newRemainderTiles;
        }

        #endregion //APageObjectBase Overrides
    }
}