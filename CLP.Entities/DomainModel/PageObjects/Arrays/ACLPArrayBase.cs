using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
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

        public CLPArrayDivision() { }

        public CLPArrayDivision(ArrayDivisionOrientation orientation, double position, double length, int value, bool isObscured = false)
        {
            Orientation = orientation;
            Position = position;
            Length = length;
            Value = value;
            IsObscured = isObscured;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArrayDivision(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Describes whether the division is horizontal or vertical.</summary>
        public ArrayDivisionOrientation Orientation
        {
            get { return GetValue<ArrayDivisionOrientation>(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly PropertyData OrientationProperty = RegisterProperty("Orientation", typeof (ArrayDivisionOrientation), ArrayDivisionOrientation.Horizontal);

        /// <summary>The position of the top (for horizontal) or left (for vertical) of the array division.</summary>
        public double Position
        {
            get { return GetValue<double>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof (double), 0);

        /// <summary>The length of the array division.</summary>
        public double Length
        {
            get { return GetValue<double>(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        public static readonly PropertyData LengthProperty = RegisterProperty("Length", typeof (double), 0);

        /// <summary>The value that was written by the student as the label on that side length. 0 if unlabelled.</summary>
        public int Value
        {
            get { return GetValue<int>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof (int), 0);

        /// <summary>Designates a Divider Region as obscured or not.</summary>
        public bool IsObscured
        {
            get { return GetValue<bool>(IsObscuredProperty); }
            set { SetValue(IsObscuredProperty, value); }
        }

        public static readonly PropertyData IsObscuredProperty = RegisterProperty("IsObscured", typeof(bool), false);

        #endregion //Properties

        #region Overrides of ModelBase

        /// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var division = obj as CLPArrayDivision;
            if (division == null)
            {
                return false;
            }

            return division.Orientation == Orientation && division.Position == Position && division.Length == Length && division.Value == Value;
        }

        #endregion
    }

    [Serializable]
    public abstract class ACLPArrayBase : APageObjectBase
    {
        public const double ARRAY_STARING_Y_POSITION = 295.0;
        public const double ARRAY_LABEL_LENGTH = 22.0;
        public const double DT_LABEL_LENGTH = 35.0;
        public const double DT_LARGE_LABEL_LENGTH = 52.5;

        #region Constructors

        /// <summary>Initializes <see cref="ACLPArrayBase" /> from scratch.</summary>
        public ACLPArrayBase() { }

        /// <summary>Initializes <see cref="ACLPArrayBase" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ACLPArrayBase" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="rows">The number of rows in the <see cref="ACLPArrayBase" />.</param>
        public ACLPArrayBase(CLPPage parentPage, int columns, int rows)
            : base(parentPage)
        {
            Columns = columns;
            Rows = rows;
            XPosition = 0.0;
            YPosition = ARRAY_STARING_Y_POSITION;
        }

        /// <summary>Initializes <see cref="ACLPArrayBase" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ACLPArrayBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public virtual double LabelLength
        {
            get { return ARRAY_LABEL_LENGTH; }
        }

        /// <summary>The number of rows in the <see cref="ACLPArrayBase" />.</summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof (int), 1);

        /// <summary>The number of columns in the <see cref="ACLPArrayBase" />.</summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof (int), 1);

        /// <summary>List of horizontal divisions in the array.</summary>
        public ObservableCollection<CLPArrayDivision> HorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(HorizontalDivisionsProperty); }
            set { SetValue(HorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions",
                                                                                           typeof (ObservableCollection<CLPArrayDivision>),
                                                                                           () => new ObservableCollection<CLPArrayDivision>());

        /// <summary>List of vertical divisions in the array.</summary>
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions",
                                                                                         typeof (ObservableCollection<CLPArrayDivision>),
                                                                                         () => new ObservableCollection<CLPArrayDivision>());

        #region Behavior Properties

        /// <summary>Turns the grid on or off.</summary>
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof (bool), true);

        /// <summary>Turns the division behavior on or off.</summary>
        public bool IsDivisionBehaviorOn
        {
            get { return GetValue<bool>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof (bool), true);

        /// <summary>Whether the array can be snapped to other arrays or not.</summary>
        public bool IsSnappable
        {
            get { return GetValue<bool>(IsSnappableProperty); }
            set { SetValue(IsSnappableProperty, value); }
        }

        public static readonly PropertyData IsSnappableProperty = RegisterProperty("IsSnappable", typeof (bool), true);

        /// <summary>Sets the visibility of the array's top label.</summary>
        public bool IsTopLabelVisible
        {
            get { return GetValue<bool>(IsTopLabelVisibleProperty); }
            set { SetValue(IsTopLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof (bool), true);

        /// <summary>Sets the visibility of the array's side label.</summary>
        public bool IsSideLabelVisible
        {
            get { return GetValue<bool>(IsSideLabelVisibleProperty); }
            set { SetValue(IsSideLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsSideLabelVisibleProperty = RegisterProperty("IsSideLabelVisible", typeof (bool), true);

        #endregion //Behavior Properties

        #region Calculated Properties

        public virtual double ArrayWidth
        {
            get { return Width - (2 * LabelLength); }
        }

        public virtual double ArrayHeight
        {
            get { return Height - (2 * LabelLength); }
        }

        public virtual double GridSquareSize
        {
            get { return ArrayWidth / Columns; }
        }

        public abstract double MinimumGridSquareSize { get; }

        /// <summary>Signifies obscuring shape over Rows.</summary>
        public bool IsRowsObscured
        {
            get { return HorizontalDivisions.Any(d => d.IsObscured); }
        }

        /// <summary>Signifies obscuring shape over Columns.</summary>
        public bool IsColumnsObscured
        {
            get { return VerticalDivisions.Any(d => d.IsObscured); }
        }

        #endregion //Calculated Properties

        #endregion //Properties

        #region Methods

        public abstract void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true);

        public double GetClosestGridLine(double position) { return Math.Round(position / GridSquareSize) * GridSquareSize; }

        public CLPArrayDivision FindDivisionAbove(double position, IEnumerable<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divAbove = null;
            foreach (var div in divisionList)
            {
                if (divAbove == null)
                {
                    if (div.Position < position)
                    {
                        divAbove = div;
                    }
                }
                else if (divAbove.Position < div.Position &&
                         div.Position < position)
                {
                    divAbove = div;
                }
            }
            return divAbove;
        }

        public CLPArrayDivision FindDivisionBelow(double position, IEnumerable<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divBelow = null;
            foreach (var div in divisionList)
            {
                if (divBelow == null)
                {
                    if (div.Position > position)
                    {
                        divBelow = div;
                    }
                }
                else if (divBelow.Position > div.Position &&
                         div.Position > position)
                {
                    divBelow = div;
                }
            }
            
            return divBelow;
        }

        public virtual void SortDivisions()
        {
            VerticalDivisions = new ObservableCollection<CLPArrayDivision>(VerticalDivisions.OrderBy(d => d.Position));
            HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(HorizontalDivisions.OrderBy(d => d.Position));
        }

        public virtual void ResizeDivisions()
        {
            SortDivisions();
            var position = 0.0;
            foreach (var division in HorizontalDivisions)
            {
                division.Position = position;
                division.Length = (GridSquareSize * division.Value) - 1.0;
                position += division.Length;
            }

            position = 0.0;
            foreach (var division in VerticalDivisions)
            {
                division.Position = position;
                division.Length = (GridSquareSize * division.Value) - 1.0;
                position += division.Length;
            }
        }

        public virtual void RotateArray()
        {
            var initialWidth = Width;
            var initialHeight = Height;

            var tempGridSquareSize = GridSquareSize;
            var tempCols = Columns;
            Columns = Rows;
            Rows = tempCols;

            var tempHorizontalDivisions = HorizontalDivisions;
            HorizontalDivisions = VerticalDivisions;
            VerticalDivisions = tempHorizontalDivisions;
            foreach (var verticalDivision in VerticalDivisions)
            {
                verticalDivision.Orientation = ArrayDivisionOrientation.Vertical;
            }
            foreach (var horizontalDivision in HorizontalDivisions)
            {
                horizontalDivision.Orientation = ArrayDivisionOrientation.Horizontal;
            }
            SizeArrayToGridLevel(tempGridSquareSize);

            if (XPosition + Width > ParentPage.Width)
            {
                XPosition = ParentPage.Width - Width;
            }
            if (YPosition + Height > ParentPage.Height)
            {
                YPosition = ParentPage.Height - Height;
            }

            //RefreshStrokeParentIDs();
            OnResized(initialWidth, initialHeight);
        }

        #endregion //Methods

        #region APageObjectBase Overrides

        public override int ZIndex
        {
            get { return 50; }
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Height")
            {
                RaisePropertyChanged("ArrayHeight");
            }
            if (e.PropertyName == "Width")
            {
                RaisePropertyChanged("ArrayWidth");
                RaisePropertyChanged("GridSquareSize");
            }
            base.OnPropertyChanged(e);
        }

        public override bool PageObjectIsOver(IPageObject pageObject, double percentage)
        {
            var areaObject = pageObject.Height * pageObject.Width;
            var area = ArrayHeight * ArrayWidth;
            var top = Math.Max(YPosition + LabelLength, pageObject.YPosition);
            var bottom = Math.Min(YPosition + LabelLength + ArrayHeight, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(XPosition + LabelLength, pageObject.XPosition);
            var right = Math.Min(XPosition + LabelLength + ArrayWidth, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            var intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && (intersectionArea / areaObject >= percentage || intersectionArea / area >= percentage);
        }

        #endregion //APageObjectBase Overrides

        #region Static Properties

        public static double DefaultGridSquareSize //used to be 45.0, then 34.0, now again 45.0
        {
            get { return 45.0; }
        }

        #endregion //Static Properties

        #region Static Methods

        public static void ApplyDistinctPosition(ACLPArrayBase array)
        {
            var currentPage = array.ParentPage;
            if (currentPage == null)
            {
                return;
            }

            var arraysToAvoid = currentPage.PageObjects.OfType<ACLPArrayBase>().Where(a => a.ID != array.ID).ToList();
            while (arraysToAvoid.Any())
            {
                var overlappingArray = arraysToAvoid.FirstOrDefault(a => IsOverlapping(a, array));
                if (overlappingArray == null)
                {
                    break;
                }

                arraysToAvoid.Remove(overlappingArray);

                if (overlappingArray is FuzzyFactorCard)
                {
                    array.YPosition = overlappingArray.YPosition + overlappingArray.Height;
                }
                else
                {
                    if (overlappingArray.Rows >= overlappingArray.Columns) //Move array right.
                    {
                        array.XPosition = overlappingArray.XPosition + overlappingArray.Width;
                        if (array.XPosition + array.Width >= currentPage.Width)
                        {
                            array.XPosition = 0.0;
                            array.YPosition = overlappingArray.YPosition + overlappingArray.Height;
                        }
                    }
                    else //Move array down.
                    {
                        array.YPosition = overlappingArray.YPosition + overlappingArray.Height;
                        if (array.YPosition + array.Height >= currentPage.Height)
                        {
                            array.XPosition = overlappingArray.XPosition + overlappingArray.Width;
                            array.YPosition = ARRAY_STARING_Y_POSITION;
                        }
                    }
                }
            }

            var rnd = new Random();

            if (array.YPosition + array.Height >= currentPage.Height)
            {
                array.YPosition = currentPage.Height - array.Height - rnd.Next(30);
            }
            if (array.XPosition + array.Width >= currentPage.Width)
            {
                array.XPosition = currentPage.Width - array.Width - rnd.Next(30);
            }
        }

        #endregion //Static Methods
    }
}