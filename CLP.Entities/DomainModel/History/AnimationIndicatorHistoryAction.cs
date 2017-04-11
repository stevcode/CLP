using System;
using Catel.Data;

namespace CLP.Entities
{
    public enum AnimationIndicatorType
    {
        Record,
        Stop
    }

    [Serializable]
    public class AnimationIndicatorHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="AnimationIndicatorHistoryAction" /> from scratch.</summary>
        public AnimationIndicatorHistoryAction() { }

        /// <summary>Initializes <see cref="AnimationIndicatorHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryAction" /> is part of.</param>
        /// <param name="animationIndicatorType">The <see cref="AnimationIndicatorType" /> of animation indication this <see cref="AnimationIndicatorHistoryAction" /> represents.</param>
        public AnimationIndicatorHistoryAction(CLPPage parentPage, Person owner, AnimationIndicatorType animationIndicatorType)
            : base(parentPage, owner)
        {
            AnimationIndicatorType = animationIndicatorType;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Indicates when an animation recording has started or stopped.</summary>
        public AnimationIndicatorType AnimationIndicatorType
        {
            get { return GetValue<AnimationIndicatorType>(AnimationIndicatorTypeProperty); }
            set { SetValue(AnimationIndicatorTypeProperty, value); }
        }

        public static readonly PropertyData AnimationIndicatorTypeProperty = RegisterProperty("AnimationIndicatorType", typeof(AnimationIndicatorType), AnimationIndicatorType.Record);

        #endregion //Properties

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 0;

        protected override string FormattedReport => AnimationIndicatorType == AnimationIndicatorType.Record ? "Animation Start." : "Animation End.";

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo) { }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo) { }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that is can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            return clonedHistoryAction;
        }

        public override void UnpackHistoryAction() { }

        #endregion // AHistoryActionBase Overrides
    }
}