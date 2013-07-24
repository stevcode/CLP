using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPTileHeightChanged : CLPHistoryItem
    {

        #region Constructor

        public CLPTileHeightChanged(string tileUniqueID, int newHeight, int oldHeight)
            : base(HistoryItemType.TileHeightChanged)
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
        protected CLPTileHeightChanged(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

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

        public override void Undo(CLPPage page)
        {
            var tile = (GetPageObjectByUniqueID(page, TileUniqueID) as CLPSnapTileContainer);
            if(tile != null)
            {
                tile.NumberOfTiles = OldHeight;
            }
        }

        override public void Redo(CLPPage page)
        {
            var tile = (GetPageObjectByUniqueID(page, TileUniqueID) as CLPSnapTileContainer);
            if(tile != null)
            {
                tile.NumberOfTiles = NewHeight;
            }
        }

        override public CLPHistoryItem GetUndoFingerprint(CLPPage page)
        {
            return null;
        }

        override public CLPHistoryItem GetRedoFingerprint(CLPPage page)
        {
            return null;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is CLPTileHeightChanged) ||
                (obj as CLPTileHeightChanged).TileUniqueID != TileUniqueID ||
                (obj as CLPTileHeightChanged).NewHeight != NewHeight ||
                (obj as CLPTileHeightChanged).OldHeight != OldHeight)
            {
                return false;
            }
            return base.Equals(obj);
        }

        #endregion //Methods
    }
}

