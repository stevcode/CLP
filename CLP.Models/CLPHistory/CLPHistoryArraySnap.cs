using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArraySnap : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryArraySnap(ICLPPage parentPage, CLPArray persistingArray, CLPArray snappedArray, bool isHorizontal) : base(parentPage)
        {
            IsHorizontal = isHorizontal;
            SnappedArray = snappedArray;
            SnappedArrayUniqueID = snappedArray.UniqueID;
            SnappedArraySquareSize = snappedArray.ArrayWidth / snappedArray.Columns;
            PersistingArrayUniqueID = persistingArray.UniqueID;
            PersistingArrayDivisionBehavior = persistingArray.IsDivisionBehaviorOn;
            PersistingArrayHorizontalDivisions = new ObservableCollection<CLPArrayDivision>(persistingArray.HorizontalDivisions);
            PersistingArrayVerticalDivisions = new ObservableCollection<CLPArrayDivision>(persistingArray.VerticalDivisions);
            PersistingArrayRowsOrColumns = isHorizontal ? persistingArray.Rows : persistingArray.Columns;
            PersistingArrayXOrYPosition = isHorizontal ? persistingArray.YPosition : persistingArray.XPosition;
        }

        /// <summary>
        ///     Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPHistoryArraySnap(SerializationInfo info, StreamingContext context) : base(info, context) {}

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>
        ///     True if the arrays snap together along a horizontal edge, false if along a vertical one
        /// </summary>
        public bool IsHorizontal
        {
            get { return GetValue<bool>(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }

        public static readonly PropertyData IsHorizontalProperty = RegisterProperty("IsHorizontal", typeof(bool));

        /// <summary>
        ///     Array that is snapped onto and then deleted.  Null if it's currently on the page.
        /// </summary>
        public CLPArray SnappedArray
        {
            get { return GetValue<CLPArray>(SnappedArrayProperty); }
            set { SetValue(SnappedArrayProperty, value); }
        }

        public static readonly PropertyData SnappedArrayProperty = RegisterProperty("SnappedArray", typeof(CLPArray));

        /// <summary>
        ///     UniqueID of the array that is snapped onto and then deleted.
        /// </summary>
        public string SnappedArrayUniqueID
        {
            get { return GetValue<string>(SnappedArrayUniqueIDProperty); }
            set { SetValue(SnappedArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData SnappedArrayUniqueIDProperty = RegisterProperty("SnappedArrayUniqueID", typeof(string), string.Empty);

        /// <summary>
        ///     Original square size of the array that is snapped onto and then deleted.
        /// </summary>
        public double SnappedArraySquareSize
        {
            get { return GetValue<double>(SnappedArraySquareSizeProperty); }
            set { SetValue(SnappedArraySquareSizeProperty, value); }
        }

        public static readonly PropertyData SnappedArraySquareSizeProperty = RegisterProperty("SnappedArraySquareSize", typeof(double));

        /// <summary>
        ///     UniqueID of the array that snaps on and continues to exist.
        /// </summary>
        public string PersistingArrayUniqueID
        {
            get { return GetValue<string>(PersistingArrayUniqueIDProperty); }
            set { SetValue(PersistingArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData PersistingArrayUniqueIDProperty = RegisterProperty("PersistingArrayUniqueID", typeof(string), string.Empty);

        /// <summary>
        ///     Horizontal divisions that the persisting array should be set to have when this
        ///     history event fires (undoes or redoes, whichever comes next).
        /// </summary>
        public ObservableCollection<CLPArrayDivision> PersistingArrayHorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(PersistingArrayHorizontalDivisionsProperty); }
            set { SetValue(PersistingArrayHorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayHorizontalDivisionsProperty = RegisterProperty("PersistingArrayHorizontalDivisions", typeof(ObservableCollection<CLPArrayDivision>));

        /// <summary>
        ///     Vertical divisions that the persisting array should be set to have when this
        ///     history event fires (undoes or redoes, whichever comes next).
        /// </summary>
        public ObservableCollection<CLPArrayDivision> PersistingArrayVerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(PersistingArrayVerticalDivisionsProperty); }
            set { SetValue(PersistingArrayVerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayVerticalDivisionsProperty = RegisterProperty("PersistingArrayVerticalDivisions", typeof(ObservableCollection<CLPArrayDivision>));


        /// <summary>
        ///     Value of IsDivisionBehaviorOn prior to the history event (which sets it true)
        /// </summary>
        public bool PersistingArrayDivisionBehavior
        {
            get { return GetValue<bool>(PersistingArrayDivisionBehaviorProperty); }
            set { SetValue(PersistingArrayDivisionBehaviorProperty, value); }
        }

        public static readonly PropertyData PersistingArrayDivisionBehaviorProperty = RegisterProperty("PersistingArrayDivisionBehavior", typeof(bool));

        /// <summary>
        ///     Rows or columns that the persisting array should be set to have when this history event fires (undoes or
        ///     redoes, whichever comes next).
        /// </summary>
        public int PersistingArrayRowsOrColumns
        {
            get { return GetValue<int>(PersistingArrayRowsOrColumnsProperty); }
            set { SetValue(PersistingArrayRowsOrColumnsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayRowsOrColumnsProperty = RegisterProperty("PersistingArrayRowsOrColumns", typeof(int));

        /// <summary>
        ///     Rows or columns that the persisting array should be set to have when this history event fires (undoes
        ///     or redoes, whichever comes next).
        /// </summary>
        public double PersistingArrayXOrYPosition
        {
            get { return GetValue<double>(PersistingArrayXOrYPositionProperty); }
            set { SetValue(PersistingArrayXOrYPositionProperty, value); }
        }

        public static readonly PropertyData PersistingArrayXOrYPositionProperty = RegisterProperty("PersistingArrayXOrYPosition", typeof(double));

        #endregion //Properties

        #region Methods

        /// <summary>
        ///     Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var persistingArray = ParentPage.GetPageObjectByUniqueID(PersistingArrayUniqueID) as CLPArray;
            if(persistingArray == null)
            {
                Logger.Instance.WriteToLog("ArraySnap Undo Failure: Can't find persisting array.");
                return;
            }

            SnappedArray.SizeArrayToGridLevel(SnappedArraySquareSize);
            SnappedArray.ParentPage = ParentPage;
            ParentPage.PageObjects.Add(SnappedArray);
            SnappedArray = null;

            double persistingArraySquareSize = persistingArray.ArrayWidth / persistingArray.Columns;

            RestoreDivisions(persistingArray);
            RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = PersistingArrayDivisionBehavior;
            persistingArray.SizeArrayToGridLevel(persistingArraySquareSize, false);
        }

        /// <summary>
        ///     Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            SnappedArray = ParentPage.GetPageObjectByUniqueID(SnappedArrayUniqueID) as CLPArray;
            if(SnappedArray == null)
            {
                Logger.Instance.WriteToLog("ArraySnap Redo Failure: Can't find snapped array.");
                return;
            }
            SnappedArray.ParentPage = ParentPage;

            var persistingArray = ParentPage.GetPageObjectByUniqueID(PersistingArrayUniqueID) as CLPArray;
            if(persistingArray == null)
            {
                Logger.Instance.WriteToLog("ArraySnap Redo Failure: Can't find persisting array.");
                return;
            }

            ParentPage.PageObjects.Remove(SnappedArray);
            double persistingArraySquareSize = persistingArray.ArrayWidth / persistingArray.Columns;

            RestoreDivisions(persistingArray);
            RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = true;
            persistingArray.SizeArrayToGridLevel(persistingArraySquareSize, false);
        }

        private void RestoreDivisions(CLPArray persistingArray)
        {
            var tempHorizontalDivisions = persistingArray.HorizontalDivisions;
            persistingArray.HorizontalDivisions = PersistingArrayHorizontalDivisions;
            PersistingArrayHorizontalDivisions = tempHorizontalDivisions;

            var tempVerticalDivisions = persistingArray.VerticalDivisions;
            persistingArray.VerticalDivisions = PersistingArrayVerticalDivisions;
            PersistingArrayVerticalDivisions = tempVerticalDivisions;
        }

        private void RestoreDimensionsAndPosition(CLPArray persistingArray)
        {
            if(IsHorizontal)
            {
                var tempRows = persistingArray.Rows;
                persistingArray.Rows = PersistingArrayRowsOrColumns;
                PersistingArrayRowsOrColumns = tempRows;

                var tempPosition = persistingArray.YPosition;
                persistingArray.YPosition = PersistingArrayXOrYPosition;
                PersistingArrayXOrYPosition = tempPosition;
            }
            else
            {
                var tempColumns = persistingArray.Columns;
                persistingArray.Columns = PersistingArrayRowsOrColumns;
                PersistingArrayRowsOrColumns = tempColumns;

                var tempPosition = persistingArray.XPosition;
                persistingArray.XPosition = PersistingArrayXOrYPosition;
                PersistingArrayXOrYPosition = tempPosition;
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryArraySnap;
            var persistingArray = ParentPage.GetPageObjectByUniqueID(PersistingArrayUniqueID) as CLPArray;
            if(clonedHistoryItem == null || persistingArray == null)
            {
                return null;
            }

            clonedHistoryItem.PersistingArrayHorizontalDivisions = persistingArray.HorizontalDivisions;
            clonedHistoryItem.PersistingArrayVerticalDivisions = persistingArray.VerticalDivisions;

            if(IsHorizontal)
            {
                clonedHistoryItem.PersistingArrayRowsOrColumns = persistingArray.Rows + SnappedArray.Rows;
                clonedHistoryItem.PersistingArrayXOrYPosition = persistingArray.YPosition;
            }
            else
            {
                clonedHistoryItem.PersistingArrayRowsOrColumns = persistingArray.Columns + SnappedArray.Columns;
                clonedHistoryItem.PersistingArrayXOrYPosition = persistingArray.XPosition;
            }

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}