using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    public enum AnimationIndicatorType
    {
        Record,
        Stop
    }

    [Serializable]
    public class CLPAnimationIndicator : ACLPHistoryItemBase
    {
        #region Constructors

        public CLPAnimationIndicator(ICLPPage parentPage, AnimationIndicatorType animationIndicatorType)
            : base(parentPage)
        {
            AnimationIndicatorType = animationIndicatorType;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPAnimationIndicator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// HistoryItem used to indicate when an animation recording has started or stopped.
        /// </summary>
        public AnimationIndicatorType AnimationIndicatorType
        {
            get { return GetValue<AnimationIndicatorType>(AnimationIndicatorTypeProperty); }
            set { SetValue(AnimationIndicatorTypeProperty, value); }
        }

        public static readonly PropertyData AnimationIndicatorTypeProperty = RegisterProperty("AnimationIndicatorType", typeof(AnimationIndicatorType));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo) {}

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo) {}

        public override ICLPHistoryItem UndoRedoCompleteClone()
        {
            var clonedHistoryItem = Clone() as CLPAnimationIndicator;
            return clonedHistoryItem;
        }

        #endregion //Methods
    }
}
