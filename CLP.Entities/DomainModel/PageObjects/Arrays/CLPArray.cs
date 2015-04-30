using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public enum ArrayTypes
    {
        Array,
        ArrayCard,
        FactorCard,
        TenByTen,
        ObscurableArray
    }

    [Serializable]
    public class CLPArray : ACLPArrayBase, ICountable, ICuttable, IStrokeAccepter
    {
        private const double MIN_ARRAY_LENGTH = 25.0;

        #region Constructors

        /// <summary>Initializes <see cref="CLPArray" /> from scratch.</summary>
        public CLPArray() { }

        /// <summary>Initializes <see cref="CLPArray" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="CLPArray" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="rows">The number of rows in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="arrayType">The type of <see cref="CLPArray" />.</param>
        public CLPArray(CLPPage parentPage, int columns, int rows, ArrayTypes arrayType)
            : base(parentPage, columns, rows)
        {
            IsGridOn = rows < 40 && columns < 40;
            ArrayType = arrayType;
        }

        public CLPArray(CLPPage parentPage, double gridSquareSize, int columns, int rows, ArrayTypes arrayType)
            : this(parentPage, columns, rows, arrayType)
        {
            Width = (gridSquareSize * columns) + (2 * ARRAY_LABEL_LENGTH);
            Height = (gridSquareSize * rows) + (2 * ARRAY_LABEL_LENGTH);
        }

        /// <summary>Initializes <see cref="CLPArray" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override double MinimumGridSquareSize
        {
            get { return Columns < Rows ? MIN_ARRAY_LENGTH / Columns : MIN_ARRAY_LENGTH / Rows; }
        }

        /// <summary>The type of <see cref="CLPArray" />.</summary>
        public ArrayTypes ArrayType
        {
            get { return GetValue<ArrayTypes>(ArrayTypeProperty); }
            set { SetValue(ArrayTypeProperty, value); }
        }

        public static readonly PropertyData ArrayTypeProperty = RegisterProperty("ArrayType", typeof (ArrayTypes), ArrayTypes.Array);

        #endregion //Properties

        #region Methods

        public override void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            var initialSquareSize = DefaultGridSquareSize;
            if (toSquareSize <= 0)
            {
                while (XPosition + 2 * LabelLength + initialSquareSize * Columns >= ParentPage.Width ||
                       YPosition + 2 * LabelLength + initialSquareSize * Rows >= ParentPage.Height)
                {
                    initialSquareSize = Math.Abs(initialSquareSize - 45.0) < .0001 ? 22.5 : initialSquareSize / 4 * 3;
                }
            }
            else
            {
                initialSquareSize = toSquareSize;
            }

            Height = (initialSquareSize * Rows) + (2 * LabelLength);
            Width = (initialSquareSize * Columns) + (2 * LabelLength);

            if (recalculateDivisions)
            {
                ResizeDivisions();
            }
        }

        public int[,] GetPartialProducts()
        {
            var horizDivs = Math.Max(HorizontalDivisions.Count, 1);
            var vertDivs = Math.Max(VerticalDivisions.Count, 1);
            var partialProducts = new int[horizDivs, vertDivs];

            for (var i = 0; i < horizDivs; i++)
            {
                for (var j = 0; j < vertDivs; j++)
                {
                    var yAxisValue = (horizDivs > 1 ? HorizontalDivisions[i].Value : Rows);
                    var xAxisValue = (vertDivs > 1 ? VerticalDivisions[j].Value : Columns);

                    partialProducts[i, j] = yAxisValue * xAxisValue;
                }
            }

            return partialProducts;
        }

        public void Obscure(bool isColumnsObscured)
        {
            if (isColumnsObscured)
            {
                VerticalDivisions.Clear();
                var obscuredDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, ArrayWidth, Columns)
                                  {
                                      IsObscured = true
                                  };
                VerticalDivisions.Add(obscuredDiv);
            }
            else
            {
                HorizontalDivisions.Clear();
                var obscuredDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, ArrayHeight, Rows)
                                  {
                                      IsObscured = true
                                  };
                HorizontalDivisions.Add(obscuredDiv);
            }
        }

        public void Unobscure(bool isColumnsUnobscured)
        {
            if (isColumnsUnobscured)
            {
                if (VerticalDivisions.Count == 1)
                {
                    VerticalDivisions.Clear();
                }

                foreach (var division in VerticalDivisions)
                {
                    division.IsObscured = false;
                }
            }
            else
            {
                if (HorizontalDivisions.Count == 1)
                {
                    HorizontalDivisions.Clear();
                }

                foreach (var division in HorizontalDivisions)
                {
                    division.IsObscured = false;
                }
            }
        }

        #endregion //Methods

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get
            {
                string arrayType;
                switch (ArrayType)
                {
                    case ArrayTypes.Array:
                        arrayType = "Array";
                        break;
                    case ArrayTypes.ArrayCard:
                        arrayType = "Array Card";
                        break;
                    case ArrayTypes.FactorCard:
                        arrayType = "Factor Card";
                        break;
                    case ArrayTypes.TenByTen:
                        arrayType = "Array (Static)";
                        break;
                    case ArrayTypes.ObscurableArray:
                        arrayType = "Obscurable Array";
                        break;
                    default:
                        arrayType = "Default Array";
                        break;
                }

                return string.Format("{0}x{1} {2}", Rows, Columns, arrayType);
            }
        }

        public override string CodedName
        {
            get { return "ARR"; }
        }

        public override string CodedID
        {
            get { return string.Format("[{0}x{1}]", Rows, Columns); }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        public override double MinimumHeight
        {
            get { return MIN_ARRAY_LENGTH + (2 * LabelLength); }
        }

        public override double MinimumWidth
        {
            get { return MIN_ARRAY_LENGTH + (2 * LabelLength); }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            base.OnAdded(fromHistory);

            if (!fromHistory)
            {
                foreach (var divisionTemplate in ParentPage.PageObjects.OfType<FuzzyFactorCard>().ToList())
                {
                    if (ParentPage.IsTagAddPrevented)
                    {
                        continue;
                    }

                    var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(ParentPage);

                    if (divisionTemplate.CurrentRemainder != divisionTemplate.Dividend % divisionTemplate.Rows)
                    {
                        var existingFactorPairErrorsTag =
                            ParentPage.Tags.OfType<DivisionTemplateFactorPairErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
                        var isArrayDimensionErrorsTagOnPage = true;

                        if (existingFactorPairErrorsTag == null)
                        {
                            existingFactorPairErrorsTag = new DivisionTemplateFactorPairErrorsTag(ParentPage,
                                                                                                  Origin.StudentPageGenerated,
                                                                                                  divisionTemplate.ID,
                                                                                                  divisionTemplate.Dividend,
                                                                                                  divisionTemplate.Rows,
                                                                                                  divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                            isArrayDimensionErrorsTagOnPage = false;
                        }

                        if (Columns == divisionTemplate.Dividend ||
                            Rows == divisionTemplate.Dividend)
                        {
                            existingFactorPairErrorsTag.CreateDividendAsDimensionDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                        }

                        if (Rows != divisionTemplate.Rows)
                        {
                            if (Columns == divisionTemplate.Rows)
                            {
                                existingFactorPairErrorsTag.CreateWrongOrientationDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                            }
                            else
                            {
                                existingFactorPairErrorsTag.CreateIncorrectDimensionDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                            }
                        }

                        var totalAreaOfArraysOnPage = ParentPage.PageObjects.OfType<CLPArray>().Sum(x => x.Rows * x.Columns);
                        if (totalAreaOfArraysOnPage > divisionTemplate.CurrentRemainder)
                        {
                            existingFactorPairErrorsTag.CreateArrayTooLargeDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                        }

                        if (!isArrayDimensionErrorsTagOnPage &&
                            existingFactorPairErrorsTag.ErrorAtemptsSum > 0)
                        {
                            ParentPage.AddTag(existingFactorPairErrorsTag);
                        }
                    }
                    else
                    {
                        var existingRemainderErrorsTag =
                            ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
                        var isRemainderErrorsTagOnPage = true;

                        if (existingRemainderErrorsTag == null)
                        {
                            existingRemainderErrorsTag = new DivisionTemplateRemainderErrorsTag(ParentPage,
                                                                                                Origin.StudentPageGenerated,
                                                                                                divisionTemplate.ID,
                                                                                                divisionTemplate.Dividend,
                                                                                                divisionTemplate.Rows,
                                                                                                divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                            isRemainderErrorsTagOnPage = false;
                        }

                        if (Columns == divisionTemplate.Dividend ||
                            Rows == divisionTemplate.Dividend)
                        {
                            existingRemainderErrorsTag.CreateDividendAsDimensionDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                        }

                        if (Rows != divisionTemplate.Rows)
                        {
                            if (Columns == divisionTemplate.Rows)
                            {
                                existingRemainderErrorsTag.CreateWrongOrientationDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                            }
                            else
                            {
                                existingRemainderErrorsTag.CreateIncorrectDimensionDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                            }
                        }

                        var totalAreaOfArraysOnPage = ParentPage.PageObjects.OfType<CLPArray>().Sum(x => x.Rows * x.Columns);
                        if (totalAreaOfArraysOnPage > divisionTemplate.CurrentRemainder)
                        {
                            existingRemainderErrorsTag.CreateArrayTooLargeDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                        }

                        if (!isRemainderErrorsTagOnPage &&
                            existingRemainderErrorsTag.ErrorAtemptsSum > 0)
                        {
                            ParentPage.AddTag(existingRemainderErrorsTag);
                        }
                    }
                }

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

        public override void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false) { }

        public override void OnResized(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            base.OnResized(oldWidth, oldHeight, fromHistory);

            SizeArrayToGridLevel(GridSquareSize);
            OnResizing(oldWidth, oldHeight);
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
            base.OnResized(oldX, oldY, fromHistory);

            OnMoving(oldX, oldY, fromHistory);
        }

        public override IPageObject Duplicate()
        {
            var newCLPArray = Clone() as CLPArray;
            if (newCLPArray == null)
            {
                return null;
            }
            newCLPArray.CreationDate = DateTime.Now;
            newCLPArray.ID = Guid.NewGuid().ToCompactID();
            newCLPArray.VersionIndex = 0;
            newCLPArray.LastVersionIndex = null;
            newCLPArray.ParentPage = ParentPage;

            return newCLPArray;
        }

        #endregion //APageObjectBase Overrides

        #region ACLPArrayBase Overrides

        public override void RotateArray()
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

            if (ArrayType == ArrayTypes.ObscurableArray)
            {
                IsSideLabelVisible = !IsRowsObscured;
                IsTopLabelVisible = !IsColumnsObscured;
            }

            //RefreshStrokeParentIDs();
            OnResized(initialWidth, initialHeight);
        } 

        #endregion //ACLPArrayBase Overrides

        #region ICountable Implementation

        /// <summary>Number of Parts the <see cref="ICountable" /> represents.</summary>
        public int Parts
        {
            get { return Rows * Columns; }
            set { }
        }

        /// <summary>Signifies the <see cref="ICountable" /> has been accepted by another <see cref="ICountable" />.</summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof (bool), false);

        /// <summary>Parts is Auto-Generated and non-modifiable (except under special circumstances).</summary>
        public bool IsPartsAutoGenerated
        {
            get { return GetValue<bool>(IsPartsAutoGeneratedProperty); }
            set { SetValue(IsPartsAutoGeneratedProperty, value); }
        }

        public static readonly PropertyData IsPartsAutoGeneratedProperty = RegisterProperty("IsPartsAutoGenerated", typeof (bool), true);

        #endregion //ICountable Implementation

        #region ICuttable Implementation

        public double CuttingStrokeDistance(Stroke cuttingStroke)
        {
            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = YPosition;
            var cuttableBottom = YPosition + Height;
            var cuttableLeft = XPosition;
            var cuttableRight = XPosition + Width;

            const double MIN_THRESHHOLD = 5.0;

            if (Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
                strokeRight <= cuttableRight &&
                strokeLeft >= cuttableLeft &&
                strokeTop - cuttableTop <= MIN_THRESHHOLD &&
                cuttableBottom - strokeBottom <= MIN_THRESHHOLD) //Vertical Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var verticalStrokeMidpoint = strokeTop + ((strokeBottom - strokeTop) / 2);
                var verticalPageObjectMidpoint = cuttableTop + ((cuttableBottom - cuttableTop) / 2);
                return Math.Abs(verticalPageObjectMidpoint - verticalStrokeMidpoint);
            }

            if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                strokeBottom <= cuttableBottom &&
                strokeTop >= cuttableTop &&
                cuttableRight - strokeRight <= MIN_THRESHHOLD &&
                strokeLeft - cuttableLeft <= MIN_THRESHHOLD) //Horizontal Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var horizontalStrokeMidpoint = strokeLeft + ((strokeRight - strokeLeft) / 2);
                var horizontalPageObjectMidpoint = cuttableLeft + ((cuttableRight - cuttableLeft) / 2);
                return Math.Abs(horizontalPageObjectMidpoint - horizontalStrokeMidpoint);
            }

            return -1.0;
        }

        public List<IPageObject> Cut(Stroke cuttingStroke)
        {
            var halvedPageObjects = new List<IPageObject>();

            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = YPosition + LabelLength;
            var cuttableBottom = cuttableTop + ArrayHeight;
            var cuttableLeft = XPosition + LabelLength;
            var cuttableRight = cuttableLeft + ArrayWidth;

            const double MIN_THRESHHOLD = 5.0;

            if (Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
                strokeRight <= cuttableRight &&
                strokeLeft >= cuttableLeft &&
                strokeTop - cuttableTop <= MIN_THRESHHOLD &&
                cuttableBottom - strokeBottom <= MIN_THRESHHOLD &&
                Columns > 1) //Vertical Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeRight + strokeLeft) / 2;
                var relativeAverage = average - LabelLength - XPosition;
                var closestColumn = Convert.ToInt32(Math.Round(relativeAverage / GridSquareSize));

                if (closestColumn == Columns ||
                    closestColumn == 0)
                {
                    return halvedPageObjects;
                }

                switch (ArrayType)
                {
                    case ArrayTypes.ArrayCard:
                    case ArrayTypes.FactorCard:
                    case ArrayTypes.TenByTen:
                    case ArrayTypes.Array:
                    {
                        var leftArray = new CLPArray(ParentPage, closestColumn, Rows, ArrayTypes.Array)
                                        {
                                            IsGridOn = IsGridOn,
                                            IsDivisionBehaviorOn = false,
                                            XPosition = XPosition,
                                            YPosition = YPosition,
                                            IsTopLabelVisible = IsTopLabelVisible,
                                            IsSideLabelVisible = IsSideLabelVisible,
                                            IsSnappable = IsSnappable
                                        };
                        leftArray.SizeArrayToGridLevel(GridSquareSize);
                        halvedPageObjects.Add(leftArray);

                        var rightArray = new CLPArray(ParentPage, Columns - closestColumn, Rows, ArrayTypes.Array)
                                         {
                                             IsGridOn = IsGridOn,
                                             IsDivisionBehaviorOn = false,
                                             XPosition = XPosition + leftArray.ArrayWidth,
                                             YPosition = YPosition,
                                             IsTopLabelVisible = IsTopLabelVisible,
                                             IsSideLabelVisible = IsSideLabelVisible,
                                             IsSnappable = IsSnappable
                                         };
                        rightArray.SizeArrayToGridLevel(GridSquareSize);
                        halvedPageObjects.Add(rightArray);
                    }
                        break;
                    case ArrayTypes.ObscurableArray:
                    {
                        SortDivisions();
                        if (IsColumnsObscured)
                        {
                            var cutDividerRegion = VerticalDivisions.FirstOrDefault(d => d.Position < relativeAverage && d.Position + d.Length > relativeAverage);
                            if (cutDividerRegion == null)
                            {
                                return halvedPageObjects;
                            }
                            var leftDividerRegions = VerticalDivisions.TakeWhile(d => d != cutDividerRegion).ToList();
                            var rightDividerRegions = VerticalDivisions.Reverse().TakeWhile(d => d != cutDividerRegion).ToList();
                            var isCutOnLeftSideOfDividerRegion = cutDividerRegion.Position < relativeAverage &&
                                                                 cutDividerRegion.Position + (cutDividerRegion.Length / 2) > relativeAverage;

                            if (isCutOnLeftSideOfDividerRegion)
                            {
                                if (cutDividerRegion.IsObscured)
                                {
                                    if (leftDividerRegions.Any())
                                    {
                                        var leftColumns = leftDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var leftArray = new CLPArray(ParentPage, leftColumns, Rows, ArrayTypes.ObscurableArray)
                                                        {
                                                            IsGridOn = IsGridOn,
                                                            IsDivisionBehaviorOn = false,
                                                            XPosition = Math.Max(0, XPosition),
                                                            YPosition = YPosition,
                                                            IsSideLabelVisible = true,
                                                            IsSnappable = IsSnappable,
                                                            VerticalDivisions = new ObservableCollection<CLPArrayDivision>(leftDividerRegions)
                                                        };
                                        leftArray.IsTopLabelVisible = !leftArray.IsColumnsObscured;
                                        leftArray.SizeArrayToGridLevel(GridSquareSize);
                                        if (leftArray.VerticalDivisions.Count == 1 &&
                                            !leftArray.IsColumnsObscured)
                                        {
                                            leftArray.VerticalDivisions.Clear();
                                        }
                                        halvedPageObjects.Add(leftArray);

                                        rightDividerRegions.Insert(0, cutDividerRegion);
                                        foreach (var dividerRegion in rightDividerRegions)
                                        {
                                            dividerRegion.Position -= leftArray.ArrayWidth;
                                        }
                                        var rightColumns = rightDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var rightArray = new CLPArray(ParentPage, rightColumns, Rows, ArrayTypes.ObscurableArray)
                                                         {
                                                             IsGridOn = IsGridOn,
                                                             IsDivisionBehaviorOn = false,
                                                             XPosition =
                                                                 Math.Min(ParentPage.Width - (2 * LabelLength) - (rightColumns * GridSquareSize),
                                                                          leftArray.XPosition + leftArray.ArrayWidth),
                                                             YPosition = YPosition,
                                                             IsSideLabelVisible = true,
                                                             VerticalDivisions = new ObservableCollection<CLPArrayDivision>(rightDividerRegions)
                                                         };
                                        rightArray.IsTopLabelVisible = !rightArray.IsColumnsObscured;
                                        rightArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(rightArray);
                                    }
                                    else
                                    {
                                        var leftSingleColumnArray = new CLPArray(ParentPage, 1, Rows, ArrayTypes.ObscurableArray)
                                                                    {
                                                                        IsGridOn = IsGridOn,
                                                                        IsDivisionBehaviorOn = false,
                                                                        XPosition = Math.Max(0, XPosition - GridSquareSize),
                                                                        YPosition = YPosition,
                                                                        IsTopLabelVisible = true,
                                                                        IsSideLabelVisible = true,
                                                                        IsSnappable = IsSnappable
                                                                    };
                                        leftSingleColumnArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(leftSingleColumnArray);
                                        halvedPageObjects.Add(this);
                                    }
                                }
                                else
                                {
                                    var cutDividerRegionLeftColumn = leftDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                    var additionalLeftColumns = closestColumn - cutDividerRegionLeftColumn;
                                    var additionalRightColumns = cutDividerRegion.Value - additionalLeftColumns;

                                    var leftColumns = leftDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalLeftColumns;
                                    var lastDiv = leftDividerRegions.LastOrDefault();
                                    var newLastDivPosition = lastDiv == null ? 0.0 : lastDiv.Position + lastDiv.Length;
                                    var rightDividerRegionOffset = (additionalRightColumns * GridSquareSize) - cutDividerRegion.Length -
                                                                   leftDividerRegions.Aggregate(0.0, (total, d) => total + d.Length);

                                    var newLastDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                          newLastDivPosition,
                                                                          additionalLeftColumns * GridSquareSize,
                                                                          additionalLeftColumns);
                                    leftDividerRegions.Add(newLastDiv);
                                    var leftArray = new CLPArray(ParentPage, leftColumns, Rows, ArrayTypes.ObscurableArray)
                                                    {
                                                        IsGridOn = IsGridOn,
                                                        IsDivisionBehaviorOn = false,
                                                        XPosition = Math.Max(0, XPosition),
                                                        YPosition = YPosition,
                                                        IsSideLabelVisible = true,
                                                        IsSnappable = IsSnappable,
                                                        VerticalDivisions = new ObservableCollection<CLPArrayDivision>(leftDividerRegions)
                                                    };
                                    leftArray.IsTopLabelVisible = !leftArray.IsColumnsObscured;
                                    leftArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (leftArray.VerticalDivisions.Count == 1 &&
                                        !leftArray.IsColumnsObscured)
                                    {
                                        leftArray.VerticalDivisions.Clear();
                                    }
                                    leftArray.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(leftArray.VerticalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(leftArray);

                                    var rightColumns = rightDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalRightColumns;
                                    foreach (var dividerRegion in rightDividerRegions)
                                    {
                                        dividerRegion.Position += rightDividerRegionOffset;
                                    }
                                    var newFirstDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0.0, additionalRightColumns * GridSquareSize, additionalRightColumns);
                                    rightDividerRegions.Insert(0, newFirstDiv);

                                    var rightArray = new CLPArray(ParentPage, rightColumns, Rows, ArrayTypes.ObscurableArray)
                                                     {
                                                         IsGridOn = IsGridOn,
                                                         IsDivisionBehaviorOn = false,
                                                         XPosition =
                                                             Math.Min(ParentPage.Width - (2 * LabelLength) - (rightColumns * GridSquareSize),
                                                                      leftArray.XPosition + leftArray.ArrayWidth),
                                                         YPosition = YPosition,
                                                         IsSideLabelVisible = true,
                                                         VerticalDivisions = new ObservableCollection<CLPArrayDivision>(rightDividerRegions)
                                                     };
                                    rightArray.IsTopLabelVisible = !rightArray.IsColumnsObscured;
                                    rightArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (rightArray.VerticalDivisions.Count == 1 &&
                                        !rightArray.IsColumnsObscured)
                                    {
                                        rightArray.VerticalDivisions.Clear();
                                    }
                                    rightArray.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(rightArray.VerticalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(rightArray);
                                }
                            }
                            else
                            {
                                if (cutDividerRegion.IsObscured)
                                {
                                    if (rightDividerRegions.Any())
                                    {
                                        leftDividerRegions.Add(cutDividerRegion);
                                        var leftColumns = leftDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var leftArray = new CLPArray(ParentPage, leftColumns, Rows, ArrayTypes.ObscurableArray)
                                                        {
                                                            IsGridOn = IsGridOn,
                                                            IsDivisionBehaviorOn = false,
                                                            XPosition = Math.Max(0, XPosition),
                                                            YPosition = YPosition,
                                                            IsSideLabelVisible = true,
                                                            IsSnappable = IsSnappable,
                                                            VerticalDivisions = new ObservableCollection<CLPArrayDivision>(leftDividerRegions)
                                                        };
                                        leftArray.IsTopLabelVisible = !leftArray.IsColumnsObscured;
                                        leftArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(leftArray);

                                        foreach (var dividerRegion in rightDividerRegions)
                                        {
                                            dividerRegion.Position -= leftArray.ArrayWidth;
                                        }
                                        var rightColumns = rightDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var rightArray = new CLPArray(ParentPage, rightColumns, Rows, ArrayTypes.ObscurableArray)
                                                         {
                                                             IsGridOn = IsGridOn,
                                                             IsDivisionBehaviorOn = false,
                                                             XPosition =
                                                                 Math.Min(ParentPage.Width - (2 * LabelLength) - (rightColumns * GridSquareSize),
                                                                          leftArray.XPosition + leftArray.ArrayWidth),
                                                             YPosition = YPosition,
                                                             IsSideLabelVisible = true,
                                                             VerticalDivisions = new ObservableCollection<CLPArrayDivision>(rightDividerRegions)
                                                         };
                                        rightArray.IsTopLabelVisible = !rightArray.IsColumnsObscured;
                                        rightArray.SizeArrayToGridLevel(GridSquareSize);
                                        if (rightArray.VerticalDivisions.Count == 1 &&
                                            !rightArray.IsColumnsObscured)
                                        {
                                            rightArray.VerticalDivisions.Clear();
                                        }
                                        halvedPageObjects.Add(rightArray);
                                    }
                                    else
                                    {
                                        halvedPageObjects.Add(this);
                                        var rightSingleColumnArray = new CLPArray(ParentPage, 1, Rows, ArrayTypes.ObscurableArray)
                                                                     {
                                                                         IsGridOn = IsGridOn,
                                                                         IsDivisionBehaviorOn = false,
                                                                         XPosition = Math.Min(ParentPage.Width - GridSquareSize - (2 * LabelLength), XPosition + ArrayWidth),
                                                                         YPosition = YPosition,
                                                                         IsTopLabelVisible = true,
                                                                         IsSideLabelVisible = true,
                                                                         IsSnappable = IsSnappable
                                                                     };
                                        rightSingleColumnArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(rightSingleColumnArray);
                                    }
                                }
                                else
                                {
                                    var cutDividerRegionLeftColumn = leftDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                    var additionalLeftColumns = closestColumn - cutDividerRegionLeftColumn;
                                    var additionalRightColumns = cutDividerRegion.Value - additionalLeftColumns;

                                    var leftColumns = leftDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalLeftColumns;
                                    var lastDiv = leftDividerRegions.LastOrDefault();
                                    var newLastDivPosition = lastDiv == null ? 0.0 : lastDiv.Position + lastDiv.Length;
                                    var rightDividerRegionOffset = (additionalRightColumns * GridSquareSize) - cutDividerRegion.Length -
                                                                   leftDividerRegions.Aggregate(0.0, (total, d) => total + d.Length);

                                    var newLastDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                          newLastDivPosition,
                                                                          additionalLeftColumns * GridSquareSize,
                                                                          additionalLeftColumns);
                                    leftDividerRegions.Add(newLastDiv);
                                    var leftArray = new CLPArray(ParentPage, leftColumns, Rows, ArrayTypes.ObscurableArray)
                                                    {
                                                        IsGridOn = IsGridOn,
                                                        IsDivisionBehaviorOn = false,
                                                        XPosition = Math.Max(0, XPosition),
                                                        YPosition = YPosition,
                                                        IsSideLabelVisible = true,
                                                        IsSnappable = IsSnappable,
                                                        VerticalDivisions = new ObservableCollection<CLPArrayDivision>(leftDividerRegions)
                                                    };
                                    leftArray.IsTopLabelVisible = !leftArray.IsColumnsObscured;
                                    leftArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (leftArray.VerticalDivisions.Count == 1 &&
                                        !leftArray.IsColumnsObscured)
                                    {
                                        leftArray.VerticalDivisions.Clear();
                                    }
                                    leftArray.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(leftArray.VerticalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(leftArray);

                                    var rightColumns = rightDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalRightColumns;
                                    foreach (var dividerRegion in rightDividerRegions)
                                    {
                                        dividerRegion.Position += rightDividerRegionOffset;
                                    }
                                    var newFirstDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0.0, additionalRightColumns * GridSquareSize, additionalRightColumns);
                                    rightDividerRegions.Insert(0, newFirstDiv);

                                    var rightArray = new CLPArray(ParentPage, rightColumns, Rows, ArrayTypes.ObscurableArray)
                                                     {
                                                         IsGridOn = IsGridOn,
                                                         IsDivisionBehaviorOn = false,
                                                         XPosition =
                                                             Math.Min(ParentPage.Width - (2 * LabelLength) - (rightColumns * GridSquareSize),
                                                                      leftArray.XPosition + leftArray.ArrayWidth),
                                                         YPosition = YPosition,
                                                         IsSideLabelVisible = true,
                                                         VerticalDivisions = new ObservableCollection<CLPArrayDivision>(rightDividerRegions)
                                                     };
                                    rightArray.IsTopLabelVisible = !rightArray.IsColumnsObscured;
                                    rightArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (rightArray.VerticalDivisions.Count == 1 &&
                                        !rightArray.IsColumnsObscured)
                                    {
                                        rightArray.VerticalDivisions.Clear();
                                    }
                                    rightArray.VerticalDivisions = new ObservableCollection<CLPArrayDivision>(rightArray.VerticalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(rightArray);
                                }
                            }
                        }
                        else
                        {
                            var leftArray = new CLPArray(ParentPage, closestColumn, Rows, ArrayTypes.ObscurableArray)
                                            {
                                                IsGridOn = IsGridOn,
                                                IsDivisionBehaviorOn = false,
                                                XPosition = XPosition,
                                                YPosition = YPosition,
                                                IsTopLabelVisible = IsTopLabelVisible,
                                                IsSideLabelVisible = IsSideLabelVisible,
                                                IsSnappable = IsSnappable,
                                            };
                            if (IsRowsObscured)
                            {
                                leftArray.Obscure(false);
                            }
                            leftArray.SizeArrayToGridLevel(GridSquareSize);
                            halvedPageObjects.Add(leftArray);

                            var rightArray = new CLPArray(ParentPage, Columns - closestColumn, Rows, ArrayTypes.ObscurableArray)
                                             {
                                                 IsGridOn = IsGridOn,
                                                 IsDivisionBehaviorOn = false,
                                                 XPosition = XPosition + leftArray.ArrayWidth,
                                                 YPosition = YPosition,
                                                 IsTopLabelVisible = IsTopLabelVisible,
                                                 IsSideLabelVisible = IsSideLabelVisible,
                                                 IsSnappable = IsSnappable
                                             };
                            if (IsRowsObscured)
                            {
                                rightArray.Obscure(false);
                            }
                            rightArray.SizeArrayToGridLevel(GridSquareSize);
                            halvedPageObjects.Add(rightArray);
                        }
                    }
                        break;
                }
            }
            else if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                     strokeBottom <= cuttableBottom &&
                     strokeTop >= cuttableTop &&
                     cuttableRight - strokeRight <= MIN_THRESHHOLD &&
                     strokeLeft - cuttableLeft <= MIN_THRESHHOLD &&
                     Rows > 1) //Horizontal Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeTop + strokeBottom) / 2;
                var relativeAverage = average - LabelLength - YPosition;
                var closestRow = Convert.ToInt32(Math.Round(relativeAverage / GridSquareSize));

                if (closestRow == Rows ||
                    closestRow == 0)
                {
                    return halvedPageObjects;
                }

                switch (ArrayType)
                {
                    case ArrayTypes.ArrayCard:
                    case ArrayTypes.FactorCard:
                    case ArrayTypes.TenByTen:
                    case ArrayTypes.Array:
                    {
                        var topArray = new CLPArray(ParentPage, Columns, closestRow, ArrayTypes.Array)
                                       {
                                           IsGridOn = IsGridOn,
                                           IsDivisionBehaviorOn = false,
                                           XPosition = XPosition,
                                           YPosition = YPosition,
                                           IsTopLabelVisible = IsTopLabelVisible,
                                           IsSideLabelVisible = IsSideLabelVisible,
                                           IsSnappable = IsSnappable
                                       };
                        topArray.SizeArrayToGridLevel(GridSquareSize);
                        halvedPageObjects.Add(topArray);

                        var bottomArray = new CLPArray(ParentPage, Columns, Rows - closestRow, ArrayTypes.Array)
                                          {
                                              IsGridOn = IsGridOn,
                                              IsDivisionBehaviorOn = false,
                                              XPosition = XPosition,
                                              YPosition = YPosition + topArray.ArrayHeight,
                                              IsTopLabelVisible = IsTopLabelVisible,
                                              IsSideLabelVisible = IsSideLabelVisible,
                                              IsSnappable = IsSnappable
                                          };
                        bottomArray.SizeArrayToGridLevel(GridSquareSize);
                        halvedPageObjects.Add(bottomArray);
                    }
                        break;
                    case ArrayTypes.ObscurableArray:
                    {
                        SortDivisions();
                        if (IsRowsObscured)
                        {
                            var cutDividerRegion = HorizontalDivisions.FirstOrDefault(d => d.Position < relativeAverage && d.Position + d.Length > relativeAverage);
                            if (cutDividerRegion == null)
                            {
                                return halvedPageObjects;
                            }
                            var topDividerRegions = HorizontalDivisions.TakeWhile(d => d != cutDividerRegion).ToList();
                            var bottomDividerRegions = HorizontalDivisions.Reverse().TakeWhile(d => d != cutDividerRegion).ToList();
                            var isCutOnTopHalfOfDividerRegion = cutDividerRegion.Position < relativeAverage &&
                                                                cutDividerRegion.Position + (cutDividerRegion.Length / 2) > relativeAverage;

                            if (isCutOnTopHalfOfDividerRegion)
                            {
                                if (cutDividerRegion.IsObscured)
                                {
                                    if (topDividerRegions.Any())
                                    {
                                        var topRows = topDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var topArray = new CLPArray(ParentPage, Columns, topRows, ArrayTypes.ObscurableArray)
                                                       {
                                                           IsGridOn = IsGridOn,
                                                           IsDivisionBehaviorOn = false,
                                                           XPosition = XPosition,
                                                           YPosition = Math.Max(0, YPosition),
                                                           IsTopLabelVisible = true,
                                                           IsSnappable = IsSnappable,
                                                           HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(topDividerRegions)
                                                       };
                                        topArray.IsSideLabelVisible = !topArray.IsRowsObscured;
                                        topArray.SizeArrayToGridLevel(GridSquareSize);
                                        if (topArray.HorizontalDivisions.Count == 1 &&
                                            !topArray.IsRowsObscured)
                                        {
                                            topArray.HorizontalDivisions.Clear();
                                        }
                                        halvedPageObjects.Add(topArray);

                                        bottomDividerRegions.Insert(0, cutDividerRegion);
                                        foreach (var dividerRegion in bottomDividerRegions)
                                        {
                                            dividerRegion.Position -= topArray.ArrayHeight;
                                        }
                                        var bottomRows = bottomDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var rightArray = new CLPArray(ParentPage, Columns, bottomRows, ArrayTypes.ObscurableArray)
                                                         {
                                                             IsGridOn = IsGridOn,
                                                             IsDivisionBehaviorOn = false,
                                                             XPosition = XPosition,
                                                             YPosition =
                                                                 Math.Min(ParentPage.Height - (2 * LabelLength) - (bottomRows * GridSquareSize),
                                                                          topArray.YPosition + topArray.ArrayHeight),
                                                             IsTopLabelVisible = true,
                                                             HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(bottomDividerRegions)
                                                         };
                                        rightArray.IsSideLabelVisible = !rightArray.IsRowsObscured;
                                        rightArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(rightArray);
                                    }
                                    else
                                    {
                                        var topSingleRowArray = new CLPArray(ParentPage, Columns, 1, ArrayTypes.ObscurableArray)
                                                                {
                                                                    IsGridOn = IsGridOn,
                                                                    IsDivisionBehaviorOn = false,
                                                                    XPosition = XPosition,
                                                                    YPosition = Math.Max(0, YPosition - GridSquareSize),
                                                                    IsTopLabelVisible = true,
                                                                    IsSideLabelVisible = true,
                                                                    IsSnappable = IsSnappable
                                                                };
                                        topSingleRowArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(topSingleRowArray);
                                        halvedPageObjects.Add(this);
                                    }
                                }
                                else
                                {
                                    var cutDividerRegionTopRow = topDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                    var additionalTopRows = closestRow - cutDividerRegionTopRow;
                                    var additionalBottomRows = cutDividerRegion.Value - additionalTopRows;

                                    var topRows = topDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalTopRows;
                                    var lastDiv = topDividerRegions.LastOrDefault();
                                    var newLastDivPosition = lastDiv == null ? 0.0 : lastDiv.Position + lastDiv.Length;
                                    var topDividerRegionOffset = (additionalBottomRows * GridSquareSize) - cutDividerRegion.Length -
                                                                 topDividerRegions.Aggregate(0.0, (total, d) => total + d.Length);

                                    var newLastDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                          newLastDivPosition,
                                                                          additionalTopRows * GridSquareSize,
                                                                          additionalTopRows);
                                    topDividerRegions.Add(newLastDiv);
                                    var topArray = new CLPArray(ParentPage, Columns, topRows, ArrayTypes.ObscurableArray)
                                                   {
                                                       IsGridOn = IsGridOn,
                                                       IsDivisionBehaviorOn = false,
                                                       XPosition = XPosition,
                                                       YPosition = Math.Max(0, YPosition),
                                                       IsTopLabelVisible = true,
                                                       IsSnappable = IsSnappable,
                                                       HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(topDividerRegions)
                                                   };
                                    topArray.IsSideLabelVisible = !topArray.IsRowsObscured;
                                    topArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (topArray.HorizontalDivisions.Count == 1 &&
                                        !topArray.IsRowsObscured)
                                    {
                                        topArray.HorizontalDivisions.Clear();
                                    }
                                    topArray.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(topArray.HorizontalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(topArray);

                                    var bottomRows = bottomDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalBottomRows;
                                    foreach (var dividerRegion in bottomDividerRegions)
                                    {
                                        dividerRegion.Position += topDividerRegionOffset;
                                    }
                                    var newFirstDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0.0, additionalBottomRows * GridSquareSize, additionalBottomRows);
                                    bottomDividerRegions.Insert(0, newFirstDiv);

                                    var rightArray = new CLPArray(ParentPage, Columns, bottomRows, ArrayTypes.ObscurableArray)
                                                     {
                                                         IsGridOn = IsGridOn,
                                                         IsDivisionBehaviorOn = false,
                                                         XPosition = XPosition,
                                                         YPosition =
                                                             Math.Min(ParentPage.Height - (2 * LabelLength) - (bottomRows * GridSquareSize),
                                                                      topArray.YPosition + topArray.ArrayHeight),
                                                         IsTopLabelVisible = true,
                                                         HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(bottomDividerRegions)
                                                     };
                                    rightArray.IsSideLabelVisible = !rightArray.IsRowsObscured;
                                    rightArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (rightArray.HorizontalDivisions.Count == 1 &&
                                        !rightArray.IsRowsObscured)
                                    {
                                        rightArray.HorizontalDivisions.Clear();
                                    }
                                    rightArray.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(rightArray.HorizontalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(rightArray);
                                }
                            }
                            else
                            {
                                if (cutDividerRegion.IsObscured)
                                {
                                    if (bottomDividerRegions.Any())
                                    {
                                        topDividerRegions.Add(cutDividerRegion);
                                        var topRows = topDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var topArray = new CLPArray(ParentPage, Columns, topRows, ArrayTypes.ObscurableArray)
                                                       {
                                                           IsGridOn = IsGridOn,
                                                           IsDivisionBehaviorOn = false,
                                                           XPosition = XPosition,
                                                           YPosition = Math.Max(0, YPosition),
                                                           IsTopLabelVisible = true,
                                                           IsSnappable = IsSnappable,
                                                           HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(topDividerRegions)
                                                       };
                                        topArray.IsSideLabelVisible = !topArray.IsColumnsObscured;
                                        topArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(topArray);

                                        foreach (var dividerRegion in bottomDividerRegions)
                                        {
                                            dividerRegion.Position -= topArray.ArrayHeight;
                                        }
                                        var bottomRows = bottomDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                        var bottomArray = new CLPArray(ParentPage, bottomRows, Rows, ArrayTypes.ObscurableArray)
                                                          {
                                                              IsGridOn = IsGridOn,
                                                              IsDivisionBehaviorOn = false,
                                                              XPosition = XPosition,
                                                              YPosition =
                                                                  Math.Min(ParentPage.Height - (2 * LabelLength) - (bottomRows * GridSquareSize),
                                                                           topArray.YPosition + topArray.ArrayHeight),
                                                              IsTopLabelVisible = true,
                                                              HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(bottomDividerRegions)
                                                          };
                                        bottomArray.IsSideLabelVisible = !bottomArray.IsRowsObscured;
                                        bottomArray.SizeArrayToGridLevel(GridSquareSize);
                                        if (bottomArray.HorizontalDivisions.Count == 1 &&
                                            !bottomArray.IsRowsObscured)
                                        {
                                            bottomArray.HorizontalDivisions.Clear();
                                        }
                                        halvedPageObjects.Add(bottomArray);
                                    }
                                    else
                                    {
                                        halvedPageObjects.Add(this);
                                        var bottomSingleRowArray = new CLPArray(ParentPage, Columns, 1, ArrayTypes.ObscurableArray)
                                                                   {
                                                                       IsGridOn = IsGridOn,
                                                                       IsDivisionBehaviorOn = false,
                                                                       XPosition = XPosition,
                                                                       YPosition = Math.Min(ParentPage.Height - GridSquareSize - (2 * LabelLength), YPosition + ArrayHeight),
                                                                       IsTopLabelVisible = true,
                                                                       IsSideLabelVisible = true,
                                                                       IsSnappable = IsSnappable
                                                                   };
                                        bottomSingleRowArray.SizeArrayToGridLevel(GridSquareSize);
                                        halvedPageObjects.Add(bottomSingleRowArray);
                                    }
                                }
                                else
                                {
                                    var cutDividerRegionTopRow = topDividerRegions.Aggregate(0, (total, d) => d.Value + total);
                                    var additionalTopRows = closestRow - cutDividerRegionTopRow;
                                    var additionalBottomRows = cutDividerRegion.Value - additionalTopRows;

                                    var topRows = topDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalTopRows;
                                    var lastDiv = topDividerRegions.LastOrDefault();
                                    var newLastDivPosition = lastDiv == null ? 0.0 : lastDiv.Position + lastDiv.Length;
                                    var topDividerRegionOffset = (additionalBottomRows * GridSquareSize) - cutDividerRegion.Length -
                                                                 topDividerRegions.Aggregate(0.0, (total, d) => total + d.Length);

                                    var newLastDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                          newLastDivPosition,
                                                                          additionalTopRows * GridSquareSize,
                                                                          additionalTopRows);
                                    topDividerRegions.Add(newLastDiv);
                                    var topArray = new CLPArray(ParentPage, Columns, topRows, ArrayTypes.ObscurableArray)
                                                   {
                                                       IsGridOn = IsGridOn,
                                                       IsDivisionBehaviorOn = false,
                                                       XPosition = XPosition,
                                                       YPosition = Math.Max(0, YPosition),
                                                       IsTopLabelVisible = true,
                                                       IsSnappable = IsSnappable,
                                                       HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(topDividerRegions)
                                                   };
                                    topArray.IsSideLabelVisible = !topArray.IsRowsObscured;
                                    topArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (topArray.HorizontalDivisions.Count == 1 &&
                                        !topArray.IsRowsObscured)
                                    {
                                        topArray.HorizontalDivisions.Clear();
                                    }
                                    topArray.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(topArray.HorizontalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(topArray);

                                    var bottomRows = bottomDividerRegions.Aggregate(0, (total, d) => d.Value + total) + additionalBottomRows;
                                    foreach (var dividerRegion in bottomDividerRegions)
                                    {
                                        dividerRegion.Position += topDividerRegionOffset;
                                    }
                                    var newFirstDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0.0, additionalBottomRows * GridSquareSize, additionalBottomRows);
                                    bottomDividerRegions.Insert(0, newFirstDiv);

                                    var rightArray = new CLPArray(ParentPage, Columns, bottomRows, ArrayTypes.ObscurableArray)
                                                     {
                                                         IsGridOn = IsGridOn,
                                                         IsDivisionBehaviorOn = false,
                                                         XPosition = XPosition,
                                                         YPosition =
                                                             Math.Min(ParentPage.Height - (2 * LabelLength) - (bottomRows * GridSquareSize),
                                                                      topArray.YPosition + topArray.ArrayHeight),
                                                         IsTopLabelVisible = true,
                                                         HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(bottomDividerRegions)
                                                     };
                                    rightArray.IsSideLabelVisible = !rightArray.IsRowsObscured;
                                    rightArray.SizeArrayToGridLevel(GridSquareSize);
                                    if (rightArray.HorizontalDivisions.Count == 1 &&
                                        !rightArray.IsRowsObscured)
                                    {
                                        rightArray.HorizontalDivisions.Clear();
                                    }
                                    rightArray.HorizontalDivisions = new ObservableCollection<CLPArrayDivision>(rightArray.HorizontalDivisions.Where(d => d.Length > 0));
                                    halvedPageObjects.Add(rightArray);
                                }
                            }
                        }
                        else
                        {
                            var topArray = new CLPArray(ParentPage, Columns, closestRow, ArrayTypes.ObscurableArray)
                                           {
                                               IsGridOn = IsGridOn,
                                               IsDivisionBehaviorOn = false,
                                               XPosition = XPosition,
                                               YPosition = YPosition,
                                               IsTopLabelVisible = IsTopLabelVisible,
                                               IsSideLabelVisible = IsSideLabelVisible,
                                               IsSnappable = IsSnappable,
                                           };
                            if (IsColumnsObscured)
                            {
                                topArray.Obscure(true);
                            }
                            topArray.SizeArrayToGridLevel(GridSquareSize);
                            halvedPageObjects.Add(topArray);

                            var bottomArray = new CLPArray(ParentPage, Columns, Rows - closestRow, ArrayTypes.ObscurableArray)
                                              {
                                                  IsGridOn = IsGridOn,
                                                  IsDivisionBehaviorOn = false,
                                                  XPosition = XPosition,
                                                  YPosition = YPosition + topArray.ArrayHeight,
                                                  IsTopLabelVisible = IsTopLabelVisible,
                                                  IsSideLabelVisible = IsSideLabelVisible,
                                                  IsSnappable = IsSnappable
                                              };
                            if (IsColumnsObscured)
                            {
                                bottomArray.Obscure(true);
                            }
                            bottomArray.SizeArrayToGridLevel(GridSquareSize);
                            halvedPageObjects.Add(bottomArray);
                        }
                    }
                        break;
                }
            }

            return halvedPageObjects;
        }

        #endregion //ICuttable Implementation

        #region IStrokeAccepter Implementation

        /// <summary>Stroke must be at least this percent contained by StrokeAcceptanceBoundingBox.</summary>
        public int StrokeHitTestPercentage
        {
            get { return 90; }
        }

        public Rect StrokeAcceptanceBoundingBox
        {
            get { return new Rect(XPosition, YPosition, Width, Height); }
        }

        /// <summary>Determines whether the <see cref="IStrokeAccepter" /> can currently accept <see cref="Stroke" />s.</summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), true);

        /// <summary>The currently accepted <see cref="Stroke" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<Stroke> AcceptedStrokes
        {
            get { return GetValue<List<Stroke>>(AcceptedStrokesProperty); }
            set { SetValue(AcceptedStrokesProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokesProperty = RegisterProperty("AcceptedStrokes", typeof(List<Stroke>), () => new List<Stroke>());

        /// <summary>The IDs of the <see cref="Stroke" />s that have been accepted.</summary>
        public List<string> AcceptedStrokeParentIDs
        {
            get { return GetValue<List<string>>(AcceptedStrokeParentIDsProperty); }
            set { SetValue(AcceptedStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedStrokeParentIDsProperty = RegisterProperty("AcceptedStrokeParentIDs", typeof(List<string>), () => new List<string>());

        public void LoadAcceptedStrokes()
        {
            if (AcceptedStrokeParentIDs == null)
            {
                AcceptedStrokeParentIDs = new List<string>();
            }

            if (!AcceptedStrokeParentIDs.Any())
            {
                return;
            }

            AcceptedStrokes = AcceptedStrokeParentIDs.Select(id => ParentPage.GetStrokeByIDOnPageOrInHistory(id)).Where(s => s != null).ToList();
        }

        public void ChangeAcceptedStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes)
        {
            if (!CanAcceptStrokes)
            {
                return;
            }

            // Remove Strokes
            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            foreach (var stroke in removedStrokesList.Where(stroke => AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Remove(stroke);
                AcceptedStrokeParentIDs.Remove(stroke.GetStrokeID());
            }

            // Add Strokes
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();
            foreach (var stroke in addedStrokesList.Where(stroke => !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
            }
        }

        public bool IsStrokeOverPageObject(Stroke stroke) { return stroke.HitTest(StrokeAcceptanceBoundingBox, StrokeHitTestPercentage); }

        public double PercentageOfStrokeOverPageObject(Stroke stroke) { return stroke.PercentContainedByBounds(StrokeAcceptanceBoundingBox); }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where IsStrokeOverPageObject(stroke)
                                    select stroke;

            return new StrokeCollection(strokesOverObject);
        }

        #endregion //IStrokeAccepter Implementation
    }
}