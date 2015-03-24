using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineEndPointsChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> from scratch.</summary>
        public NumberLineEndPointsChangedHistoryItem() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public NumberLineEndPointsChangedHistoryItem(CLPPage parentPage, Person owner, string numberLineID, int previousStartValue, int previousEndValue, double previousWidth, double preStretchedWidth)
            : base(parentPage, owner)
        {
            NumberLineID = numberLineID;
            PreviousStartValue = previousStartValue;
            PreviousEndValue = previousEndValue;
            PreviousWidth = previousWidth;
            PreStretchedWidth = preStretchedWidth;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected NumberLineEndPointsChangedHistoryItem(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int AnimationDelay
        {
            get { return 600; }
        }

        /// <summary>ID of the numberline whose values have changed.</summary>
        public string NumberLineID
        {
            get { return GetValue<string>(NumberLineIDProperty); }
            set { SetValue(NumberLineIDProperty, value); }
        }

        public static readonly PropertyData NumberLineIDProperty = RegisterProperty("NumberLineID", typeof (string));

        /// <summary>Previous start value of the number line.</summary>
        public int PreviousStartValue
        {
            get { return GetValue<int>(PreviousStartValueProperty); }
            set { SetValue(PreviousStartValueProperty, value); }
        }

        public static readonly PropertyData PreviousStartValueProperty = RegisterProperty("PreviousStartValue", typeof (int));

        /// <summary>Previous end value of the number line.</summary>
        public int PreviousEndValue
        {
            get { return GetValue<int>(PreviousEndValueProperty); }
            set { SetValue(PreviousEndValueProperty, value); }
        }

        public static readonly PropertyData PreviousEndValueProperty = RegisterProperty("PreviousEndValue", typeof (int));

        /// <summary>
        /// Previous Width of the Number Line in the event a resize was necessary after the EndValue changes.
        /// </summary>
        public double PreviousWidth
        {
            get { return GetValue<double>(PreviousWidthProperty); }
            set { SetValue(PreviousWidthProperty, value); }
        }

        public static readonly PropertyData PreviousWidthProperty = RegisterProperty("PreviousWidth", typeof (double));

        /// <summary>
        /// Width before a resize that involves stretching captured ink strokes.
        /// </summary>
        public double PreStretchedWidth
        {
            get { return GetValue<double>(PreStretchedWidthProperty); }
            set { SetValue(PreStretchedWidthProperty, value); }
        }

        public static readonly PropertyData PreStretchedWidthProperty = RegisterProperty("PreStretchedWidth", typeof (double));

        #endregion //Properties

        #region Methods

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            if (Math.Abs(numberLine.Width - PreStretchedWidth) > 0.0001)
            {
                var oldWidth = numberLine.Width;
                var oldHeight = numberLine.Height;
                numberLine.Width = PreStretchedWidth;
                PreStretchedWidth = oldWidth;
                numberLine.OnResized(oldWidth, oldHeight, true);
            }

            ToggleEndPointValues(numberLine);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            ToggleEndPointValues(numberLine);

            if (Math.Abs(numberLine.Width - PreStretchedWidth) > 0.0001)
            {
                var oldWidth = numberLine.Width;
                var oldHeight = numberLine.Height;
                numberLine.Width = PreStretchedWidth;
                PreStretchedWidth = oldWidth;
                numberLine.OnResized(oldWidth, oldHeight, true);
            }
        }

        private void ToggleEndPointValues(NumberLine numberLine)
        {
            var tempPreviousEnd = numberLine.NumberLineSize;
            numberLine.ChangeNumberLineSize(PreviousEndValue);
            PreviousEndValue = tempPreviousEnd;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as NumberLineEndPointsChangedHistoryItem;
            if (clonedHistoryItem == null)
            {
                return null;
            }
            var numberLine = ParentPage.GetPageObjectByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return clonedHistoryItem;
            }

            clonedHistoryItem.PreviousEndValue = numberLine.NumberLineSize;

            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}