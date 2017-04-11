using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateArraySnappedInHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplateArraySnappedInHistoryAction" /> from scratch.</summary>
        public DivisionTemplateArraySnappedInHistoryAction() { }

        /// <summary>Initializes <see cref="DivisionTemplateArraySnappedInHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public DivisionTemplateArraySnappedInHistoryAction(CLPPage parentPage, Person owner, string divisionTemplateID, CLPArray snappedInArray)
            : base(parentPage, owner)
        {
            DivisionTemplateID = divisionTemplateID;
            SnappedInArrayID = snappedInArray.ID;
            parentPage.History.TrashedPageObjects.Add(snappedInArray);
        }

        #endregion // Constructor

        #region Properties

        /// <summary>UniqueID of the Division Template which had an array snapped inside</summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof(string), string.Empty);

        /// <summary>UniqueID of the array that wass snapped in and then deleted.</summary>
        public string SnappedInArrayID
        {
            get { return GetValue<string>(SnappedInArrayIDProperty); }
            set { SetValue(SnappedInArrayIDProperty, value); }
        }

        public static readonly PropertyData SnappedInArrayIDProperty = RegisterProperty("SnappedInArrayID", typeof(string), string.Empty);

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
                    return "[ERROR] Division Template for Array Snapped In not found on page or in history.";
                }

                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(SnappedInArrayID) as CLPArray;
                if (array == null)
                {
                    return "[ERROR] Array for Array Snapped In not found on page or in history.";
                }

                return $"Snapped {array.FormattedName} into {divisionTemplate.FormattedName}.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
            if (divisionTemplate == null)
            {
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Division Template for Array Snapped In not found on page or in history.");
                return;
            }

            var array = ParentPage.GetVerifiedPageObjectInTrashByID(SnappedInArrayID) as CLPArray;
            if (array == null)
            {
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Array for Array Snapped In not found on page or in history.");
                return;
            }

            divisionTemplate.RemoveLastDivision();
            ParentPage.History.TrashedPageObjects.Remove(array);
            ParentPage.PageObjects.Add(array);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
            if (divisionTemplate == null)
            {
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Division Template for Array Snapped In not found on page or in history.");
                return;
            }

            var array = ParentPage.GetVerifiedPageObjectOnPageByID(SnappedInArrayID) as CLPArray;
            if (array == null)
            {
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Array for Array Snapped In not found on page or in history.");
                return;
            }

            ParentPage.PageObjects.Remove(array);
            ParentPage.History.TrashedPageObjects.Add(array);
            divisionTemplate.SnapInArray(array.Columns);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return DivisionTemplateID == id || SnappedInArrayID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}