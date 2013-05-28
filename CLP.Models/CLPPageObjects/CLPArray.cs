using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
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
    public class CLPArrayDivision : DataObjectBase<CLPArrayDivision>
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
        /// The value that was written by the student as the label on that side length. null if unlabelled.
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
    public class CLPArray : CLPPageObjectBase
    {
        public static double LargeLabelLength
        {
            get
            {
                return 50;
            }
        }

        public static double SmallLabelLength
        {
            get
            {
                return 40;
            }
        }

        #region Constructors

        public CLPArray(int rows, int columns, CLPPage page)
            : base(page)
        {
            IsGridOn = false;

            Rows = rows;
            Columns = columns;

            YPosition = 150;

            ParentPage = page;
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            CanAcceptStrokes = true;
            CanAcceptPageObjects = true;

            ArrayHeight = 450;
            ArrayWidth = 450;

            EnforceAspectRatio(columns * 1.0 / rows);

            if (Width > page.PageWidth / 2)
            {
                ArrayHeight = 200;
                EnforceAspectRatio(columns * 1.0 / rows);
            }

            ApplyDistinctPosition(this);

            CalculateGridLines();
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

        public override ICLPPageObject Duplicate()
        {
            CLPArray newArray = Clone() as CLPArray;
            newArray.UniqueID = Guid.NewGuid().ToString();
            newArray.ParentPage = ParentPage;
            return newArray;
        }

        //aspectRatio is Width/Height
        public override void EnforceAspectRatio(double aspectRatio)
        {
            ArrayWidth = ArrayHeight * aspectRatio;

            if(ArrayWidth + LargeLabelLength + 2 * SmallLabelLength + XPosition > ParentPage.PageWidth)
            {
                ArrayWidth = ParentPage.PageWidth - XPosition - LargeLabelLength - 2 * SmallLabelLength;
                ArrayHeight = ArrayWidth / aspectRatio;
            }

            if (ArrayHeight + LargeLabelLength + 2 * SmallLabelLength + YPosition > ParentPage.PageHeight)
            {
                ArrayHeight = ParentPage.PageHeight - YPosition - LargeLabelLength - 2 * SmallLabelLength;
                ArrayWidth = ArrayHeight * aspectRatio;
            }

            Height = ArrayHeight + LargeLabelLength + 2 * SmallLabelLength;
            Width = ArrayWidth + LargeLabelLength + 2 * SmallLabelLength;
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

        public override bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            double areaObject = pageObject.Height * pageObject.Width;
            double area = ArrayHeight * ArrayWidth;
            double top = Math.Max(YPosition - LargeLabelLength, pageObject.YPosition);
            double bottom = Math.Min(YPosition + LargeLabelLength + ArrayHeight, pageObject.YPosition + pageObject.Height);
            double left = Math.Max(XPosition + LargeLabelLength, pageObject.XPosition);
            double right = Math.Min(XPosition + LargeLabelLength + ArrayWidth, pageObject.XPosition + pageObject.Width);
            double deltaY = bottom - top;
            double deltaX = right - left;
            double intersectionArea = deltaY * deltaX;
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

        public void RefreshArrayDimensions()
        {
            ArrayHeight = Height - LargeLabelLength - 2 * SmallLabelLength;
            ArrayWidth = Width - LargeLabelLength - 2 * SmallLabelLength;
        }

        public void CalculateGridLines()
        {
            HorizontalGridLines.Clear();
            VerticalGridLines.Clear();
            double squareSize = ArrayWidth / Columns;
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
            double oldHeight = 0;
            foreach(CLPArrayDivision division in HorizontalDivisions)
            {
                oldHeight = oldHeight + division.Length;
            }
            foreach(CLPArrayDivision division in HorizontalDivisions)
            {
                division.Position = division.Position * ArrayHeight / oldHeight;
                division.Length = division.Length * ArrayHeight / oldHeight;
            }

            double oldWidth = 0;
            foreach(CLPArrayDivision division in VerticalDivisions)
            {
                oldWidth = oldWidth + division.Length;
            }
            foreach(CLPArrayDivision division in VerticalDivisions)
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

        #endregion //Methods
    }
}
