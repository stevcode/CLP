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
    /// <summary>Extension methods for the <see cref="Stroke" /> class.</summary>
    public static class StrokeExtension
    {
        private static readonly Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid StrokeOwnerIDKey = new Guid("00000000-0000-0000-0000-000000000002");

        public static StrokeDTO ToStrokeDTO(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var strokeDTO = new StrokeDTO
                            {
                                ID = stroke.GetStrokeID(),
                                PersonID = stroke.GetStrokeOwnerID(),
                                Height = stroke.DrawingAttributes.Height,
                                Width = stroke.DrawingAttributes.Width,
                                IsHighlighter = stroke.DrawingAttributes.IsHighlighter,
                                FitToCurve = stroke.DrawingAttributes.FitToCurve,
                                IgnorePressure = stroke.DrawingAttributes.IgnorePressure,
                                Color = stroke.DrawingAttributes.Color.ToString(),
                                StylusTip = stroke.DrawingAttributes.StylusTip == StylusTip.Ellipse ? StylusTipType.Ellipse : StylusTipType.Rectangle
                            };

            var strokePoints = new StringBuilder();
            foreach (var strokePoint in stroke.StylusPoints)
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

        /// <summary>Scales a <see cref="Stroke" /> with respect to a center point.</summary>
        public static void Stretch(this Stroke stroke, double scaleX, double scaleY, double centerX, double centerY)
        {
            Argument.IsNotNull("stroke", stroke);

            if (double.IsPositiveInfinity(scaleX) ||
                double.IsNegativeInfinity(scaleX) ||
                double.IsNaN(scaleX))
            {
                scaleX = 1;
            }

            if (double.IsPositiveInfinity(scaleY) ||
                double.IsNegativeInfinity(scaleY) ||
                double.IsNaN(scaleY))
            {
                scaleY = 1;
            }

            var transform = new Matrix();
            transform.ScaleAt(scaleX, scaleY, centerX, centerY);
            stroke.Transform(transform, false);
        }

        /// <summary>Moves every <see cref="StylusPoint" /> in a <see cref="Stroke" /> by an offset.</summary>
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

        /// <summary>Finds the centroid of a stroke. The centroid calculation takes into account pressure sensitivity and ascribes more importance to points with higher pressure values.</summary>
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

        /// <summary>Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation. Distance Squared can still be used as a comparison for closeness.</summary>
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

        /// <summary>Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation. Distance Squared can still be used as a comparison for closeness.</summary>
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

        /// <summary>Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation. Distance Squared can still be used as a comparison for closeness.</summary>
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

        /// <summary>Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation. Distance Squared can still be used as a comparison for closeness.</summary>
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

        /// <summary>Distance Squared is much faster to calculate because Math.Sqrt is a fairly expensive operation. Distance Squared can still be used as a comparison for closeness.</summary>
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

        /// <summary>Signifies the stroke was on the page immediately after the historyAction at the given historyIndex was performed</summary>
        public static bool IsOnPageAtHistoryIndex<T>(this Stroke stroke, CLPPage page, int historyIndex) where T : IStrokesOnPageChangedHistoryAction
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("historyIndex", historyIndex);

            var orderedStrokesChangedHistoryActions = page.History.CompleteOrderedHistoryActions.OfType<T>().ToList();
            var strokeID = stroke.GetStrokeID();

            var addedAtAnyPointHistoryAction = orderedStrokesChangedHistoryActions.FirstOrDefault(h => h.StrokeIDsAdded.Contains(strokeID));
            var isPartOfHistory = addedAtAnyPointHistoryAction != null;

            var addedOrRemovedBeforeThisHistoryIndexHistoryAction =
                orderedStrokesChangedHistoryActions.LastOrDefault(h => (h.StrokeIDsAdded.Contains(strokeID) || h.StrokeIDsRemoved.Contains(strokeID)) && h.HistoryActionIndex <= historyIndex);

            var isOnPageBefore = addedOrRemovedBeforeThisHistoryIndexHistoryAction != null && addedOrRemovedBeforeThisHistoryIndexHistoryAction.StrokeIDsAdded.Contains(strokeID);

            return isOnPageBefore || !isPartOfHistory;
        }

        /// <summary>Signifies the stroke was added to the page between the given historyIndexes (including strokes that were added by the historyActions at both historyIndexes)</summary>
        public static bool IsAddedBetweenHistoryIndexes<T>(this Stroke stroke, CLPPage page, int startHistoryIndex, int endHistoryIndex) where T : IStrokesOnPageChangedHistoryAction
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("page", page);
            Argument.IsNotNull("startHistoryIndex", startHistoryIndex);
            Argument.IsNotNull("endHistoryIndex", endHistoryIndex);

            var orderedStrokesChangedHistoryActions = page.History.CompleteOrderedHistoryActions.OfType<ObjectsOnPageChangedHistoryAction>().ToList();
            var strokeID = stroke.GetStrokeID();

            var isAddedBetweenIndexes = orderedStrokesChangedHistoryActions.Any(h => h.StrokeIDsAdded.Contains(strokeID) && h.HistoryActionIndex >= startHistoryIndex && h.HistoryActionIndex <= endHistoryIndex);

            return isAddedBetweenIndexes;
        }

        #endregion // History

        #region Shape Detection

        public static double AbsoluteAngleBetweenStroke(this Stroke stroke, Stroke otherStroke)
        {
            return stroke.WeightedCenter().AbsoluteSlopeBetweenPointsInDegrees(otherStroke.WeightedCenter());
        }

        public static bool IsInvisiblySmall(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            const double MAX_STROKE_WEIGHT = 4.5;

            return stroke.StrokeWeight() <= MAX_STROKE_WEIGHT;
        }

        public static bool IsEnclosedShape(this Stroke stroke, CLPPage page = null)
        {
            Argument.IsNotNull("stroke", stroke);

            const int MIN_BOUNDS = 60;
            const double MIN_ASPECT_RATIO = 0.5;
            const double CELL_SIZE_RATIO = 5.0;

            if (stroke.GetBounds().Width < MIN_BOUNDS ||
                stroke.GetBounds().Height < MIN_BOUNDS)
            {
                return false;
            }

            double aspectRatio = stroke.GetBounds().Width / stroke.GetBounds().Height;

            if (aspectRatio < MIN_ASPECT_RATIO ||
                aspectRatio > (1.0 / MIN_ASPECT_RATIO))
            {
                return false;
            }

            var cellHeight = Math.Min(MIN_BOUNDS, (int)(stroke.GetBounds().Height / CELL_SIZE_RATIO));
            var cellWidth = Math.Min(MIN_BOUNDS, (int)(stroke.GetBounds().Width / CELL_SIZE_RATIO));

            var occupiedCells = FindCellsOccupiedByStroke(stroke, cellWidth, cellHeight, (int)stroke.GetBounds().X, (int)stroke.GetBounds().Y);
            if (page != null)
            {
                var strokeBounds = stroke.GetBounds();
                var tempGrid = new TemporaryGrid(page, strokeBounds.X, strokeBounds.Y, strokeBounds.Height, strokeBounds.Width, cellWidth, cellHeight, occupiedCells);
                page.PageObjects.Add(tempGrid);
            }

            // Debug.WriteLine("found " + occupiedCells.Count + " occupied cells");

            return DetectCycle(occupiedCells, cellWidth, cellHeight);
        }

        private static int RoundToNearestCell(double pos, int cellSize)
        {
            return (int)(pos / cellSize) * cellSize;
        }

        /*
            Need to do DFS for cycle detection
            TODO once we detect on multiple strokes, we'll have to pass in starting 
            points for DFS on each stroke to be extra careful in case there are 
            disconnected graphs
        */

        public static bool DetectCycle(List<Point> occupiedCells, int cellWidth, int cellHeight)
        {
            // var visited = new PointCollection();
            var cellStack = new Stack<List<Point>>();

            if (occupiedCells.Count() < 4)
            {
                return false;
            }
            var thisCell = occupiedCells.First(); //there has to be at least one occupied cell
            // var immediateAncestor = thisCell;
            var startCellList = new List<Point>();
            startCellList.Add(thisCell);

            cellStack.Push(startCellList);
            while (cellStack.Any())
            {
                var thisPath = cellStack.Pop();
                thisCell = thisPath.Last();

                var neighbors = GetNeighbors(thisCell, cellWidth, cellHeight);
                foreach (var neighbor in neighbors)
                {
                    if (occupiedCells.Contains(neighbor))
                    {
                        if (thisPath.Contains(neighbor))
                        {
                            var cycle = thisPath.Skip(thisPath.IndexOf(neighbor)).Take(thisPath.Count() - thisPath.IndexOf(neighbor)).ToArray();
                            if (CycleBoundsLargeEnough(cycle, 4 * cellWidth, 4 * cellHeight))
                            {
                                var i = 0;
                                while (i < cycle.Count())
                                {
                                    // Debug.WriteLine("{0}, {1}", cycle[i].X, cycle[i].Y);
                                    i++;
                                }
                                return true;
                            }
                        }
                        else
                        {
                            var neighborsNeighbors = GetNeighbors(neighbor, cellWidth, cellHeight);
                            int adj = 0;
                            foreach (var neighborNeighbor in neighborsNeighbors)
                            {
                                if (thisPath.Contains(neighborNeighbor))
                                {
                                    adj++;
                                }
                            }

                            if (adj < 3)
                            {
                                var newPath = new List<Point>();
                                newPath.AddRange(thisPath);
                                newPath.Add(neighbor);
                                cellStack.Push(newPath);
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool CycleBoundsLargeEnough(IEnumerable<Point> cycle, int widthThreshold, int heightThreshold)
        {
            int minX = 1000000, minY = 1000000;
            int maxX = 0, maxY = 0;

            foreach (var cell in cycle)
            {
                minX = Math.Min(minX, (int)cell.X);
                minY = Math.Min(minY, (int)cell.Y);
                maxX = Math.Max(maxX, (int)cell.X);
                maxY = Math.Max(maxY, (int)cell.Y);
            }
            return (maxX - minX >= widthThreshold && maxY - minY >= heightThreshold);
        }

        private static List<Point> GetNeighbors(Point thisPoint, int cellWidth, int cellHeight)
        {
            var neighbors = new List<Point>();
            neighbors.Add(new Point(thisPoint.X, thisPoint.Y + cellHeight));
            neighbors.Add(new Point(thisPoint.X, thisPoint.Y - cellHeight));
            neighbors.Add(new Point(thisPoint.X + cellWidth, thisPoint.Y));
            neighbors.Add(new Point(thisPoint.X - cellWidth, thisPoint.Y));

            neighbors.Add(new Point(thisPoint.X - cellWidth, thisPoint.Y + cellHeight));
            neighbors.Add(new Point(thisPoint.X + cellWidth, thisPoint.Y + cellHeight));
            neighbors.Add(new Point(thisPoint.X + cellWidth, thisPoint.Y - cellHeight));
            neighbors.Add(new Point(thisPoint.X - cellWidth, thisPoint.Y - cellHeight));

            // neighbors.Remove(immediateAncestor); // Make sure this works with object equality
            return neighbors;
        }

        public static List<Point> FindCellsOccupiedByStroke(Stroke stroke, int cellWidth, int cellHeight, int xOffset, int yOffset)
        {
            var occupiedCells = new List<Point>();
            int i = 1;
            var stylusPoints = stroke.StylusPoints;
            var thisPoint = stylusPoints[0].ToPoint();
            thisPoint.X = RoundToNearestCell(thisPoint.X, cellWidth) - xOffset;
            thisPoint.Y = RoundToNearestCell(thisPoint.Y, cellHeight) - yOffset;
            var nextPoint = new Point();
            while (i < stylusPoints.Count)
            {
                nextPoint = stylusPoints[i].ToPoint();
                // Debug.WriteLine("{0} = {1}", nextPoint.X, ((int)(nextPoint.X / CELL_SIZE)) * CELL_SIZE - xOffset) ;
                // Debug.WriteLine("{0} = {1}", nextPoint.Y, ((int)(nextPoint.Y / CELL_SIZE)) * CELL_SIZE - yOffset);
                nextPoint.X = RoundToNearestCell(nextPoint.X, cellWidth) - xOffset;
                nextPoint.Y = RoundToNearestCell(nextPoint.Y, cellHeight) - yOffset;

                // TODO the following is a complete guess about the shape of the curve
                // We can do better by using the bezzier curve, but this might not be easily exposed
                if (thisPoint.Y <= nextPoint.Y)
                {
                    int j = (int)thisPoint.Y;
                    while (j <= nextPoint.Y)
                    {
                        var occupiedCell = new Point((int)thisPoint.X, j);
                        occupiedCells.Add(occupiedCell);
                        j += cellHeight;
                    }
                }
                else
                {
                    int j = (int)nextPoint.Y;
                    while (j <= thisPoint.Y)
                    {
                        var occupiedCell = new Point((int)thisPoint.X, j);
                        occupiedCells.Add(occupiedCell);
                        j += cellHeight;
                    }
                }

                if (thisPoint.X <= nextPoint.X)
                {
                    int k = (int)thisPoint.X;
                    while (k <= nextPoint.X)
                    {
                        var occupiedCell = new Point(k, (int)thisPoint.Y);
                        occupiedCells.Add(occupiedCell);
                        k += cellWidth;
                    }
                }
                else
                {
                    int k = (int)nextPoint.X;
                    while (k <= thisPoint.X)
                    {
                        var occupiedCell = new Point(k, (int)nextPoint.Y);
                        occupiedCells.Add(occupiedCell);
                        k += cellWidth;
                    }
                }

                thisPoint = nextPoint;
                i++;
            }

            return occupiedCells.Distinct().ToList();
        }

        private static bool IsBoundsOverlappingByPercentage(Rect firstBounds, Rect secondBounds, double percentage)
        {
            var intersectRect = Rect.Intersect(firstBounds, secondBounds);
            return intersectRect.Area() / secondBounds.Area() >= percentage;
        }

        public static bool IsStrokeEnclosure(this Stroke stroke, StrokeCollection strokes)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("strokes", strokes);

            const double OVERLAP_PERCENTAGE_THRESHOLD = 75.0;

            if (!IsEnclosedShape(stroke))
            {
                return false;
            }

            var strokeBounds = stroke.GetBounds();

            foreach (var thisStroke in strokes)
            {
                if (IsInvisiblySmall(thisStroke))
                {
                    continue;
                }

                if (IsBoundsOverlappingByPercentage(strokeBounds, thisStroke.GetBounds(), OVERLAP_PERCENTAGE_THRESHOLD))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPageObjectEnclosure(this Stroke stroke, List<IPageObject> pageObjects)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("pageObjects", pageObjects);

            const double OVERLAP_PERCENTAGE_THRESHOLD = 75.0;

            if (!IsEnclosedShape(stroke))
            {
                return false;
            }

            var strokeBounds = stroke.GetBounds();

            foreach (var thisPageObject in pageObjects)
            {
                var pageObjectBounds = new Rect(thisPageObject.XPosition, thisPageObject.YPosition, thisPageObject.Width, thisPageObject.Height);
                if (IsBoundsOverlappingByPercentage(strokeBounds, pageObjectBounds, OVERLAP_PERCENTAGE_THRESHOLD))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsHorizontalLine(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);
            if (stroke.StylusPoints.Count < 2)
            {
                return false;
            }
            const double AVG_SLOPE_THRESHOLD_DEGREES = 15;
            const double VARIATION_THRESHOLD_DEGREES = 40; // this is high because there are many -90, 0, and 90 degree slopes

            var slopes = GetSlopesBetweenPoints(stroke);

            //DEBUG
            int i = 0;
            while (i < slopes.Count)
            {
                // Debug.WriteLine("slope: {0}", slopes[i]);
                i++;
            }

            double avg = slopes.Average();
            double variation = CalculateStdDev(slopes);
            // Debug.WriteLine("avg: {0}, stddev: {1}", avg, variation);

            return (Math.Abs(avg) <= AVG_SLOPE_THRESHOLD_DEGREES && variation <= VARIATION_THRESHOLD_DEGREES);
        }

        private static double CalculateStdDev(IEnumerable<double> values)
        {
            //found here: http://stackoverflow.com/questions/3141692/standard-deviation-of-generic-list
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        private static List<double> GetSlopesBetweenPoints(Stroke stroke)
        {
            var slopes = new List<double>();
            var stylusPoints = stroke.StylusPoints;
            var thisPoint = stylusPoints.First().ToPoint();
            var nextPoint = new Point();

            int i = 1;
            while (i < stylusPoints.Count)
            {
                nextPoint = stylusPoints[i].ToPoint();
                var slope = thisPoint.SlopeBetweenPointsInDegrees(nextPoint);
                slopes.Add(slope);
                thisPoint = nextPoint;
                i++;
            }

            return slopes;
        }

        public static bool IsVerticalLine(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);
            if (stroke.StylusPoints.Count < 2)
            {
                return false;
            }
            const double AVG_SLOPE_THRESHOLD_DEGREES = 15;
            //TODO somehow make this variation less? Might require modifying getSlopesBetweenPoints
            const double VARIATION_THRESHOLD_DEGREES = 40; // this is high because there are many -90, 0, and 90 degree slopes

            var slopes = GetSlopesBetweenPoints(stroke);

            //reformat for vertical slopes, so 0 is now facing upward
            int i = 0;
            while (i < slopes.Count)
            {
                slopes[i] = slopes[i] - 90;
                if (slopes[i] <= -90)
                {
                    slopes[i] += 180;
                }
                // Debug.WriteLine("slope: {0}", slopes[i]);
                i++;
            }

            double avg = slopes.Average();
            double variation = CalculateStdDev(slopes);
            // Debug.WriteLine("avg: {0}, stddev: {1}", avg, variation);

            return (Math.Abs(avg) <= AVG_SLOPE_THRESHOLD_DEGREES && variation <= VARIATION_THRESHOLD_DEGREES);
        }

        public static bool IsLine(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return IsHorizontalLine(stroke) || IsVerticalLine(stroke);
        }

        public static bool IsDot(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            const double MAX_STROKE_WEIGHT = 10.0;
            const double MAX_STROKE_BOUNDS = 5.0;

            var strokeBounds = stroke.GetBounds();

            // Debug.WriteLine("stroke weight: {0}", stroke.StrokeWeight());

            return stroke.StrokeWeight() <= MAX_STROKE_WEIGHT && strokeBounds.Height <= MAX_STROKE_BOUNDS && strokeBounds.Width <= MAX_STROKE_BOUNDS;
        }

        public static bool IsCircle(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return false;
        }

        public static bool IsVerticalStroke(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var strokeTop = stroke.GetBounds().Top;
            var strokeBottom = stroke.GetBounds().Bottom;
            var strokeLeft = stroke.GetBounds().Left;
            var strokeRight = stroke.GetBounds().Right;

            return Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom);
        }

        #endregion // Shape Detection
    }
}