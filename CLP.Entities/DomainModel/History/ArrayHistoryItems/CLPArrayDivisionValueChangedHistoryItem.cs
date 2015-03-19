using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayDivisionValueChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> from scratch.
        /// </summary>
        public CLPArrayDivisionValueChangedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArrayDivisionValueChangedHistoryItem(CLPPage parentPage, Person owner, string arrayID, bool isHorizontalDivision, int divisionIndex, int previousValue)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            IsHorizontalDivision = isHorizontalDivision;
            DivisionIndex = divisionIndex;
            PreviousValue = previousValue;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArrayDivisionValueChangedHistoryItem(SerializationInfo info, StreamingContext context)
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
        /// Whether to operate on a horizontal or vertical division.
        /// </summary>
        public bool IsHorizontalDivision
        {
            get { return GetValue<bool>(IsHorizontalDivisionProperty); }
            set { SetValue(IsHorizontalDivisionProperty, value); }
        }

        public static readonly PropertyData IsHorizontalDivisionProperty = RegisterProperty("IsHorizontalDivision", typeof(bool), true);

        /// <summary>
        /// Index of the Division who's value has changed.
        /// </summary>
        public int DivisionIndex
        {
            get { return GetValue<int>(DivisionIndexProperty); }
            set { SetValue(DivisionIndexProperty, value); }
        }

        public static readonly PropertyData DivisionIndexProperty = RegisterProperty("DivisionIndex", typeof(int), 0);

        /// <summary>
        /// Previous value of the Division.
        /// </summary>
        public int PreviousValue
        {
            get { return GetValue<int>(PreviousValueProperty); }
            set { SetValue(PreviousValueProperty, value); }
        }

        public static readonly PropertyData PreviousValueProperty = RegisterProperty("PreviousValue", typeof(int), 0);

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo) { ToggleDivisionValue(); }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo) { ToggleDivisionValue(); }

        private void ToggleDivisionValue()
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as CLPArray;
            if(array == null)
            {
                return;
            }

            try
            {
                var division = IsHorizontalDivision ? array.HorizontalDivisions[DivisionIndex] : array.VerticalDivisions[DivisionIndex];

                var tempParts = division.Value;
                division.Value = PreviousValue;
                PreviousValue = tempParts;
            }
            catch(Exception e) { }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArrayDivisionValueChangedHistoryItem;
            if(clonedHistoryItem == null)
            {
                return null;
            }
            var array = ParentPage.GetPageObjectByID(ArrayID) as CLPArray;
            if(array == null)
            {
                return clonedHistoryItem;
            }

            var division = IsHorizontalDivision ? array.HorizontalDivisions[DivisionIndex] : array.VerticalDivisions[DivisionIndex];

            clonedHistoryItem.PreviousValue = division.Value;

            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}