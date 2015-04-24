using System;
using System.Collections.Generic;
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
        TenByTen
    }

    [Serializable]
    public class CLPArray : ACLPArrayBase, ICountable, ICuttable, IStrokeAccepter
    {
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
            IsGridOn = rows < 26 && columns < 26;
            ArrayType = arrayType;
        }

        public CLPArray(CLPPage parentPage, double gridSize, int columns, int rows, ArrayTypes arrayType)
            : base(parentPage, columns, rows)
        {
            IsGridOn = rows < 26 && columns < 26;
            ArrayType = arrayType;
            Width = gridSize * columns + 2 * LabelLength;
            Height = gridSize * rows + 2 * LabelLength;
        }

        /// <summary>Initializes <see cref="CLPArray" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        private const double MIN_ARRAY_LENGTH = 25.0;

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

        #endregion //Methods

        #region APageObjectBase Overrides

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
            if (!fromHistory)
            {
                foreach (var divisionTemplate in ParentPage.PageObjects.OfType<FuzzyFactorCard>().ToList())
                {
                    divisionTemplate.UpdateRemainderRegion();

                    if (ParentPage.IsTagAddPrevented)
                    {
                        continue;
                    }

                    var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(ParentPage);

                    if (divisionTemplate.CurrentRemainder != divisionTemplate.Dividend % divisionTemplate.Rows)
                    {
                        var existingFactorPairErrorsTag = ParentPage.Tags.OfType<DivisionTemplateFactorPairErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
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
                        var existingRemainderErrorsTag = ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
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

            foreach (var divisionTemplate in ParentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                divisionTemplate.UpdateRemainderRegion();
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
            // If FFC with remainder on page, update
            foreach (var divisionTemplate in ParentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                divisionTemplate.UpdateRemainderRegion();
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

        public override void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false) { }

        public override void OnResized(double oldWidth, double oldHeight, bool fromHistory = false)
        {
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

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false) { OnMoving(oldX, oldY, fromHistory); }

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

            if (ArrayType != ArrayTypes.Array)
            {
                return halvedPageObjects;
            }

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

                if (closestColumn == Columns)
                {
                    return halvedPageObjects;
                }

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

                if (closestRow == Rows)
                {
                    return halvedPageObjects;
                }

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

            return halvedPageObjects;
        }

        #endregion //ICuttable Implementation

        #region IStrokeAccepter Implementation

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public int StrokeHitTestPercentage
        {
            get { return 90; }
        }

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
            foreach (var stroke in addedStrokesList.Where(stroke => IsStrokeOverPageObject(stroke) && !AcceptedStrokeParentIDs.Contains(stroke.GetStrokeID())))
            {
                AcceptedStrokes.Add(stroke);
                AcceptedStrokeParentIDs.Add(stroke.GetStrokeID());
            }
        }

        public bool IsStrokeOverPageObject(Stroke stroke)
        {
            var arrayBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            return stroke.HitTest(arrayBoundingBox, StrokeHitTestPercentage);
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var arrayBoundingBox = new Rect(XPosition, YPosition, Width, Height);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(arrayBoundingBox, StrokeHitTestPercentage)
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

            ChangeAcceptedStrokes(strokesOverObject, new StrokeCollection());
        }

        #endregion //IStrokeAccepter Implementation
    }
}