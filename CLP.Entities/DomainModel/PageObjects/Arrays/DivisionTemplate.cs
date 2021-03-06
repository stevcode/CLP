﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplate : ACLPArrayBase, IReporter
    {
        private const double MIN_ARRAY_LENGTH = 185.0;
        public const int MAX_NUMBER_OF_REMAINDER_TILES = 50;

        #region Constructors

        /// <summary>Initializes <see cref="DivisionTemplate" /> from scratch.</summary>
        public DivisionTemplate() { }

        /// <summary>Initializes <see cref="DivisionTemplate" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplate" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="DivisionTemplate" />.</param>
        /// <param name="rows">The number of rows in the <see cref="DivisionTemplate" />.</param>
        /// <param name="dividend">The total number the <see cref="DivisionTemplate" /> represents.</param>
        /// <param name="isRemainderTilesVisible">Signifies the <see cref="DivisionTemplate" /> is using a <see cref="RemainderRegion" />.</param>
        public DivisionTemplate(CLPPage parentPage, int columns, int rows, int dividend, bool isRemainderTilesVisible = false)
            : base(parentPage, columns, rows)
        {
            Dividend = dividend;
            IsSnappable = true;
            IsRemainderTilesVisible = isRemainderTilesVisible;
            if (!CanShowRemainderTiles)
            {
                return;
            }

            InitializeRemainderTiles();
        }

        public DivisionTemplate(CLPPage parentPage, double gridSquareSize, int columns, int rows, int dividend, bool isRemainderRegionDisplayed = false)
            : this(parentPage, columns, rows, dividend, isRemainderRegionDisplayed)
        {
            Width = (gridSquareSize * columns) + DT_LABEL_LENGTH + DT_LARGE_LABEL_LENGTH;
            Height = (gridSquareSize * rows) + (2 * DT_LABEL_LENGTH);
        }

        public void InitializeRemainderTiles()
        {
            if (RemainderTiles == null)
            {
                RemainderTiles = new RemainderTiles(ParentPage, this);
            }

            RemainderTiles.YPosition = YPosition;
            RemainderTiles.XPosition = XPosition + Width;

            if (RemainderTiles.YPosition + RemainderTiles.Height >= ParentPage.Height)
            {
                RemainderTiles.YPosition = ParentPage.Height - RemainderTiles.Height;
            }

            if (RemainderTiles.XPosition + RemainderTiles.Width >= ParentPage.Width)
            {
                RemainderTiles.XPosition = XPosition - RemainderTiles.Width;
            }

            if (RemainderTiles.XPosition < 0)
            {
                RemainderTiles.XPosition = 0;
            }
        }

        #endregion //Constructors

        #region Properties

        /// <summary>The total number the <see cref="DivisionTemplate" /> represents.</summary>
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (int), 1);

        /// <summary>
        /// Toggles visibility of associated <see cref="RemainderTiles" />.
        /// </summary>
        public bool IsRemainderTilesVisible
        {
            get { return GetValue<bool>(IsRemainderTilesVisibleProperty); }
            set { SetValue(IsRemainderTilesVisibleProperty, value); }
        }

        public static readonly PropertyData IsRemainderTilesVisibleProperty = RegisterProperty("IsRemainderTilesVisible", typeof (bool), false);

        #region Navigation Properties

        /// <summary>Unique Identifier for the <see cref="DivisionTemplate" />'s <see cref="RemainderTiles" />.</summary>
        /// <remarks>Composite Foreing Key.</remarks>
        public string RemainderTilesID
        {
            get { return GetValue<string>(RemainderTilesIDProperty); }
            set { SetValue(RemainderTilesIDProperty, value); }
        }

        public static readonly PropertyData RemainderTilesIDProperty = RegisterProperty("RemainderTilesID", typeof (string), string.Empty);

        /// <summary>Unique Identifier of the <see cref="Person" /> who owns the <see cref="RemainderTiles" />.</summary>
        public string RemainderTilesOwnerID
        {
            get { return GetValue<string>(RemainderTilesOwnerIDProperty); }
            set { SetValue(RemainderTilesOwnerIDProperty, value); }
        }

        public static readonly PropertyData RemainderTilesOwnerIDProperty = RegisterProperty("RemainderTilesOwnerID", typeof (string), string.Empty);

        /// <summary><see cref="RemainderTiles" /> for the <see cref="DivisionTemplate" />.</summary>
        public RemainderTiles RemainderTiles
        {
            get { return GetValue<RemainderTiles>(RemainderTilesProperty); }
            set
            {
                SetValue(RemainderTilesProperty, value);
                if (value == null)
                {
                    RemainderTilesID = string.Empty;
                    RemainderTilesOwnerID = string.Empty;
                    return;
                }
                RemainderTilesID = value.ID;
                RemainderTilesOwnerID = value.OwnerID;
            }
        }

        public static readonly PropertyData RemainderTilesProperty = RegisterProperty("RemainderTiles", typeof (RemainderTiles));

        #endregion //Navigation Properties

        #region Calculated Properties

        public double LargeLabelLength => DT_LARGE_LABEL_LENGTH;

        public int GroupsSubtracted
        {
            get { return VerticalDivisions.Sum(division => division.Value); }
        }

        public int CurrentRemainder
        {
            get { return Dividend - GroupsSubtracted * Rows; }
        }

        public double LastDivisionPosition
        {
            get { return VerticalDivisions.Any() ? VerticalDivisions.Last().Position : 0.0; }
        }

        public bool CanShowRemainderTiles
        {
            get { return IsRemainderTilesVisible && Dividend <= MAX_NUMBER_OF_REMAINDER_TILES; }
        }

        #endregion //Calculated Properties

        #endregion //Properties

        #region Methods

        public void SnapInArray(int value)
        {
            var position = LastDivisionPosition + value * (ArrayHeight / Rows);
            var divAbove = FindDivisionAbove(position, VerticalDivisions);
            var divBelow = FindDivisionBelow(position, VerticalDivisions);

            CLPArrayDivision topDiv;
            if (divAbove == null)
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, value);
            }
            else
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, value);
                VerticalDivisions.Remove(divAbove);
            }
            VerticalDivisions.Add(topDiv);
            var bottomDiv = divBelow == null ? new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, ArrayWidth - position, 0)
                                             : new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);
            VerticalDivisions.Add(bottomDiv);
            UpdateReport();

            RaisePropertyChanged("GroupsSubtracted");
            RaisePropertyChanged("CurrentRemainder");
            RaisePropertyChanged("LastDivisionPosition");
        }

        public void RemoveLastDivision()
        {
            if (VerticalDivisions.Count <= 1)
            {
                return;
            }
            var lastDiv = VerticalDivisions.Last();
            var prevDiv = VerticalDivisions[VerticalDivisions.Count - 2];

            VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, prevDiv.Position, prevDiv.Length + lastDiv.Length, 0));
            VerticalDivisions.Remove(lastDiv);
            VerticalDivisions.Remove(prevDiv);

            RaisePropertyChanged("GroupsSubtracted");
            RaisePropertyChanged("CurrentRemainder");
            RaisePropertyChanged("LastDivisionPosition");

            UpdateReport();
        }

        #endregion //Methods

        #region ACLPArrayBase Overrides

        public override double LabelLength => DT_LABEL_LENGTH;

        public override double ArrayWidth
        {
            get { return Width - (LargeLabelLength + LabelLength); }
        }

        public override double ArrayHeight
        {
            get { return Height - (2 * LabelLength); }
        }

        public override double GridSquareSize
        {
            get { return ArrayWidth / Columns; }
        }

        public override double MinimumGridSquareSize
        {
            get { return Columns < Rows ? MIN_ARRAY_LENGTH / Columns : MIN_ARRAY_LENGTH / Rows; }
        }

        public override void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            var initialWidth = Width;
            var initialHeight = Height;
            var initialSquareSize = DefaultGridSquareSize;
            if (toSquareSize <= 0)
            {
                while (XPosition + LabelLength + LargeLabelLength + initialSquareSize * Columns >= ParentPage.Width ||
                       YPosition + 2 * LabelLength + initialSquareSize * Rows >= ParentPage.Height)
                {
                    initialSquareSize = Math.Abs(initialSquareSize - 45.0) < .0001 ? 22.5 : initialSquareSize / 4 * 3;
                }
            }
            else
            {
                initialSquareSize = toSquareSize;
            }

            Height = (initialSquareSize * Rows) + 2 * LabelLength;
            Width = (initialSquareSize * Columns) + LabelLength + LargeLabelLength;

            if (recalculateDivisions)
            {
                ResizeDivisions();
            }
            OnResized(initialWidth, initialHeight);
        }

        public override void ResizeDivisions()
        {
            SortDivisions();
            var position = 0.0;
            var oldArrayWidth = VerticalDivisions.Aggregate(0.0, (total, d) => total + d.Length);
            var oldArrayHeight = HorizontalDivisions.Aggregate(0.0, (total, d) => total + d.Length);
            var oldGridSquareSize = VerticalDivisions.Any() ? oldArrayWidth / Columns : oldArrayHeight / Rows;
            foreach (var division in HorizontalDivisions)
            {
                var actualValue = division.GetActualValue(oldGridSquareSize);
                division.Position = position;
                division.Length = (GridSquareSize * actualValue) - 1.0;
                position += division.Length;
            }

            position = 0.0;
            foreach (var division in VerticalDivisions)
            {
                var actualValue = division.GetActualValue(oldGridSquareSize);
                division.Position = position;
                division.Length = (GridSquareSize * actualValue) - 1.0;
                position += division.Length;
            }
        }

        #endregion //ACLPArrayBase Overrides

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return $"{Dividend} / {Columns} Division Template with {CurrentRemainder} remaining"; }
        }

        public override string CodedName => Codings.OBJECT_DIVISION_TEMPLATE;

        public override string CodedID
        {
            get { return $"{Dividend} / {Rows}"; }
        }

        public override int ZIndex => 40;

        public override bool IsBackgroundInteractable => true;

        public override double MinimumHeight
        {
            get { return MIN_ARRAY_LENGTH + (2 * LabelLength); }
        }

        public override double MinimumWidth
        {
            get { return MIN_ARRAY_LENGTH + LargeLabelLength + LabelLength; }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            base.OnAdded(fromHistory);

            if (CanShowRemainderTiles &&
                RemainderTiles != null &&
                ParentPage != null)
            {
                ParentPage.PageObjects.Add(RemainderTiles);
                RemainderTiles.CreatorID = CreatorID;
                UpdateReport();
            }

            // TODO: Make part of Semantic Event analysis
            //var divisionDefinitions = ParentPage.Tags.OfType<DivisionRelationDefinitionTag>().ToList();

            //foreach (var divisionRelationDefinitionTag in divisionDefinitions)
            //{
            //    if (Dividend == divisionRelationDefinitionTag.Dividend &&
            //        Rows == divisionRelationDefinitionTag.Divisor)
            //    {
            //        continue;
            //    }

            //    var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(ParentPage);

            //    ITag divisionCreationErrorTag = null;
            //    if (Dividend == divisionRelationDefinitionTag.Divisor &&
            //        Rows == divisionRelationDefinitionTag.Dividend)
            //    {
            //        divisionCreationErrorTag = new DivisionTemplateCreationErrorTag(ParentPage,
            //                                                                        Origin.StudentPageGenerated,
            //                                                                        ID,
            //                                                                        Dividend,
            //                                                                        Rows,
            //                                                                        divisionTemplateIDsInHistory.IndexOf(ID),
            //                                                                        DivisionTemplateIncorrectCreationReasons.SwappedDividendAndDivisor);
            //    }

            //    if (Dividend == divisionRelationDefinitionTag.Dividend &&
            //        Rows != divisionRelationDefinitionTag.Divisor)
            //    {
            //        divisionCreationErrorTag = new DivisionTemplateCreationErrorTag(ParentPage,
            //                                                                        Origin.StudentPageGenerated,
            //                                                                        ID,
            //                                                                        Dividend,
            //                                                                        Rows,
            //                                                                        divisionTemplateIDsInHistory.IndexOf(ID),
            //                                                                        DivisionTemplateIncorrectCreationReasons.WrongDivisor);
            //    }

            //    if (Dividend != divisionRelationDefinitionTag.Dividend &&
            //        Rows == divisionRelationDefinitionTag.Divisor)
            //    {
            //        divisionCreationErrorTag = new DivisionTemplateCreationErrorTag(ParentPage,
            //                                                                        Origin.StudentPageGenerated,
            //                                                                        ID,
            //                                                                        Dividend,
            //                                                                        Rows,
            //                                                                        divisionTemplateIDsInHistory.IndexOf(ID),
            //                                                                        DivisionTemplateIncorrectCreationReasons.WrongDividend);
            //    }

            //    if (Dividend != divisionRelationDefinitionTag.Dividend &&
            //        Rows != divisionRelationDefinitionTag.Divisor)
            //    {
            //        divisionCreationErrorTag = new DivisionTemplateCreationErrorTag(ParentPage,
            //                                                                        Origin.StudentPageGenerated,
            //                                                                        ID,
            //                                                                        Dividend,
            //                                                                        Rows,
            //                                                                        divisionTemplateIDsInHistory.IndexOf(ID),
            //                                                                        DivisionTemplateIncorrectCreationReasons.WrongDividendAndDivisor);
            //    }

            //    if (divisionCreationErrorTag != null)
            //    {
            //        ParentPage.AddTag(divisionCreationErrorTag);
            //    }
            //}
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            base.OnDeleted(fromHistory);

            if (ParentPage != null &&
                RemainderTiles != null &&
                ParentPage.PageObjects.Contains(RemainderTiles))
            {
                ParentPage.PageObjects.Remove(RemainderTiles);
            }

            var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(ParentPage);

            var arrayDimensions = VerticalDivisions.Where(division => division.Value != 0).Select(division => Rows + "x" + division.Value).ToList();

            var tag = new DivisionTemplateDeletedTag(ParentPage, Origin.StudentPageObjectGenerated, ID, Dividend, Rows, divisionTemplateIDsInHistory.IndexOf(ID), arrayDimensions);
            ParentPage.AddTag(tag);
        }

        public override void OnResized(double oldWidth, double oldHeight, bool fromHistory = false)
        {
            base.OnResized(oldWidth, oldHeight, fromHistory);

            if (fromHistory)
            {
                ResizeDivisions();
            }

            RaisePropertyChanged(nameof(LastDivisionPosition));
        }

        #endregion //APageObjectBase Overrides

        #region IReporter Implementation

        public void UpdateReport()
        {
            if (ParentPage == null ||
                RemainderTiles == null ||
                !CanShowRemainderTiles)
            {
                return;
            }

            if (CurrentRemainder <= 0)
            {
                RemainderTiles.TileColors.Clear();
                return;
            }

            var numberOfBlackTiles = ParentPage.PageObjects.OfType<CLPArray>().Where(a => a.ArrayType == ArrayTypes.Array).ToList().Sum(a => a.Rows * a.Columns);
            numberOfBlackTiles = Math.Min(numberOfBlackTiles, CurrentRemainder);

            if (RemainderTiles.TileColors == null)
            {
                RemainderTiles.TileColors = new ObservableCollection<string>();
            }
            else
            {
                RemainderTiles.TileColors.Clear();
            }

            for (var i = 0; i < CurrentRemainder - numberOfBlackTiles; i++)
            {
                RemainderTiles.TileColors.Add("DodgerBlue");
            }

            for (var i = 0; i < numberOfBlackTiles; i++)
            {
                RemainderTiles.TileColors.Add("Black");
            }

            RemainderTiles.Height = Math.Ceiling(RemainderTiles.TileColors.Count / RemainderTiles.NUMBER_OF_TILES_PER_ROW) * RemainderTiles.TILE_HEIGHT;
            RemainderTiles.Width = RemainderTiles.NUMBER_OF_TILES_PER_ROW * RemainderTiles.TILE_HEIGHT;
        }

        #endregion //IReporter Implementation
    }
}