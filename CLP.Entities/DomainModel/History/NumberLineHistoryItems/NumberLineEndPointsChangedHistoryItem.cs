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
        public NumberLineEndPointsChangedHistoryItem(CLPPage parentPage,
                                                     Person owner,
                                                     string numberLineID,
                                                     int previousStartValue,
                                                     int newStartValue,
                                                     int previousEndValue,
                                                     int newEndValue,
                                                     double preStretchedWidth,
                                                     double newStretchedWidth)
            : base(parentPage, owner)
        {
            NumberLineID = numberLineID;
            PreviousStartValue = previousStartValue;
            NewStartValue = newStartValue;
            PreviousEndValue = previousEndValue;
            NewEndValue = newEndValue;
            PreStretchedWidth = preStretchedWidth;
            NewStretchedWidth = newStretchedWidth;

            CachedFormattedValue = FormattedValue;
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

        /// <summary>New start value of the number line.</summary>
        public int NewStartValue
        {
            get { return GetValue<int>(NewStartValueProperty); }
            set { SetValue(NewStartValueProperty, value); }
        }

        public static readonly PropertyData NewStartValueProperty = RegisterProperty("NewStartValue", typeof (int));

        /// <summary>Previous end value of the number line.</summary>
        public int PreviousEndValue
        {
            get { return GetValue<int>(PreviousEndValueProperty); }
            set { SetValue(PreviousEndValueProperty, value); }
        }

        public static readonly PropertyData PreviousEndValueProperty = RegisterProperty("PreviousEndValue", typeof (int));

        /// <summary>New end value of the number line.</summary>
        public int NewEndValue
        {
            get { return GetValue<int>(NewEndValueProperty); }
            set { SetValue(NewEndValueProperty, value); }
        }

        public static readonly PropertyData NewEndValueProperty = RegisterProperty("NewEndValue", typeof (int));

        /// <summary>Width before a resize that involves stretching captured ink strokes.</summary>
        public double PreStretchedWidth
        {
            get { return GetValue<double>(PreStretchedWidthProperty); }
            set { SetValue(PreStretchedWidthProperty, value); }
        }

        public static readonly PropertyData PreStretchedWidthProperty = RegisterProperty("PreStretchedWidth", typeof (double));

        /// <summary>Width after a resize that involves stretching captured ink strokes.</summary>
        public double NewStretchedWidth
        {
            get { return GetValue<double>(NewStretchedWidthProperty); }
            set { SetValue(NewStretchedWidthProperty, value); }
        }

        public static readonly PropertyData NewStretchedWidthProperty = RegisterProperty("NewStretchedWidth", typeof (double));

        public override string FormattedValue
        {
            get
            {
                var numberLine = ParentPage.GetPageObjectByIDOnPageOrInHistory(NumberLineID) as NumberLine;
                return numberLine == null
                           ? string.Format("[ERROR] on Index #{0}, Number Line not found on page or in history.", HistoryIndex)
                           : string.Format("Index #{0}, Changed Number Line end point from {1} to {2}.",
                                           HistoryIndex,
                                           PreviousEndValue - PreviousStartValue,
                                           numberLine.NumberLineSize);
            }
        }

        #endregion //Properties

        #region Methods

        protected override void ConversionUndoAction()
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Number Line for Jump Size Changed not found on page or in history.", HistoryIndex);
                return;
            }

            NewStartValue = 0;

            NewStretchedWidth = numberLine.Width;
            StretchInk(numberLine, PreStretchedWidth);

            NewEndValue = numberLine.NumberLineSize;
            numberLine.ChangeNumberLineSize(PreviousEndValue);
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Number Line for Jump Size Changed not found on page or in history.", HistoryIndex);
                return;
            }

            StretchInk(numberLine, PreStretchedWidth);

            numberLine.ChangeNumberLineSize(PreviousEndValue);
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            numberLine.ChangeNumberLineSize(NewEndValue);

            StretchInk(numberLine, NewStretchedWidth);
        }

        private void StretchInk(NumberLine numberLine, double newWidth)
        {
            if (Math.Abs(NewStretchedWidth - PreStretchedWidth) < 0.0001)
            {
                return;
            }

            var oldWidth = numberLine.Width;
            var oldHeight = numberLine.Height;
            numberLine.Width = newWidth;
            numberLine.OnResized(oldWidth, oldHeight, true);
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

        public override bool IsUsingTrashedPageObject(string id) { return NumberLineID == id; }

        #endregion //Methods
    }
}