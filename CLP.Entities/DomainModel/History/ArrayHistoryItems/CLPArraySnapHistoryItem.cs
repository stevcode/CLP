using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArraySnapHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArraySnapHistoryItem" /> from scratch.
        /// </summary>
        public CLPArraySnapHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="CLPArraySnapHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArraySnapHistoryItem(CLPPage parentPage, Person owner, ACLPArrayBase persistingArray, ACLPArrayBase snappedArray, bool isHorizontal)
            : base(parentPage, owner)
        {
            IsHorizontal = isHorizontal;
            SnappedArrayID = snappedArray.ID;
            SnappedArraySquareSize = snappedArray.ArrayWidth / snappedArray.Columns;
            ParentPage.History.TrashedPageObjects.Add(snappedArray);

            PersistingArrayID = persistingArray.ID;
            PersistingArrayDivisionBehavior = persistingArray.IsDivisionBehaviorOn;
            PersistingArrayHorizontalDivisions = new List<CLPArrayDivision>(persistingArray.HorizontalDivisions);
            PersistingArrayVerticalDivisions = new List<CLPArrayDivision>(persistingArray.VerticalDivisions);
            PersistingArrayRowsOrColumns = isHorizontal ? persistingArray.Rows : persistingArray.Columns;
            PersistingArrayXOrYPosition = isHorizontal ? persistingArray.YPosition : persistingArray.XPosition;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArraySnapHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>
        /// True if the arrays snap together along a horizontal edge, false if along a vertical one
        /// </summary>
        public bool IsHorizontal
        {
            get { return GetValue<bool>(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }

        public static readonly PropertyData IsHorizontalProperty = RegisterProperty("IsHorizontal", typeof(bool));

        /// <summary>
        /// UniqueID of the array that is snapped onto and then deleted.
        /// </summary>
        public string SnappedArrayID
        {
            get { return GetValue<string>(SnappedArrayIDProperty); }
            set { SetValue(SnappedArrayIDProperty, value); }
        }

        public static readonly PropertyData SnappedArrayIDProperty = RegisterProperty("SnappedArrayID", typeof(string), string.Empty);

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
        public string PersistingArrayID
        {
            get { return GetValue<string>(PersistingArrayIDProperty); }
            set { SetValue(PersistingArrayIDProperty, value); }
        }

        public static readonly PropertyData PersistingArrayIDProperty = RegisterProperty("PersistingArrayID", typeof(string), string.Empty);

        /// <summary>
        /// Horizontal divisions that the persisting array should be set to have when this
        /// history event fires (undoes or redoes, whichever comes next).
        /// </summary>
        public List<CLPArrayDivision> PersistingArrayHorizontalDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(PersistingArrayHorizontalDivisionsProperty); }
            set { SetValue(PersistingArrayHorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayHorizontalDivisionsProperty = RegisterProperty("PersistingArrayHorizontalDivisions", typeof(List<CLPArrayDivision>));

        /// <summary>
        /// Vertical divisions that the persisting array should be set to have when this
        /// history event fires (undoes or redoes, whichever comes next).
        /// </summary>
        public List<CLPArrayDivision> PersistingArrayVerticalDivisions
        {
            get { return GetValue<List<CLPArrayDivision>>(PersistingArrayVerticalDivisionsProperty); }
            set { SetValue(PersistingArrayVerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData PersistingArrayVerticalDivisionsProperty = RegisterProperty("PersistingArrayVerticalDivisions", typeof(List<CLPArrayDivision>));

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
            var persistingArray = ParentPage.GetVerifiedPageObjectOnPageByID(PersistingArrayID) as CLPArray;
            if(persistingArray == null)
            {
                return;
            }

            var snappedArray = ParentPage.GetVerifiedPageObjectInTrashByID(SnappedArrayID) as CLPArray;
            if(snappedArray == null)
            {
                return;
            }

            snappedArray.SizeArrayToGridLevel(SnappedArraySquareSize);
            snappedArray.ParentPage = ParentPage;
            ParentPage.PageObjects.Add(snappedArray);
            ParentPage.History.TrashedPageObjects.Remove(snappedArray);

            var persistingArraySquareSize = persistingArray.ArrayWidth / persistingArray.Columns;

            RestoreDivisions(persistingArray);
            RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = PersistingArrayDivisionBehavior;
            persistingArray.SizeArrayToGridLevel(persistingArraySquareSize, false);
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var snappedArray = ParentPage.GetVerifiedPageObjectOnPageByID(SnappedArrayID) as CLPArray;
            var persistingArray = ParentPage.GetVerifiedPageObjectOnPageByID(PersistingArrayID) as CLPArray;
            if(snappedArray == null ||
               persistingArray == null)
            {
                return;
            }

            ParentPage.History.TrashedPageObjects.Add(snappedArray);
            ParentPage.PageObjects.Remove(snappedArray);
            var persistingArraySquareSize = persistingArray.ArrayWidth / persistingArray.Columns;

            RestoreDivisions(persistingArray);
            RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = true;
            persistingArray.SizeArrayToGridLevel(persistingArraySquareSize, false);
        }

        private void RestoreDivisions(CLPArray persistingArray)
        {
            var tempHorizontalDivisions = persistingArray.HorizontalDivisions;
            persistingArray.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(PersistingArrayHorizontalDivisions);
            PersistingArrayHorizontalDivisions = tempHorizontalDivisions.ToList();

            var tempVerticalDivisions = persistingArray.VerticalDivisions;
            persistingArray.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(PersistingArrayVerticalDivisions);
            PersistingArrayVerticalDivisions = tempVerticalDivisions.ToList();
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

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArraySnapHistoryItem;
            var persistingArray = ParentPage.GetVerifiedPageObjectOnPageByID(PersistingArrayID) as CLPArray;
            if(clonedHistoryItem == null ||
               persistingArray == null)
            {
                return null;
            }

            clonedHistoryItem.PersistingArrayHorizontalDivisions = persistingArray.HorizontalDivisions.ToList();
            clonedHistoryItem.PersistingArrayVerticalDivisions = persistingArray.VerticalDivisions.ToList();

            if(IsHorizontal)
            {
                clonedHistoryItem.PersistingArrayRowsOrColumns = persistingArray.Rows;
                clonedHistoryItem.PersistingArrayXOrYPosition = persistingArray.YPosition;
            }
            else
            {
                clonedHistoryItem.PersistingArrayRowsOrColumns = persistingArray.Columns;
                clonedHistoryItem.PersistingArrayXOrYPosition = persistingArray.XPosition;
            }

            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id, bool isUndoItem) { return isUndoItem && SnappedArrayID == id; }

        #endregion //Methods
    }
}