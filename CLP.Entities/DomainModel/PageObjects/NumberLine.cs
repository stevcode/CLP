using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineTick : AEntityBase
    {
        #region Constructors

        public NumberLineTick() { }

        public NumberLineTick(int value, bool isNumberVisible)
        {
            TickValue = value;
            IsNumberVisible = isNumberVisible;
        }

        public NumberLineTick(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public double TickHeight
        {
            get { return 25.0; }
        }

        /// <summary>Number of the Tick</summary>
        public int TickValue
        {
            get { return GetValue<int>(TickValueProperty); }
            set { SetValue(TickValueProperty, value); }
        }

        public static readonly PropertyData TickValueProperty = RegisterProperty("TickValue", typeof (int), 0);

        /// <summary>Is the tick marks number visible</summary>
        public bool IsNumberVisible
        {
            get { return GetValue<bool>(IsNumberVisibleProperty); }
            set { SetValue(IsNumberVisibleProperty, value); }
        }

        public static readonly PropertyData IsNumberVisibleProperty = RegisterProperty("IsNumberVisible", typeof (bool), false);

        /// <summary>Is the tick visible on the number line</summary>
        public bool IsTickVisible
        {
            get { return GetValue<bool>(IsTickVisibleProperty); }
            set { SetValue(IsTickVisibleProperty, value); }
        }

        public static readonly PropertyData IsTickVisibleProperty = RegisterProperty("IsTickVisible", typeof (bool), true);

        /// <summary>Has the user marked the tick mark</summary>
        public bool IsMarked
        {
            get { return GetValue<bool>(IsMarkedProperty); }
            set { SetValue(IsMarkedProperty, value); }
        }

        public static readonly PropertyData IsMarkedProperty = RegisterProperty("IsMarked", typeof (bool), false);

        /// <summary>Color of the Tick Value</summary>
        public string TickColor
        {
            get { return GetValue<string>(TickColorProperty); }
            set { SetValue(TickColorProperty, value); }
        }

        public static readonly PropertyData TickColorProperty = RegisterProperty("TickColor", typeof (string), "Black");

        #endregion //Properties
    }

    [Serializable]
    public class NumberLineJumpSize : AEntityBase
    {
        #region Constructors

        public NumberLineJumpSize() { }

        public NumberLineJumpSize(int jumpSizeValue, int startTickIndex)
        {
            JumpSize = jumpSizeValue;
            StartingTickIndex = startTickIndex;
        }

        public NumberLineJumpSize(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Jump size of arrow</summary>
        public int JumpSize
        {
            get { return GetValue<int>(JumpSizeProperty); }
            set { SetValue(JumpSizeProperty, value); }
        }

        public static readonly PropertyData JumpSizeProperty = RegisterProperty("JumpSize", typeof (int), 0);

        /// <summary>Tick where jump begins</summary>
        public int StartingTickIndex
        {
            get { return GetValue<int>(StartingTickIndexProperty); }
            set { SetValue(StartingTickIndexProperty, value); }
        }

        public static readonly PropertyData StartingTickIndexProperty = RegisterProperty("StartingTickIndex", typeof (int), 0);

        #endregion //Properties
    }

    [Serializable]
    public class NumberLine : APageObjectBase, IStrokeAccepter
    {
        public const int MAX_ALL_TICKS_VISIBLE_LENGTH = 30;

        #region Constructors

        public NumberLine() { }

        public NumberLine(CLPPage parentPage, int numberLength)
            : base(parentPage)
        {
            NumberLineSize = numberLength;
            Height = NumberLineHeight;
            Width = parentPage.Width - 100;
            XPosition = (parentPage.Width / 2.0) - (Width / 2.0);
        }

        public NumberLine(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

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

        public static readonly PropertyData NumberLineSizeProperty = RegisterProperty("NumberLineSize", typeof (int), 0, OnNumberLineSizeChanged);

        private static void OnNumberLineSizeChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var numberLine = sender as NumberLine;
            if (numberLine == null ||
                !advancedPropertyChangedEventArgs.IsNewValueMeaningful)
            {
                return;
            }

            var oldValue = (int)advancedPropertyChangedEventArgs.OldValue;
            var newValue = (int)advancedPropertyChangedEventArgs.NewValue;

            if (oldValue < newValue)
            {
                numberLine.CreateTicks();
            }
            else
            {
                numberLine.DeleteTicks();
            }
        }

        /// <summary>Whether or not to show the Jump Size Labels.</summary>
        public bool IsJumpSizeLabelsVisible
        {
            get { return GetValue<bool>(IsJumpSizeLabelsVisibleProperty); }
            set { SetValue(IsJumpSizeLabelsVisibleProperty, value); }
        }

        public static readonly PropertyData IsJumpSizeLabelsVisibleProperty = RegisterProperty("IsJumpSizeLabelsVisible", typeof (bool), true);

        /// <summary>List of the values of the jumps</summary>
        public ObservableCollection<NumberLineJumpSize> JumpSizes
        {
            get { return GetValue<ObservableCollection<NumberLineJumpSize>>(JumpSizesProperty); }
            set { SetValue(JumpSizesProperty, value); }
        }

        public static readonly PropertyData JumpSizesProperty = RegisterProperty("JumpSizes",
                                                                                 typeof (ObservableCollection<NumberLineJumpSize>),
                                                                                 () => new ObservableCollection<NumberLineJumpSize>());

        /// <summary>A collection of the ticks of the number line</summary>
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks",
                                                                             typeof (ObservableCollection<NumberLineTick>),
                                                                             () => new ObservableCollection<NumberLineTick>());

        #endregion //Properties

        #region Methods

        public void CreateTicks()
        {
            var defaultInteger = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH ? 1 : 5;
            if (Ticks.LastOrDefault() != null)
            {
                if (Ticks.LastOrDefault().TickValue % defaultInteger != 0)
                {
                    Ticks.LastOrDefault().IsNumberVisible = false;
                }
            }

            if (!Ticks.Any())
            {
                for (var i = 0; i <= NumberLineSize; i++)
                {
                    var labelVisible = false;
                    if (i == 0 ||
                        i == NumberLineSize)
                    {
                        labelVisible = true;
                    }
                    else if (i % defaultInteger == 0)
                    {
                        labelVisible = true;
                    }

                    Ticks.Add(new NumberLineTick(i, labelVisible));
                }
            }
            else
            {
                Ticks.Add(new NumberLineTick(Ticks.Count, true));
            }
        }

        public void DeleteTicks()
        {
            if (Ticks.Any())
            {
                Ticks.Remove(Ticks.LastOrDefault());
            }
        }

        public NumberLineTick FindClosestTick(StrokeCollection strokes, bool isLookingForRightTick)
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

        public double FindTallestPoint(IEnumerable<Stroke> strokes) { return strokes.Select(stroke => stroke.GetBounds().Top).Concat(new[] { ParentPage.Height }).Min(); }

        #endregion //Methods

        #region APageObjectBase Overrides

        public override int ZIndex
        {
            get { return 60; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width")
            {
                RaisePropertyChanged("TickLength");
                RaisePropertyChanged("NumberLineLength");
            }
            base.OnPropertyChanged(e);
        }

        public override void OnAdded(bool fromHistory = false)
        {
            if (!fromHistory)
            {
                ApplyDistinctPosition(this);

                var multiplicationDefinitions = ParentPage.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
                var numberLineIDsInHistory = NumberLineAnalysis.GetListOfNumberLineIDsInHistory(ParentPage);

                foreach (var multiplicationRelationDefinitionTag in multiplicationDefinitions)
                {
                    var distanceFromAnswer = NumberLineSize - multiplicationRelationDefinitionTag.Product;

                    var tag = new NumberLineCreationTag(ParentPage, Origin.StudentPageObjectGenerated, ID, 0, NumberLineSize, numberLineIDsInHistory.IndexOf(ID), distanceFromAnswer);
                    ParentPage.AddTag(tag);
                }
            }
            else
            {
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
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            if (!fromHistory)
            {
                var jumpSizes = JumpSizes.Select(x => x.JumpSize).ToList();

                var lastMarkedTick = Ticks.LastOrDefault(x => x.IsMarked);
                var lastMarkedTickNumber = lastMarkedTick != null ? (int?)lastMarkedTick.TickValue : null;

                var numberLineIDsInHistory = NumberLineAnalysis.GetListOfNumberLineIDsInHistory(ParentPage);
                var tag = new NumberLineDeletedTag(ParentPage,
                                                   Origin.StudentPageObjectGenerated,
                                                   ID,
                                                   0,
                                                   NumberLineSize,
                                                   numberLineIDsInHistory.IndexOf(ID),
                                                   jumpSizes,
                                                   lastMarkedTickNumber);
                ParentPage.AddTag(tag);
            }

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
            if (!CanAcceptStrokes)
            {
                return;
            }

            var scaleX = NumberLineLength / (oldWidth - 2 * ArrowLength);

            AcceptedStrokes.StretchAll(scaleX, 1.0, XPosition + ArrowLength, YPosition);
        }

        public override void OnResized(double oldWidth, double oldHeight, bool fromHistory = false) { OnResizing(oldWidth, oldHeight, fromHistory); }

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

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false) { OnMoving(oldX, oldY, fromHistory); }

        public override IPageObject Duplicate()
        {
            var newNumberLine = Clone() as NumberLine;
            if (newNumberLine == null)
            {
                return null;
            }
            newNumberLine.CreationDate = DateTime.Now;
            newNumberLine.ID = Guid.NewGuid().ToCompactID();
            newNumberLine.VersionIndex = 0;
            newNumberLine.LastVersionIndex = null;
            newNumberLine.ParentPage = ParentPage;

            return newNumberLine;
        }

        #endregion //APageObjectBase Overrides

        #region IStrokeAccepter Implementation

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="Stroke" />s.</summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof (bool), true);

        /// <summary>The currently accepted <see cref="Stroke" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<Stroke> AcceptedStrokes
        {
            get { return GetValue<List<Stroke>>(AcceptedStrokesProperty); }
            set { SetValue(AcceptedStrokesProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokesProperty = RegisterProperty("AcceptedStrokes", typeof (List<Stroke>), () => new List<Stroke>());

        /// <summary>The IDs of the <see cref="Stroke" />s that have been accepted.</summary>
        public List<string> AcceptedStrokeParentIDs
        {
            get { return GetValue<List<string>>(AcceptedStrokeParentIDsProperty); }
            set { SetValue(AcceptedStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokeParentIDsProperty = RegisterProperty("AcceptedStrokeParentIDs", typeof (List<string>), () => new List<string>());

        public void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            // Remove Strokes
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var actuallyRemovedStrokes = new StrokeCollection();
            foreach (var stroke in removedStrokesList.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
                actuallyRemovedStrokes.Add(stroke);
            }
            var tickR = FindClosestTick(actuallyRemovedStrokes, true);
            var tickL = FindClosestTick(actuallyRemovedStrokes, false);
            if (tickR != null &&
                tickL != null &&
                tickR != tickL)
            {
                var deletedStartTickValue = tickL.TickValue;
                var deletedJumpSize = tickR.TickValue - tickL.TickValue;

                var jumpsToRemove = JumpSizes.Where(jump => jump.JumpSize == deletedJumpSize && jump.StartingTickIndex == deletedStartTickValue).ToList();
                foreach (var jump in jumpsToRemove)
                {
                    JumpSizes.Remove(jump);

                    //
                }

                if (JumpSizes.All(x => x.StartingTickIndex != tickR.TickValue))
                {
                    tickR.IsMarked = false;
                    tickR.TickColor = "Black";
                    tickR.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || tickR.TickValue % 5 == 0;
                }

                if (JumpSizes.All(x => x.StartingTickIndex + x.JumpSize != tickL.TickValue))
                {
                    tickL.IsMarked = false;
                    tickL.TickColor = "Black";
                    tickL.IsNumberVisible = NumberLineSize <= MAX_ALL_TICKS_VISIBLE_LENGTH || tickL.TickValue % 5 == 0;
                }
            }

            // Add Strokes
            var actuallyAcceptedStrokes = new StrokeCollection();
            var numberLineBodyBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            foreach (var stroke in addedStrokes.Where(stroke => stroke.HitTest(numberLineBodyBoundingBox, 5) && !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
                actuallyAcceptedStrokes.Add(stroke);
            }
            var tickRight = FindClosestTick(actuallyAcceptedStrokes, true);
            var tickLeft = FindClosestTick(actuallyAcceptedStrokes, false);
            if (tickRight != null &&
                tickLeft != null &&
                tickRight != tickLeft)
            {
                tickRight.IsMarked = true;
                tickRight.IsNumberVisible = true;
                tickLeft.IsMarked = true;
                tickLeft.IsNumberVisible = true;
                var lastStroke = actuallyAcceptedStrokes.Last();
                if (lastStroke.DrawingAttributes.Color == Colors.Black)
                {
                    tickRight.TickColor = "Blue";
                    tickLeft.TickColor = "Blue";
                }
                else
                {
                    tickRight.TickColor = lastStroke.DrawingAttributes.Color.ToString();
                    tickLeft.TickColor = lastStroke.DrawingAttributes.Color.ToString();
                }

                if (!JumpSizes.Any())
                {
                    var tallestPoint = FindTallestPoint(actuallyAcceptedStrokes);
                    tallestPoint = tallestPoint - 40;

                    if (tallestPoint < 0)
                    {
                        tallestPoint = 0;
                    }

                    if (tallestPoint > YPosition + Height - NumberLineHeight)
                    {
                        tallestPoint = YPosition + Height - NumberLineHeight;
                    }

                    Height = Height + (YPosition - tallestPoint);
                    YPosition = tallestPoint;
                }

                var jumpSize = tickRight.TickValue - tickLeft.TickValue;
                JumpSizes.Add(new NumberLineJumpSize(jumpSize, tickLeft.TickValue));

                //
            }
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var numberLineBodyBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(numberLineBodyBoundingBox, 5) //Stroke must be at least 5% contained by numberline.
                                    select stroke;

            return new StrokeCollection(strokesOverObject);
        }

        public void RefreshAcceptedStrokes()
        {
            AcceptedStrokes.Clear();
            AcceptedStrokeParentIDs.Clear();
            if (!CanAcceptStrokes)
            {
                return;
            }

            var strokesOverObject = GetStrokesOverPageObject();

            AcceptStrokes(strokesOverObject, new StrokeCollection());
        }

        #endregion //IStrokeAccepter Implementation
    }
}