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
    public class CLPShadingRegion : ACLPInkRegion
    {
        #region Constructors

        public CLPShadingRegion(int rows, int cols, CLPPage page) : base(page)
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
        /// Percent filled in
        /// </summary>
        public double PercentFilled
        {
            get { return GetValue<double>(PercentFilledProperty); }
            set { SetValue(PercentFilledProperty, value); }
        }

        /// <summary>
        /// Register the PercentFilled property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PercentFilledProperty = RegisterProperty("PercentFilled", typeof(double), 0);

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
        /// The feature vector representation of the region
        /// </summary>
        public double[] FeatureVector
        {
            get { return GetValue<double[]>(FeatureVectorProperty); }
            set { SetValue(FeatureVectorProperty, value); }
        }

        /// <summary>
        /// Register the FeatureVector property so it is known in the class.
        /// </summary>
        public static readonly PropertyData FeatureVectorProperty = RegisterProperty("FeatureVector", typeof(double[]), new double[11]);

        #endregion // Properties

        #region Methods

        public override void DoInterpretation()
        {
            //ObservableCollection<List<byte>> StrokesNoDuplicates = new ObservableCollection<List<byte>>(PageObjectByteStrokes.Distinct().ToList());
            int[,] result = InkInterpretation.InterpretShading(GetStrokesOverPageObject(), Width, Height, 15.0);
            double total_shaded = 0.0;
            double total = 0.0;
            foreach (int i in result)
            {
                total_shaded += i;
                total += 1;
            }
            PercentFilled = total_shaded / total;
            MakeFeatureVector(result);
            foreach (double i in FeatureVector)
                Console.Write(i + ", ");
            Console.WriteLine("");
        }

        public int Idx2Dto1D(double x, double y)
        {
            return (int)(y * Cols + x);
        }

        //private static double[] weights = { 1.85, 1.02, 1.25, 0.78, 1.65, 2.43, 0.78, 0.15, 2.43, 0.49, 0.15 };
        private void MakeFeatureVector(int[,] shading)
        {
            // Percent filled
            FeatureVector[0] = PercentFilled;

            // Centroids
            double totalX = 0;
            double totalY = 0;
            double total = 0;
            for (int i = 0; i < shading.GetLength(0); i++)
            {
                for (int j = 0; j < shading.GetLength(1); j++)
                {
                    if (shading[i, j] > 0)
                    {
                        totalX += i;
                        totalY += j;
                        total++;
                    }
                }
            }
            FeatureVector[1] = (totalX / total) / shading.GetLength(0);
            FeatureVector[2] = (Math.Abs(totalX / total - shading.GetLength(0) / 2)) / (shading.GetLength(0) / 2);
            FeatureVector[3] = (totalY / total) / shading.GetLength(0);
            FeatureVector[4] = (Math.Abs(totalY / total - shading.GetLength(0) / 2)) / (shading.GetLength(0) / 2);
            FeatureVector[5] = (FeatureVector[2] + FeatureVector[4]);

            // Bounding Box
            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = 0;
            double maxY = 0;

            for (int i = 0; i < shading.GetLength(0); i++)
            {
                for (int j = 0; j < shading.GetLength(1); j++)
                {
                    if (shading[i, j] > 0)
                    {
                        minX = Math.Min(minX, i);
                        minY = Math.Min(minY, j);
                        maxX = Math.Max(maxX, i);
                        maxY = Math.Max(maxY, j);
                    }
                }
            }

            FeatureVector[6] = (maxX - minX) / shading.GetLength(0);
            FeatureVector[7] = (maxY - minY) / shading.GetLength(1);
            FeatureVector[8] = (maxX - minX) * (maxY - minY) / (shading.GetLength(0) * shading.GetLength(1));
            FeatureVector[9] = (minX) / shading.GetLength(0);
            FeatureVector[10] = (minY) / shading.GetLength(1);
        }

        #endregion // Methods
    }
}
