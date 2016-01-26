using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Ink;
using System.Windows.Input;

namespace CLP.InkInterpretation
{
    public class ClusterPoint
    {
        public const double UNDEFINED = -1.0;

        public ClusterPoint(StylusPoint p)
        {
            _point = p;
            CoreDistanceSquared = UNDEFINED;
            ReachabilityDistanceSquared = UNDEFINED;
            IsProcessed = false;
        }

        private StylusPoint _point;

        public double X { get { return _point.X; } }
        public double Y { get { return _point.Y; } }

        public double CoreDistanceSquared { get; set; }
        public double ReachabilityDistanceSquared { get; set; }
        public bool IsProcessed { get; set; }

        // Distance squared is faster to calculate (by an order of magnitude) and is still accurate as a comparison of distance.
        public double EuclideanDistanceSquared(ClusterPoint p)
        {
            var diffX = p.X - X;
            var diffY = p.Y - Y;
            return diffX * diffX + diffY * diffY;
        }

        public double ManhattanDistance(ClusterPoint p)
        {
            var diffX = p.X - X;
            var diffY = p.Y - Y;
            return diffX + diffY;
        }
    }

    public static class InkClustering
    {


        // https://en.wikipedia.org/wiki/OPTICS_algorithm
        // https://github.com/Euphoric/OpticsClustering
        //  epsilon needs to be figured out experimentally. It is the maximum distance (radius) away from a point to consider another point part of the current cluster.
        // probably needs to be adjusted based on average properties of the current page, or possibly the current student. epsilon could be infinity, meant to limit
        // complexity  of the scan, should be called max_epsilon. set to width/heigh of page?
        //
        // min cluster size should be the number of points in the shortest stroke
        public static void OPTICS_Clustering(List<StylusPoint> points, double epsilon, int minClusterSize = 1)
        {
            var clusterPoints = points.Select(p => new ClusterPoint(p)).ToList();
            var unprocessedPoints = clusterPoints.ToList();
            var epsilonSquared = epsilon * epsilon; //Squared to correctly compare against EuclideanDistanceSquared.

            foreach (var p in clusterPoints)
            {
                if (p.IsProcessed)
                {
                    continue;
                }

                p.ReachabilityDistanceSquared = ClusterPoint.UNDEFINED;
                var neighborhood = GetNeighbors(p, epsilonSquared, clusterPoints, minClusterSize);
                p.IsProcessed = true;

                if (p.CoreDistanceSquared == ClusterPoint.UNDEFINED)
                {
                    continue;
                }

                Update(p, neighborhood);
            }
        }

        private static void Update(ClusterPoint p1, List<ClusterPoint> neighborhood)
        {
            foreach (var p2 in neighborhood)
            {
                if (p2.IsProcessed)
                {
                    continue;
                }

                var newReachabilityDistanceSquared = Math.Max(p1.CoreDistanceSquared, p1.EuclideanDistanceSquared(p2));

                if (p2.ReachabilityDistanceSquared == ClusterPoint.UNDEFINED)
                {
                    p2.ReachabilityDistanceSquared = newReachabilityDistanceSquared;
                    //?
                }
                else if (newReachabilityDistanceSquared < p2.ReachabilityDistanceSquared)
                {
                    p2.ReachabilityDistanceSquared = newReachabilityDistanceSquared;
                    //?
                }
            }
        }

        // See github Optics Clustering for better optimization.
        private static List<ClusterPoint> GetNeighbors(ClusterPoint p1, double epsilonSquared, List<ClusterPoint> points, int minClusterSize)
        {
            var neighborhood = new List<ClusterPoint>();
            foreach (var p2 in points)
            {
                var distanceSquared = p1.EuclideanDistanceSquared(p2);
                if (distanceSquared <= epsilonSquared)
                {
                    neighborhood.Add(p2);
                }
            }

            if (neighborhood.Count < minClusterSize)
            {
                p1.CoreDistanceSquared = ClusterPoint.UNDEFINED;
                return neighborhood;
            }
            var orderedNeighborhood = neighborhood.OrderBy(p1.EuclideanDistanceSquared).ToList();

            p1.CoreDistanceSquared = orderedNeighborhood[minClusterSize - 1].EuclideanDistanceSquared(p1);

            return orderedNeighborhood;
        }
    }
}
