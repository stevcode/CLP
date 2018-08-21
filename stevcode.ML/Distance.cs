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
        public static double EuclideanDistance(List<double> a, List<double> b, bool isFastDistance = true)
        {
            var total = a.Select((t, i) => t - b[i]).Sum(difference => difference * difference);

            return isFastDistance ? total : Math.Sqrt(total);
        }

        public static double ManhattanDistance(List<double> a, List<double> b)
        {
            return a.Select((t, i) => Math.Abs(t - b[i])).Sum();
        }

        /// <summary>
        /// Calculates the similarity between 2 points using distance.
        /// Returns a value between 0 and 1 where 1 means they are identical
        /// </summary>
        public static double Similarity(double distance)
        {
            return 1 / (1 + distance);
        }
    }
}
