using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class PartsValueChangedHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="PartsValueChangedHistoryAction" /> from scratch.</summary>
        public PartsValueChangedHistoryAction() { }

        /// <summary>Initializes <see cref="PartsValueChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public PartsValueChangedHistoryAction(CLPPage parentPage, Person owner, string pageObjectID, int previousValue, int newValue)
            : base(parentPage, owner)
        {
            PageObjectID = pageObjectID;
            PreviousValue = previousValue;
            NewValue = newValue;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ICountable" /> this <see cref="IHistoryAction" /> modifies.</summary>
        public string PageObjectID
        {
            get { return GetValue<string>(PageObjectIDProperty); }
            set { SetValue(PageObjectIDProperty, value); }
        }

        public static readonly PropertyData PageObjectIDProperty = RegisterProperty("PageObjectID", typeof(string), string.Empty);

        /// <summary>Previous value of the Parts Value.</summary>
        public int PreviousValue
        {
            get { return GetValue<int>(PreviousValueProperty); }
            set { SetValue(PreviousValueProperty, value); }
        }

        public static readonly PropertyData PreviousValueProperty = RegisterProperty("PreviousValue", typeof(int), 0);

        /// <summary>New Value of the Parts Value.</summary>
        public int NewValue
        {
            get { return GetValue<int>(NewValueProperty); }
            set { SetValue(NewValueProperty, value); }
        }

        public static readonly PropertyData NewValueProperty = RegisterProperty("NewValue", typeof(int), 0);

        #endregion // Properties

        #region Methods

        private void TogglePartsValue(bool isUndo)
        {
            var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (pageObject == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, ICountable for Parts Value Changed not found on page or in history.", HistoryActionIndex);
                return;
            }

            pageObject.Parts = isUndo ? PreviousValue : NewValue;
        }

        #endregion // Methods

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var pageObject = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectID) as ICountable;
                return pageObject == null
                           ? "[ERROR] ICountable for Parts Value Changed not found on page or in history."
                           : $"Changed value of {pageObject.FormattedName} parts from {PreviousValue} to {NewValue}.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            TogglePartsValue(true);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            TogglePartsValue(false);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }
            var iCountable = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (iCountable == null)
            {
                return clonedHistoryAction;
            }

            clonedHistoryAction.PreviousValue = iCountable.Parts;

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return PageObjectID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}