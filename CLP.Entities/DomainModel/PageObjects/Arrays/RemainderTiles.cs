using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Ann
{
    [Serializable]
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
            FuzzyFactorCardID = fuzzyFactorCard.ID;
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

        public override int ZIndex { get { return 40; } }

        /// <summary>
        /// Unique Identifier of the <see cref="FuzzyFactorCard" /> this <see cref="RemainderTiles" /> is attached to.
        /// </summary>
        public string FuzzyFactorCardID
        {
            get { return GetValue<string>(FuzzyFactorCardIDProperty); }
            set { SetValue(FuzzyFactorCardIDProperty, value); }
        }

        public static readonly PropertyData FuzzyFactorCardIDProperty = RegisterProperty("FuzzyFactorCardID", typeof(string), string.Empty);

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

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

        #region Methods

        public override IPageObject Duplicate()
        {
            var newRemainderTiles = Clone() as RemainderTiles;
            if(newRemainderTiles == null)
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

        public override void OnDeleted()
        {
            var fuzzyFactorCard = ParentPage.GetPageObjectByID(FuzzyFactorCardID) as FuzzyFactorCard;
            if(fuzzyFactorCard != null)
            {
                fuzzyFactorCard.RemainderTiles = null;
            }
            base.OnDeleted();
        }

        #endregion //Methods
    }
}