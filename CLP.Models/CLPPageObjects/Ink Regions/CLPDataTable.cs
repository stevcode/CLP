using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows;

namespace CLP.Models
{
    [Serializable]
    public class CLPDataTable : ACLPInkRegion
    {
        #region Constructors

        public CLPDataTable(int rows, int cols, CLPHandwritingAnalysisType analysis_type, ICLPPage page)
            : base(page)
        {
            Rows = rows;
            Cols = cols;

            for (int i = 0; i < rows * cols; i++)
            {
                DataValues.Add(new CLPNamedInkSet());
            }

            AnalysisType = analysis_type;
            //Console.WriteLine(DataTableCols + " ... " + DataTableRows);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPDataTable"; }
        }

        /// <summary>
        /// Stored ink strokes and values for each cell in the table
        /// </summary>
        public List<CLPNamedInkSet> DataValues
        {
            get { return GetValue<List<CLPNamedInkSet>>(DataValuesProperty); }
            set { SetValue(DataValuesProperty, value); }
        }

        /// <summary>
        /// Register the DataValues property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DataValuesProperty = RegisterProperty("DataValues", typeof(List<CLPNamedInkSet>), () => new List<CLPNamedInkSet>());

        /// <summary>
        /// Number of rows
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Register the DataTableRows property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 0);

        /// <summary>
        /// Number of cols
        /// </summary>
        public int Cols
        {
            get { return GetValue<int>(ColsProperty); }
            set { SetValue(ColsProperty, value); }
        }

        /// <summary>
        /// Register the DataTableCols property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColsProperty = RegisterProperty("Cols", typeof(int), 0);

        /// <summary>
        /// Handwriting analysis type
        /// </summary>
        public CLPHandwritingAnalysisType AnalysisType
        {
            get { return GetValue<CLPHandwritingAnalysisType>(AnalysisTypeProperty); }
            set { SetValue(AnalysisTypeProperty, value); }
        }

        /// <summary>
        /// Register the DataTableCols property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AnalysisTypeProperty = RegisterProperty("AnalysisType", typeof(CLPHandwritingAnalysisType), CLPHandwritingAnalysisType.DEFAULT);

        #endregion // Properties

        #region Methods

        public override void DoInterpretation()
        {
            //KELSEY - Changed InkStrokes, don't really understand this part of the interpretation, commented this out for now -steve
            //DataValues = new List<CLPNamedInkSet>();
            //for (int i = 0; i < Rows * Cols; i++)
            //{
            //    DataValues.Add(new CLPNamedInkSet());
            //}
            //// Add strokes to appropriate bins
            ////ObservableCollection<List<byte>> StrokesNoDuplicates = new ObservableCollection<List<byte>>(PageObjectByteStrokes.Distinct().ToList());
            //List<Point> locations = InkInterpretation.InterpretTable(GetStrokesOverPageObject(), Width, Height, Rows, Cols);
            //for (int i = 0; i < locations.Count; i++)
            //{
            //    Point p = locations[i];
            //    // if the point is not outside of the grid
            //    if (p.X >= 0 && p.Y >= 0 && p.X < Cols && p.Y < Rows)
            //    {
            //        List<byte> stroke = StrokesNoDuplicates[i];
            //        DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes.Add(stroke);
            //        StrokeCollection newStrokeCollection = CLPPage.BytesToStrokes(DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes);
            //        string result = InkInterpretation.InterpretHandwriting(newStrokeCollection, AnalysisType);
            //        DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeType = result;
            //    }
            //}
        }

        public int Idx2Dto1D(double x, double y)
        {
            return (int)(y * Cols + x);
        }

        #endregion // Methods
    }
}
