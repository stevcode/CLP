﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RemainderTilesVisibilityToggledHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="RemainderTilesVisibilityToggledHistoryItem" /> from scratch.</summary>
        public RemainderTilesVisibilityToggledHistoryItem() { }

        /// <summary>Initializes <see cref="RemainderTilesVisibilityToggledHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public RemainderTilesVisibilityToggledHistoryItem(CLPPage parentPage, Person owner, string divisionTemplateID, bool isVisibile)
            : base(parentPage, owner)
        {
            DivisionTemplateID = divisionTemplateID;
            IsVisible = isVisibile;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected RemainderTilesVisibilityToggledHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

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
                var divisionTemplate = ParentPage.GetPageObjectByIDOnPageOrInHistory(DivisionTemplateID) as FuzzyFactorCard;
                if (divisionTemplate == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Division Template not found on page or in history.", HistoryIndex);
                }

                var toggleState = IsVisible ? "on" : "off";
                return string.Format("Index #{0}, Toggled Remainder Tiles {1} for Division Template [{2}/{3}]",
                                     HistoryIndex,
                                     toggleState,
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
            ToggleRemainderTilesVisibility(!IsVisible);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            ToggleRemainderTilesVisibility(IsVisible);
        }

        private void ToggleRemainderTilesVisibility(bool isToggledVisible)
        {
            var divisionTemplate = ParentPage.GetVerifiedPageObjectOnPageByID(DivisionTemplateID) as FuzzyFactorCard;
            if (divisionTemplate == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Division Template for Remainder Tiles Toggle not found on page or in history.", HistoryIndex);
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

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as RemainderTilesVisibilityToggledHistoryItem;
            if (clonedHistoryItem == null)
            {
                return null;
            }

            // TODO: Package history item.

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem()
        {
            // TODO: Unpack history item.
        }

        #endregion //Methods
    }
}