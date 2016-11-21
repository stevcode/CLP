using System;
using System.Runtime.Serialization;
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

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>Unique Identifier for the <see cref="ICountable" /> this <see cref="IHistoryAction" /> modifies.</summary>
        public string PageObjectID
        {
            get { return GetValue<string>(PageObjectIDProperty); }
            set { SetValue(PageObjectIDProperty, value); }
        }

        public static readonly PropertyData PageObjectIDProperty = RegisterProperty("PageObjectID", typeof (string));

        /// <summary>Previous value of the Parts Value.</summary>
        public int PreviousValue
        {
            get { return GetValue<int>(PreviousValueProperty); }
            set { SetValue(PreviousValueProperty, value); }
        }

        public static readonly PropertyData PreviousValueProperty = RegisterProperty("PreviousValue", typeof (int));

        /// <summary>New Value of the Parts Value.</summary>
        public int NewValue
        {
            get { return GetValue<int>(NewValueProperty); }
            set { SetValue(NewValueProperty, value); }
        }

        public static readonly PropertyData NewValueProperty = RegisterProperty("NewValue", typeof (int));

        public override string FormattedValue
        {
            get
            {
                var pageObject = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectID) as ICountable;
                return pageObject == null
                           ? string.Format("[ERROR] on Index #{0}, ICountable for Parts Value Changed not found on page or in history.", HistoryActionIndex)
                           : string.Format("Index #{0}, Changed value of {1} parts from {2} to {3}.", HistoryActionIndex, pageObject.FormattedName, PreviousValue, NewValue);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (pageObject == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, ICountable for Parts Value Changed not found on page or in history.", HistoryActionIndex);
                return;
            }

            NewValue = pageObject.Parts;
            pageObject.Parts = PreviousValue;
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

        private void TogglePartsValue(bool isUndo)
        {
            var pageObject = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (pageObject == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, ICountable for Parts Value Changed not found on page or in history.", HistoryActionIndex);
                return;
            }

            pageObject.Parts = isUndo ? PreviousValue : NewValue;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryItem = this.DeepCopy();
            if (clonedHistoryItem == null)
            {
                return null;
            }
            var iCountable = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (iCountable == null)
            {
                return clonedHistoryItem;
            }

            clonedHistoryItem.PreviousValue = iCountable.Parts;

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id) { return PageObjectID == id; }

        #endregion //Methods
    }
}