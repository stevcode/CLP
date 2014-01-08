using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryArrayDivisionValueChanged : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryArrayDivisionValueChanged(ICLPPage parentPage, string arrayUniqueID, bool isHorizontalDivision, int divisionIndex, int previousValue)
            : base(parentPage)
        {
            ArrayUniqueID = arrayUniqueID;
            IsHorizontalDivision = isHorizontalDivision;
            DivisionIndex = divisionIndex;
            PreviousValue = previousValue;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistoryArrayDivisionValueChanged(SerializationInfo info, StreamingContext context)
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
        /// UniqueID of the Array whose divisions have been modified
        /// </summary>
        public string ArrayUniqueID
        {
            get { return GetValue<string>(ArrayUniqueIDProperty); }
            set { SetValue(ArrayUniqueIDProperty, value); }
        }

        public static readonly PropertyData ArrayUniqueIDProperty = RegisterProperty("ArrayUniqueID", typeof(string), string.Empty);

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
            var array = ParentPage.GetPageObjectByUniqueID(ArrayUniqueID) as CLPArray;
            if(array == null)
            {
                Logger.Instance.WriteToLog("Array not found on page for UndoAction");
                return;
            }

            try
            {
                var division = IsHorizontalDivision ? array.HorizontalDivisions[DivisionIndex] : array.VerticalDivisions[DivisionIndex];

                var tempParts = division.Value;
                division.Value = PreviousValue;
                PreviousValue = tempParts;
            }
            catch(Exception e) 
            {
                Logger.Instance.WriteToLog("Division not found for array in Undo or Redo Action");
            }
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryArrayDivisionValueChanged;
            var array = ParentPage.GetPageObjectByUniqueID(ArrayUniqueID) as CLPArray;
            if(clonedHistoryItem == null || array == null)
            {
                Logger.Instance.WriteToLog("CLPHistoryPartsChanged UndoRedoCompleteClone Failure: Clone failed or pageObject not found on page.");
                return null;
            }

            try
            {
                var division = IsHorizontalDivision ? array.HorizontalDivisions[DivisionIndex] : array.VerticalDivisions[DivisionIndex];

                clonedHistoryItem.PreviousValue = division.Value;
                return clonedHistoryItem;
            }
            catch(Exception e) 
            {
                Logger.Instance.WriteToLog("Division not found for array in UndoRedoCompleteClone");
                return null;
            }
        }

        #endregion //Methods
    }
}
