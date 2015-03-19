using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineJumpSizesChangedHistoryItem : AHistoryItemBase
    {
         #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> from scratch.
        /// </summary>
        public NumberLineJumpSizesChangedHistoryItem() { }

        /// <summary>
        /// Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> with a parent <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public NumberLineJumpSizesChangedHistoryItem(CLPPage parentPage,
                                                     Person owner,
                                                     string numberLineID,
                                                     List<NumberLineJumpSize> addedJumpSizes,
                                                     List<NumberLineJumpSize> removedJumpSizes)
            : base(parentPage, owner)
        {
            NumberLineID = numberLineID;
            AddedJumpSizes = addedJumpSizes;
            RemovedJumpSizes = removedJumpSizes;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected NumberLineJumpSizesChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>
        /// ID of the numberline whose values have changed.
        /// </summary>
        public string NumberLineID
        {
            get { return GetValue<string>(NumberLineIDProperty); }
            set { SetValue(NumberLineIDProperty, value); }
        }

        public static readonly PropertyData NumberLineIDProperty = RegisterProperty("NumberLineID", typeof(string));

        /// <summary>
        /// NumberLineJumpSizes added to the Number Line.
        /// </summary>
        public List<NumberLineJumpSize> AddedJumpSizes
        {
            get { return GetValue<List<NumberLineJumpSize>>(AddedJumpSizesProperty); }
            set { SetValue(AddedJumpSizesProperty, value); }
        }

        public static readonly PropertyData AddedJumpSizesProperty = RegisterProperty("AddedJumpSizes", typeof (List<NumberLineJumpSize>));

        /// <summary>
        /// NumberLineJumpSizes removed from the Number Line.
        /// </summary>
        public List<NumberLineJumpSize> RemovedJumpSizes
        {
            get { return GetValue<List<NumberLineJumpSize>>(RemovedJumpSizesProperty); }
            set { SetValue(RemovedJumpSizesProperty, value); }
        }

        public static readonly PropertyData RemovedJumpSizesProperty = RegisterProperty("RemovedJumpSizes", typeof (List<NumberLineJumpSize>));

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var numberLine = ParentPage.GetPageObjectByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            foreach (var jumpSize in AddedJumpSizes)
            {
                var jumpSizeToRemove = numberLine.JumpSizes.FirstOrDefault(j => j.StartingTickIndex == jumpSize.StartingTickIndex && j.JumpSize == jumpSize.JumpSize);
                numberLine.JumpSizes.Remove(jumpSizeToRemove);
                var tickL = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex);
                var tickR = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex + jumpSize.JumpSize);
                if (tickL == null ||
                    tickR == null)
                {
                    continue;
                }

                if (numberLine.JumpSizes.All(x => x.StartingTickIndex != tickR.TickValue))
                {
                    tickR.IsMarked = false;
                    tickR.TickColor = "Black";
                    tickR.IsNumberVisible = numberLine.NumberLineSize <= NumberLine.MAX_ALL_TICKS_VISIBLE_LENGTH || tickR.TickValue % 5 == 0;
                }

                if (numberLine.JumpSizes.All(x => x.StartingTickIndex + x.JumpSize != tickL.TickValue))
                {
                    tickL.IsMarked = false;
                    tickL.TickColor = "Black";
                    tickL.IsNumberVisible = numberLine.NumberLineSize <= NumberLine.MAX_ALL_TICKS_VISIBLE_LENGTH || tickL.TickValue % 5 == 0;
                }
            }

            foreach (var jumpSize in RemovedJumpSizes)
            {
                numberLine.JumpSizes.Add(jumpSize);
                var tickL = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex);
                var tickR = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex + jumpSize.JumpSize);
                if (tickL == null ||
                    tickR == null)
                {
                    continue;
                }

                tickL.TickColor = "Blue";
                tickL.IsMarked = true;
                tickL.IsNumberVisible = true;
                tickR.TickColor = "Blue";
                tickR.IsMarked = false;
                tickR.IsNumberVisible = true;
            }
        }

        /// <summary>
        /// Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.
        /// </summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var numberLine = ParentPage.GetPageObjectByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            foreach (var jumpSize in RemovedJumpSizes)
            {
                var jumpSizeToRemove = numberLine.JumpSizes.FirstOrDefault(j => j.StartingTickIndex == jumpSize.StartingTickIndex && j.JumpSize == jumpSize.JumpSize);
                numberLine.JumpSizes.Remove(jumpSizeToRemove);
                var tickL = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex);
                var tickR = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex + jumpSize.JumpSize);
                if (tickL == null ||
                    tickR == null)
                {
                    continue;
                }

                if (numberLine.JumpSizes.All(x => x.StartingTickIndex != tickR.TickValue))
                {
                    tickR.IsMarked = false;
                    tickR.TickColor = "Black";
                    tickR.IsNumberVisible = numberLine.NumberLineSize <= NumberLine.MAX_ALL_TICKS_VISIBLE_LENGTH || tickR.TickValue % 5 == 0;
                }

                if (numberLine.JumpSizes.All(x => x.StartingTickIndex + x.JumpSize != tickL.TickValue))
                {
                    tickL.IsMarked = false;
                    tickL.TickColor = "Black";
                    tickL.IsNumberVisible = numberLine.NumberLineSize <= NumberLine.MAX_ALL_TICKS_VISIBLE_LENGTH || tickL.TickValue % 5 == 0;
                }
            }

            foreach (var jumpSize in AddedJumpSizes)
            {
                numberLine.JumpSizes.Add(jumpSize);
                var tickL = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex);
                var tickR = numberLine.Ticks.FirstOrDefault(t => t.TickValue == jumpSize.StartingTickIndex + jumpSize.JumpSize);
                if (tickL == null ||
                    tickR == null)
                {
                    continue;
                }

                tickL.TickColor = "Blue";
                tickL.IsMarked = true;
                tickL.IsNumberVisible = true;
                tickR.TickColor = "Blue";
                tickR.IsMarked = false;
                tickR.IsNumberVisible = true;
            }
        }

        /// <summary>
        /// Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.
        /// </summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as NumberLineJumpSizesChangedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>
        /// Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.
        /// </summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}