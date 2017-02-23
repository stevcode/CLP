using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateArrayRemovedHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateArrayRemovedHistoryAction" /> from scratch.</summary>
        public DivisionTemplateArrayRemovedHistoryAction() { }

        /// <summary>Initializes <see cref="DivisionTemplateArrayRemovedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public DivisionTemplateArrayRemovedHistoryAction(CLPPage parentPage, Person owner, string divisionTemplateID, int divisionValue)
            : base(parentPage, owner)
        {
            DivisionTemplateID = divisionTemplateID;
            DivisionValue = divisionValue;
        }

        #endregion // Constructor

        #region Properties

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

        public static readonly PropertyData DivisionValueProperty = RegisterProperty("DivisionValue", typeof (int), 0);

        #endregion // Properties

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(DivisionTemplateID) as DivisionTemplate;
                if (divisionTemplate == null)
                {
                    return "[ERROR] Division Template for Array Removed not found on page or in history.";
                }

                return $"Removed division of value {DivisionValue} from {divisionTemplate.FormattedName}.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
            if (divisionTemplate == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Division Template for Array Removed not found on page or in history.", HistoryActionIndex);
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
                Debug.WriteLine("[ERROR] on Index #{0}, Division Template for Array Removed not found on page or in history.", HistoryActionIndex);
                return;
            }

            divisionTemplate.RemoveLastDivision();
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id) { return DivisionTemplateID == id; }

        #endregion // AHistoryActionBase Overrides
    }
}