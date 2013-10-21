using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArrayRotate : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryArrayRotate(ICLPPage parentPage, string arrayUniqueID, double initXPos, double initYPos)
            : base(parentPage)
        {
            ArrayUniqueID = arrayUniqueID;
            ArrayXCoord = initXPos;
            ArrayYCoord = initYPos;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryArrayRotate(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 600;
            }
        }

        /// <summary>
        /// UniqueID of the Array that has been rotated.
        /// </summary>
        public string ArrayUniqueID
        {
            get { return GetValue<string>(ArrayUniqueIDProperty); }
            set { SetValue(ArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData ArrayUniqueIDProperty = RegisterProperty("ArrayUniqueID", typeof(string), string.Empty);

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
        protected override void UndoAction(bool isAnimationUndo)
        {
            RotateArray();
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            RotateArray();
        }

        private void RotateArray()
        {
            var array = ParentPage.GetPageObjectByUniqueID(ArrayUniqueID) as CLPArray;
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
            else
            {
                Logger.Instance.WriteToLog("Array not found on page for UndoAction");
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryArrayRotate;
            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}


