using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineJumpSizesChangedHistoryItem : AHistoryItemBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> from scratch.</summary>
        public NumberLineJumpSizesChangedHistoryItem() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryItem" /> with a parent <see cref="CLPPage" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> is part of.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public NumberLineJumpSizesChangedHistoryItem(CLPPage parentPage,
                                                     Person owner,
                                                     string numberLineID,
                                                     List<Stroke> addedJumpStrokes,
                                                     List<Stroke> removedJumpStrokes,
                                                     double previousHeight,
                                                     double previousYPosition)
            : base(parentPage, owner)
        {
            NumberLineID = numberLineID;
            PreviousHeight = previousHeight;
            PreviousYPosition = previousYPosition;

            AddedJumpStrokeIDs = addedJumpStrokes.Select(s => s.GetStrokeID()).ToList();
            foreach (var stroke in removedJumpStrokes)
            {
                RemovedJumpStrokeIDs.Add(stroke.GetStrokeID());
                ParentPage.History.TrashedInkStrokes.Add(stroke);
            }
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
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

        /// <summary>ID of the numberline whose values have changed.</summary>
        public string NumberLineID
        {
            get { return GetValue<string>(NumberLineIDProperty); }
            set { SetValue(NumberLineIDProperty, value); }
        }

        public static readonly PropertyData NumberLineIDProperty = RegisterProperty("NumberLineID", typeof (string));

        /// <summary>IDs of the Strokes used to create a Jump.</summary>
        public List<string> AddedJumpStrokeIDs
        {
            get { return GetValue<List<string>>(AddedJumpStrokeIDsProperty); }
            set { SetValue(AddedJumpStrokeIDsProperty, value); }
        }

        public static readonly PropertyData AddedJumpStrokeIDsProperty = RegisterProperty("AddedJumpStrokeIDs", typeof (List<string>));

        /// <summary>IDs of the Strokes used in a removed Jump.</summary>
        public List<string> RemovedJumpStrokeIDs
        {
            get { return GetValue<List<string>>(RemovedJumpStrokeIDsProperty); }
            set { SetValue(RemovedJumpStrokeIDsProperty, value); }
        }

        public static readonly PropertyData RemovedJumpStrokeIDsProperty = RegisterProperty("RemovedJumpStrokeIDs", typeof (List<string>), () => new List<string>());

        /// <summary>Previous Height of the number line.</summary>
        public double PreviousHeight
        {
            get { return GetValue<double>(PreviousHeightProperty); }
            set { SetValue(PreviousHeightProperty, value); }
        }

        public static readonly PropertyData PreviousHeightProperty = RegisterProperty("PreviousHeight", typeof (double));

        /// <summary>Previous YPosition of the number line.</summary>
        public double PreviousYPosition
        {
            get { return GetValue<double>(PreviousYPositionProperty); }
            set { SetValue(PreviousYPositionProperty, value); }
        }

        public static readonly PropertyData PreviousYPositionProperty = RegisterProperty("PreviousYPosition", typeof (double));

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

            foreach (var stroke in AddedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
                numberLine.RemoveJumpFromStroke(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>(),
                                                 new List<Stroke>
                                                 {
                                                     stroke
                                                 });
            }

            foreach (var stroke in RemovedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
                numberLine.AddJumpFromStroke(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>
                                                 {
                                                     stroke
                                                 },
                                                 new List<Stroke>());
            }

            ResizeNumberLineHeight();
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                return;
            }

            foreach (var stroke in RemovedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
                numberLine.RemoveJumpFromStroke(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>(),
                                                 new List<Stroke>
                                                 {
                                                     stroke
                                                 });
            }

            foreach (var stroke in AddedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("ERROR: Null stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem on History Index {0}.", HistoryIndex);
                    continue;
                }
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
                numberLine.AddJumpFromStroke(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>
                                                 {
                                                     stroke
                                                 },
                                                 new List<Stroke>());
            }

            ResizeNumberLineHeight();
        }

        private void ResizeNumberLineHeight()
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null ||
                Math.Abs(numberLine.Height - PreviousHeight) < 0.0001)
            {
                return;
            }

            var oldWidth = numberLine.Width;
            var oldHeight = numberLine.Height;
            numberLine.Height = PreviousHeight;
            PreviousHeight = oldHeight;

            var oldXPosition = numberLine.XPosition;
            var oldYPosition = numberLine.YPosition;
            numberLine.YPosition = PreviousYPosition;
            PreviousYPosition = oldYPosition;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as NumberLineJumpSizesChangedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        #endregion //Methods
    }
}