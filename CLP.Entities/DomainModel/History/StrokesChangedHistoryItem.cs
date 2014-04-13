using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class StrokesChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="StrokesChangedHistoryItem" /> from scratch.
        /// </summary>
        public StrokesChangedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="StrokesChangedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        public StrokesChangedHistoryItem(CLPPage parentPage)
            : base(parentPage) {  }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected StrokesChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 100; }
        }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo) { }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo) { }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that is can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem UndoRedoCompleteClone()
        {
            // TODO
            var clonedHistoryItem = Clone() as StrokesChangedHistoryItem;
            return clonedHistoryItem;
            // TODO
        }

        #endregion //Methods
    }
}