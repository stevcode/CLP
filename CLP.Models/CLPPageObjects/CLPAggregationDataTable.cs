using System;
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
        public CLPGridPart(GridPartOrientation orientation, double height, double width, double headerStretch)
        {
            Orientation = orientation;
            Height = height;
            Width = width;
            HeaderStretch = headerStretch;
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
        /// The distance between the start of the Header and the grid cells.
        /// MIGHT NOT BE NEEDED? CAN HAVE 1 ROWHEADERWIDTH AND 1 COLHEADERHEIGHT IN CLPAGREEGATIONDATATABLE
        /// </summary>
        public double HeaderStretch
        {
            get { return GetValue<double>(HeaderStretchProperty); }
            set { SetValue(HeaderStretchProperty, value); }
        }

        public static readonly PropertyData HeaderStretchProperty = RegisterProperty("HeaderStretch", typeof(double), null);

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
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPAggregationDataTable(CLPPage page)
            : base(page)
        {
            XPosition = 50;
            YPosition = 50;
            Height = 200;
            Width = 400;
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

        #endregion

    }
}
