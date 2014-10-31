﻿using System;
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

        /// <summary>
        /// Jump size of arrow
        /// </summary>
        public int JumpSize
        {
            get { return GetValue<int>(JumpSizeProperty); }
            set { SetValue(JumpSizeProperty, value); }
        }

        public static readonly PropertyData JumpSizeProperty = RegisterProperty("JumpSize", typeof (int), 0);

        /// <summary>
        /// Tick where jump begins
        /// </summary>
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
        #region Constructors

        public NumberLine() { }

        public NumberLine(CLPPage parentPage, int numberLength)
            : base(parentPage)
        {
            NumberLineSize = numberLength;
            Height = NumberLineHeight;
            Width = 800;
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

        public override bool IsBackgroundInteractable
        {
            get { return true; }
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
            if (numberLine == null || !advancedPropertyChangedEventArgs.IsNewValueMeaningful)
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

        public static readonly PropertyData JumpSizesProperty = RegisterProperty("JumpSizes", typeof (ObservableCollection<NumberLineJumpSize>), () => new ObservableCollection<NumberLineJumpSize>());

        /// <summary>A collection of the ticks of the number line</summary>
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks",
                                                                             typeof (ObservableCollection<NumberLineTick>),
                                                                             () => new ObservableCollection<NumberLineTick>());

        #region IStrokeAccepter Members

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="Stroke" />s.</summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof (bool), true);

        /// <summary>The currently accepted <see cref="Stroke" />s.</summary>
        [XmlIgnore]
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

        #endregion //IStrokeAccepter Members

        #endregion //Properties

        #region Methods

        public override void OnDeleted()
        {
            if (!CanAcceptStrokes ||
                !AcceptedStrokes.Any())
            {
                return;
            }

            ParentPage.InkStrokes.Remove(new StrokeCollection(AcceptedStrokes));
            ParentPage.History.TrashedInkStrokes.Add(new StrokeCollection(AcceptedStrokes));
        }
        
        public void CreateTicks()
        {
            var defaultInteger = NumberLineSize <= 10 ? 1 : 5;
            if (Ticks.LastOrDefault() != null)
            {
                if (Ticks.LastOrDefault().TickValue % 5 != 0)
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
            if(Ticks.Any())
            {
                Ticks.Remove(Ticks.LastOrDefault());
            }
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

        public override void OnResizing(double oldWidth, double oldHeight)
        {
            var scaleX = NumberLineLength / (oldWidth - 2 * ArrowLength);

            if (CanAcceptStrokes)
            {
                foreach (var stroke in AcceptedStrokes)
                {
                    var transform = new Matrix();
                    transform.ScaleAt(scaleX, 1.0, XPosition + ArrowLength, YPosition);
                    stroke.Transform(transform, false);
                }
            }
        }

        public override void OnResized(double oldWidth, double oldHeight)
        {
            OnResizing(oldWidth, oldHeight);
        }


        public override void OnMoving(double oldX, double oldY)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            if (CanAcceptStrokes)
            {
                foreach (var stroke in AcceptedStrokes)
                {
                    var transform = new Matrix();
                    transform.Translate(deltaX, deltaY);
                    stroke.Transform(transform, true);
                }
            }
        }

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

        public void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            foreach (var stroke in removedStrokes.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
                var theRemovedStrokes = new StrokeCollection(removedStrokes);
                var tickR = FindClosestTick(theRemovedStrokes);
                var tickL = FindClosestTickLeft(theRemovedStrokes);

                var deletedStartTickValue = tickL.TickValue;
                var deletedJumpSize = tickR.TickValue - tickL.TickValue;

                foreach (var jump in JumpSizes)
                {
                    if (jump.JumpSize == deletedJumpSize &&
                        jump.StartingTickIndex == deletedStartTickValue)
                    {
                        jump.JumpSize = 0;
                    }
                }

                if (NumberLineSize < 11)
                {
                    tickL.TickColor = "Black";
                    tickR.TickColor = "Black";
                }
                else
                {
                    if (tickL.TickValue % 5 == 0 &&
                        tickR.TickValue % 5 == 0)
                    {
                        tickL.TickColor = "Black";
                        tickR.TickColor = "Black";
                    }
                    else if (tickL.TickValue % 5 == 0)
                    {
                        tickL.TickColor = "Black";
                        tickR.IsMarked = false;
                        tickR.IsNumberVisible = false;
                    }
                    else if (tickR.TickValue % 5 == 0)
                    {
                        tickR.TickColor = "Black";
                        tickL.IsMarked = false;
                        tickL.IsNumberVisible = false;
                    }
                    else
                    {
                        tickL.IsMarked = false;
                        tickL.IsNumberVisible = false;
                        tickR.IsMarked = false;
                        tickR.IsNumberVisible = false;
                    }
                
                }

            }

            var actuallyAcceptedStrokes = new StrokeCollection();
            var numberLineBodyBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            foreach (var stroke in addedStrokes.Where(stroke => stroke.HitTest(numberLineBodyBoundingBox, 5) && !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
                actuallyAcceptedStrokes.Add(stroke);
            }

            //Grab the lowest right point
            var tick = FindClosestTick(actuallyAcceptedStrokes);
            var tick2 = FindClosestTickLeft(actuallyAcceptedStrokes);

            if (tick == null &&
                tick2 == null)
            {
                return;
            }

            tick.IsMarked = true;
            tick.IsNumberVisible = true;
            var lastStroke = actuallyAcceptedStrokes.Last();
            if (lastStroke.DrawingAttributes.Color == Colors.Black)
            {
                tick.TickColor = "Blue";
                tick2.TickColor = "Blue";
            }
            else
            {
                tick.TickColor = lastStroke.DrawingAttributes.Color.ToString();
                tick2.TickColor = lastStroke.DrawingAttributes.Color.ToString();
            }

            tick2.IsMarked = true;
            tick2.IsNumberVisible = true;

            if (!JumpSizes.Any())
            {
                var tallestPoint = FindTallestPoint(actuallyAcceptedStrokes);
                tallestPoint = tallestPoint - 20;

                if (tallestPoint < 0)
                {
                    tallestPoint = 0;
                }

                if (tallestPoint > YPosition)
                {
                    tallestPoint = YPosition;
                }

                Height = Height + (YPosition - tallestPoint);
                YPosition = tallestPoint;
            }
            
            var jumpSize = tick.TickValue - tick2.TickValue;
            if (tick == tick2)
            {
                var lastMarkedTick = Ticks.Reverse().FirstOrDefault(x => x.IsMarked && x.TickValue < tick.TickValue);
                if (lastMarkedTick == null)
                {
                    jumpSize = tick.TickValue;
                    JumpSizes.Add(new NumberLineJumpSize(jumpSize, 0));
                }
                else
                {
                    jumpSize = tick.TickValue - lastMarkedTick.TickValue;
                    JumpSizes.Add(new NumberLineJumpSize(jumpSize, lastMarkedTick.TickValue));
                }
                
            }
            else
            {
                JumpSizes.Add(new NumberLineJumpSize(jumpSize, tick2.TickValue));
            }
        }

        public void RefreshAcceptedStrokes()
        {
            AcceptedStrokes.Clear();
            AcceptedStrokeParentIDs.Clear();
            if (!CanAcceptStrokes)
            {
                return;
            }

            var numberLineBodyBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(numberLineBodyBoundingBox, 5) //Stroke must be at least 5% contained by Stamp body.
                                    select stroke;

            AcceptStrokes(new StrokeCollection(strokesOverObject), new StrokeCollection());
        }

        public NumberLineTick FindClosestTick(StrokeCollection strokes)
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

            var midX = (x2 - x1) / 2.0 + x1;

            var lowestPoint = new StylusPoint(0.0, 0.0);
            foreach (var stroke in strokes)
            {
                foreach (var point in stroke.StylusPoints)
                {
                    if (point.Y > lowestPoint.Y &&
                        point.X > midX)
                    {
                        lowestPoint = point;
                    }
                }
            }


            //Find closest Tick

            var normalXLowest = (lowestPoint.X - XPosition - ArrowLength) / TickLength;
            var tickIndex = (int)Math.Round(normalXLowest);

            if (tickIndex < 0 ||
                tickIndex >= Ticks.Count)
            {
                return null;
            }

            return Ticks[tickIndex];
        }

        public NumberLineTick FindClosestTickLeft(StrokeCollection strokes)
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

            var midX = (x2 - x1) / 2.0 + x1;

            var lowestPoint = new StylusPoint(0.0, 0.0);
            foreach (var stroke in strokes)
            {
                foreach (var point in stroke.StylusPoints)
                {
                    if (point.Y > lowestPoint.Y &&
                        point.X < midX)
                    {
                        lowestPoint = point;
                    }
                }
            }


            //Find closest Tick

            var normalXLowest = (lowestPoint.X - XPosition - ArrowLength) / TickLength;
            var tickIndex = (int)Math.Round(normalXLowest);

            if (tickIndex < 0 ||
                tickIndex >= Ticks.Count)
            {
                return null;
            }

            return Ticks[tickIndex];
        }

        public double FindTallestPoint(StrokeCollection theCollection)
        {
            var tallestPoint = ParentPage.Height;
            //find highest point on ink
            foreach (var stroke in theCollection)
            {
                tallestPoint = Math.Min(tallestPoint, stroke.GetBounds().Top);
            }

            return tallestPoint;
        }

        #endregion //Methods
    }
}