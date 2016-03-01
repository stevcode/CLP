using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Catel;

namespace CLP.Entities
{
    /// <summary>
    /// Extension methods for the <see cref="Stroke" /> class.
    /// </summary>
    public static class StrokeExtension
    {
        private static readonly Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid StrokeOwnerIDKey = new Guid("00000000-0000-0000-0000-000000000002");
        private static readonly Guid StrokeVersionIndexKey = new Guid("00000000-0000-0000-0000-000000000003");
        private static readonly Guid StrokeDifferentiationGroupKey = new Guid("00000000-0000-0000-0000-000000000004");

        public static StrokeDTO ToStrokeDTO(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var strokeDTO = new StrokeDTO
                            {
                                ID = stroke.GetStrokeID(),
                                PersonID = stroke.GetStrokeOwnerID(),
                                //DifferentiationGroup = stroke.GetStrokeDifferentiationGroup(),
                                Height = stroke.DrawingAttributes.Height,
                                Width = stroke.DrawingAttributes.Width,
                                IsHighlighter = stroke.DrawingAttributes.IsHighlighter,
                                FitToCurve = stroke.DrawingAttributes.FitToCurve,
                                IgnorePressure = stroke.DrawingAttributes.IgnorePressure,
                                Color = stroke.DrawingAttributes.Color.ToString(),
                                StylusTip = stroke.DrawingAttributes.StylusTip == StylusTip.Ellipse ? StylusTipType.Ellipse : StylusTipType.Rectangle
                            };

            var strokePoints = new StringBuilder();
            foreach(StylusPoint strokePoint in stroke.StylusPoints)
            {
                strokePoints.Append(strokePoint.X);
                strokePoints.Append(':');
                strokePoints.Append(strokePoint.Y);
                strokePoints.Append(':');
                strokePoints.Append(strokePoint.PressureFactor);
                strokePoints.Append(",");
            }
            strokePoints.Remove(strokePoints.Length - 1, 1);
            strokeDTO.StrokePoints = strokePoints.ToString();

            return strokeDTO;
        }

        #region ExtendedProperties

        public static bool HasStrokeID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return stroke.ContainsPropertyData(StrokeIDKey);
        }

        public static string GetStrokeID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            if (stroke.ContainsPropertyData(StrokeIDKey))
            {
                return stroke.GetPropertyData(StrokeIDKey) as string;
            }

