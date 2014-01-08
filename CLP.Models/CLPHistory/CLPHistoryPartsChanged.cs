using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistoryPartsChanged : ACLPHistoryItemBase
    {
        #region Constructor

        public CLPHistoryPartsChanged(ICLPPage parentPage, string pageObjectID, int previousParts) 
            : base(parentPage)
        {
            PageObjectID = pageObjectID;
            Parts = previousParts;
        }

        /// <summary>
        ///     Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPHistoryPartsChanged(SerializationInfo info, StreamingContext context) : base(info, context) {}

        #endregion //Constructor

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 40;
            }
        }

        /// <summary>
        /// UniqueID of the pageObject.
        /// </summary>
        public string PageObjectID
        {
            get { return GetValue<string>(PageObjectIDProperty); }
            set { SetValue(PageObjectIDProperty, value); }
        }

        public static readonly PropertyData PageObjectIDProperty = RegisterProperty("PageObjectID", typeof(string), string.Empty);

        /// <summary>
        /// Previous Parts value of the PageObject.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), 0);

        #endregion //Properties

        #region Methods

        /// <summary>
        ///     Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            ToggleParts();
        }

        /// <summary>
        ///     Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            ToggleParts();
        }

        private void ToggleParts()
        {
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectID);
            if(pageObject == null)
            {
                Logger.Instance.WriteToLog("CLPHistoryPartsChanged Redo Failure: Can't find pageObject.");
                return;
            }

            var tempParts = pageObject.Parts;
            pageObject.Parts = Parts;
            Parts = tempParts;
        }

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPHistoryPartsChanged;
            var pageObject = ParentPage.GetPageObjectByUniqueID(PageObjectID);
            if(clonedHistoryItem == null || pageObject == null)
            {
                Logger.Instance.WriteToLog("CLPHistoryPartsChanged UndoRedoCompleteClone Failure: Clone failed or pageObject not found on page.");
                return null;
            }

            clonedHistoryItem.Parts = pageObject.Parts;

            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
