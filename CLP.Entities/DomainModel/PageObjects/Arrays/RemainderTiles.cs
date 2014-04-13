using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class RemainderTiles : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="RemainderTiles" /> from scratch.
        /// </summary>
        public RemainderTiles() { }

        /// <summary>
        /// Initializes <see cref="RemainderTiles" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="RemainderTiles" /> belongs to.</param>
        public RemainderTiles(CLPPage parentPage, FuzzyFactorCard fuzzyFactorCard)
            : base(parentPage)
        {
            XPosition = fuzzyFactorCard.XPosition + fuzzyFactorCard.Width + 20.0;
            YPosition = fuzzyFactorCard.YPosition + fuzzyFactorCard.LabelLength;

            Height = Math.Ceiling(fuzzyFactorCard.CurrentRemainder / 5.0) * 61.0; 
            Width = 305.0; 
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

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        /// <summary>
        /// Offsets of each tile
        /// </summary>
        public ObservableCollection<string> TileOffsets
        {
            get { return GetValue<ObservableCollection<string>>(TileOffsetsProperty); }
            set { SetValue(TileOffsetsProperty, value); }
        }

        public static readonly PropertyData TileOffsetsProperty = RegisterProperty("TileOffsets", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newRemainderTiles = Clone() as RemainderTiles;
            if(newRemainderTiles == null)
            {
                return null;
            }
            newRemainderTiles.CreationDate = DateTime.Now;
            newRemainderTiles.ID = Guid.NewGuid().ToString();
            newRemainderTiles.VersionIndex = 0;
            newRemainderTiles.LastVersionIndex = null;
            newRemainderTiles.ParentPage = ParentPage;

            return newRemainderTiles;
        }

        #endregion //Methods
    }
}