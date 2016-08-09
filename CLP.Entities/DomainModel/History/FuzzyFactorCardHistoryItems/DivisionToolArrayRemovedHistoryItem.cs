using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionToolArrayRemovedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolArrayRemovedHistoryItem" /> from scratch.</summary>
        public DivisionToolArrayRemovedHistoryItem() { }

        /// <summary>Initializes <see cref="DivisionToolArrayRemovedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public DivisionToolArrayRemovedHistoryItem(CLPPage parentPage, Person owner, string divisionToolID, int divisionValue)
            : base(parentPage, owner)
        {
            DivisionToolID = divisionToolID;
            DivisionValue = divisionValue;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected DivisionToolArrayRemovedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>UniqueID of the Division Tool which had an array snapped inside</summary>
        public string DivisionToolID
        {
            get { return GetValue<string>(DivisionToolIDProperty); }
            set { SetValue(DivisionToolIDProperty, value); }
        }

        public static readonly PropertyData DivisionToolIDProperty = RegisterProperty("DivisionToolID", typeof (string), string.Empty);

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
                var divisionTool = ParentPage.GetPageObjectByIDOnPageOrInHistory(DivisionToolID) as DivisionTool;
                if (divisionTool == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Division Tool for Array Removed not found on page or in history.", HistoryIndex);
                }

                return string.Format("Index #{0}, Removed division of value {1} from {2}.",
                                     HistoryIndex,
                                     DivisionValue,
                                     divisionTool.FormattedName);
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
            var divisionTool = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionToolID) as DivisionTool;
            if (divisionTool == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Tool for Array Removed not found on page or in history.", HistoryIndex);
                return;
            }

            divisionTool.SnapInArray(DivisionValue);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var divisionTool = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionToolID) as DivisionTool;
            if (divisionTool == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Tool for Array Removed not found on page or in history.", HistoryIndex);
                return;
            }

            divisionTool.RemoveLastDivision();
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = this.DeepCopy();
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return DivisionToolID == id; }

        #endregion //Methods
    }
}