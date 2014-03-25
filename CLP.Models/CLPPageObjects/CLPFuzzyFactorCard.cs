﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPFuzzyFactorCard : CLPArray
    {

        public double LabelLength { get { return 35; } }
        public double LargeLabelLength { get { return LabelLength * 2 + 12.0; } }
        //public double LargeLabelLength { get { return LabelLength + 10.0; } }

        #region Constructors

        public CLPFuzzyFactorCard(int rows, int columns, int dividend, ICLPPage page, bool displayRemainderRegion=false)
            : base(rows, columns, page)
        {
            Dividend = dividend;
            IsGridOn = rows < 45 && columns < 45;
            IsAnswerVisible = true;
            IsArrayDivisionLabelOnTop = true;

            //if(displayRemainderRegion)
            //{
            //    CLPFuzzyFactorCardRemainder remainderRegion = new CLPFuzzyFactorCardRemainder(this, dividend, page);
            //    page.PageObjects.Add(remainderRegion);
            //    RemainderRegionUniqueID = remainderRegion.UniqueID;
            //}
        }
        
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFuzzyFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region A/B Testing Toggles

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsCurlyEdge
        {
            get
            {
                return GetValue<bool>(IsCurlyEdgeProperty);
            }
            set
            {
                SetValue(IsCurlyEdgeProperty, value);
            }
        }

        /// <summary>
        /// Register the IsCurlyEdge property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsCurlyEdgeProperty = RegisterProperty("IsCurlyEdge", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsStraightEdge
        {
            get
            {
                return GetValue<bool>(IsStraightEdgeProperty);
            }
            set
            {
                SetValue(IsStraightEdgeProperty, value);
            }
        }

        /// <summary>
        /// Register the IsStraightEdge property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsStraightEdgeProperty = RegisterProperty("IsStraightEdge", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsNoRightEdge
        {
            get
            {
                return GetValue<bool>(IsNoRightEdgeProperty);
            }
            set
            {
                SetValue(IsNoRightEdgeProperty, value);
            }
        }

        /// <summary>
        /// Register the IsNoRightEdge property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsNoRightEdgeProperty = RegisterProperty("IsNoRightEdge", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string DefaultFuzzyEdgeColor
        {
            get
            {
                return GetValue<string>(DefaultFuzzyEdgeColorProperty);
            }
            set
            {
                SetValue(DefaultFuzzyEdgeColorProperty, value);
            }
        }

        //TODO Liz: update name to start with Is
        public bool HasRemainder
        {
            get
            {
                if((double)Dividend % (double)Rows == 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool NoRemainder
        {
            get
            {
                return !HasRemainder;
            }
        }

        public bool IsStraightEdgeRemainder
        {
            get
            {
                return ((double)Dividend % (double)Rows != 0 && IsStraightEdge);
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsDividendOnEdge
        {
            get
            {
                return GetValue<bool>(IsDividendOnEdgeProperty);
            }
            set
            {
                SetValue(IsDividendOnEdgeProperty, value);
            }
        }

        public static readonly PropertyData IsDividendOnEdgeProperty = RegisterProperty("IsDividendOnEdge", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsFuzzyEdgeVisible
        {
            get
            {
                return !(IsDividendOnEdge && CurrentRemainder == 0);
            }
        }

        /// <summary>
        /// Register the DefaultFuzzyEdgeColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DefaultFuzzyEdgeColorProperty = RegisterProperty("DefaultFuzzyEdgeColor", typeof(string), "Gray");

        /// <summary>
        /// Whether or not the answer is displayed.
        /// </summary>
        public bool IsAnswerVisible
        {
            get
            {
                return GetValue<bool>(IsAnswerVisibleProperty);
            }
            set
            {
                SetValue(IsAnswerVisibleProperty, value);
            }
        }

        public static readonly PropertyData IsAnswerVisibleProperty = RegisterProperty("IsAnswerVisible", typeof(bool), true);

        /// <summary>
        /// True if division labels are on top and answer (if shown) is on bottom.
        /// </summary>
        public bool IsArrayDivisionLabelOnTop
        {
	        get { return GetValue<bool>(IsArrayDivisionLabelOnTopProperty); }
	        set { SetValue(IsArrayDivisionLabelOnTopProperty, value); }
        }

        public static readonly PropertyData IsArrayDivisionLabelOnTopProperty = RegisterProperty("IsArrayDivisionLabelOnTop", typeof(bool), true);

        #endregion //A/B Testing Toggles

        #region Properties
        public override string PageObjectType
        {
            get { return "CLPFuzzyFactorCard"; }
        }

        /// <summary>
        /// True if FFC is aligned so that fuzzy edge is on the right
        /// </summary>
        public bool IsHorizontallyAligned
        {
	        get { return GetValue<bool>(IsHorizontallyAlignedProperty); }
	        set { SetValue(IsHorizontallyAlignedProperty, value); }
        }

        public static readonly PropertyData IsHorizontallyAlignedProperty = RegisterProperty("IsHorizontallyAligned", typeof(bool), true);

        /// <summary>
        /// Value of the dividend.
        /// </summary>
        public int Dividend
        {
            get
            {
                return GetValue<int>(DividendProperty);
            }
            set
            {
                SetValue(DividendProperty, value);
            }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int), null);

        public int GroupsSubtracted
        {
            get
            {
                int groupsSubtracted = 0;
                foreach(var division in VerticalDivisions)
                {
                    groupsSubtracted += division.Value;
                }
                return groupsSubtracted;
            }
        }
        public int CurrentRemainder
        {
            get
            {
                return Dividend - GroupsSubtracted * Rows;
            }
        }

        public double LastDivisionPosition
        {
            get
            {
                if(!VerticalDivisions.Any())
                {
                    return 0.0;
                }
                return VerticalDivisions.Last().Position;
            }
        }

        /// <summary>
        /// UniqueID of the region of remainder tiles. Null if not displaying remainder tiles.
        /// </summary>
        public string RemainderRegionUniqueID
        {
            get
            {
                return GetValue<string>(RemainderRegionUniqueIDProperty);
            }
            set
            {
                SetValue(RemainderRegionUniqueIDProperty, value);
            }
        }

        /// <summary>
        /// Register the RemainderRegionUniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RemainderRegionUniqueIDProperty = RegisterProperty("RemainderRegionUniqueID", typeof(string), null);

        #endregion //Properties

        #region Methods

        public override ICLPPageObject Duplicate()
        {
            var newArray = Clone() as CLPFuzzyFactorCard;
            if(newArray != null)
            {
                newArray.UniqueID = Guid.NewGuid().ToString();
                newArray.ParentPage = ParentPage;
                return newArray;
            }
            return null;
        }

        public override void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            // TODO Liz: add a min size for FFC
            var rightLabelLength = IsHorizontallyAligned ? LargeLabelLength : LabelLength;
            var bottomLabelLength = IsHorizontallyAligned ? LabelLength : LargeLabelLength;
            var initialSquareSize = 45.0;
            if(toSquareSize <= 0)
            {
                while(XPosition + LabelLength + rightLabelLength + initialSquareSize * Columns >= ParentPage.PageWidth || YPosition + LabelLength + bottomLabelLength + initialSquareSize * Rows >= ParentPage.PageHeight)
                {
                    initialSquareSize = Math.Abs(initialSquareSize - 45.0) < .0001 ? 22.5 : initialSquareSize / 4 * 3;
                }
            }
            else
            {
                initialSquareSize = toSquareSize;
            }

            ArrayHeight = initialSquareSize * Rows;
            ArrayWidth = initialSquareSize * Columns;

            Height = ArrayHeight + LabelLength + bottomLabelLength;
            Width = ArrayWidth + LabelLength + rightLabelLength;
            if(IsGridOn)
            {
                CalculateGridLines();
            }
            if(recalculateDivisions)
            {
                ResizeDivisions();
                RaisePropertyChanged("LastDivisionPosition");
            }
        }

        public override void CalculateGridLines()
        {
            HorizontalGridLines.Clear();
            VerticalGridLines.Clear();
            var squareSize = ArrayWidth / Columns;
            for(int i = 1; i < Rows; i++)
            {
                HorizontalGridLines.Add(i * squareSize);
            }
            for(int i = 1; i < GroupsSubtracted; i++)
            {
                VerticalGridLines.Add(i * squareSize);
            }
        }

        public override void RefreshArrayDimensions()
        {
            var rightLabelLength = IsHorizontallyAligned ? LargeLabelLength : LabelLength;
            var bottomLabelLength = IsHorizontallyAligned ? LabelLength : LargeLabelLength;
            ArrayHeight = Height - LabelLength - bottomLabelLength;
            ArrayWidth = Width - LabelLength - rightLabelLength;
        }

        public override void OnResized()
        {
            base.OnResized();
            RaisePropertyChanged("LastDivisionPosition");
        }

        public override void OnRemoved()
        {
            CLPFuzzyFactorCardRemainder remainderRegion = ParentPage.GetPageObjectByUniqueID(RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
            ParentPage.PageObjects.Remove(remainderRegion);
            base.OnRemoved();
        }

        public void SnapInArray(int value)
        {
            if(IsHorizontallyAligned)
            {
                double position = LastDivisionPosition + (double)value * (ArrayHeight / Rows);
                CLPArrayDivision divAbove = FindDivisionAbove(position, VerticalDivisions);
                CLPArrayDivision divBelow = FindDivisionBelow(position, VerticalDivisions);

                CLPArrayDivision topDiv;
                if(divAbove == null)
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, value);
                }
                else
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, value);
                    VerticalDivisions.Remove(divAbove);
                }
                VerticalDivisions.Add(topDiv);

                CLPArrayDivision bottomDiv;
                if(divBelow == null)
                {
                    bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, ArrayWidth - position, 0);
                }
                else
                {
                    bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);
                }

                VerticalDivisions.Add(bottomDiv);

                //Update Remainder Region
                if(RemainderRegionUniqueID != null)
                {
                    CLPFuzzyFactorCardRemainder remainderRegion = ParentPage.GetPageObjectByUniqueID(RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
                    remainderRegion.RemoveTiles(value * Rows);
                }
            }

            //TODO Liz: Add ability to snap in arrays when rotated

            //To Do Liz: Add this to any division removal code and history items
            RaisePropertyChanged("GroupsSubtracted");
            RaisePropertyChanged("CurrentRemainder");
            RaisePropertyChanged("LastDivisionPosition");
            RaisePropertyChanged("IsFuzzyEdgeVisible");

            if(IsGridOn)
            {
                CalculateGridLines();
            }
        }

        public void RotateArray()
        {
            IsHorizontallyAligned = !IsHorizontallyAligned;
            var rightLabelLength = IsHorizontallyAligned ? LargeLabelLength : LabelLength;
            var bottomLabelLength = IsHorizontallyAligned ? LabelLength : LargeLabelLength;
            var tempCols = Columns;
            Columns = Rows;
            Rows = tempCols;
            var tempArrayHeight = ArrayHeight;
            ArrayHeight = ArrayWidth;
            ArrayWidth = tempArrayHeight;
            Height = ArrayHeight + LabelLength + bottomLabelLength;
            Width = ArrayWidth + LabelLength + rightLabelLength;
            CalculateGridLines();
            var tempHorizontalDivisions = HorizontalDivisions;
            HorizontalDivisions = VerticalDivisions;
            VerticalDivisions = tempHorizontalDivisions;
            ResizeDivisions();
            foreach(var verticalDivision in VerticalDivisions) 
            {
                verticalDivision.Orientation = ArrayDivisionOrientation.Vertical;
            }
            foreach(var horizontalDivision in HorizontalDivisions) 
            {
                horizontalDivision.Orientation = ArrayDivisionOrientation.Horizontal;
            }

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

        public void RemoveLastDivision()
        {
            if(VerticalDivisions.Count > 1)
            {
                var lastDiv = VerticalDivisions.Last();
                var prevDiv = VerticalDivisions[VerticalDivisions.Count - 2];

                VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, prevDiv.Position, prevDiv.Length + lastDiv.Length, 0));
                VerticalDivisions.Remove(lastDiv);
                VerticalDivisions.Remove(prevDiv);

                RaisePropertyChanged("GroupsSubtracted");
                RaisePropertyChanged("CurrentRemainder");
                RaisePropertyChanged("LastDivisionPosition");
                RaisePropertyChanged("IsFuzzyEdgeVisible");

                CalculateGridLines();

                //Update Remainder Region
                if(RemainderRegionUniqueID != null)
                {
                    CLPFuzzyFactorCardRemainder remainderRegion = ParentPage.GetPageObjectByUniqueID(RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
                    remainderRegion.AddTiles(prevDiv.Value * Rows);
                }
            }
        }

        #endregion //Methods
    }
}