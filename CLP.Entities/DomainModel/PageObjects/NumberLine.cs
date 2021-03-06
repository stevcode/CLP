﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Entities
{
    public enum NumberLineTypes
    {
        NumberLine,
        AutoArcs
    }

    [Serializable]
    public class NumberLineTick : ASerializableBase
    {
        #region Constructors

        public NumberLineTick() { }

        public NumberLineTick(int value, bool isNumberVisible)
        {
            TickValue = value;
            IsNumberVisible = isNumberVisible;
        }

        #endregion //Constructors

        #region Properties

        public double TickHeight => 25.0;

        /// <summary>Number of the Tick</summary>
        public int TickValue
        {
            get { return GetValue<int>(TickValueProperty); }
            set { SetValue(TickValueProperty, value); }
        }

        public static readonly PropertyData TickValueProperty = RegisterProperty("TickValue", typeof(int), 0);

        /// <summary>Is the tick marks number visible</summary>
        public bool IsNumberVisible
        {
            get { return GetValue<bool>(IsNumberVisibleProperty); }
            set { SetValue(IsNumberVisibleProperty, value); }
        }

        public static readonly PropertyData IsNumberVisibleProperty = RegisterProperty("IsNumberVisible", typeof(bool), false);

        /// <summary>Is the tick visible on the number line</summary>
        public bool IsTickVisible
        {
            get { return GetValue<bool>(IsTickVisibleProperty); }
            set { SetValue(IsTickVisibleProperty, value); }
        }

        public static readonly PropertyData IsTickVisibleProperty = RegisterProperty("IsTickVisible", typeof(bool), true);

        /// <summary>Has the user marked the tick mark</summary>
        public bool IsMarked
        {
            get { return GetValue<bool>(IsMarkedProperty); }
            set { SetValue(IsMarkedProperty, value); }
        }

        public static readonly PropertyData IsMarkedProperty = RegisterProperty("IsMarked", typeof(bool), false);

        /// <summary>Color of the Tick Value</summary>
        public string TickColor
        {
            get { return GetValue<string>(TickColorProperty); }
            set { SetValue(TickColorProperty, value); }
        }

        public static readonly PropertyData TickColorProperty = RegisterProperty("TickColor", typeof(string), "Black");

        #endregion //Properties
    }

    [Serializable]
    public class NumberLineJumpSize : ASerializableBase
    {
        #region Constructors

        public NumberLineJumpSize() { }

        public NumberLineJumpSize(int jumpSizeValue, int startTickIndex, string jumpColor)
        {
            JumpSize = jumpSizeValue;
            StartingTickIndex = startTickIndex;
            JumpColor = jumpColor;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Jump size of arrow</summary>
        public int JumpSize
        {
            get { return GetValue<int>(JumpSizeProperty); }
            set { SetValue(JumpSizeProperty, value); }
        }

        public static readonly PropertyData JumpSizeProperty = RegisterProperty("JumpSize", typeof(int), 0);

        /// <summary>Tick where jump begins</summary>
        public int StartingTickIndex
        {
            get { return GetValue<int>(StartingTickIndexProperty); }
            set { SetValue(StartingTickIndexProperty, value); }
        }

        public static readonly PropertyData StartingTickIndexProperty = RegisterProperty("StartingTickIndex", typeof(int), 0);

        /// <summary>Color of the Autogenerated Arc.</summary>
        public string JumpColor
        {
            get { return GetValue<string>(JumpColorProperty); }
            set { SetValue(JumpColorProperty, value); }
        }

        public static readonly PropertyData JumpColorProperty = RegisterProperty("JumpColor", typeof(string), "Black");

        #endregion //Properties
    }

    [Serializable]
    public class NumberLine : AStrokeAccepter
    {
        public const int MAX_ALL_TICKS_VISIBLE_LENGTH = 30;
        public const int NUMBER_LINE_MAX_SIZE = 84;

        #region Constructors

        public NumberLine() { }

        public NumberLine(CLPPage parentPage, int numberLength, NumberLineTypes numberLineType)
            : base(parentPage)
        {
            NumberLineSize = numberLength;

            switch (numberLineType)
            {
                case NumberLineTypes.NumberLine:
                    Height = NumberLineHeight;
                    break;
                case NumberLineTypes.AutoArcs:
                    Height = NumberLineHeight + 50;
                    break;
                default:
                    Height = NumberLineHeight;
                    break;
            }

            Width = parentPage.Width - 100;
            XPosition = (parentPage.Width / 2.0) - (Width / 2.0);
            NumberLineType = numberLineType;
            IsAutoArcsVisible = NumberLineType == NumberLineTypes.AutoArcs;
            InitializeTicks();
        }

        #endregion //Constructors

        #region Properties

        /// <summary>The type of number line.</summary>
        public NumberLineTypes NumberLineType
        {
            get { return GetValue<NumberLineTypes>(NumberLineTypeProperty); }
            set { SetValue(NumberLineTypeProperty, value); }
        }

        public static readonly PropertyData NumberLineTypeProperty = RegisterProperty("NumberLineType", typeof(NumberLineTypes), NumberLineTypes.NumberLine);

        public double NumberLineHeight
        {
            get { return 75.0; }
        }

        public double ArrowLength
        {
            get { return 40.0; }
        }

        public double NumberLineLength
        {
            get { return Width - ArrowLength * 2; }
        }

        public double TickLength
        {
            get { return NumberLineLength / NumberLineSize; }
        }

        /// <summary>Length of number line</summary>
        public int NumberLineSize
        {
            get { return GetValue<int>(NumberLineSizeProperty); }
            set { SetValue(NumberLineSizeProperty, value); }
        }

        public static readonly PropertyData NumberLineSizeProperty = RegisterProperty("NumberLineSize", typeof(int), 0);

        /// <summary>Whether or not to show the Jump Size Labels.</summary>
        public bool IsJumpSizeLabelsVisible
        {
            get { return GetValue<bool>(IsJumpSizeLabelsVisibleProperty); }
            set { SetValue(IsJumpSizeLabelsVisibleProperty, value); }
        }

        public static readonly PropertyData IsJumpSizeLabelsVisibleProperty = RegisterProperty("IsJumpSizeLabelsVisible", typeof(bool), true);

        /// <summary>Toggles visibility of auto-generated arcs.</summary>
        public bool IsAutoArcsVisible
        {
            get { return GetValue<bool>(IsAutoArcsVisibleProperty); }
            set { SetValue(IsAutoArcsVisibleProperty, value); }
        }

        public static readonly PropertyData IsAutoArcsVisibleProperty = RegisterProperty("IsAutoArcsVisible", typeof(bool), false);

        /// <summary>List of the values of the jumps</summary>
        public ObservableCollection<NumberLineJumpSize> JumpSizes
        {
            get { return GetValue<ObservableCollection<NumberLineJumpSize>>(JumpSizesProperty); }
            set { SetValue(JumpSizesProperty, value); }
        }

        public static readonly PropertyData JumpSizesProperty = RegisterProperty("JumpSizes", typeof(ObservableCollection<NumberLineJumpSize>), () => new ObservableCollection<NumberLineJumpSize>());

        /// <summary>A collection of the ticks of the number line</summary>
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks", typeof(ObservableCollection<NumberLineTick>), () => new ObservableCollection<NumberLineTick>());

        #endregion //Properties

        #region Methods

        public void InitializeTicks()
        {
            for (var i = 0; i <= NumberLineSize; i++)
            {
                Ticks.Add(new NumberLineTick(i, true));
            }
            RefreshTickLabels();
        }

        public void ChangeNumberLineSize(int newNumberLineEndPoint)
        {
            var oldNumberLineEndPoint = NumberLineSize;
            var isBigger = newNumberLineEndPoint > oldNumberLineEndPoint;
            var numberLineSizeDifference = isBigger ? newNumberLineEndPoint - oldNumberLineEndPoint : oldNumberLineEndPoint - newNumberLineEndPoint;
            var tickLength = TickLength;

            NumberLineSize = newNumberLineEndPoint;

            foreach (var tickNumber in Enumerable.Range(0, numberLineSizeDifference))
            {
                if (isBigger)
                {
                    Ticks.Add(new NumberLineTick(Ticks.Count, true));
                    Width += tickLength;
                }
                else if (Ticks.Any())
                {
                    Ticks.Remove(Ticks.LastOrDefault());
                    Width -= tickLength;
                }
            }

            RefreshTickLabels();
        }

        public void RefreshTickLabels()
        {
            var defaultInteger = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH ? 1 : 5;
            for (var i = 0; i < Ticks.Count; i++)
            {
                var isLabelVisible = i == 0 || i == NumberLineSize || i % defaultInteger == 0 || JumpSizes.Any(j => j.StartingTickIndex == i || j.StartingTickIndex + j.JumpSize == i);

                Ticks[i].IsNumberVisible = isLabelVisible;
            }
        }

        public List<NumberLineJumpSize> RemoveJumpFromStroke(Stroke stroke)
        {
            var removedJumps = new List<NumberLineJumpSize>();
            switch (NumberLineType)
            {
                case NumberLineTypes.NumberLine:
                {
                    var tickR = FindClosestTickToArcStroke(stroke, true);
                    var tickL = FindClosestTickToArcStroke(stroke, false);
                    if (tickR == null ||
                        tickL == null ||
                        tickR == tickL)
                    {
                        return removedJumps;
                    }

                    var deletedStartTickValue = tickL.TickValue;
                    var deletedJumpSize = tickR.TickValue - tickL.TickValue;

                    var jumpToRemove = JumpSizes.FirstOrDefault(jump => jump.JumpSize == deletedJumpSize && jump.StartingTickIndex == deletedStartTickValue);
                    if (jumpToRemove == null)
                    {
                        return removedJumps;
                    }

                    removedJumps.Add(jumpToRemove);
                    JumpSizes.Remove(jumpToRemove);

                    if (JumpSizes.All(x => x.StartingTickIndex != tickR.TickValue))
                    {
                        tickR.IsMarked = false;
                        tickR.TickColor = "Black";
                        tickR.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || tickR.TickValue % 5 == 0 || tickR.TickValue == NumberLineSize;
                    }

                    if (JumpSizes.All(x => x.StartingTickIndex + x.JumpSize != tickL.TickValue))
                    {
                        tickL.IsMarked = false;
                        tickL.TickColor = "Black";
                        tickL.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || tickL.TickValue % 5 == 0;
                    }

                    break;
                }
                case NumberLineTypes.AutoArcs:
                {
                    var closestTick = FindClosestTickToTickStroke(stroke);
                    if (closestTick == null)
                    {
                        return removedJumps;
                    }

                    var jumpsToRemove = JumpSizes.Where(j => j.StartingTickIndex == closestTick.TickValue || j.StartingTickIndex + j.JumpSize == closestTick.TickValue).ToList();
                    foreach (var jump in jumpsToRemove)
                    {
                        removedJumps.Add(jump);
                        JumpSizes.Remove(jump);
                        var leftTick = Ticks.FirstOrDefault(t => t.TickValue == jump.StartingTickIndex);
                        var rightTick = Ticks.FirstOrDefault(t => t.TickValue == jump.StartingTickIndex + jump.JumpSize);

                        if (rightTick != null &&
                            JumpSizes.All(x => x.StartingTickIndex != rightTick.TickValue))
                        {
                            rightTick.IsMarked = false;
                            rightTick.TickColor = "Black";
                            rightTick.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || rightTick.TickValue % 5 == 0 || rightTick.TickValue == NumberLineSize;
                        }

                        if (leftTick != null &&
                            JumpSizes.All(x => x.StartingTickIndex + x.JumpSize != leftTick.TickValue))
                        {
                            leftTick.IsMarked = false;
                            leftTick.TickColor = "Black";
                            leftTick.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || leftTick.TickValue % 5 == 0 || leftTick.TickValue == NumberLineSize;
                        }
                    }

                    if (closestTick.IsMarked)
                    {
                        closestTick.IsMarked = false;
                        closestTick.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || closestTick.TickValue % 5 == 0 || closestTick.TickValue == NumberLineSize;
                        closestTick.TickColor = "Black";
                    }

                    break;
                }
            }

            return removedJumps;
        }

        public List<NumberLineJumpSize> AddJumpFromStroke(Stroke stroke)
        {
            var addedJumps = new List<NumberLineJumpSize>();
            switch (NumberLineType)
            {
                case NumberLineTypes.NumberLine:
                {
                    var tickR = FindClosestTickToArcStroke(stroke, true);
                    var tickL = FindClosestTickToArcStroke(stroke, false);
                    if (tickR == null ||
                        tickL == null ||
                        tickR == tickL)
                    {
                        return addedJumps;
                    }

                    tickR.IsMarked = true;
                    tickR.IsNumberVisible = true;
                    tickL.IsMarked = true;
                    tickL.IsNumberVisible = true;
                    if (stroke.DrawingAttributes.Color == Colors.Black)
                    {
                        tickR.TickColor = "Blue";
                        tickL.TickColor = "Blue";
                    }
                    else
                    {
                        tickR.TickColor = stroke.DrawingAttributes.Color.ToString();
                        tickL.TickColor = stroke.DrawingAttributes.Color.ToString();
                    }

                    var jumpSize = tickR.TickValue - tickL.TickValue;
                    var jump = new NumberLineJumpSize(jumpSize, tickL.TickValue, stroke.DrawingAttributes.Color.ToString());
                    JumpSizes.Add(jump);
                    addedJumps.Add(jump);
                    break;
                }
                case NumberLineTypes.AutoArcs:
                {
                    var closestTick = FindClosestTickToTickStroke(stroke);
                    if (closestTick == null)
                    {
                        return addedJumps;
                    }

                    if (!closestTick.IsMarked)
                    {
                        closestTick.IsMarked = true;
                        closestTick.IsNumberVisible = true;
                        closestTick.TickColor = stroke.DrawingAttributes.Color == Colors.Black ? "Blue" : stroke.DrawingAttributes.Color.ToString();
                    }

                    var markedTickToTheLeft = Ticks.LastOrDefault(t => t.IsMarked && t.TickValue < closestTick.TickValue);
                    if (markedTickToTheLeft != null)
                    {
                        var leftJumpSize = closestTick.TickValue - markedTickToTheLeft.TickValue;
                        var jump = new NumberLineJumpSize(leftJumpSize, markedTickToTheLeft.TickValue, stroke.DrawingAttributes.Color.ToString());
                        JumpSizes.Add(jump);
                        addedJumps.Add(jump);
                    }

                    var markedTickToTheRight = Ticks.FirstOrDefault(t => t.IsMarked && t.TickValue > closestTick.TickValue);
                    if (markedTickToTheRight != null)
                    {
                        var rightJumpSize = markedTickToTheRight.TickValue - closestTick.TickValue;
                        var jump = new NumberLineJumpSize(rightJumpSize, closestTick.TickValue, stroke.DrawingAttributes.Color.ToString());
                        JumpSizes.Add(jump);
                        addedJumps.Add(jump);
                    }
                    break;
                }
            }

            return addedJumps;
        }

        public int GetJumpStartFromStroke(Stroke stroke)
        {
            var tickR = FindClosestTickToArcStroke(stroke, true);
            var tickL = FindClosestTickToArcStroke(stroke, false);
            if (tickR == null ||
                tickL == null ||
                tickR == tickL)
            {
                return -1;
            }

            return tickL.TickValue;
        }

        public int GetJumpEndFromStroke(Stroke stroke)
        {
            var tickR = FindClosestTickToArcStroke(stroke, true);
            var tickL = FindClosestTickToArcStroke(stroke, false);
            if (tickR == null ||
                tickL == null ||
                tickR == tickL)
            {
                return -1;
            }

            return tickR.TickValue;
        }

        public NumberLineTick FindClosestTickToArcStroke(Stroke stroke, bool isLookingForRightTick)
        {
            return FindClosestTickToArcStroke(new StrokeCollection
                                              {
                                                  stroke
                                              },
                                              isLookingForRightTick);
        }

        private NumberLineTick FindClosestTickToArcStroke(StrokeCollection strokes, bool isLookingForRightTick)
        {
            // Get lowest Point
            var x1 = ParentPage.Width;
            var x2 = 0.0;
            var y1 = ParentPage.Height;
            var y2 = 0.0;

            foreach (var stroke in strokes)
            {
                x1 = Math.Min(x1, stroke.GetBounds().Left);
                x2 = Math.Max(x2, stroke.GetBounds().Right);
                y1 = Math.Min(y1, stroke.GetBounds().Top);
                y2 = Math.Max(y2, stroke.GetBounds().Bottom);
            }

            var midXOffSet = isLookingForRightTick ? TickLength * 0.05 : TickLength * -0.05;
            var midX = (x2 - x1) / 2.0 + x1 + midXOffSet;

            var pointClosestToNumberLine = new StylusPoint(0.0, 0.0);
            var numberLineYPosition = YPosition + Height - NumberLineHeight / 2;
            var pointCountAboveNumberLine = 0;
            var pointCountBelowNumberLine = 0;
            foreach (var point in from stroke in strokes
                                  from point in stroke.StylusPoints
                                  select point)
            {
                if (point.Y <= numberLineYPosition + 5)
                {
                    pointCountAboveNumberLine++;
                }
                else
                {
                    pointCountBelowNumberLine++;
                }

                if (isLookingForRightTick ? point.X > midX : point.X < midX)
                {
                    var closestPointYDifference = Math.Abs(pointClosestToNumberLine.Y - numberLineYPosition);
                    var pointYDifference = Math.Abs(point.Y - numberLineYPosition);
                    if (pointYDifference < closestPointYDifference)
                    {
                        pointClosestToNumberLine = point;
                    }
                }
            }

            if (pointClosestToNumberLine.Y > YPosition + Height ||
                pointClosestToNumberLine.Y < YPosition + Height - NumberLineHeight ||
                pointCountBelowNumberLine > pointCountAboveNumberLine)
            {
                return null;
            }

            //Find closest Tick
            var normalXLowest = (pointClosestToNumberLine.X - XPosition - ArrowLength) / TickLength;
            var tickIndex = (int)Math.Round(normalXLowest);

            if (tickIndex < 0 ||
                tickIndex >= Ticks.Count)
            {
                return null;
            }

            return Ticks[tickIndex];
        }

        public NumberLineTick FindClosestTickToTickStroke(Stroke stroke)
        {
            return FindClosestTickToTickStroke(new StrokeCollection
                                               {
                                                   stroke
                                               });
        }

        private NumberLineTick FindClosestTickToTickStroke(StrokeCollection strokes)
        {
            // Get Mid Point.X of Strokes
            var leftStrokeBounds = ParentPage.Width;
            var rightStrokeBounds = 0.0;
            var topStrokeBounds = ParentPage.Height;
            var bottomStrokeBounds = 0.0;

            foreach (var stroke in strokes)
            {
                leftStrokeBounds = Math.Min(leftStrokeBounds, stroke.GetBounds().Left);
                rightStrokeBounds = Math.Max(rightStrokeBounds, stroke.GetBounds().Right);
                topStrokeBounds = Math.Min(topStrokeBounds, stroke.GetBounds().Top);
                bottomStrokeBounds = Math.Max(bottomStrokeBounds, stroke.GetBounds().Bottom);
            }

            var midX = (rightStrokeBounds - leftStrokeBounds) / 2.0 + leftStrokeBounds;

            // Find closest Tick Index
            var normalXLowest = (midX - XPosition - ArrowLength) / TickLength;
            var tickIndex = (int)Math.Round(normalXLowest);

            if (tickIndex < 0 ||
                tickIndex >= Ticks.Count)
            {
                return null;
            }

            // Return null if vertical stroke doesn't intersect the mid-line of the number line.
            var numberLineYPosition = YPosition + Height - NumberLineHeight / 2;
            if (numberLineYPosition > bottomStrokeBounds ||
                numberLineYPosition < topStrokeBounds)
            {
                return null;
            }

            return Ticks[tickIndex];
        }

        public double FindTallestPoint(IEnumerable<Stroke> strokes)
        {
            return strokes.Select(stroke => stroke.GetBounds().Top).Concat(new[] { ParentPage.Height }).Min();
        }

        public int GetNumberLineSizeAtHistoryIndex(int historyIndex)
        {
            var sizeHistoryAction =
                ParentPage.History.CompleteOrderedHistoryActions.OfType<NumberLineEndPointsChangedHistoryAction>().FirstOrDefault(h => h.NumberLineID == ID && h.HistoryActionIndex >= historyIndex);
            return sizeHistoryAction == null ? NumberLineSize : sizeHistoryAction.PreviousEndValue;
        }

        #endregion //Methods

        #region APageObjectBase Overrides

        public override string FormattedName => $"Number Line from 0 to {NumberLineSize} with {JumpSizes.Count} jumps";

        public override string CodedName => Codings.OBJECT_NUMBER_LINE;

        public override string CodedID => NumberLineSize.ToString();

        public override int ZIndex => 60;

        public override bool IsBackgroundInteractable => true;

        /// <summary>Minimum Height of the <see cref="IPageObject" />.</summary>
        public override double MinimumHeight => NumberLineHeight;

        /// <summary>Minimum Width of the <see cref="IPageObject" />.</summary>
        public override double MinimumWidth => 10;

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width")
            {
                RaisePropertyChanged(nameof(NumberLineLength));
                RaisePropertyChanged(nameof(TickLength));
            }
            base.OnPropertyChanged(e);
        }

        public override void OnAdded(bool fromHistory = false)
        {
            base.OnAdded(fromHistory);

            if (!fromHistory)
            {
                return;
            }

            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            var strokesToRestore = new StrokeCollection();

            foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.History.TrashedInkStrokes.Contains(stroke)))
            {
                strokesToRestore.Add(stroke);
            }

            ParentPage.InkStrokes.Add(strokesToRestore);
            ParentPage.History.TrashedInkStrokes.Remove(strokesToRestore);
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            base.OnDeleted(fromHistory);

            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            var strokesToTrash = new StrokeCollection();

            foreach (var stroke in AcceptedStrokes.Where(stroke => ParentPage.InkStrokes.Contains(stroke)))
            {
                strokesToTrash.Add(stroke);
            }

            ParentPage.InkStrokes.Remove(strokesToTrash);
            ParentPage.History.TrashedInkStrokes.Add(strokesToTrash);
        }

        public override void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            var scaleX = NumberLineLength / (oldWidth - (2 * ArrowLength));

            AcceptedStrokes.StretchAll(scaleX, 1.0, XPosition + ArrowLength, YPosition);
        }

        public override void OnResized(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            base.OnResized(oldWidth, oldHeight, fromHistory);

            OnResizing(oldWidth, oldHeight, fromHistory);
        }

        public override void OnMoving(double oldX, double oldY, bool fromHistory = false)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            AcceptedStrokes.MoveAll(deltaX, deltaY);
        }

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            base.OnMoved(oldX, oldY, fromHistory);

            OnMoving(oldX, oldY, fromHistory);
        }

        /// <summary>Gets CodedID just before the historyAction at historyIndex executes Redo(). To get CodedID just after historyAction executes Redo(), add 1 to historyIndex.</summary>
        public override string GetCodedIDAtHistoryIndex(int historyIndex)
        {
            return GetNumberLineSizeAtHistoryIndex(historyIndex).ToString();
        }

        #endregion //APageObjectBase Overrides

        #region AStrokeAccepter Overrides

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public override int StrokeHitTestPercentage => 5;

        #endregion //AStrokeAccepter Overrides

        #region Static Methods

        public static string ConsolidateJumps(List<NumberLineJumpSize> jumps)
        {
            jumps = jumps.OrderBy(j => j.StartingTickIndex).ToList();
            var groupedJumps = new List<List<NumberLineJumpSize>>();

            var buffer = new List<NumberLineJumpSize>();
            for (var i = 0; i < jumps.Count; i++)
            {
                var currentJump = jumps[i];
                buffer.Add(currentJump);

                var nextJump = i + 1 < jumps.Count ? jumps[i + 1] : null;
                if (nextJump != null &&
                    (currentJump.JumpSize != nextJump.JumpSize || currentJump.StartingTickIndex + currentJump.JumpSize != nextJump.StartingTickIndex))
                {
                    groupedJumps.Add(buffer);
                    buffer = new List<NumberLineJumpSize>();
                }
            }

            groupedJumps.Add(buffer);
            var jumpSegments = new List<string>();
            foreach (var jumpGroup in groupedJumps.Where(jg => jg.Any()))
            {
                var firstJump = jumpGroup.First();
                var jumpSegment = $"{firstJump.JumpSize}, {firstJump.StartingTickIndex}-{firstJump.StartingTickIndex + (firstJump.JumpSize * jumpGroup.Count)}";
                jumpSegments.Add(jumpSegment);
            }

            return string.Join("; ", jumpSegments);
        }

        #endregion // Static Methods
    }
}