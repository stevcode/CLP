using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using Catel;

namespace CLP.Entities
{
    /// <summary>Extension methods for the <see cref="StrokeCollectionExtension" /> class.</summary>
    public static class StrokeCollectionExtension
    {
        #region Transformation

        public static void StretchAll(this IEnumerable<Stroke> strokes, double scaleX, double scaleY, double centerX, double centerY)
        {
            Argument.IsNotNull("strokes", strokes);

            foreach (var stroke in strokes)
            {
                if (stroke == null)
                {
                    Console.WriteLine("Null stroke in StrokeCollection of StretchAll");
                    continue;
                }

                stroke.Stretch(scaleX, scaleY, centerX, centerY);
            }
        }

        public static void MoveAll(this IEnumerable<Stroke> strokes, double deltaX, double deltaY)
        {
            Argument.IsNotNull("strokes", strokes);

            foreach (var stroke in strokes)
            {
                if (stroke == null)
                {
                    Console.WriteLine("Null stroke in StrokeCollection of MoveAll");
                    continue;
                }

                stroke.Move(deltaX, deltaY);
            }
        }

        public static void RotateAll(this IEnumerable<Stroke> strokes, double angle, double centerX, double centerY, double offsetX, double offsetY)
        {
            Argument.IsNotNull("strokes", strokes);

            foreach (var stroke in strokes)
            {
                if (stroke == null)
                {
                    Console.WriteLine("Null stroke in StrokeCollection of RotateAll");
                    continue;
                }

                stroke.Rotate(angle, centerX, centerY, offsetX, offsetY);
            }
        }

        #endregion //Transformation

        #region HitTesting

        public static double StrokesWeight(this IEnumerable<Stroke> strokes) { return strokes.Sum(stroke => stroke.StrokeWeight()); }

        #endregion // HitTesting
    }
}