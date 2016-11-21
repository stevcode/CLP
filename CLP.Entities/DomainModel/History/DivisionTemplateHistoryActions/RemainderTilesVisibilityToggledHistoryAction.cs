using System;
using System.Runtime.Serialization;
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

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>ID of the Division Template that owns the Remainder Tiles.</summary>
        public string DivisionTemplateID
        {
            get { return GetValue<string>(DivisionTemplateIDProperty); }
            set { SetValue(DivisionTemplateIDProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIDProperty = RegisterProperty("DivisionTemplateID", typeof (string), string.Empty);

        /// <summary>Visibility state the Remainder Tiles were toggled into.</summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof (bool), false);

        public override string FormattedValue
        {
            get
            {
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(DivisionTemplateID) as DivisionTemplate;
                if (divisionTemplate == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Division Template for Remainder Tiles Toggle not found on page or in history.", HistoryActionIndex);
                }

                var toggleState = IsVisible ? "on" : "off";
                return string.Format("Index #{0}, Toggled Remainder Tiles {1} for {2}.",
                                     HistoryActionIndex,
                                     toggleState,
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
            ToggleRemainderTilesVisibility(!IsVisible);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            ToggleRemainderTilesVisibility(IsVisible);
        }

        private void ToggleRemainderTilesVisibility(bool isToggledVisible)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as DivisionTemplate;
            if (divisionTemplate == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Template for Remainder Tiles Toggle not found on page or in history.", HistoryActionIndex);
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

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryItem = this.DeepCopy();
            if (clonedHistoryItem == null)
            {
                return null;
            }

            // TODO: Package history item.

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction()
        {
            // TODO: Unpack history item.
        }

        public override bool IsUsingTrashedPageObject(string id) { return DivisionTemplateID == id; }

        #endregion //Methods
    }
}