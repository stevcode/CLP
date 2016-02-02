using System;
using System.Windows;

namespace CLP.Entities
{
    public static class PointExtensions
    {
        public static double SlopeInDegrees(this Point p1, Point p2)
        {
            var deltaX = p2.X - p1.X;
            var deltaY = p1.Y - p2.Y; // Reversed because the Y coords in C# run from top to bottom.

            var angleInDegrees = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
            return angleInDegrees;
        }
    }
}