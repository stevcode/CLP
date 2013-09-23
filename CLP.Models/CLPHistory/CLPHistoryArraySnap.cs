using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArraySnap : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryArraySnap(ICLPPage parentPage, CLPArray persistingArray, CLPArray snappedArray, bool horizontal)
            : base(parentPage)
        {
            Horizontal = horizontal;
            SnappedArray = snappedArray;
            SnappedArrayUniqueID = snappedArray.UniqueID;
            SnappedArraySquareSize = snappedArray.ArrayWidth / snappedArray.Columns;
            PersistingArrayUniqueID = persistingArray.UniqueID;
            PersistingArrayDivisionBehavior = persistingArray.IsDivisionBehaviorOn;
            PersistingArrayDivisions = new ObservableCollection<CLPArrayDivision>(horizontal ? persistingArray.HorizontalDivisions : persistingArray.VerticalDivisions);
            PersistingArrayRowsOrColumns = horizontal ? persistingArray.Rows : persistingArray.Columns;
            PersistingArrayXOrYPosition = horizontal ? persistingArray.YPosition : persistingArray.XPosition;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryArraySnap(SerializationInfo info, StreamingContext context)
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
        /// True if the arrays snap together along a horizontal edge, false if along a vertical one
        /// </summary>
        public bool Horizontal
        {
            get { return GetValue<bool>(HorizontalProperty); }
            set { SetValue(HorizontalProperty, value); }
        }

        public static readonly PropertyData HorizontalProperty = RegisterProperty("Horizontal", typeof(bool));


        /// <summary>
        /// Array that is snapped onto and then deleted.  Null if it's currently on the page.
        /// </summary>
        public CLPArray SnappedArray
        {
            get { return GetValue<CLPArray>(SnappedArrayProperty); }
            set { SetValue(SnappedArrayProperty, value); }
        }

        public static readonly PropertyData SnappedArrayProperty = RegisterProperty("SnappedArray", typeof(CLPArray));

        /// <summary>
        /// UniqueID of the array that is snapped onto and then deleted.
        /// </summary>
        public string SnappedArrayUniqueID
        {
            get { return GetValue<string>(SnappedArrayUniqueIDProperty); }
            set { SetValue(SnappedArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData SnappedArrayUniqueIDProperty = RegisterProperty("SnappedArrayUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// Original square size of the array that is snapped onto and then deleted.
        /// </summary>
        public double SnappedArraySquareSize
        {
            get { return GetValue<double>(SnappedArraySquareSizeProperty); }
            set { SetValue(SnappedArraySquareSizeProperty, value); }
        }

        public static readonly PropertyData SnappedArraySquareSizeProperty = RegisterProperty("SnappedArraySquareSize", typeof(double));

        /// <summary>
        /// UniqueID of the array that snaps on and continues to exist.
        /// </summary>
        public string PersistingArrayUniqueID
        {
            get { return GetValue<string>(PersistingArrayUniqueIDProperty); }
            set { SetValue(PersistingArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData PersistingArrayUniqueIDProperty = RegisterProperty("PersistingArrayUniqueID", typeof(string), string.Empty);

        /// <summary>
        /// Divisions (either horizontal or vertical) that the persisting array should be set to have when this
        /// history event fires (undoes or redoes, whichever comes next).
        /// </summary>
        public ObservableCollection<CLPArrayDivision> PersistingArrayDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(PersistingArrayDivisionsProperty); }
            set { SetValue(PersistingArrayDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayDivisionsProperty = RegisterProperty("PersistingArrayDivisions", typeof(ObservableCollection<CLPArrayDivision>));

        /// <summary>
        /// Value of IsDivisionBehaviorOn prior to the history event (which sets it true)
        /// </summary>
        public bool PersistingArrayDivisionBehavior
        {
            get { return GetValue<bool>(PersistingArrayDivisionBehaviorProperty); }
            set { SetValue(PersistingArrayDivisionBehaviorProperty, value); }
        }

        public static readonly PropertyData PersistingArrayDivisionBehaviorProperty = RegisterProperty("PersistingArrayDivisionBehavior", typeof(bool));

        /// <summary>
        /// Rows or columns that the persisting array should be set to have when this history event fires (undoes or
        /// redoes, whichever comes next).
        /// </summary>
        public int PersistingArrayRowsOrColumns
        {
            get { return GetValue<int>(PersistingArrayRowsOrColumnsProperty); }
            set { SetValue(PersistingArrayRowsOrColumnsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayRowsOrColumnsProperty = RegisterProperty("PersistingArrayRowsOrColumns", typeof(int));

        /// <summary>
        /// Rows or columns that the persisting array should be set to have when this history event fires (undoes
        /// or redoes, whichever comes next).
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
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
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
            ParentPage.PageObjects.Add(SnappedArray);
            SnappedArray = null;

            double persistingArraySquareSize = persistingArray.ArrayWidth / persistingArray.Columns;

            if(Horizontal)
            {
                toggleHorizontal(persistingArray);
            }
            else
            {
                toggleVertical(persistingArray);
            }

            persistingArray.IsDivisionBehaviorOn = PersistingArrayDivisionBehavior;
            persistingArray.SizeArrayToGridLevel(persistingArraySquareSize, false);
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            SnappedArray = ParentPage.GetPageObjectByUniqueID(SnappedArrayUniqueID) as CLPArray;
            if (SnappedArray == null) 
            {
                Logger.Instance.WriteToLog("ArraySnap Redo Failure: Can't find snapped array.");
                return;
            }

            var persistingArray = ParentPage.GetPageObjectByUniqueID(PersistingArrayUniqueID) as CLPArray;
            if(persistingArray == null)
            {
                Logger.Instance.WriteToLog("ArraySnap Redo Failure: Can't find persisting array.");
                return;
            }

            ParentPage.PageObjects.Remove(SnappedArray);
            var persistingArraySquareSize = persistingArray.ArrayWidth / persistingArray.Columns;

            if (Horizontal) 
            {
                toggleHorizontal(persistingArray);
            } else 
            {
                toggleVertical(persistingArray);
            }

            persistingArray.IsDivisionBehaviorOn = true;
            persistingArray.SizeArrayToGridLevel(persistingArraySquareSize, false);
        }

        private void toggleHorizontal(CLPArray persistingArray)
        {
            var tempDivisions = persistingArray.HorizontalDivisions;
            persistingArray.HorizontalDivisions = PersistingArrayDivisions;
            PersistingArrayDivisions = tempDivisions;

            var tempRows = persistingArray.Rows;
            persistingArray.Rows = PersistingArrayRowsOrColumns;
            PersistingArrayRowsOrColumns = tempRows;

            var tempPosition = persistingArray.YPosition;
            persistingArray.YPosition = PersistingArrayXOrYPosition;
            PersistingArrayXOrYPosition = tempPosition;
        }

        private void toggleVertical(CLPArray persistingArray)
        {
            var tempDivisions = persistingArray.VerticalDivisions;
            persistingArray.VerticalDivisions = PersistingArrayDivisions;
            PersistingArrayDivisions = tempDivisions;

            var tempColumns = persistingArray.Columns;
            persistingArray.Columns = PersistingArrayRowsOrColumns;
            PersistingArrayRowsOrColumns = tempColumns;

            var tempPosition = persistingArray.XPosition;
            persistingArray.XPosition = PersistingArrayXOrYPosition;
            PersistingArrayXOrYPosition = tempPosition;
        }

        #endregion //Methods
    }
}
