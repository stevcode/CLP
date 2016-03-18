using System;
using System.Windows;

namespace CLP.Entities
{
    public static class PointExtensions
    {
        public static double AbsoluteSlopeBetweenPointsInDegrees(this Point p1, Point p2)
        {
            var slope = SlopeBetweenPointsInDegrees(p1, p2);
            return Math.Abs(slope);
        }

        // Uses the point on the left as the origin and finds the arc to the other point, from -90 to 90.
        public static double SlopeBetweenPointsInDegrees(this Point p1, Point p2)
        {
            var deltaX = p2.X - p1.X;
            var deltaY = p1.Y - p2.Y; // Reversed because the Y coords in C# run from top to bottom.

            if (Math.Abs(deltaY) < 0.0001)
            {
                return 0.0;
            }

            if (Math.Abs(deltaX) < 0.0001)
            {
                return p1.Y < p2.Y ? -90.0 : 90.0;
            }

            var angleInDegrees = Math.Atan(deltaY / deltaX) * 180.0 / Math.PI;
            return angleInDegrees;
        }

        // Uses p1 as origin and finds the arc to the other point, from 0 to 360.
        public static double SlopeToOtherPointInDegrees(this Point p1, Point p2)
        {
            var deltaX = p2.X - p1.X;
            var deltaY = p1.Y - p2.Y; // Reversed because the Y coords in C# run from top to bottom.

            var angleInDegrees = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
            return angleInDegrees;
        }
    }
}