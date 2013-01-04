using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPAggregationDataTable : CLPPageObjectBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPAggregationDataTable(CLPPage page, int rows, int cols)
            : base(page)
        {
            XPosition = 50;
            YPosition = 50;
            Height = 200;
            Width = 400;
            Rows = rows;
            Columns = cols;
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
        /// Number of Rows in the DataTable.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 1);

        /// <summary>
        /// Number of Columns in the DataTable.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), 1);

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
