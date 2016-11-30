using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Ann
{
    [Serializable]
    public class CLPArrayRotateHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArrayRotateHistoryItem" /> from scratch.
        /// </summary>
        public CLPArrayRotateHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="CLPArrayRotateHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArrayRotateHistoryItem(CLPPage parentPage, Person owner, string arrayID, double initialXPosition, double initialYPosition)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            ArrayXCoord = initialXPosition;
            ArrayYCoord = initialYPosition;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
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

        /// <summary>
        /// Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryItem" /> modifies.
        /// </summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof(string));

        /// <summary>
        /// X coordinate to restore the array's position to.
        /// </summary>
        public double ArrayXCoord
        {
            get { return GetValue<double>(ArrayXCoordProperty); }
            set { SetValue(ArrayXCoordProperty, value); }
        }

        public static readonly PropertyData ArrayXCoordProperty = RegisterProperty("ArrayXCoord", typeof(double));

        /// <summary>
        /// Y coordinate to restore the array's position to.
        /// </summary>
        public double ArrayYCoord
        {
            get { return GetValue<double>(ArrayYCoordProperty); }
            set { SetValue(ArrayYCoordProperty, value); }
        }

        public static readonly PropertyData ArrayYCoordProperty = RegisterProperty("ArrayYCoord", typeof(double));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo) { RotateArray(); }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo) { RotateArray(); }

        private void RotateArray()
        {
            var array = ParentPage.GetPageObjectByID(ArrayID) as ACLPArrayBase;
            if(array != null)
            {
                var tempX = array.XPosition;
                var tempY = array.YPosition;
                array.RotateArray();
                array.XPosition = ArrayXCoord;
                array.YPosition = ArrayYCoord;
                ArrayXCoord = tempX;
                ArrayYCoord = tempY;
            }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArrayRotateHistoryItem;
            if(clonedHistoryItem == null)
            {
                return null;
            }

            var array = ParentPage.GetPageObjectByID(ArrayID) as ACLPArrayBase;
            clonedHistoryItem.ArrayXCoord = array.XPosition;
            clonedHistoryItem.ArrayYCoord = array.YPosition;

            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}