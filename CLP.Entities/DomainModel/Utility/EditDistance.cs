using System;
using System.Collections.Generic;
using Catel;

namespace CLP.Entities
{
    // Adapted From: https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/
    public static class EditDistance
    {
        /// <SUMMARY>Computes the Levenshtein Edit Distance between two enumerables.</SUMMARY>
        /// <TYPEPARAM name="T">The type of the items in the enumerables.</TYPEPARAM>
        /// <PARAM name="x">The first enumerable.</PARAM>
        /// <PARAM name="y">The second enumerable.</PARAM>
        /// <RETURNS>The edit distance.</RETURNS>
        public static int Compute<T>(IEnumerable<T> x, IEnumerable<T> y) where T : IEquatable<T>
        {
            // Validate parameters
            Argument.IsNotNull("x", x);
            Argument.IsNotNull("y", y);

            // Convert the parameters into IList instances
            // in order to obtain indexing capabilities

            var first = x as IList<T> ?? new List<T>(x);
            var second = y as IList<T> ?? new List<T>(y);

            // Get the length of both. If either is 0, return
            // the length of the other, since that number of insertions
            // would be required.
            var firstLength = first.Count;
            var secondLength = second.Count;

            if (firstLength == 0)
            {
                return secondLength;
            }

            if (secondLength == 0)
            {
                return firstLength;
            }

            // Rather than maintain an entire matrix (which would require O(n*m) space),
            // just store the current row and the next row, each of which has a length m+1,
            // so just O(m) space. Initialize the current row.
            var currentRow = 0;
            var nextRow = 1;

            var rows = new[] { new int[secondLength + 1], new int[secondLength + 1] };

            for (var j = 0; j <= secondLength; ++j)
            {
                rows[currentRow][j] = j;
            }

            // For each virtual row (since we only have physical storage for two)
            for (var i = 1; i <= firstLength; ++i)
            {
                // Fill in the values in the row
                rows[nextRow][0] = i;

                for (var j = 1; j <= secondLength; ++j)
                {
                    var dist1 = rows[currentRow][j] + 1;
                    var dist2 = rows[nextRow][j - 1] + 1;
                    var dist3 = rows[currentRow][j - 1] + (first[i - 1].Equals(second[j - 1]) ? 0 : 1);

                    rows[nextRow][j] = Math.Min(dist1, Math.Min(dist2, dist3));
                }

                // Swap the current and next rows
                if (currentRow == 0)
                {
                    currentRow = 1;
                    nextRow = 0;
                }
                else
                {
                    currentRow = 0;
                    nextRow = 1;
                }
            }

            // Return the computed edit distance
            return rows[currentRow][secondLength];
        }
    }
}