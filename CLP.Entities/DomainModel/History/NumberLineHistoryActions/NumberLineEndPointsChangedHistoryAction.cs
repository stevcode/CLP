using System;
using System.Diagnostics;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineEndPointsChangedHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryAction" /> from scratch.</summary>
        public NumberLineEndPointsChangedHistoryAction() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public NumberLineEndPointsChangedHistoryAction(CLPPage parentPage,
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
        }

        #endregion // Constructors

        #region Properties

        /// <summary>ID of the numberline whose values have changed.</summary>
        public string NumberLineID
        {
            get { return GetValue<string>(NumberLineIDProperty); }
            set { SetValue(NumberLineIDProperty, value); }
        }

        public static readonly PropertyData NumberLineIDProperty = RegisterProperty("NumberLineID", typeof(string), string.Empty);

        /// <summary>Previous start value of the number line.</summary>
        public int PreviousStartValue
        {
            get { return GetValue<int>(PreviousStartValueProperty); }
            set { SetValue(PreviousStartValueProperty, value); }
        }

        public static readonly PropertyData PreviousStartValueProperty = RegisterProperty("PreviousStartValue", typeof(int), 0);

        /// <summary>New start value of the number line.</summary>
        public int NewStartValue
        {
            get { return GetValue<int>(NewStartValueProperty); }
            set { SetValue(NewStartValueProperty, value); }
        }

        public static readonly PropertyData NewStartValueProperty = RegisterProperty("NewStartValue", typeof(int), 0);

        /// <summary>Previous end value of the number line.</summary>
        public int PreviousEndValue
        {
            get { return GetValue<int>(PreviousEndValueProperty); }
            set { SetValue(PreviousEndValueProperty, value); }
        }

        public static readonly PropertyData PreviousEndValueProperty = RegisterProperty("PreviousEndValue", typeof(int), 0);

        /// <summary>New end value of the number line.</summary>
        public int NewEndValue
        {
            get { return GetValue<int>(NewEndValueProperty); }
            set { SetValue(NewEndValueProperty, value); }
        }

        public static readonly PropertyData NewEndValueProperty = RegisterProperty("NewEndValue", typeof(int), 0);

        /// <summary>Width before a resize that involves stretching captured ink strokes.</summary>
        public double PreStretchedWidth
        {
            get { return GetValue<double>(PreStretchedWidthProperty); }
            set { SetValue(PreStretchedWidthProperty, value); }
        }

        public static readonly PropertyData PreStretchedWidthProperty = RegisterProperty("PreStretchedWidth", typeof(double), 0.0);

        /// <summary>Width after a resize that involves stretching captured ink strokes.</summary>
        public double NewStretchedWidth
        {
            get { return GetValue<double>(NewStretchedWidthProperty); }
            set { SetValue(NewStretchedWidthProperty, value); }
        }

        public static readonly PropertyData NewStretchedWidthProperty = RegisterProperty("NewStretchedWidth", typeof(double), 0.0);

        #endregion // Properties

        #region Methods

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

        #endregion // Methods

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var numberLine = ParentPage.GetPageObjectByIDOnPageOrInHistory(NumberLineID) as NumberLine;
                return numberLine == null
                           ? "[ERROR] Number Line not found on page or in history."
                           : $"Changed Number Line end point from {PreviousEndValue - PreviousStartValue} to {numberLine.NumberLineSize}.";
            }
        }

        /// <summary>Method that will actually undo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void UndoAction(bool isAnimationUndo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Number Line for Jump Size Changed not found on page or in history.");
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

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            if (clonedHistoryAction == null)
            {
                return null;
            }
            var numberLine = ParentPage.GetPageObjectByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return clonedHistoryAction;
            }

            clonedHistoryAction.PreviousEndValue = numberLine.NumberLineSize;

            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return NumberLineID == id;
        }

        #endregion // AHistoryActionBase Overrides
    }
}