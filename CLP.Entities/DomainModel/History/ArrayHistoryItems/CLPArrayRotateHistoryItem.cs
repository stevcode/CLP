using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayRotateHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayRotateHistoryItem" /> from scratch.</summary>
        public CLPArrayRotateHistoryItem() { }

        /// <summary>Initializes <see cref="CLPArrayRotateHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArrayRotateHistoryItem(CLPPage parentPage,
                                         Person owner,
                                         string arrayID,
                                         double oldXPosition,
                                         double oldYPosition,
                                         double newXPosition,
                                         double newYPosition,
                                         double oldWidth,
                                         double oldHeight,
                                         int oldRows,
                                         int oldColumns)
            : base(parentPage, owner)
        {
            // TODO: Need old height/width
            ArrayID = arrayID;
            OldXPosition = oldXPosition;
            OldYPosition = oldYPosition;
            NewXPosition = newXPosition;
            NewYPosition = newYPosition;
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            OldRows = oldRows;
            OldColumns = oldColumns;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArrayRotateHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryItem" /> modifies.</summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof (string));

        /// <summary>X Position before rotate.</summary>
        /// <remarks>Legacy property name prior to conversion, should be OldXPosition.</remarks>
        [Obsolete("Use OldXPosition now.")]
        public double ArrayXCoord
        {
            get { return GetValue<double>(ArrayXCoordProperty); }
            set { SetValue(ArrayXCoordProperty, value); }
        }

        public static readonly PropertyData ArrayXCoordProperty = RegisterProperty("ArrayXCoord", typeof (double));

        /// <summary>Y Position before rotate.</summary>
        /// <remarks>Legacy property name prior to conversion, should be OldYPosition.</remarks>
        [Obsolete("Use OldYPosition now.")]
        public double ArrayYCoord
        {
            get { return GetValue<double>(ArrayYCoordProperty); }
            set { SetValue(ArrayYCoordProperty, value); }
        }

        public static readonly PropertyData ArrayYCoordProperty = RegisterProperty("ArrayYCoord", typeof (double));

        /// <summary>X Position before rotate.</summary>
        public double OldXPosition
        {
            get { return GetValue<double>(OldXPositionProperty); }
            set { SetValue(OldXPositionProperty, value); }
        }

        public static readonly PropertyData OldXPositionProperty = RegisterProperty("OldXPosition", typeof (double));

        /// <summary>Y Position before rotate.</summary>
        public double OldYPosition
        {
            get { return GetValue<double>(OldYPositionProperty); }
            set { SetValue(OldYPositionProperty, value); }
        }

        public static readonly PropertyData OldYPositionProperty = RegisterProperty("OldYPosition", typeof (double));
        

        /// <summary>X Position after rotate.</summary>
        public double NewXPosition
        {
            get { return GetValue<double>(NewXPositionProperty); }
            set { SetValue(NewXPositionProperty, value); }
        }

        public static readonly PropertyData NewXPositionProperty = RegisterProperty("NewXPosition", typeof (double));

        /// <summary>Y Position after rotate.</summary>
        public double NewYPosition
        {
            get { return GetValue<double>(NewYPositionProperty); }
            set { SetValue(NewYPositionProperty, value); }
        }

        public static readonly PropertyData NewYPositionProperty = RegisterProperty("NewYPosition", typeof (double));

        /// <summary>Rows value before rotate.</summary>
        public int OldRows
        {
            get { return GetValue<int>(OldRowsProperty); }
            set { SetValue(OldRowsProperty, value); }
        }

        public static readonly PropertyData OldRowsProperty = RegisterProperty("OldRows", typeof (int));

        /// <summary>Columns value before rotate.</summary>
        public int OldColumns
        {
            get { return GetValue<int>(OldColumnsProperty); }
            set { SetValue(OldColumnsProperty, value); }
        }

        public static readonly PropertyData OldColumnsProperty = RegisterProperty("OldColumns", typeof (int));

        /// <summary>Width before rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double OldWidth
        {
            get { return GetValue<double>(OldWidthProperty); }
            set { SetValue(OldWidthProperty, value); }
        }

        public static readonly PropertyData OldWidthProperty = RegisterProperty("OldWidth", typeof (double));


        /// <summary>Height before rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double OldHeight
        {
            get { return GetValue<double>(OldHeightProperty); }
            set { SetValue(OldHeightProperty, value); }
        }

        public static readonly PropertyData OldHeightProperty = RegisterProperty("OldHeight", typeof (double));

        /// <summary>Width after rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double NewWidth
        {
            get { return GetValue<double>(NewWidthProperty); }
            set { SetValue(NewWidthProperty, value); }
        }

        public static readonly PropertyData NewWidthProperty = RegisterProperty("NewWidth", typeof (double));

        /// <summary>Height after rotate.</summary>
        /// <remarks>Not necessary for history playback, but serves in history analysis.</remarks>
        public double NewHeight
        {
            get { return GetValue<double>(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        public static readonly PropertyData NewHeightProperty = RegisterProperty("NewHeight", typeof (double));

        public override string FormattedValue
        {
            get
            {
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                return array == null
                           ? string.Format("[ERROR] on Index #{0}, Array for Rotate not found on page or in history.", HistoryIndex)
                           : string.Format("Index #{0}, Array rotated from [{1}x{2}] to [{2}x{1}]", HistoryIndex, OldRows, OldColumns);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Rotate not found on page or in history.", HistoryIndex);
                return;
            }

            NewXPosition = array.XPosition;
            NewYPosition = array.YPosition;
            NewWidth = array.Width;
            NewHeight = array.Height;
            array.RotateArray();
            array.XPosition = ArrayXCoord;
            array.YPosition = ArrayYCoord;
            OldXPosition = ArrayXCoord;
            OldYPosition = ArrayYCoord;
            OldWidth = array.Width;
            OldHeight = array.Height;
            OldRows = array.Rows;
            OldColumns = array.Columns;
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            RotateArray(true);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            RotateArray(false);
        }

        private void RotateArray(bool isUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Rotate not found on page or in history.", HistoryIndex);
                return;
            }

            array.RotateArray();
            array.XPosition = isUndo ? ArrayXCoord : NewXPosition;
            array.YPosition = isUndo ? ArrayYCoord : NewYPosition;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArrayRotateHistoryItem;
            if (clonedHistoryItem == null)
            {
                return null;
            }

            var array = ParentPage.GetPageObjectByID(ArrayID) as ACLPArrayBase;
            clonedHistoryItem.ArrayXCoord = array.XPosition;
            clonedHistoryItem.ArrayYCoord = array.YPosition;

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return ArrayID == id; }

        #endregion //Methods
    }
}