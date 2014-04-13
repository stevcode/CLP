using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    public enum ArrayTypes
    {
        Array,
        ArrayCard,
        FactorCard
    }

    public class CLPArray : ACLPArrayBase, ICountable, ICuttable
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPArray" /> from scratch.
        /// </summary>
        public CLPArray() { }

        /// <summary>
        /// Initializes <see cref="CLPArray" /> from
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="CLPArray" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="ACLPArrayBase" />.</param>
        /// <param name="rows">The number of rows in the <see cref="ACLPArrayBase" />.</param>
        public CLPArray(CLPPage parentPage, int columns, int rows, ArrayTypes arrayType)
            : base(parentPage, columns, rows)
        {
            IsGridOn = rows < 50 && columns < 50;
            ArrayType = arrayType;
        }

        /// <summary>
        /// Initializes <see cref="CLPArray" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        private const double MIN_ARRAY_LENGTH = 25.0;

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

        public override double MinimumGridSquareSize
        {
            get { return Columns < Rows ? MIN_ARRAY_LENGTH / Columns : MIN_ARRAY_LENGTH / Rows; }
        }

        /// <summary>
        /// The type of <see cref="CLPArray" />.
        /// </summary>
        public ArrayTypes ArrayType
        {
            get { return GetValue<ArrayTypes>(ArrayTypeProperty); }
            set { SetValue(ArrayTypeProperty, value); }
        }

        public static readonly PropertyData ArrayTypeProperty = RegisterProperty("ArrayType", typeof(ArrayTypes), ArrayTypes.Array);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newCLPArray = Clone() as CLPArray;
            if(newCLPArray == null)
            {
                return null;
            }
            newCLPArray.CreationDate = DateTime.Now;
            newCLPArray.ID = Guid.NewGuid().ToString();
            newCLPArray.VersionIndex = 0;
            newCLPArray.LastVersionIndex = null;
            newCLPArray.ParentPage = ParentPage;

            return newCLPArray;
        }

        #region Overrides of APageObjectBase

        public override void OnAdded()
        {
            base.OnAdded();
            // If FFC with remainder on page, update
            foreach(FuzzyFactorCard ffc in ParentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                ffc.AnalyzeArrays();
                ffc.UpdateRemainderRegion();
            }
        }

        public override void OnDeleted()
        {
            base.OnDeleted();
            // If FFC with remainder on page, update
            foreach(FuzzyFactorCard ffc in ParentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                ffc.AnalyzeArrays();
                ffc.UpdateRemainderRegion();
            }
        }

        #endregion //Overrides of APageObjectBase

        public override void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            var initialSquareSize = 45.0;
            if(toSquareSize <= 0)
            {
                while(XPosition + 2 * LabelLength + initialSquareSize * Columns >= ParentPage.Width ||
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

            if(recalculateDivisions)
            {
                ResizeDivisions();
            }
        }

        public int[,] GetPartialProducts()
        {
            var horizDivs = Math.Max(HorizontalDivisions.Count, 1);
            var vertDivs = Math.Max(VerticalDivisions.Count, 1);
            var partialProducts = new int[horizDivs, vertDivs];

            for(var i = 0; i < horizDivs; i++)
            {
                for(var j = 0; j < vertDivs; j++)
                {
                    var yAxisValue = (horizDivs > 1 ? HorizontalDivisions[i].Value : Rows);
                    var xAxisValue = (vertDivs > 1 ? VerticalDivisions[j].Value : Columns);

                    partialProducts[i, j] = yAxisValue * xAxisValue;
                }
            }

            return partialProducts;
        }

        #endregion //Methods

        #region Implementation of ICountable

        /// <summary>
        /// Number of Parts the <see cref="ICountable" /> represents.
        /// </summary>
        public int Parts
        {
            get { return Rows * Columns; }
            set { }
        }

        /// <summary>
        /// Signifies the <see cref="ICountable" /> has been accepted by another <see cref="ICountable" />.
        /// </summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof(bool), false);

        #endregion

        #region Implementation of ICuttable

        public List<IPageObject> Cut(Stroke cuttingStroke)
        {
            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = YPosition + LabelLength;
            var cuttableBottom = cuttableTop + ArrayHeight;
            var cuttableLeft = XPosition + LabelLength;
            var cuttableRight = cuttableLeft + ArrayWidth;

            var halvedPageObjects = new List<IPageObject>();

            if(ArrayType != ArrayTypes.Array)
            {
                return halvedPageObjects;
            }

            const double MIN_THRESHHOLD = 5.0;

            if(Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
               strokeRight <= cuttableRight &&
               strokeLeft >= cuttableLeft &&
               strokeTop - cuttableTop <= MIN_THRESHHOLD &&
               cuttableBottom - strokeBottom <= MIN_THRESHHOLD &&
               Columns > 1) //Vertical Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeRight + strokeLeft) / 2;
                var relativeAverage = average - LabelLength - XPosition;
                var closestColumn = Convert.ToInt32(Math.Round(relativeAverage / GridSquareSize));

                var leftArray = new CLPArray(ParentPage, closestColumn, Rows, ArrayTypes.Array)
                                {
                                    IsGridOn = IsGridOn,
                                    IsDivisionBehaviorOn = IsDivisionBehaviorOn,
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
                                     IsDivisionBehaviorOn = IsDivisionBehaviorOn,
                                     XPosition = XPosition,
                                     YPosition = YPosition,
                                     IsTopLabelVisible = IsTopLabelVisible,
                                     IsSideLabelVisible = IsSideLabelVisible,
                                     IsSnappable = IsSnappable
                                 };
                rightArray.SizeArrayToGridLevel(GridSquareSize);
                halvedPageObjects.Add(rightArray);
            }
            else if(Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                    strokeBottom <= cuttableBottom &&
                    strokeTop >= cuttableTop &&
                    strokeRight - cuttableRight <= MIN_THRESHHOLD &&
                    cuttableLeft - strokeLeft <= MIN_THRESHHOLD &&
                    Rows > 1) //Horizontal Cut Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeRight + strokeLeft) / 2;
                var relativeAverage = average - LabelLength - XPosition;
                var closestRow = Convert.ToInt32(Math.Round(relativeAverage / GridSquareSize));

                var topArray = new CLPArray(ParentPage, Columns, closestRow, ArrayTypes.Array)
                               {
                                   IsGridOn = IsGridOn,
                                   IsDivisionBehaviorOn = IsDivisionBehaviorOn,
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
                                      IsDivisionBehaviorOn = IsDivisionBehaviorOn,
                                      XPosition = XPosition,
                                      YPosition = YPosition,
                                      IsTopLabelVisible = IsTopLabelVisible,
                                      IsSideLabelVisible = IsSideLabelVisible,
                                      IsSnappable = IsSnappable
                                  };
                bottomArray.SizeArrayToGridLevel(GridSquareSize);
                halvedPageObjects.Add(bottomArray);
            }

            return halvedPageObjects;
        }

        #endregion
    }
}