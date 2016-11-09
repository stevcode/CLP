using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateArrayRemovedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateArrayRemovedHistoryItem" /> from scratch.</summary>
        public DivisionTemplateArrayRemovedHistoryItem() { }

        /// <summary>Initializes <see cref="DivisionTemplateArrayRemovedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public DivisionTemplateArrayRemovedHistoryItem(CLPPage parentPage, Person owner, string divisionTemplateID, int divisionValue)
            : base(parentPage, owner)
        {
            DivisionTemplateID = divisionTemplateID;
            DivisionValue = divisionValue;
        }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>UniqueID of the Division Template which had an array snapped inside</summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof (string), string.Empty);

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
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(DivisionTemplateID) as DivisionTemplate;
                if (divisionTemplate == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Division Template for Array Removed not found on page or in history.", HistoryIndex);
                }

                return string.Format("Index #{0}, Removed division of value {1} from {2}.",
                                     HistoryIndex,
                                     DivisionValue,
                                     divisionTemplate.FormattedName);
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
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
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
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
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
            var clonedHistoryItem = this.DeepCopy();
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return DivisionTemplateID == id; }

        #endregion //Methods
    }
}