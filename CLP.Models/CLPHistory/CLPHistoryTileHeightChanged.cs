using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryTileHeightChanged : ACLPHistoryItemBase
    {

        #region Constructors

        public CLPHistoryTileHeightChanged(ICLPPage parentPage, string tileUniqueID, int newHeight, int oldHeight)
            : base(parentPage)
        {
            TileUniqueID = tileUniqueID;
            NewHeight = newHeight;
            OldHeight = oldHeight;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryTileHeightChanged(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// UniqueID of the Tile this change effects.
        /// </summary>
        public string TileUniqueID
        {
            get { return GetValue<string>(TileUniqueIDProperty); }
            set { SetValue(TileUniqueIDProperty, value); }
        }

        public static readonly PropertyData TileUniqueIDProperty = RegisterProperty("TileUniqueID", typeof(string));

        /// <summary>
        /// New Height of Tile after a snap or tile deletion.
        /// </summary>
        public int NewHeight
        {
            get { return GetValue<int>(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        public static readonly PropertyData NewHeightProperty = RegisterProperty("NewHeight", typeof(int));

        /// <summary>
        /// Old Height of Tile after a snap or tile deletion.
        /// </summary>
        public int OldHeight
        {
            get { return GetValue<int>(OldHeightProperty); }
            set { SetValue(OldHeightProperty, value); }
        }

        public static readonly PropertyData OldHeightProperty = RegisterProperty("OldHeight", typeof(int));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var tile = ParentPage.GetPageObjectByUniqueID(TileUniqueID) as CLPSnapTileContainer;
            if(tile != null)
            {
                tile.NumberOfTiles = OldHeight;
            }
            else
            {
                Logger.Instance.WriteToLog("Tile not found on page for UndoAction");
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var tile = ParentPage.GetPageObjectByUniqueID(TileUniqueID) as CLPSnapTileContainer;
            if(tile != null)
            {
                tile.NumberOfTiles = NewHeight;
            }
            else
            {
                Logger.Instance.WriteToLog("Tile not found on page for RedoAction");
            }
        }

        #endregion //Methods
    }
}

