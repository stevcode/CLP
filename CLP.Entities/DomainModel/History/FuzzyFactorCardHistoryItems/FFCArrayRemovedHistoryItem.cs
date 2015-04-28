using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class FFCArrayRemovedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="FFCArrayRemovedHistoryItem" /> from scratch.</summary>
        public FFCArrayRemovedHistoryItem() { }

        /// <summary>Initializes <see cref="FFCArrayRemovedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public FFCArrayRemovedHistoryItem(CLPPage parentPage, Person owner, string divisionTemplateID, int divisionValue)
            : base(parentPage, owner)
        {
            FuzzyFactorCardID = divisionTemplateID;
            DivisionValue = divisionValue;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected FFCArrayRemovedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>UniqueID of the FFC which had an array snapped inside</summary>
        public string FuzzyFactorCardID
        {
            get { return GetValue<string>(FuzzyFactorCardIDProperty); }
            set { SetValue(FuzzyFactorCardIDProperty, value); }
        }

        public static readonly PropertyData FuzzyFactorCardIDProperty = RegisterProperty("FuzzyFactorCardID", typeof (string), string.Empty);

        /// <summary>Value of the label on the removed division.</summary>
        public int DivisionValue
        {
            get { return GetValue<int>(DivisionValueProperty); }
            set { SetValue(DivisionValueProperty, value); }
        }

        public static readonly PropertyData DivisionValueProperty = RegisterProperty("DivisionValue", typeof (int));

        public override string FormattedValue
        {
            get
            {
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(FuzzyFactorCardID) as FuzzyFactorCard;
                if (divisionTemplate == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Division Template for Array Removed not found on page or in history.", HistoryIndex);
                }

                return string.Format("Index #{0}, Removed division of value {1} from Division Template [{2}/{3}]",
                                     HistoryIndex,
                                     DivisionValue,
                                     divisionTemplate.Dividend,
                                     divisionTemplate.Rows);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            UndoAction(false);
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(FuzzyFactorCardID) as FuzzyFactorCard;
            if (divisionTemplate == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Template for Array Removed not found on page or in history.", HistoryIndex);
                return;
            }
            divisionTemplate.SnapInArray(DivisionValue);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(FuzzyFactorCardID) as FuzzyFactorCard;
            if (divisionTemplate == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Template for Array Removed not found on page or in history.", HistoryIndex);
                return;
            }
            divisionTemplate.RemoveLastDivision();
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as FFCArrayRemovedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}