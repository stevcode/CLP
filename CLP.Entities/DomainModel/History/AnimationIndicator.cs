using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum AnimationIndicatorType
    {
        Record,
        Stop
    }

    [Serializable]
    public class AnimationIndicator : AHistoryItemBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="AnimationIndicator" /> from scratch.
        /// </summary>
        public AnimationIndicator() { }

        /// <summary>
        /// Initializes <see cref="AnimationIndicator" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="animationIndicatorType">The <see cref="AnimationIndicatorType" /> of animation indication this <see cref="AnimationIndicator" /> represents.</param>
        public AnimationIndicator(CLPPage parentPage, Person owner, AnimationIndicatorType animationIndicatorType)
            : base(parentPage, owner) { AnimationIndicatorType = animationIndicatorType; }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected AnimationIndicator(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 0; }
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
        protected override void UndoAction(bool isAnimationUndo) { }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo) { }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that is can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as AnimationIndicator;
            return clonedHistoryItem;
        }

        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}