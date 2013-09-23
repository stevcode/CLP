using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    public enum ArrayDivisionOrientation
    {
        Vertical,
        Horizontal
    }

    [Serializable]
    public class CLPArrayDivision : ModelBase
    {

        #region Constructors

        public CLPArrayDivision(ArrayDivisionOrientation orientation, double position, double length, int value)
        {
            Orientation = orientation;
            Position = position;
            Length = length;
            Value = value;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPArrayDivision(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Describes whether the division is horizontal or vertical.
        /// </summary>
        public ArrayDivisionOrientation Orientation
        {
            get { return GetValue<ArrayDivisionOrientation>(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly PropertyData OrientationProperty = RegisterProperty("Orientation", typeof(ArrayDivisionOrientation), ArrayDivisionOrientation.Horizontal);

        /// <summary>
        /// The position of the top (for horizontal) or left (for vertical) of the array division.
        /// </summary>
        public double Position
        {
            get { return GetValue<double>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof(double), 0);

        /// <summary>
        /// The length of the array division.
        /// </summary>
        public double Length
        {
            get { return GetValue<double>(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        public static readonly PropertyData LengthProperty = RegisterProperty("Length", typeof(double), 0);

        /// <summary>
        /// The value that was written by the student as the label on that side length. 0 if unlabelled.
        /// </summary>
        public int Value
        {
            get { return GetValue<int>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(int), 0);

        #endregion //Properties
    }

    [Serializable]
    public class CLPArray : ACLPPageObjectBase
    {
        public double LabelLength
        {
            get
            {
                return IsLabelVisible ? 22 : 0;
            }
        }

        #region Constructors

        public CLPArray(int rows, int columns, ICLPPage page)
            : base(page)
        {
            CanAcceptStrokes = true;
            CanAcceptPageObjects = true;

            IsGridOn = rows < 45 && columns < 45;
            Rows = rows;
            Columns = columns;

            XPosition = 0;
            YPosition = 150;

            SizeArrayToGridLevel();

            ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPArray"; }
        }

        /// <summary>
        /// Turns the grid on or off.
        /// </summary>
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof(bool), true);

        /// <summary>
        /// Turns the division behavior on or off.
        /// </summary>
        public bool IsDivisionBehaviorOn
        {
            get { return GetValue<bool>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof(bool), true);

        /// <summary>
        /// Sets the visibility of array labels
        /// </summary>
        public bool IsLabelVisible
        {
            get { return GetValue<bool>(IsLabelVisibleProperty); }
            set { SetValue(IsLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsLabelVisibleProperty = RegisterProperty("IsLabelVisible", typeof(bool), true);

        /// <summary>
        /// The Height of the Array.
        /// </summary>
        public double ArrayHeight
        {
            get { return GetValue<double>(ArrayHeightProperty); }
            set { SetValue(ArrayHeightProperty, value); }
        }

        public static readonly PropertyData ArrayHeightProperty = RegisterProperty("ArrayHeight", typeof(double), 0.0);

        /// <summary>
        /// The Width of the Array.
        /// </summary>
        public double ArrayWidth
        {
            get { return GetValue<double>(ArrayWidthProperty); }
            set { SetValue(ArrayWidthProperty, value); }
        }

        public static readonly PropertyData ArrayWidthProperty = RegisterProperty("ArrayWidth", typeof(double), 0.0);

        /// <summary>
        /// The number of rows in the array.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 1);

        /// <summary>
        /// The number of columns in the array.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), 1);

        /// <summary>
        /// Gets or sets the HorizontalGridDivs value.
        /// </summary>
        public ObservableCollection<double> HorizontalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalGridLinesProperty); }
            set { SetValue(HorizontalGridLinesProperty, value); }
        }

        public static readonly PropertyData HorizontalGridLinesProperty = RegisterProperty("HorizontalGridLines", typeof(ObservableCollection<double>), () => new ObservableCollection<double>());

        /// <summary>
        /// Gets or sets the VerticalGridLines value.
        /// </summary>
        public ObservableCollection<double> VerticalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(VerticalGridLinesProperty); }
            set { SetValue(VerticalGridLinesProperty, value); }
        }

        public static readonly PropertyData VerticalGridLinesProperty = RegisterProperty("VerticalGridLines", typeof(ObservableCollection<double>), () => new ObservableCollection<double>());

        /// <summary>
        /// List of horizontal divisions in the array.
        /// </summary>
        public ObservableCollection<CLPArrayDivision> HorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(HorizontalDivisionsProperty); }
            set { SetValue(HorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions", typeof(ObservableCollection<CLPArrayDivision>), () => new ObservableCollection<CLPArrayDivision>());

        /// <summary>
        /// List of vertical divisions in the array.
        /// </summary>
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions", typeof(ObservableCollection<CLPArrayDivision>), () => new ObservableCollection<CLPArrayDivision>());

        #endregion //Properties

        #region Methods

        public override List<ICLPPageObject> Cut(Stroke cuttingStroke)
        {
            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = YPosition + LabelLength;
            var cuttableBottom = cuttableTop + ArrayHeight;
            var cuttableLeft = XPosition + LabelLength;
            var cuttableRight = cuttableLeft + ArrayWidth;

            var halvedPageObjects = new List<ICLPPageObject>();

            //TODO: Tim - This is fine for now, but you could have an instance where a really wide, but short rectangle is made
            // and a stroke could be made that was only a few pixels high, and quite wide, that would try to make a horizontal
            // cut instead of the vertical cut that was intended. See Also Shape.Cut().
            if(Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom))
            {
                var average = (strokeRight + strokeLeft)/2;

                if((cuttableLeft <= strokeLeft && cuttableRight >= strokeRight) &&
                   (strokeTop - cuttableTop < 15 && cuttableBottom - strokeBottom < 15))
                {
                    if(Columns == 1)
                    {
                        return halvedPageObjects;
                    }

                    var relativeAverage = average - LabelLength - XPosition;

                    var minDistance = ArrayWidth;
                    var closestLinePosition = 0.0;
                    var closestLineIndex = 0;
                    foreach(var line in VerticalGridLines)
                    {
                        var distance = Math.Abs(relativeAverage - line);
                        if(distance >= minDistance)
                        {
                            continue;
                        }
                        minDistance = distance;
                        closestLinePosition = line;
                        closestLineIndex = VerticalGridLines.IndexOf(line);
                    }

                    var leftArray = new CLPArray(Rows, closestLineIndex + 1, ParentPage)
                    {
                        IsGridOn = IsGridOn,
                        IsDivisionBehaviorOn = IsDivisionBehaviorOn,
                        XPosition = XPosition,
                        YPosition = YPosition,
                        ArrayHeight = ArrayHeight
                    };
                    leftArray.EnforceAspectRatio(leftArray.Columns * 1.0 / leftArray.Rows);
                    leftArray.CalculateGridLines();
                    halvedPageObjects.Add(leftArray);

                    var rightArray = new CLPArray(Rows, Columns - closestLineIndex - 1, ParentPage)
                    {
                        IsGridOn = IsGridOn,
                        IsDivisionBehaviorOn = IsDivisionBehaviorOn,
                        XPosition = XPosition + closestLinePosition,
                        YPosition = YPosition,
                        ArrayHeight = ArrayHeight
                    };
                    rightArray.EnforceAspectRatio(rightArray.Columns * 1.0 / rightArray.Rows);
                    rightArray.CalculateGridLines();
                    halvedPageObjects.Add(rightArray);
                }
            }
            else
            {
                var average = (strokeTop + strokeBottom) / 2;

                if((cuttableTop <= strokeTop && cuttableBottom >= strokeBottom) &&
                   (strokeLeft - cuttableLeft < 15 && cuttableRight - strokeRight < 15))
                {
                    if(Rows == 1)
                    {
                        return halvedPageObjects;
                    }

                    var relativeAverage = average - LabelLength - YPosition;

                    var minDistance = ArrayHeight;
                    var closestLinePosition = 0.0;
                    var closestLineIndex = 0;
                    foreach(var line in HorizontalGridLines)
                    {
                        var distance = Math.Abs(relativeAverage - line);
                        if(distance >= minDistance)
                        {
                            continue;
                        }
                        minDistance = distance;
                        closestLinePosition = line;
                        closestLineIndex = HorizontalGridLines.IndexOf(line);
                    }

                    var topArray = new CLPArray(closestLineIndex + 1, Columns, ParentPage)
                    {
                        IsGridOn = IsGridOn,
                        IsDivisionBehaviorOn = IsDivisionBehaviorOn,
                        XPosition = XPosition,
                        YPosition = YPosition,
                        ArrayWidth = ArrayWidth
                    };
                    topArray.ArrayHeight = ArrayWidth / (topArray.Columns * 1.0 / topArray.Rows);
                    topArray.EnforceAspectRatio(topArray.Columns * 1.0 / topArray.Rows);
                    topArray.CalculateGridLines();
                    halvedPageObjects.Add(topArray);

                    var bottomArray = new CLPArray(Rows - closestLineIndex - 1, Columns, ParentPage)
                    {
                        IsGridOn = IsGridOn,
                        IsDivisionBehaviorOn = IsDivisionBehaviorOn,
                        XPosition = XPosition,
                        YPosition = YPosition + closestLinePosition,
                        ArrayWidth = ArrayWidth
                    };
                    bottomArray.ArrayHeight = ArrayWidth / (bottomArray.Columns * 1.0 / bottomArray.Rows);
                    bottomArray.EnforceAspectRatio(bottomArray.Columns * 1.0 / bottomArray.Rows);
                    bottomArray.CalculateGridLines();
                    halvedPageObjects.Add(bottomArray);
                }
            }

            return halvedPageObjects;
        }

        public override ICLPPageObject Duplicate()
        {
            var newArray = Clone() as CLPArray;
            if(newArray != null) {
                newArray.UniqueID = Guid.NewGuid().ToString();
                newArray.ParentPage = ParentPage;
                return newArray;
            }
            return null;
        }

        //aspectRatio is Width/Height
        //sealed because called in Constructor.
        public override sealed void EnforceAspectRatio(double aspectRatio)
        {
            ArrayWidth = ArrayHeight * aspectRatio;
            if(ArrayWidth < 30)
            {
                ArrayWidth = 30;
                ArrayHeight = ArrayWidth / aspectRatio;
            }
            if(ArrayWidth + LabelLength + XPosition > ParentPage.PageWidth)
            {
                ArrayWidth = ParentPage.PageWidth - XPosition - LabelLength;
                ArrayHeight = ArrayWidth / aspectRatio;
            }

            if (ArrayHeight + LabelLength + YPosition > ParentPage.PageHeight)
            {
                ArrayHeight = ParentPage.PageHeight - YPosition - LabelLength;
                ArrayWidth = ArrayHeight * aspectRatio;
            }

            Height = ArrayHeight + LabelLength;
            Width = ArrayWidth + LabelLength;
        }

        public override void OnRemoved()
        {
            foreach(Stroke stroke in GetStrokesOverPageObject())
            {
                ParentPage.InkStrokes.Remove(stroke);
            }

            foreach(ICLPPageObject po in GetPageObjectsOverPageObject())
            {
                //TODO: Steve - Make CLPPage level method RemovePageObject to guarantee OnRemoved() is called.
                po.OnRemoved();
                ParentPage.PageObjects.Remove(po);
            }
        }

        public override void OnResized()
        {
            RefreshArrayDimensions();
            CalculateGridLines();
            ResizeDivisions();
            base.OnResized();
        }

        public override bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            var areaObject = pageObject.Height * pageObject.Width;
            var area = ArrayHeight * ArrayWidth;
            var top = Math.Max(YPosition - LabelLength, pageObject.YPosition);
            var bottom = Math.Min(YPosition + LabelLength + ArrayHeight, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(XPosition + LabelLength, pageObject.XPosition);
            var right = Math.Min(XPosition + LabelLength + ArrayWidth, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            var intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && (intersectionArea / areaObject >= percentage || intersectionArea / area >= percentage);
        }

        public override void AcceptObjects(ObservableCollection<ICLPPageObject> addedPageObjects, ObservableCollection<ICLPPageObject> removedPageObjects)
        {
            if(CanAcceptPageObjects)
            {
                foreach(ICLPPageObject pageObject in removedPageObjects)
                {
                    if(PageObjectObjectParentIDs.Contains(pageObject.UniqueID))
                    {
                        Parts = (Parts - pageObject.Parts > 0) ? Parts - pageObject.Parts : 0;
                        PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
                    }
                }

                foreach(ICLPPageObject pageObject in addedPageObjects)
                {
                    if(!PageObjectObjectParentIDs.Contains(pageObject.UniqueID) && pageObject is CLPStrokePathContainer)
                    {
                        Parts += pageObject.Parts;
                        PageObjectObjectParentIDs.Add(pageObject.UniqueID);
                    }
                }
            }
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            if(CanAcceptStrokes)
            {
                foreach(Stroke s in removedStrokes)
                {
                    string strokeID = s.GetStrokeUniqueID();
                    try
                    {
                        PageObjectStrokeParentIDs.Remove(strokeID);
                    }
                    catch(Exception)
                    {
                        Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                    }
                }

                foreach(Stroke s in addedStrokes)
                {
                    var rect = new Rect(XPosition + LabelLength, YPosition + LabelLength, ArrayWidth, ArrayHeight);
                    if(s.HitTest(rect,80))
                    {
                        PageObjectStrokeParentIDs.Add(s.GetStrokeUniqueID());
                    }
                }
            }
        }

        public void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            var initialSquareSize = 45.0;
            if(toSquareSize <= 0)
            {
                while(XPosition + 2*LabelLength + initialSquareSize * Columns >= ParentPage.PageWidth || YPosition + 2*LabelLength + initialSquareSize * Rows >= ParentPage.PageHeight)
                {
                    initialSquareSize = initialSquareSize / 2;
                }
            }
            else
            {
                initialSquareSize = toSquareSize;
            }

            ArrayHeight = initialSquareSize * Rows;
            ArrayWidth = initialSquareSize * Columns;

            Height = ArrayHeight + 2*LabelLength;
            Width = ArrayWidth + 2*LabelLength;
            CalculateGridLines();
            if(recalculateDivisions)
            {
                ResizeDivisions();
            }  
        }

        public void RefreshArrayDimensions()
        {
            ArrayHeight = Height - LabelLength;
            ArrayWidth = Width - LabelLength;
        }

        public void CalculateGridLines()
        {
            HorizontalGridLines.Clear();
            VerticalGridLines.Clear();
            var squareSize = ArrayWidth / Columns;
            for(int i = 1; i < Rows; i++)
            {
                HorizontalGridLines.Add(i * squareSize);
            }
            for(int i = 1; i < Columns; i++)
            {
                VerticalGridLines.Add(i * squareSize);
            }
        }

        public void ResizeDivisions()
        {
            var oldHeight = HorizontalDivisions.Aggregate<CLPArrayDivision, double>(0, (current, division) => current + division.Length);
            foreach(var division in HorizontalDivisions)
            {
                division.Position = division.Position * ArrayHeight / oldHeight;
                division.Length = division.Length * ArrayHeight / oldHeight;
            }

            var oldWidth = VerticalDivisions.Aggregate<CLPArrayDivision, double>(0, (current, division) => current + division.Length);
            foreach(var division in VerticalDivisions)
            {
                division.Position = division.Position * ArrayWidth / oldWidth;
                division.Length = division.Length * ArrayWidth / oldWidth;
            }
        }

        public CLPArrayDivision FindDivisionAbove(double position, ObservableCollection<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divAbove = null;
            foreach(CLPArrayDivision div in divisionList)
            {
                if(divAbove == null)
                {
                    if(div.Position < position)
                    {
                        divAbove = div;
                    }
                }
                else if(divAbove.Position < div.Position && div.Position < position)
                {
                    divAbove = div;
                }
            }
            return divAbove;
        }

        public CLPArrayDivision FindDivisionBelow(double position, ObservableCollection<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divBelow = null;
            foreach(CLPArrayDivision div in divisionList)
            {
                if(divBelow == null)
                {
                    if(div.Position > position)
                    {
                        divBelow = div;
                    }
                }
                else if(divBelow.Position > div.Position && div.Position > position)
                {
                    divBelow = div;
                }
            }
            return divBelow;
        }

        public ObservableCollection<int> GetHorizontalRegionLabels()
        {
            ObservableCollection<int> labels = new ObservableCollection<int>();
            foreach(CLPArrayDivision div in HorizontalDivisions){
                labels.Add(div.Value);
            }
            return labels;
        }

        public ObservableCollection<int> GetVerticalRegionLabels()
        {
            ObservableCollection<int> labels = new ObservableCollection<int>();
            foreach(CLPArrayDivision div in VerticalDivisions){
                labels.Add(div.Value);
            }
            return labels;
        }

        public void RotateArray()
        {
            var tempCols = Columns;
            Columns = Rows;
            Rows = tempCols;
            var tempArrayHeight = ArrayHeight;
            ArrayHeight = ArrayWidth;
            ArrayWidth = tempArrayHeight;
            Height = ArrayHeight + 2*LabelLength;
            Width = ArrayWidth + 2*LabelLength;
            CalculateGridLines();
            var tempHorizontalDivisions = HorizontalDivisions;
            HorizontalDivisions = VerticalDivisions;
            VerticalDivisions = tempHorizontalDivisions;
            ResizeDivisions();

            if(XPosition + Width > ParentPage.PageWidth)
            {
                XPosition = ParentPage.PageWidth - Width;
            }
            if(YPosition + Height > ParentPage.PageHeight)
            {
                YPosition = ParentPage.PageHeight - Height;
            }

            RefreshStrokeParentIDs();
        }

        #endregion //Methods
    }
}
