using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineJumpSizesChangedHistoryAction : AHistoryActionBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryAction" /> from scratch.</summary>
        public NumberLineJumpSizesChangedHistoryAction() { }

        /// <summary>Initializes <see cref="CLPArrayDivisionValueChangedHistoryAction" /> with a parent <see cref="CLPPage" />.</summary>
        public NumberLineJumpSizesChangedHistoryAction(CLPPage parentPage,
                                                       Person owner,
                                                       string numberLineID,
                                                       List<Stroke> addedJumpStrokes,
                                                       List<Stroke> removedJumpStrokes,
                                                       List<NumberLineJumpSize> jumpsAdded,
                                                       List<NumberLineJumpSize> jumpsRemoved,
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
            JumpsAdded = jumpsAdded;
            JumpsRemoved = jumpsRemoved;

            AddedJumpStrokeIDs = addedJumpStrokes.Select(s => s.GetStrokeID()).ToList();
            foreach (var stroke in removedJumpStrokes)
            {
                RemovedJumpStrokeIDs.Add(stroke.GetStrokeID());
                if (!isConversionCreation)
                {
                    ParentPage.History.TrashedInkStrokes.Add(stroke);
                }
            }
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

        /// <summary>IDs of the Strokes used to create a Jump.</summary>
        public List<string> AddedJumpStrokeIDs
        {
            get { return GetValue<List<string>>(AddedJumpStrokeIDsProperty); }
            set { SetValue(AddedJumpStrokeIDsProperty, value); }
        }

        public static readonly PropertyData AddedJumpStrokeIDsProperty = RegisterProperty("AddedJumpStrokeIDs", typeof(List<string>), () => new List<string>());

        /// <summary>Jumps added by the strokes in AddedJumpStrokeIDs</summary>
        public List<NumberLineJumpSize> JumpsAdded
        {
            get { return GetValue<List<NumberLineJumpSize>>(JumpsAddedProperty); }
            set { SetValue(JumpsAddedProperty, value); }
        }

        public static readonly PropertyData JumpsAddedProperty = RegisterProperty("JumpsAdded", typeof(List<NumberLineJumpSize>), () => new List<NumberLineJumpSize>());

        /// <summary>IDs of the Strokes used in a removed Jump.</summary>
        public List<string> RemovedJumpStrokeIDs
        {
            get { return GetValue<List<string>>(RemovedJumpStrokeIDsProperty); }
            set { SetValue(RemovedJumpStrokeIDsProperty, value); }
        }

        public static readonly PropertyData RemovedJumpStrokeIDsProperty = RegisterProperty("RemovedJumpStrokeIDs", typeof(List<string>), () => new List<string>());

        /// <summary>Jumps removed by the strokes in RemovedJumpStrokeIDs</summary>
        public List<NumberLineJumpSize> JumpsRemoved
        {
            get { return GetValue<List<NumberLineJumpSize>>(JumpsRemovedProperty); }
            set { SetValue(JumpsRemovedProperty, value); }
        }

        public static readonly PropertyData JumpsRemovedProperty = RegisterProperty("JumpsRemoved", typeof(List<NumberLineJumpSize>), () => new List<NumberLineJumpSize>());

        /// <summary>Previous Height of the number line.</summary>
        public double PreviousHeight
        {
            get { return GetValue<double>(PreviousHeightProperty); }
            set { SetValue(PreviousHeightProperty, value); }
        }

        public static readonly PropertyData PreviousHeightProperty = RegisterProperty("PreviousHeight", typeof(double), 0.0);

        /// <summary>Previous YPosition of the number line.</summary>
        public double PreviousYPosition
        {
            get { return GetValue<double>(PreviousYPositionProperty); }
            set { SetValue(PreviousYPositionProperty, value); }
        }

        public static readonly PropertyData PreviousYPositionProperty = RegisterProperty("PreviousYPosition", typeof(double), 0.0);

        /// <summary>New Height of the number line.</summary>
        public double NewHeight
        {
            get { return GetValue<double>(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        public static readonly PropertyData NewHeightProperty = RegisterProperty("NewHeight", typeof(double), 0.0);

        /// <summary>New YPositiong of the number line.</summary>
        public double NewYPosition
        {
            get { return GetValue<double>(NewYPositionProperty); }
            set { SetValue(NewYPositionProperty, value); }
        }

        public static readonly PropertyData NewYPositionProperty = RegisterProperty("NewYPosition", typeof(double), 0.0);

        #endregion // Properties

        #region AHistoryActionBase Overrides

        public override int AnimationDelay => 600;

        protected override string FormattedReport
        {
            get
            {
                var numberLine = ParentPage.GetPageObjectByIDOnPageOrInHistory(NumberLineID) as NumberLine;
                if (numberLine == null)
                {
                    return "[ERROR] Number Line not found on page or in history.";
                }

                var removedString = !RemovedJumpStrokeIDs.Any() ? string.Empty : $"Removed {RemovedJumpStrokeIDs.Count} jump(s)";
                var addedString = !AddedJumpStrokeIDs.Any()
                                      ? string.Empty
                                      : !RemovedJumpStrokeIDs.Any() ? $"Added {AddedJumpStrokeIDs.Count} jump(s)" : $" and added {AddedJumpStrokeIDs.Count} jump(s)";

                return $"{removedString}{addedString} on Number Line [{numberLine.NumberLineSize}].";
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

            foreach (var stroke in AddedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history.");
                    continue;
                }
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>(),
                                                 new List<Stroke>
                                                 {
                                                     stroke
                                                 });
            }

            foreach (var jump in JumpsAdded)
            {
                var jumpToRemove = numberLine.JumpSizes.FirstOrDefault(j => j.StartingTickIndex == jump.StartingTickIndex && j.JumpSize == jump.JumpSize && j.JumpColor == jump.JumpColor);
                if (jumpToRemove == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Jump in JumpsAdded not found on Number Line during Undo.");
                    continue;
                }

                numberLine.JumpSizes.Remove(jumpToRemove);
            }

            foreach (var stroke in RemovedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history.");
                    continue;
                }
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>
                                                 {
                                                     stroke
                                                 },
                                                 new List<Stroke>());
            }

            foreach (var jump in JumpsRemoved)
            {
                numberLine.JumpSizes.Add(jump);
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
                CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Number Line for Jump Size Changed not found on page or in history.");
                return;
            }

            foreach (var stroke in RemovedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeOnPageByID(id)))
            {
                if (stroke == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history.");
                    continue;
                }
                ParentPage.InkStrokes.Remove(stroke);
                ParentPage.History.TrashedInkStrokes.Add(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>(),
                                                 new List<Stroke>
                                                 {
                                                     stroke
                                                 });
            }

            foreach (var jump in JumpsRemoved)
            {
                var jumpToRemove = numberLine.JumpSizes.FirstOrDefault(j => j.StartingTickIndex == jump.StartingTickIndex && j.JumpSize == jump.JumpSize && j.JumpColor == jump.JumpColor);
                if (jumpToRemove == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Jump in JumpsRemoved not found on Number Line during Redo.");
                    continue;
                }

                numberLine.JumpSizes.Remove(jumpToRemove);
            }

            foreach (var stroke in AddedJumpStrokeIDs.Select(id => ParentPage.GetVerifiedStrokeInHistoryByID(id)))
            {
                if (stroke == null)
                {
                    CLogger.AppendToLog($"[ERROR] on Index #{HistoryActionIndex}, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history.");
                    continue;
                }
                ParentPage.History.TrashedInkStrokes.Remove(stroke);
                ParentPage.InkStrokes.Add(stroke);
                numberLine.ChangeAcceptedStrokes(new List<Stroke>
                                                 {
                                                     stroke
                                                 },
                                                 new List<Stroke>());
            }

            foreach (var jump in JumpsAdded)
            {
                numberLine.JumpSizes.Add(jump);
            }

            numberLine.YPosition = NewYPosition;
            numberLine.Height = NewHeight;
        }

        /// <summary>Method that prepares a clone of the <see cref="IHistoryAction" /> so that it can call Redo() when sent to another machine.</summary>
        public override IHistoryAction CreatePackagedHistoryAction()
        {
            var clonedHistoryAction = this.DeepCopy();
            return clonedHistoryAction;
        }

        /// <summary>Method that unpacks the <see cref="IHistoryAction" /> after it has been sent to another machine.</summary>
        public override void UnpackHistoryAction() { }

        public override bool IsUsingTrashedPageObject(string id)
        {
            return NumberLineID == id;
        }

        public override bool IsUsingTrashedInkStroke(string id)
        {
            return AddedJumpStrokeIDs.Contains(id) || RemovedJumpStrokeIDs.Contains(id);
        }

        #endregion // AHistoryActionBase Overrides
    }
}