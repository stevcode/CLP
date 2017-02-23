using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RemainderTilesVisibilityToggledHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="RemainderTilesVisibilityToggledHistoryAction" /> from scratch.</summary>
        public RemainderTilesVisibilityToggledHistoryAction() { }

        /// <summary>Initializes <see cref="RemainderTilesVisibilityToggledHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryAction" />.</param>
        public RemainderTilesVisibilityToggledHistoryAction(CLPPage parentPage, Person owner, string divisionTemplateID, bool isVisibile)
            : base(parentPage, owner)
        {
            DivisionTemplateID = divisionTemplateID;
            IsVisible = isVisibile;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>ID of the Division Template that owns the Remainder Tiles.</summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof(string), string.Empty);

        /// <summary>Visibility state the Remainder Tiles were toggled into.</summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof(bool), false);

        #endregion // Properties

        #region Methods

        private void ToggleRemainderTilesVisibility(bool isToggledVisible)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
            if (divisionTemplate == null)
            {
                Debug.WriteLine("[ERROR] on Index #{0}, Division Template for Remainder Tiles Toggle not found on page or in history.", HistoryActionIndex);
                return;
            }

            divisionTemplate.IsRemainderTilesVisible = isToggledVisible;
            if (isToggledVisible && !ParentPage.PageObjects.Contains(divisionTemplate.RemainderTiles))
            {
                ParentPage.PageObjects.Add(divisionTemplate.RemainderTiles);
            }
            else if (ParentPage.PageObjects.Contains(divisionTemplate.RemainderTiles))
            {
                ParentPage.PageObjects.Remove(divisionTemplate.RemainderTiles);
            }

            divisionTemplate.UpdateReport();
        }

        #endregion // Methods

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(DivisionTemplateID) as DivisionTemplate;
                if (divisionTemplate == null)
                {
                    return "[ERROR] Division Template for Remainder Tiles Toggle not found on page or in history.";
                }

                var toggleState = IsVisible ? "on" : "off";
                return $"Toggled Remainder Tiles {toggleState} for {divisionTemplate.FormattedName}.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            ToggleRemainderTilesVisibility(!IsVisible);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            ToggleRemainderTilesVisibility(IsVisible);
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }

            // TODO: Package history action.

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction()
        {
            // TODO: Unpack history action.
        }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return DivisionTemplateID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}