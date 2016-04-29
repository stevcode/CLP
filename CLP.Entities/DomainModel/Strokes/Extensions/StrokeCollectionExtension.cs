using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        public static Point WeightedCentroid(this IEnumerable<Stroke> strokes)
        {
            Argument.IsNotNull("strokes", strokes);

            var strokesList = strokes as IList<Stroke> ?? strokes.ToList();
            var allStrokesWeight = strokesList.StrokesWeight();
            var weightedXAverage = 0.0;
            var weightedYAverage = 0.0;

            foreach (var stroke in strokesList)
            {
                var da = stroke.DrawingAttributes;
                var stylusPoints = stroke.StylusPoints;
                for (var i = 0; i < stylusPoints.Count; i++)
                {
                    var pointWeight = 0.0;
                    if (i == 0)
                    {
                        pointWeight += Math.Sqrt(da.Width * da.Width + da.Height * da.Height) / 2.0;
                    }
                    else
                    {
                        var spine = (Point)stylusPoints[i] - (Point)stylusPoints[i - 1];
                        pointWeight += Math.Sqrt(spine.LengthSquared) / 2.0;
                    }

                    if (i == stylusPoints.Count - 1)
                    {
                        pointWeight += Math.Sqrt(da.Width * da.Width + da.Height * da.Height) / 2.0;
                    }
                    else
                    {
                        var spine = (Point)stylusPoints[i + 1] - (Point)stylusPoints[i];
                        pointWeight += Math.Sqrt(spine.LengthSquared) / 2.0;

                    }

                    var importance = pointWeight / allStrokesWeight;
                    weightedXAverage += importance * stylusPoints[i].X;
                    weightedYAverage += importance * stylusPoints[i].Y;
                }
            }

            return new Point(weightedXAverage, weightedYAverage);
        }

        #endregion // HitTesting

        #region Shape Detection

        public static bool IsEnclosedShape(this IEnumerable<Stroke> strokes)
        {
            Argument.IsNotNull("strokes", strokes);

            return false;
        }

        #endregion // Shape Detection
    }
}