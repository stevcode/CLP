using System;
using System.Collections.Generic;
using System.Linq;

namespace stevcode.ML
{
    public static class Distance
    {
        /// <summary>
        /// Calculates the Euclidean Distance in n-dimensions.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="isFastDistance">Doesn't return the square root of the total, instead just returns a value that is proportionally far away.</param>
        /// <returns></returns>
        public static double EuclideanDistance(List<double> a, List<double> b, bool isFastDistance)
        {
            var total = a.Select((t, i) => t - b[i]).Sum(difference => difference * difference);

            return isFastDistance ? total : Math.Sqrt(total);
        }
    }
}
