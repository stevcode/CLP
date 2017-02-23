using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayDivisionValueChangedHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryAction" /> from scratch.</summary>
        public CLPArrayDivisionValueChangedHistoryAction() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public CLPArrayDivisionValueChangedHistoryAction(CLPPage parentPage, Person owner, string arrayID, bool isHorizontalDivision, int divisionIndex, int previousValue, int newValue)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            IsHorizontalDivision = isHorizontalDivision;
            DivisionIndex = divisionIndex;
            PreviousValue = previousValue;
            NewValue = newValue;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryAction" /> modifies.</summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof(string), string.Empty);

        /// <summary>Whether to operate on a horizontal or vertical division.</summary>
        public bool IsHorizontalDivision
        {
            get { return GetValue<bool>(IsHorizontalDivisionProperty); }
            set { SetValue(IsHorizontalDivisionProperty, value); }
        }

        public static readonly PropertyData IsHorizontalDivisionProperty = RegisterProperty("IsHorizontalDivision", typeof(bool), false);

        /// <summary>Index of the Division who's value has changed.</summary>
        public int DivisionIndex
        {
            get { return GetValue<int>(DivisionIndexProperty); }
            set { SetValue(DivisionIndexProperty, value); }
        }

        public static readonly PropertyData DivisionIndexProperty = RegisterProperty("DivisionIndex", typeof(int), 0);

        /// <summary>Previous value of the Division.</summary>
        public int PreviousValue
        {
            get { return GetValue<int>(PreviousValueProperty); }
            set { SetValue(PreviousValueProperty, value); }
        }

        public static readonly PropertyData PreviousValueProperty = RegisterProperty("PreviousValue", typeof(int), 0);

        /// <summary>New value of the Division.</summary>
        public int NewValue
        {
            get { return GetValue<int>(NewValueProperty); }
            set { SetValue(NewValueProperty, value); }
        }

        public static readonly PropertyData NewValueProperty = RegisterProperty("NewValue", typeof(int), 0);

        #endregion // Properties

        #region Methods

        private void ToggleDivisionValue(bool isUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array for Division Value Changed not found on page or in history.", HistoryActionIndex);
                return;
            }

            try
            {
                var division = IsHorizontalDivision ? array.HorizontalDivisions[DivisionIndex] : array.VerticalDivisions[DivisionIndex];

                division.Value = isUndo ? PreviousValue : NewValue;
            }
            catch (Exception)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Array for Division Value Changed DivisionIndex out of bounds.", HistoryActionIndex);
            }
        }

        #endregion // Methods

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                return array == null ? "[ERROR] Array for Division Value Changed not found on page or in history." : $"Changed {array.FormattedName} division value from {PreviousValue} to {NewValue}";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            ToggleDivisionValue(true);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            ToggleDivisionValue(false);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }
            var array = ParentPage.GetPageObjectByID(ArrayID) as CLPArray;
            if (array == null)
            {
                return clonedHistoryAction;
            }

            var division = IsHorizontalDivision ? array.HorizontalDivisions[DivisionIndex] : array.VerticalDivisions[DivisionIndex];

            clonedHistoryAction.PreviousValue = division.Value;

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return ArrayID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}