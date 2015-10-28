using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPArrayGridToggleHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayGridToggleHistoryItem" /> from scratch.</summary>
        public CLPArrayGridToggleHistoryItem() { }

        /// <summary>Initializes <see cref="CLPArrayGridToggleHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public CLPArrayGridToggleHistoryItem(CLPPage parentPage, Person owner, string arrayID, bool isToggledOn)
            : base(parentPage, owner)
        {
            ArrayID = arrayID;
            IsToggledOn = isToggledOn;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArrayGridToggleHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>Unique Identifier for the <see cref="ACLPArrayBase" /> this <see cref="IHistoryItem" /> modifies.</summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof (string));

        /// <summary>Grid for Array was toggled to visible.</summary>
        public bool IsToggledOn
        {
            get { return GetValue<bool>(IsToggledOnProperty); }
            set { SetValue(IsToggledOnProperty, value); }
        }

        public static readonly PropertyData IsToggledOnProperty = RegisterProperty("IsToggledOn", typeof (bool), false);

        public override string FormattedValue
        {
            get
            {
                var array = ParentPage.GetPageObjectByIDOnPageOrInHistory(ArrayID) as ACLPArrayBase;
                return array == null
                           ? string.Format("[ERROR] on Index #{0}, Array for Grid Toggle not found on page or in history.", HistoryIndex)
                           : string.Format("Index #{0}, Toggled Grid of {1} {2}.", HistoryIndex, array.FormattedName, IsToggledOn ? "on" : "off");
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Grid Toggle not found on page or in history.", HistoryIndex);
                return;
            }

            IsToggledOn = array.IsGridOn;

            array.IsGridOn = !IsToggledOn;
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo) { ToggleGrid(true); }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo) { ToggleGrid(false); }

        private void ToggleGrid(bool isUndo)
        {
            var array = ParentPage.GetVerifiedPageObjectOnPageByID(ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Array for Grid Toggle not found on page or in history.", HistoryIndex);
                return;
            }

            array.IsGridOn = isUndo ? !IsToggledOn : IsToggledOn;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as CLPArrayGridToggleHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return ArrayID == id; }

        #endregion //Methods
    }
}