            return "noStrokeID";
        }

        public static void SetStrokeID(this Stroke stroke, string uniqueID)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeIDKey, uniqueID);
        }

        public static string GetStrokeOwnerID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            if (stroke.ContainsPropertyData(StrokeOwnerIDKey))
            {
                return stroke.GetPropertyData(StrokeOwnerIDKey) as string;
            }

            return "noStrokeOwnerID";
        }

        public static void SetStrokeOwnerID(this Stroke stroke, string uniqueID)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeOwnerIDKey, uniqueID);
        }

        public static string GetStrokeDifferentiationGroup(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            if (stroke.ContainsPropertyData(StrokeDifferentiationGroupKey))
            {
                return stroke.GetPropertyData(StrokeDifferentiationGroupKey) as string;
            }

            return "noStrokeDifferentiationGroup";
        }

        public static void SetStrokeDifferentiationGroup(this Stroke stroke, string group)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeDifferentiationGroupKey, group);
        }

        public static string GetStrokeVersionIndex(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);


            if (stroke.ContainsPropertyData(StrokeVersionIndexKey))
            {
                return stroke.GetPropertyData(StrokeVersionIndexKey) as string;
            }

            return "noStrokeVersionIndex";
        }

        public static void SetStrokeVersionIndex(this Stroke stroke, int index)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeVersionIndexKey, index);
        }

        #endregion //ExtendedProperties

        #region Equality

        public static bool IsEqualByID(this Stroke stroke, Stroke otherStroke)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("otherStroke", otherStroke);

            var strokeID = stroke.GetStrokeID();
            var otherID = otherStroke.GetStrokeID();

            return strokeID == otherID && strokeID != "noStrokeID" && otherID != "noStrokeID";
        } 

        #endregion // Equality

        #region Transformation

        /// <summary>
        /// Scales a <see cref="Stroke" /> with respect to a center point.
        /// </summary>
        public static void Stretch(this Stroke stroke, double scaleX, double scaleY, double centerX, double centerY)
        {
            Argument.IsNotNull("stroke", stroke);

            var transform = new Matrix();
            transform.ScaleAt(scaleX, scaleY, centerX, centerY);
            stroke.Transform(transform, false);
        }

        /// <summary>
        /// Moves every <see cref="StylusPoint" /> in a <see cref="Stroke" /> by an offset.
        /// </summary>
        public static void Move(this Stroke stroke, double deltaX, double deltaY)
        {
            Argument.IsNotNull("stroke", stroke);

            var transform = new Matrix();
            transform.Translate(deltaX, deltaY);
            stroke.Transform(transform, true);
        }

        public static void Rotate(this Stroke stroke, double angle, double centerX, double centerY, double offsetX, double offsetY)
        {
            Argument.IsNotNull("stroke", stroke);

            var transform = new Matrix();
            transform.RotateAt(90, centerX, centerY);
            transform.Translate(offsetX, offsetY);
            stroke.Transform(transform, false);
        }

        #endregion //Transformation

        #region HitTesting

        public static double PercentContainedByBounds(this Stroke stroke, Rect bounds)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("bounds", bounds);

            var da = stroke.DrawingAttributes;
            var stylusPoints = stroke.StylusPoints;
            var weightContained = 0.0;
            var weightNotContained = 0.0;
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

                if (bounds.Contains((Point)stylusPoints[i]))
                {
                    weightContained += pointWeight;
                }
                else
                {
                    weightNotContained += pointWeight;
                }
            }

            var totalWeight = weightContained + weightNotContained;
            return weightContained / totalWeight;
        }

        public static double StrokeWeight(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var da = stroke.DrawingAttributes;
            var stylusPoints = stroke.StylusPoints;
            var weight = 0.0;
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

                weight += pointWeight;
            }

            return weight;
        }

        /// <summary>
        /// Finds the centroid of a stroke. The centroid calculation takes into account pressure
        /// sensitivity and ascribes more importance to points with higher pressure values.
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns></returns>
        public static Point WeightedCenter(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var strokeWeight = stroke.StrokeWeight();
            var weightedXAverage = 0.0;
            var weightedYAverage = 0.0;

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

                var importance = pointWeight / strokeWeight;
                weightedXAverage += importance * stylusPoints[i].X;
                weightedYAverage += importance * stylusPoints[i].Y;
            }

            return new Point(weightedXAverage, weightedYAverage);
        }

        #endregion //HitTesting

        #region Distances Squared

        /// <summary>
        /// Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation.
        /// Distance Squared can still be used as a comparison for closeness.
        /// </summary>
        public static double DistanceSquaredByCenter(this Stroke stroke, Stroke otherStroke)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("otherStroke", otherStroke);

            var center = stroke.GetBounds().Center();
            var otherCenter = otherStroke.GetBounds().Center();

            var deltaX = center.X - otherCenter.X;
            var deltaY = center.Y - otherCenter.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            return distanceSquared;
        }

        /// <summary>
        /// Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation.
        /// Distance Squared can still be used as a comparison for closeness.
        /// </summary>
        public static double DistanceSquaredByWeightedCenter(this Stroke stroke, Stroke otherStroke)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("otherStroke", otherStroke);

            var center = stroke.WeightedCenter();
            var otherCenter = otherStroke.WeightedCenter();

            var deltaX = center.X - otherCenter.X;
            var deltaY = center.Y - otherCenter.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            return distanceSquared;
        }

        /// <summary>
        /// Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation.
        /// Distance Squared can still be used as a comparison for closeness.
        /// </summary>
        public static double DistanceSquaredByClosestPoint(this Stroke stroke, Stroke otherStroke)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("otherStroke", otherStroke);

            var smallestDistanceSquared = (from point in stroke.StylusPoints
                                           from otherPoint in otherStroke.StylusPoints
                                           let deltaX = point.X - otherPoint.X
                                           let deltaY = point.Y - otherPoint.Y
                                           select (deltaX * deltaX) + (deltaY * deltaY)).Min();

            return smallestDistanceSquared;
        }

        /// <summary>
        /// Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation.
        /// Distance Squared can still be used as a comparison for closeness.
        /// </summary>
        public static double DistanceSquaredByAveragePointDistance(this Stroke stroke, Stroke otherStroke)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("otherStroke", otherStroke);

            var allDistanceSquaredTotal = 0.0;
            foreach (var point in stroke.StylusPoints)
            {
                var singleDistanceSquaredTotal = 0.0;
                foreach (var otherPoint in otherStroke.StylusPoints)
                {
                    var deltaX = point.X - otherPoint.X;
                    var deltaY = point.Y - otherPoint.Y;
                    var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
                    singleDistanceSquaredTotal += distanceSquared;
                }
                var singleAverageDistanceSquared = singleDistanceSquaredTotal / otherStroke.StylusPoints.Count;
                allDistanceSquaredTotal += singleAverageDistanceSquared;
            }
            var allAverageDistanceSquared = allDistanceSquaredTotal / stroke.StylusPoints.Count;

            return allAverageDistanceSquared;
        }

        /// <summary>
        /// Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation.
        /// Distance Squared can still be used as a comparison for closeness.
        /// </summary>
        public static double DistanceSquaredByAveragePointDistanceOfStrokeHalves(this Stroke stroke, Stroke otherStroke)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("otherStroke", otherStroke);

            var halfWayIndex = stroke.StylusPoints.Count / 2;
            Stroke strokeFrontHalf;
            Stroke strokeBackHalf;
            if (halfWayIndex == 0)
            {
                strokeFrontHalf = stroke;
                strokeBackHalf = stroke;
            }
            else
            {
                strokeFrontHalf = new Stroke(new StylusPointCollection(stroke.StylusPoints.Take(halfWayIndex)));
                strokeBackHalf = new Stroke(new StylusPointCollection(stroke.StylusPoints.Skip(halfWayIndex)));
            }

            var otherHalfWayIndex = otherStroke.StylusPoints.Count / 2;
            Stroke otherStrokeFrontHalf;
            Stroke otherStrokeBackHalf;
            if (otherHalfWayIndex == 0)
            {
                otherStrokeFrontHalf = otherStroke;
                otherStrokeBackHalf = otherStroke;
            }
            else
            {
                otherStrokeFrontHalf = new Stroke(new StylusPointCollection(otherStroke.StylusPoints.Take(otherHalfWayIndex)));
                otherStrokeBackHalf = new Stroke(new StylusPointCollection(otherStroke.StylusPoints.Skip(otherHalfWayIndex)));
            }

            var averagePointDistances = new List<double>
                                        {
                                            strokeFrontHalf.DistanceSquaredByAveragePointDistance(otherStrokeFrontHalf),
                                            strokeFrontHalf.DistanceSquaredByAveragePointDistance(otherStrokeBackHalf),
                                            strokeBackHalf.DistanceSquaredByAveragePointDistance(otherStrokeFrontHalf),
                                            strokeBackHalf.DistanceSquaredByAveragePointDistance(otherStrokeBackHalf)
                                        };

            var minAveragePointDistance = averagePointDistances.Min();

            return minAveragePointDistance;
        }

        public static Stroke FindClosestStroke(this Stroke stroke, List<Stroke> strokes)
        {
            Stroke closestStroke = null;
            foreach (var otherStroke in strokes)
            {
                if (otherStroke == stroke)
                {
                    continue;
                }

                if (closestStroke == null)
                {
                    closestStroke = otherStroke;
                    continue;
                }

                closestStroke = stroke.FindCloserStroke(closestStroke, otherStroke);
            }

            return closestStroke;
        }

        public static Stroke FindCloserStroke(this Stroke stroke, Stroke stroke1, Stroke stroke2)
        {
            //var d1 = stroke.DistanceSquaredByCenter(stroke1);
            //var d2 = stroke.DistanceSquaredByWeightedCenter(stroke1);
            //var d3 = stroke.DistanceSquaredByClosestPoint(stroke1);
            //var d4 = stroke.DistanceSquaredByAveragePointDistance(stroke1);
            //var d5 = stroke.DistanceSquaredByAveragePointDistanceOfStrokeHalves(stroke1);

            //var o1 = stroke.DistanceSquaredByCenter(stroke2);
            //var o2 = stroke.DistanceSquaredByWeightedCenter(stroke2);
            //var o3 = stroke.DistanceSquaredByClosestPoint(stroke2);
            //var o4 = stroke.DistanceSquaredByAveragePointDistance(stroke2);
            //var o5 = stroke.DistanceSquaredByAveragePointDistanceOfStrokeHalves(stroke2);

            //var score = 0;
            //if (d1 < o1)
            //{
            //    score++;
            //}
            //if (d2 < o2)
            //{
            //    score++;
            //}
            //if (d3 < o3)
            //{
            //    score++;
            //}
            //if (d4 < o4)
            //{
            //    score++;
            //}
            //if (d5 < o5)
            //{
            //    score++;
            //}

            //return score >= 3 ? stroke1 : stroke2;

            return stroke.DistanceSquaredByClosestPoint(stroke1) < stroke.DistanceSquaredByClosestPoint(stroke2) ? stroke1 : stroke2;
        }

        #endregion // Distances

        #region History

        public static Stroke GetStrokeCopyAtHistoryIndex(this Stroke stroke, CLPPage page, int historyIndex)
        {
            return stroke.Clone();
        }

        /// <summary>
        /// Signifies the stroke was on the page immediately after the historyItem at the given historyIndex was performed
        /// </summary>
        public static bool IsOnPageAtHistoryIndex(this Stroke stroke, CLPPage page, int historyIndex)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var orderedObjectsChangedHistoryItems = page.History.CompleteOrderedHistoryItems.OfType<ObjectsOnPageChangedHistoryItem>().ToList();
            var strokeID = stroke.GetStrokeID();

            var addedAtAnyPointHistoryItem = orderedObjectsChangedHistoryItems.FirstOrDefault(h => h.StrokeIDsAdded.Contains(strokeID));
            var isPartOfHistory = addedAtAnyPointHistoryItem != null;

            var addedOrRemovedBeforeThisHistoryIndexHistoryItem =
                orderedObjectsChangedHistoryItems.LastOrDefault(h => (h.StrokeIDsAdded.Contains(strokeID) || h.StrokeIDsRemoved.Contains(strokeID)) && h.HistoryIndex <= historyIndex);

            var isOnPageBefore = addedOrRemovedBeforeThisHistoryIndexHistoryItem != null && addedOrRemovedBeforeThisHistoryIndexHistoryItem.StrokeIDsAdded.Contains(strokeID);

            return isOnPageBefore || !isPartOfHistory;
        }

        /// <summary>
        /// Signifies the stroke was added to the page between the given historyIndexes (including strokes that were added by the
        /// historyItems at both historyIndexes)
        /// </summary>
        public static bool IsAddedBetweenHistoryIndexes(this Stroke stroke, CLPPage page, int startHistoryIndex, int endHistoryIndex)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("startHistoryIndex", startHistoryIndex);
            Argument.IsNotNull("endHistoryIndex", endHistoryIndex);

            var orderedObjectsChangedHistoryItems = page.History.CompleteOrderedHistoryItems.OfType<ObjectsOnPageChangedHistoryItem>().ToList();
            var strokeID = stroke.GetStrokeID();

            var isAddedBetweenIndexes = orderedObjectsChangedHistoryItems.Any(h => h.StrokeIDsAdded.Contains(strokeID) && h.HistoryIndex >= startHistoryIndex && h.HistoryIndex <= endHistoryIndex);

            return isAddedBetweenIndexes;
        }

        #endregion // History

        #region Shape Detection

        public static double AbsoluteAngleBetweenStroke(this Stroke stroke, Stroke otherStroke) { return stroke.WeightedCenter().AbsoluteSlopeBetweenPointsInDegrees(otherStroke.WeightedCenter()); }

        public static bool IsInvisiblySmall(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            const double MAX_STROKE_WEIGHT = 4.5;

            return stroke.StrokeWeight() <= MAX_STROKE_WEIGHT;
        }

        public static bool IsEnclosedShape(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);
            // ALEX
            return false;
        }

        public static bool IsStrokeEnclosure(this Stroke stroke, StrokeCollection strokes)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("strokes", strokes);

            return false;
        }

        public static bool IsPageObjectEnclosure(this Stroke stroke, List<IPageObject> pageObjects)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("pageObjects", pageObjects);

            return false;
        }

        public static bool IsHorizontalLine(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return false;
        }

        public static bool IsVerticalLine(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return false;
        }

        public static bool IsLine(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return false;
        }

        public static bool IsDot(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return false;
        }

        public static bool IsCircle(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return false;
        }

        #endregion // Shape Detection
    }
}