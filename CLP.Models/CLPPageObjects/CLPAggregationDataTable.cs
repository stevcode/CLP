using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    public enum GridPartOrientation
    {
        Row,
        Column
    }

    public enum AggregationType
    {
        None,
        Single,
        Group
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
            UniqueID = Guid.NewGuid().ToString();
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
        /// UniqueID of the GridPart.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), null);

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

        /// <summary>
        /// Stokes to be sent with GridPart on Aggregation.
        /// </summary>
        public ObservableCollection<List<byte>> ByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(ByteStrokesProperty); }
            set { SetValue(ByteStrokesProperty, value); }
        }

        public static readonly PropertyData ByteStrokesProperty = RegisterProperty("ByteStrokes", typeof(ObservableCollection<List<byte>>), () => new ObservableCollection<List<byte>>());

        /// <summary>
        /// Signifies that this GridPart will be aggregated on submit.
        /// </summary>
        public bool IsAggregated
        {
            get { return GetValue<bool>(IsAggregatedProperty); }
            set { SetValue(IsAggregatedProperty, value); }
        }

        public static readonly PropertyData IsAggregatedProperty = RegisterProperty("IsAggregated", typeof(bool), false);

        /// <summary>
        /// Submitter if in Single Aggregation Mode (Person).
        /// </summary>
        public Person PersonSubmitter
        {
            get { return GetValue<Person>(PersonSubmitterProperty); }
            set { SetValue(PersonSubmitterProperty, value); }
        }

        public static readonly PropertyData PersonSubmitterProperty = RegisterProperty("PersonSubmitter", typeof(Person), null);

        /// <summary>
        /// Submitter if in Group Aggregation Mode.
        /// </summary>
        public Group GroupSubmitter
        {
            get { return GetValue<Group>(GroupSubmitterProperty); }
            set { SetValue(GroupSubmitterProperty, value); }
        }

        public static readonly PropertyData GroupSubmitterProperty = RegisterProperty("GroupSubmitter", typeof(Group), null);

        #endregion //Properties
    }

    [Serializable]
    public class CLPAggregationDataTable : CLPPageObjectBase, ISubmittable
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
            CanAcceptStrokes = true;
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
        /// Type of Aggregation for this DataTable.
        /// </summary>
        public AggregationType AggregationType
        {
            get { return GetValue<AggregationType>(AggregationTypeProperty); }
            set { SetValue(AggregationTypeProperty, value); }
        }

        /// <summary>
        /// Register the AggregationType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AggregationTypeProperty = RegisterProperty("AggregationType", typeof(AggregationType), AggregationType.None);

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

        /// <summary>
        /// UniqueID of Page containing original DataTable.
        /// </summary>
        public string LinkedPageID
        {
            get { return GetValue<string>(LinkedPageIDProperty); }
            set { SetValue(LinkedPageIDProperty, value); }
        }

        public static readonly PropertyData LinkedPageIDProperty = RegisterProperty("LinkedPageID", typeof(string), null);

        /// <summary>
        /// UniqueID of the GridPart on original DataTable that this DataTable is aggregating.
        /// </summary>
        public string AggregatingGridPartID
        {
            get { return GetValue<string>(AggregatingGridPartIDProperty); }
            set { SetValue(AggregatingGridPartIDProperty, value); }
        }

        public static readonly PropertyData AggregatingGridPartIDProperty = RegisterProperty("AggregatingGridPartID", typeof(string), null);
        
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

        public void AddAggregatedGridPart(CLPGridPart gridPart)
        {
            if (gridPart.IsAggregated)
            {
                switch(gridPart.Orientation)
                {
                    case GridPartOrientation.Row:
                        int replaceIndex = -1;

                        if (Rows.Count == 0)
                        {
                            switch(AggregationType)
                            {
                                case AggregationType.None:
                                    return;
                                case AggregationType.Single:
                                    gridPart.Header = gridPart.PersonSubmitter.FullName;
                                    break;
                                case AggregationType.Group:
                                    gridPart.Header = gridPart.GroupSubmitter.GroupName;
                                    break;
                                default:
                                    break;
                            }
                        }

                        foreach(CLPGridPart row in Rows)
                        {
                            switch(AggregationType)
                            {
                                case AggregationType.None:
                                    return;
                                case AggregationType.Single:
                                    gridPart.Header = gridPart.PersonSubmitter.FullName;
                                    if (row.PersonSubmitter.FullName == gridPart.PersonSubmitter.FullName)
                                    {
                                        gridPart.XPosition = row.XPosition;
                                        gridPart.YPosition = row.YPosition;
                                        gridPart.Height = row.Height;
                                        gridPart.Width = row.Width;
                                        replaceIndex = Rows.IndexOf(row);
                                    }
                                    break;
                                case AggregationType.Group:
                                    gridPart.Header = gridPart.GroupSubmitter.GroupName;
                                    if(row.GroupSubmitter.GroupName == gridPart.GroupSubmitter.GroupName)
                                    {
                                        gridPart.XPosition = row.XPosition;
                                        gridPart.YPosition = row.YPosition;
                                        gridPart.Height = row.Height;
                                        gridPart.Width = row.Width;
                                        replaceIndex = Rows.IndexOf(row);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (replaceIndex > -1)
                        {
                            var strokesToRemove =
                                from pageStroke in ParentPage.InkStrokes
                                from objectStroke in CLPPage.BytesToStrokes(Rows[replaceIndex].ByteStrokes)
                                where pageStroke.GetStrokeUniqueID() == objectStroke.GetStrokeUniqueID()
                                select pageStroke;

                            StrokeCollection sc = new StrokeCollection(strokesToRemove);

                            foreach(Stroke s in sc)
                            {
                                ParentPage.IsInkAutoAdding = true;
                                ParentPage.InkStrokes.Remove(s);
                                ParentPage.IsInkAutoAdding = false;
                            }

                            Rows.RemoveAt(replaceIndex);
                            Rows.Insert(replaceIndex, gridPart);
                        }
                        else
                        {
                            AddGridPart(gridPart);
                        }                       

                        StrokeCollection gridPartStrokes = CLPPage.BytesToStrokes(gridPart.ByteStrokes);
                        foreach(Stroke stroke in gridPartStrokes)
                        {
                            Matrix transform = new Matrix();
                            transform.Translate(gridPart.XPosition + RowHeaderWidth + XPosition, gridPart.YPosition + ColumnHeaderHeight + YPosition);
                            stroke.Transform(transform, true);
                            ParentPage.IsInkAutoAdding = true;
                            ParentPage.InkStrokes.Add(stroke);
                            ParentPage.IsInkAutoAdding = false;
                        }
                        break;
                    case GridPartOrientation.Column:
                        //TODO: Steve - Expand to allow for aggregation of columns
                        break;
                    default:
                        break;
                } 
            }
        }

        public CLPAggregationDataTable CreateAggregatedTable(CLPGridPart gridPart)
        {
            CLPAggregationDataTable newTable = new CLPAggregationDataTable(ParentPage);

            newTable.ParentID = UniqueID;
            newTable.LinkedPageID = ParentPage.UniqueID;
            newTable.AggregatingGridPartID = gridPart.UniqueID;
            newTable.ColumnHeaderHeight = ColumnHeaderHeight;
            newTable.RowHeaderWidth = RowHeaderWidth;

            switch(gridPart.Orientation)
            {
                case GridPartOrientation.Row:
                    foreach(CLPGridPart col in Columns)
                    {
                        CLPGridPart newCol = new CLPGridPart(col.Orientation, RowHeaderWidth ,col.Width);
                        newCol.Header = col.Header;
                        newCol.XPosition = col.XPosition;
                        newCol.YPosition = col.YPosition;
                        newTable.AddGridPart(newCol);
                    }
                    break;
                case GridPartOrientation.Column:
                    foreach(CLPGridPart row in Rows)
                    {
                        CLPGridPart newRow = new CLPGridPart(row.Orientation, row.Height, ColumnHeaderHeight);
                        newRow.Header = row.Header;
                        newRow.XPosition = row.XPosition;
                        newRow.YPosition = row.YPosition;
                        newTable.AddGridPart(newRow);
                    }
                    break;
                default:
                    break;
            }

            return newTable;
        }

        public override void RefreshStrokeParentIDs()
        {
            if(CanAcceptStrokes)
            {
                foreach(CLPGridPart gridPart in Rows)
                {
                    if(gridPart.IsAggregated)
                    {
                        gridPart.ByteStrokes.Clear();
                        Rect rect = new Rect(gridPart.XPosition + RowHeaderWidth + XPosition, gridPart.YPosition + ColumnHeaderHeight + YPosition, gridPart.Width - RowHeaderWidth, gridPart.Height);
                        var strokesOverObject =
                            from stroke in ParentPage.InkStrokes
                            where stroke.HitTest(rect, 3)
                            select stroke;

                        StrokeCollection clonedStrokes = new StrokeCollection();
                        foreach(Stroke stroke in strokesOverObject)
                        {
                            StrokeCollection clippedStrokes = stroke.GetClipResult(rect);
                            foreach (Stroke clippedStroke in clippedStrokes)
                            {
                                Stroke newStroke = clippedStroke.Clone();
                                Matrix transform = new Matrix();
                                transform.Translate(-gridPart.XPosition - RowHeaderWidth - XPosition, -gridPart.YPosition - ColumnHeaderHeight - YPosition);
                                newStroke.Transform(transform, true);
                                newStroke.SetStrokeUniqueID(Guid.NewGuid().ToString());
                                clonedStrokes.Add(newStroke);
                            }
                        }
                        gridPart.ByteStrokes = CLPPage.StrokesToBytes(clonedStrokes);
                    }
                }
            }
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            //This pageObject can accept strokes, but never needs strokes in real time, so RefreshStrokeParentIDs
            //is called when the strokes are needed. Blank method to override default behavior.
        }

        #endregion

        #region ISubmittable Members

        public void BeforeSubmit(bool isGroupSubmit, CLPNotebook notebook)
        {
            
        }

        public void AfterSubmit(bool isGroupSubmit, CLPNotebook notebook)
        {
            RefreshStrokeParentIDs();

            int autoPageIndex = ParentPage.PageIndex; //off by one indexing
            CLPPage autoGeneratedPage = notebook.GetPageAt(autoPageIndex, -1);
            if(autoGeneratedPage != null)
            {
                CLPAggregationDataTable linkedDataTable;
                foreach(ICLPPageObject pageObject in autoGeneratedPage.PageObjects)
                {
                    if(pageObject is CLPAggregationDataTable)
                    {
                        if((pageObject as CLPAggregationDataTable).LinkedPageID == ParentPage.UniqueID)
                        {
                            linkedDataTable = pageObject as CLPAggregationDataTable;

                            switch(linkedDataTable.AggregationType)
                            {
                                case AggregationType.None:
                                    break;
                                case AggregationType.Single:
                                    if(!isGroupSubmit)
                                    {
                                        foreach(CLPGridPart gridPart in Rows)
                                        {
                                            if(gridPart.IsAggregated)
                                            {
                                                gridPart.PersonSubmitter = ParentPage.Submitter;
                                                gridPart.GroupSubmitter = ParentPage.GroupSubmitter;
                                                linkedDataTable.AddAggregatedGridPart(gridPart);
                                            }
                                        }
                                    }
                                    break;
                                case AggregationType.Group:
                                    if(isGroupSubmit)
                                    {
                                        foreach(CLPGridPart gridPart in Rows)
                                        {
                                            if(gridPart.IsAggregated)
                                            {
                                                gridPart.PersonSubmitter = ParentPage.Submitter;
                                                gridPart.GroupSubmitter = ParentPage.GroupSubmitter;
                                                linkedDataTable.AddAggregatedGridPart(gridPart);
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            break;
                        }
                    }
                }
            }
        }

        #endregion //ISubmittable Members
    }
}
