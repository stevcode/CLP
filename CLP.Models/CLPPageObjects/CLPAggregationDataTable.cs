using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    public enum GridPartOrientation
    {
        Row,
        Column
    }

    [Serializable]
    public class CLPGridPart : DataObjectBase<CLPGridPart>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPGridPart(GridPartOrientation orientation, double height, double width)
        {
            Orientation = orientation;
            Height = height;
            Width = width;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPGridPart(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// X Position for GridPart.
        /// 0 for Rows, variable for Columns.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 0);

        /// <summary>
        /// Y Position for GridPart.
        /// 0 for Columns, variable for Rows.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 0);

        /// <summary>
        /// The Height of the GridPart.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), null);

        /// <summary>
        /// The Width of the GridPart.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), null);

        /// <summary>
        /// The text of the GridPart Header.
        /// </summary>
        public string Header
        {
            get { return GetValue<string>(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly PropertyData HeaderProperty = RegisterProperty("Header", typeof(string), "");

        /// <summary>
        /// Orientation of the GridPart.
        /// </summary>
        public GridPartOrientation Orientation
        {
            get { return GetValue<GridPartOrientation>(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly PropertyData OrientationProperty = RegisterProperty("Orientation", typeof(GridPartOrientation), null);

        #endregion //Properties
    }

    [Serializable]
    public class CLPAggregationDataTable : CLPPageObjectBase
    {
        #region Variables

        private const double DEFAULT_COLUMN_HEADER_HEIGHT = 75;
        private const double DEFAULT_ROW_HEADER_WIDTH = 150;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPAggregationDataTable(CLPPage page)
            : base(page)
        {
            XPosition = 50;
            YPosition = 50;
            Height = DEFAULT_COLUMN_HEADER_HEIGHT;
            Width = DEFAULT_ROW_HEADER_WIDTH;
        }        

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPAggregationDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Height of the Header Section of each Column.
        /// </summary>
        public double ColumnHeaderHeight
        {
            get { return GetValue<double>(ColumnHeaderHeightProperty); }
            set { SetValue(ColumnHeaderHeightProperty, value); }
        }

        public static readonly PropertyData ColumnHeaderHeightProperty = RegisterProperty("ColumnHeaderHeight", typeof(double), DEFAULT_COLUMN_HEADER_HEIGHT);

        /// <summary>
        /// All the Columns of the DataTable.
        /// </summary>
        public ObservableCollection<CLPGridPart> Columns
        {
            get { return GetValue<ObservableCollection<CLPGridPart>>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(ObservableCollection<CLPGridPart>), () => new ObservableCollection<CLPGridPart>());

        /// <summary>
        /// Width of Header section of each Row.
        /// </summary>
        public double RowHeaderWidth
        {
            get { return GetValue<double>(RowHeaderWidthProperty); }
            set { SetValue(RowHeaderWidthProperty, value); }
        }

        public static readonly PropertyData RowHeaderWidthProperty = RegisterProperty("RowHeaderWidth", typeof(double), DEFAULT_ROW_HEADER_WIDTH);

        /// <summary>
        /// All the Rows of the DataTable.
        /// </summary>
        public ObservableCollection<CLPGridPart> Rows
        {
            get { return GetValue<ObservableCollection<CLPGridPart>>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(ObservableCollection<CLPGridPart>), () => new ObservableCollection<CLPGridPart>());

        #endregion //Properties

        #region Methods

        public override string PageObjectType
        {
            get { return "CLPAggregationDataTable"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPAggregationDataTable newDataTable = this.Clone() as CLPAggregationDataTable;
            newDataTable.UniqueID = Guid.NewGuid().ToString();
            newDataTable.ParentPage = ParentPage;

            return newDataTable;
        }

        public void AddGridPart(CLPGridPart gridPart)
        {
            switch(gridPart.Orientation)
            {
                case GridPartOrientation.Row:
                    gridPart.XPosition = 0;

                    double yPos = 0;
                    foreach(var row in Rows)
                    {
                        yPos += row.Height;
                    }

                    gridPart.YPosition = yPos;

                    Rows.Add(gridPart);
                    break;
                case GridPartOrientation.Column:
                    gridPart.YPosition = 0;

                    double xPos = 0;
                    foreach(var col in Columns)
                    {
                        xPos += col.Width;
                    }

                    gridPart.XPosition = xPos;

                    Columns.Add(gridPart);
                    break;
                default:
                    break;
            }

            RefreshDataTableDimensions();
        }

        public void RefreshDataTableDimensions()
        {
            double newHeight = ColumnHeaderHeight;
            foreach(var row in Rows)
            {
                newHeight += row.Height;
            }
            Height = newHeight;

            double newWidth = RowHeaderWidth;
            foreach(var column in Columns)
            {
                column.Height = Height;
                newWidth += column.Width;
            }
            Width = newWidth;

            foreach(var row in Rows)
            {
                row.Width = Width;
            }
        }

        #endregion

    }
}
