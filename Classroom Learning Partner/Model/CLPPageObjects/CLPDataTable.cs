using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Resources;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPDataTable : CLPInkRegion
    {
        #region Constructors

        //Parameterless constructor for protobuf
        public CLPDataTable()
            : base()
        {
        }
        public CLPDataTable(int rows, int cols) : base()
        {
            Rows = rows;
            Cols = cols;

            for (int i = 0; i < rows * cols; i++)
            {
                DataValues.Add(new CLPNamedInkSet());
            }
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
        public static readonly PropertyData DataValuesProperty = RegisterProperty("DataValues", typeof(List<CLPNamedInkSet>), new List<CLPNamedInkSet>());

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

        #endregion // Properties

        #region Methods

        public override void DoInterpretation(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            // Add strokes to appropriate bins
            List<Point> discr_added = InkInterpretation.InterpretTable(addedStrokes, Position, Width, Height, Rows, Cols);
            for (int i = 0; i < discr_added.Count; i++)
            {
                Point p = discr_added.ElementAt<Point>(i);
                // if the point is not outside of the grid
                if (p.X >= 0 && p.Y >= 0 && p.X < Cols && p.Y < Rows)
                {
                    string stroke = CLPPage.StrokeToString(addedStrokes[i]);
                    DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes.Add(stroke);
                    DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes =
                        new ObservableCollection<string>(DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes.Distinct().ToList());
                    StrokeCollection newStrokeCollection = CLPPage.StringsToStrokes(DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes);
                    string result = InkInterpretation.InterpretHandwriting(newStrokeCollection, CLPHandwritingAnalysisType.DEFAULT);
                    DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeType = result;
                }
            }

            // Delete strokes from appropriate bins
            List<Point> discr_removed = InkInterpretation.InterpretTable(removedStrokes, Position, Width, Height, Rows, Cols);
            for (int i = 0; i < discr_removed.Count; i++)
            {
                Point p = discr_removed.ElementAt<Point>(i);
                // if the point is not outside of the grid
                if (p.X >= 0 && p.Y >= 0 && p.X < Cols && p.Y < Rows)
                {
                    string stroke = CLPPage.StrokeToString(removedStrokes[i]);
                    while (DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes.Contains(stroke))
                        DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes.Remove(stroke);
                    StrokeCollection newStrokeCollection = CLPPage.StringsToStrokes(DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeStrokes);
                    string result = InkInterpretation.InterpretHandwriting(newStrokeCollection, CLPHandwritingAnalysisType.DEFAULT);
                    DataValues[Idx2Dto1D(p.X, p.Y)].InkShapeType = result;
                }
            }
        }

        public int Idx2Dto1D(double x, double y)
        {
            return (int)(y * Cols + x);
        }

        #endregion // Methods
    }
}
