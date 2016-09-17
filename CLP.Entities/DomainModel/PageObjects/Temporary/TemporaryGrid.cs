using System.Collections.Generic;
using System.Windows;
using Catel.Data;

namespace CLP.Entities
{
    public class TemporaryGrid : APageObjectBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="TemporaryGrid" /> from scratch.</summary>
        public TemporaryGrid() { }

        /// <summary>Initializes <see cref="TemporaryBoundary" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TemporaryGrid" /> belongs to.</param>
        public TemporaryGrid(CLPPage parentPage, double xPosition, double yPosition, double height, double width, int cellWidth, int cellHeight, List<Point> occupiedCells)
            : base(parentPage)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            Height = height;
            Width = width;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            OccupiedCells = occupiedCells;
        }

        #endregion //Constructors

        #region Properties

        public int CellWidth
        {
            get { return GetValue<int>(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        public static readonly PropertyData CellWidthProperty = RegisterProperty("CellWidth", typeof(int), 0);

        public int CellHeight
        {
            get { return GetValue<int>(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        public static readonly PropertyData CellHeightProperty = RegisterProperty("CellHeight", typeof(int), 0);

        public List<Point> OccupiedCells
        {
            get { return GetValue<List<Point>>(OccupiedCellsProperty); }
            set { SetValue(OccupiedCellsProperty, value); }
        }

        public static readonly PropertyData OccupiedCellsProperty = RegisterProperty("OccupiedCells", typeof(List<Point>), () => new List<Point>());

        #endregion // Properties

        #region APageObjectBase Overrides

        public override int ZIndex
        {
            get { return 1000; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        #endregion //Methods
    }
}