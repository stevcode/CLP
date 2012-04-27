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
    public class CLPShadingRegion : CLPInkRegion
    {
        #region Constructors

        public CLPShadingRegion(int rows, int cols) : base()
        {
            Rows = rows;
            Cols = cols;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPShadingRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPShadingRegion"; }
        }

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
            ObservableCollection<string> StrokesNoDuplicates = new ObservableCollection<string>(PageObjectStrokes.Distinct().ToList());
            int[,] result = InkInterpretation.InterpretShading(CLPPage.StringsToStrokes(StrokesNoDuplicates), Position, Width, Height, 15.0);
            double total_shaded = 0.0;
            double total = 0.0;
            foreach (int i in result)
            {
                total_shaded += i;
                total += 1;
            }
            /*for (int i = 0; i < result.GetUpperBound(1); i++)
            {
                for (int j = 0; j < result.GetUpperBound(0); j++)
                {
                    Console.Write(result[j, i] + ", ");
                }
                Console.WriteLine("\n");
            }*/
            Console.WriteLine("Percent shaded: " + total_shaded / total);
        }

        public int Idx2Dto1D(double x, double y)
        {
            return (int)(y * Cols + x);
        }

        #endregion // Methods
    }
}
