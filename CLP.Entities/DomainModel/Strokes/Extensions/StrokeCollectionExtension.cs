using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel;

namespace CLP.Entities.Demo
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

        public static bool IsEnclosedShape(this IEnumerable<Stroke> strokes, CLPPage page = null)
        {
            Argument.IsNotNull("strokes", strokes);

            const int MIN_BOUNDS = 60;
            const double MIN_ASPECT_RATIO = 0.5;
            const double CELL_SIZE_RATIO = 5.0;

            var bounds = GetBounds(strokes);

            if (bounds.Width < MIN_BOUNDS || bounds.Height < MIN_BOUNDS)
            {
                return false;
            }

            double aspectRatio = bounds.Width / bounds.Height;

            if (aspectRatio < MIN_ASPECT_RATIO || aspectRatio > (1.0 / MIN_ASPECT_RATIO))
            {
                return false;
            }

            var cellHeight = Math.Min(MIN_BOUNDS, (int)(bounds.Height / CELL_SIZE_RATIO));
            var cellWidth = Math.Min(MIN_BOUNDS, (int)(bounds.Width / CELL_SIZE_RATIO));

            var occupiedCells = new List<Point>();

            foreach (var stroke in strokes) 
            {
                var theseOccupiedCells = StrokeExtension.FindCellsOccupiedByStroke(stroke, cellWidth, cellHeight, (int)bounds.X, (int)bounds.Y);
                occupiedCells.AddRange(theseOccupiedCells);
            }
            occupiedCells = occupiedCells.Distinct().ToList();

            if (page != null)
            {
                var tempGrid = new TemporaryGrid(page, bounds.X, bounds.Y, bounds.Height, bounds.Width, cellWidth, cellHeight, occupiedCells);
                page.PageObjects.Add(tempGrid);
            }

            Console.WriteLine("found " + occupiedCells.Count + " occupied cells");

            return StrokeExtension.DetectCycle(occupiedCells, cellWidth, cellHeight);

        }

        private static Rect GetBounds(this IEnumerable<Stroke> strokes)
        {
            double minX = 1000000, minY = 1000000;
            double maxX = 0, maxY = 0;
            foreach( var stroke in strokes) 
            {
                var bounds = stroke.GetBounds();
                minX = Math.Min(minX, bounds.X);
                minY = Math.Min(minY, bounds.Y);
                maxX = Math.Max(maxX, bounds.X + bounds.Width);
                maxY = Math.Max(maxY, bounds.Y + bounds.Height);
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        #endregion // Shape Detection
    }
}