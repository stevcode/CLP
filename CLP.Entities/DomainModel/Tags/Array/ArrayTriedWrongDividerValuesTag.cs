using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DividerValuesOrientation
    {
        Horizontal,
        Vertical
    }

    [Serializable]
    public class ArrayTriedWrongDividerValuesTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayTriedWrongDividerValuesTag" /> from scratch.
        /// </summary>
        public ArrayTriedWrongDividerValuesTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayTriedWrongDividerValuesTag" /> from a set of values.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayTriedWrongDividerValuesTag" /> belongs to.</param>
        public ArrayTriedWrongDividerValuesTag(CLPPage parentPage, Origin origin, string arrayID, int rows, int columns, DividerValuesOrientation dividerValuesOrientation, List<int> dividerValues)
            : base(parentPage, origin)
        {
            ArrayID = arrayID;
            Rows = rows;
            Columns = columns;
            DividerValuesOrientation = dividerValuesOrientation;
            DividerValues = dividerValues;
        }

        /// <summary>
        /// Initializes <see cref="ArrayTriedWrongDividerValuesTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayTriedWrongDividerValuesTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="CLPArray" /> on which the Divider Value incorrect attempt was tried.
        /// </summary>
        public string ArrayID
        {
            get { return GetValue<string>(ArrayIDProperty); }
            set { SetValue(ArrayIDProperty, value); }
        }

        public static readonly PropertyData ArrayIDProperty = RegisterProperty("ArrayID", typeof(string), string.Empty);

        /// <summary>
        /// Rows of the <see cref="CLPArray" />.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 0);

        /// <summary>
        /// Columns of the <see cref="CLPArray" />.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), 0);

        /// <summary>
        /// Determines whether or not the failed Divider values were along the horizontal side of the <see cref="CLPArray" />.
        /// AKA DividerValues compared against Columns.
        /// </summary>
        public DividerValuesOrientation DividerValuesOrientation
        {
            get { return GetValue<DividerValuesOrientation>(DividerValuesOrientationProperty); }
            set { SetValue(DividerValuesOrientationProperty, value); }
        }

        public static readonly PropertyData DividerValuesOrientationProperty = RegisterProperty("DividerValuesOrientation", typeof(DividerValuesOrientation), DividerValuesOrientation.Horizontal);

        /// <summary>
        /// A list of all the values of each Divider.
        /// </summary>
        public List<int> DividerValues
        {
            get { return GetValue<List<int>>(DividerValuesProperty); }
            set { SetValue(DividerValuesProperty, value); }
        }

        public static readonly PropertyData DividerValuesProperty = RegisterProperty("DividerValues", typeof(List<int>), () => new List<int>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("ID: {0}, a {1}x{2} array.\n" + "Values: {3}.\n" + "Values compared against {4}.",
                                     ArrayID,
                                     Rows,
                                     Columns,
                                     string.Join("+", DividerValues) + " = " + DividerValues.Sum(),
                                     DividerValuesOrientation == DividerValuesOrientation.Horizontal ? "Columns" : "Rows");
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}