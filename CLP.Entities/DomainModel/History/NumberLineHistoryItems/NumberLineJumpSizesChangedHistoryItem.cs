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
                                                     double previousYPosition,
                                                     double newHeight,
                                                     double newYPosition,
                                                     bool isConversionCreation = false)
            : base(parentPage, owner)
        {
            NumberLineID = numberLineID;
            PreviousHeight = previousHeight;
            PreviousYPosition = previousYPosition;
            NewHeight = newHeight;
            NewYPosition = newYPosition;

            AddedJumpStrokeIDs = addedJumpStrokes.Select(s => s.GetStrokeID()).ToList();
            foreach (var stroke in removedJumpStrokes)
            {
                RemovedJumpStrokeIDs.Add(stroke.GetStrokeID());
                if (!isConversionCreation)
                {
                    ParentPage.History.TrashedInkStrokes.Add(stroke);
                }
            }

            CachedFormattedValue = FormattedValue;
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

        /// <summary>New Height of the number line.</summary>
        public double NewHeight
        {
            get { return GetValue<double>(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        public static readonly PropertyData NewHeightProperty = RegisterProperty("NewHeight", typeof (double));

        /// <summary>New YPositiong of the number line.</summary>
        public double NewYPosition
        {
            get { return GetValue<double>(NewYPositionProperty); }
            set { SetValue(NewYPositionProperty, value); }
        }

        public static readonly PropertyData NewYPositionProperty = RegisterProperty("NewYPosition", typeof (double));

        public override string FormattedValue
        {
            get
            {
                var numberLine = ParentPage.GetPageObjectByIDOnPageOrInHistory(NumberLineID) as NumberLine;
                if (numberLine == null)
                {
                    return string.Format("[ERROR] on Index #{0}, Number Line not found on page or in history.", HistoryIndex);
                }

                var removedString = !RemovedJumpStrokeIDs.Any() ? string.Empty : string.Format("Removed {0} jump(s)", RemovedJumpStrokeIDs.Count);
                var addedString = !AddedJumpStrokeIDs.Any()
                                      ? string.Empty
                                      : !RemovedJumpStrokeIDs.Any()
                                            ? string.Format("Added {0} jumps", AddedJumpStrokeIDs.Count)
                                            : string.Format(" and added {0} jumps", AddedJumpStrokeIDs.Count);

                var formattedValue = string.Format("Index #{0}, {1}{2} on Number Line [{3}].", HistoryIndex, removedString, addedString, numberLine.NumberLineSize);
                return formattedValue;
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

            foreach (var stroke in AddedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem not found on page or in history.", HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem not found on page or in history.",
                                      HistoryIndex);
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

            NewYPosition = numberLine.YPosition;
            NewHeight = numberLine.Height;
            numberLine.YPosition = PreviousYPosition;
            numberLine.Height = PreviousHeight;
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

            foreach (var stroke in AddedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem not found on page or in history.", HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem not found on page or in history.",
                                      HistoryIndex);
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

            numberLine.YPosition = PreviousYPosition;
            numberLine.Height = PreviousHeight;
        }

        /// <summary>Method that will actually redo the action. Already incorporates error checking for existance of ParentPage.</summary>
        protected override void RedoAction(bool isAnimationRedo)
        {
            var numberLine = ParentPage.GetVerifiedPageObjectOnPageByID(NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                Console.WriteLine("[ERROR] on Index #{0}, Number Line for Jump Size Changed not found on page or in history.", HistoryIndex);
                return;
            }

            foreach (var stroke in RemovedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    Console.WriteLine("[ERROR] on Index #{0}, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem not found on page or in history.",
                                      HistoryIndex);
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
                    Console.WriteLine("[ERROR] on Index #{0}, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryItem not found on page or in history.", HistoryIndex);
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

            numberLine.YPosition = NewYPosition;
            numberLine.Height = NewHeight;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryItem" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryItem CreatePackagedHistoryItem()
        {
            var clonedHistoryItem = Clone() as NumberLineJumpSizesChangedHistoryItem;
            return clonedHistoryItem;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryItem" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryItem() { }

        public override bool IsUsingTrashedPageObject(string id) { return NumberLineID == id; }

        public override bool IsUsingTrashedInkStroke(string id) { return AddedJumpStrokeIDs.Contains(id) || RemovedJumpStrokeIDs.Contains(id); }

        #endregion //Methods
    }
}