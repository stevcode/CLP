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
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected CLPArrayDivision(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

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

    public abstract class ACLPArrayBase : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ACLPArrayBase" /> from scratch.
        /// </summary>
        public ACLPArrayBase() { }

        /// <summary>
        /// Initializes <see cref="ACLPArrayBase" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ACLPArrayBase" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="rows">The number of rows in the <see cref="ACLPArrayBase" />.</param>
        public ACLPArrayBase(CLPPage parentPage, int columns, int rows)
            : base(parentPage)
        {
            Columns = columns;
            Rows = rows;
        }

        /// <summary>
        /// Initializes <see cref="ACLPArrayBase" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ACLPArrayBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public virtual double LabelLength
        {
            get { return 22; }
        }

        /// <summary>
        /// The number of rows in the <see cref="ACLPArrayBase" />.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 1);

        /// <summary>
        /// The number of columns in the <see cref="ACLPArrayBase" />.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), 1);

        /// <summary>
        /// List of horizontal divisions in the array.
        /// </summary>
        public ObservableCollection<CLPArrayDivision> HorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(HorizontalDivisionsProperty); }
            set { SetValue(HorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions",
                                                                                           typeof(ObservableCollection<CLPArrayDivision>),
                                                                                           () => new ObservableCollection<CLPArrayDivision>());

        /// <summary>
        /// List of vertical divisions in the array.
        /// </summary>
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions",
                                                                                         typeof(ObservableCollection<CLPArrayDivision>),
                                                                                         () => new ObservableCollection<CLPArrayDivision>());

        #region Behavior Properties

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

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof(bool), false);

        /// <summary>
        /// Whether the array can be snapped to other arrays or not.
        /// </summary>
        public bool IsSnappable
        {
            get { return GetValue<bool>(IsSnappableProperty); }
            set { SetValue(IsSnappableProperty, value); }
        }

        public static readonly PropertyData IsSnappableProperty = RegisterProperty("IsSnappable", typeof(bool), true);

        /// <summary>
        /// Sets the visibility of the array's top label.
        /// </summary>
        public bool IsTopLabelVisible
        {
            get { return GetValue<bool>(IsTopLabelVisibleProperty); }
            set { SetValue(IsTopLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof(bool), true);

        /// <summary>
        /// Sets the visibility of the array's side label.
        /// </summary>
        public bool IsSideLabelVisible
        {
            get { return GetValue<bool>(IsSideLabelVisibleProperty); }
            set { SetValue(IsSideLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsSideLabelVisibleProperty = RegisterProperty("IsSideLabelVisible", typeof(bool), true);

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

        #endregion //Calculated Properties

        #endregion //Properties

        #region Methods

        public abstract void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true);

        public double GetClosestGridLine(double position) { return Math.Round(position / GridSquareSize) * GridSquareSize; }

        public CLPArrayDivision FindDivisionAbove(double position, IEnumerable<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divAbove = null;
            foreach(var div in divisionList)
            {
                if(divAbove == null)
                {
                    if(div.Position < position)
                    {
                        divAbove = div;
                    }
                }
                else if(divAbove.Position < div.Position &&
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
            foreach(var div in divisionList)
            {
                if(divBelow == null)
                {
                    if(div.Position > position)
                    {
                        divBelow = div;
                    }
                }
                else if(divBelow.Position > div.Position &&
                        div.Position > position)
                {
                    divBelow = div;
                }
            }
            return divBelow;
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

        public virtual void RotateArray()
        {
            var tempGridSquareSize = GridSquareSize;
            var tempCols = Columns;
            Columns = Rows;
            Rows = tempCols;

            var tempHorizontalDivisions = HorizontalDivisions;
            HorizontalDivisions = VerticalDivisions;
            VerticalDivisions = tempHorizontalDivisions;
            foreach(var verticalDivision in VerticalDivisions)
            {
                verticalDivision.Orientation = ArrayDivisionOrientation.Vertical;
            }
            foreach(var horizontalDivision in HorizontalDivisions)
            {
                horizontalDivision.Orientation = ArrayDivisionOrientation.Horizontal;
            }
            SizeArrayToGridLevel(tempGridSquareSize);

            if(XPosition + Width > ParentPage.Width)
            {
                XPosition = ParentPage.Width - Width;
            }
            if(YPosition + Height > ParentPage.Height)
            {
                YPosition = ParentPage.Height - Height;
            }

            //RefreshStrokeParentIDs();
            OnResized();
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

        #endregion //Methods

        #region Overrides of ObservableObject

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Height")
            {
                RaisePropertyChanged("ArrayHeight");
            }
            if(e.PropertyName == "Width")
            {
                RaisePropertyChanged("ArrayWidth");
                RaisePropertyChanged("GridSquareSize");
            }
            base.OnPropertyChanged(e);
        }

        #endregion //Overrides of ObservableObject

        #region Static Methods

        public static void ApplyDistinctPosition(ACLPArrayBase array, string currentUserID)
        {
            //squareSize will be the grid size of the most recently placed array, or 0 if there are no non-background arrays
            var lastArray =
                array.ParentPage.PageObjects.LastOrDefault(x => x is ACLPArrayBase && (x.OwnerID != Person.Author.ID || currentUserID == Person.Author.ID) && x.ID != array.ID) as ACLPArrayBase;
            if(lastArray == null)
            {
                return;
            }

            var previousGridSquareSize = array.GridSquareSize;
            array.SizeArrayToGridLevel(lastArray.GridSquareSize);
            //if(lastArray is CLPArray)
            //{
            //    array.YPosition = lastArray.YPosition;
            //    array.XPosition = lastArray.YPosition + lastArray.LabelLength + lastArray.Width;
            //    if(array.XPosition + array.Width >= array.ParentPage.Width ||
            //       array.YPosition + array.Height >= array.ParentPage.Height)
            //    {
            //        array.XPosition = lastArray.XPosition;
            //        array.YPosition = lastArray.YPosition + lastArray.LabelLength + lastArray.Height;
            //    }
            //    if(array.XPosition + array.Width >= array.ParentPage.Width ||
            //       array.YPosition + array.Height >= array.ParentPage.Height)
            //    {
            //        ApplyDistinctPosition(array);
            //    }
            //}
            //else if(lastArray is FuzzyFactorCard)
            //{
            //    array.XPosition = lastArray.XPosition;
            //    array.YPosition = lastArray.YPosition + lastArray.LabelLength + lastArray.Height + (lastArray as FuzzyFactorCard).LargeLabelLength - array.LabelLength;
            //    if(array.XPosition + array.Width >= array.ParentPage.Width ||
            //       array.YPosition + array.Height >= array.ParentPage.Height)
            //    {
            //        array.XPosition = 0.0;
            //    }
            //    if(array.XPosition + array.Width >= array.ParentPage.Width ||
            //       array.YPosition + array.Height >= array.ParentPage.Height)
            //    {
            //        array.YPosition = 100.0;
            //        ApplyDistinctPosition(array);
            //    }
            //}
        }

        #endregion //Static Methods
    }
}