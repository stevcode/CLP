using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class PartsValueChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="PartsValueChangedHistoryItem" /> from scratch.
        /// </summary>
        public PartsValueChangedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="PartsValueChangedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public PartsValueChangedHistoryItem(CLPPage parentPage, Person owner, string pageObjectID, int previousValue)
            : base(parentPage, owner)
        {
            PageObjectID = pageObjectID;
            PreviousValue = previousValue;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected PartsValueChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>
        /// Unique Identifier for the <see cref="ICountable" /> this <see cref="IHistoryItem" /> modifies.
        /// </summary>
        public string PageObjectID
        {
            get { return GetValue<string>(PageObjectIDProperty); }
            set { SetValue(PageObjectIDProperty, value); }
        }

        public static readonly PropertyData PageObjectIDProperty = RegisterProperty("PageObjectID", typeof (string));

        /// <summary>
        /// Previous value of the Part Value.
        /// </summary>
        public int PreviousValue
        {
            get { return GetValue<int>(PreviousValueProperty); }
            set { SetValue(PreviousValueProperty, value); }
        }

        public static readonly PropertyData PreviousValueProperty = RegisterProperty("PreviousValue", typeof(int), 0);

        public override string FormattedValue
        {
            get
            {
                var pageObject = ParentPage.GetPageObjectByIDOnPageOrInHistory(PageObjectID) as ICountable;
                var objectParts = pageObject.Parts;
                var formattedValue = string.Format("Index # {0}, Changed value of parts from {1} to {2}.",
                    HistoryIndex, PreviousValue, objectParts);
                return formattedValue;
            }
        }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo) { TogglePartsValue(); }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo) { TogglePartsValue(); }

        private void TogglePartsValue()
        {
            var iCountable = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (iCountable == null)
            {
                return;
            }

            var tempParts = iCountable.Parts;
            iCountable.Parts = PreviousValue;
            PreviousValue = tempParts;
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
            var iCountable = ParentPage.GetVerifiedPageObjectOnPageByID(PageObjectID) as ICountable;
            if (iCountable == null)
            {
                return clonedHistoryItem;
            }


            clonedHistoryItem.PreviousValue = iCountable.Parts;

            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}