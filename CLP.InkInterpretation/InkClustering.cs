using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using Windows.Foundation;
using Priority_Queue;

namespace CLP.InkInterpretation
{
    public class ClusterPoint : PriorityQueueNode
    {
        public const double UNDEFINED = -1.0;

        public ClusterPoint(StylusPoint p)
        {
            _point = p;
            CoreDistanceSquared = UNDEFINED;
            ReachabilityDistanceSquared = UNDEFINED;
            IsProcessed = false;
        }

        public StylusPoint _point;

        public double X { get { return _point.X; } }
        public double Y { get { return _point.Y; } }

        public double CoreDistanceSquared { get; set; }
        public double ReachabilityDistanceSquared { get; set; }
        public double ReachabilityDistance {  get { return Math.Sqrt(ReachabilityDistanceSquared); } }
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

        public bool IsEqualToStylusPoint(StylusPoint p) { return p.X == X && p.Y == Y; }
    }

    public static class InkClustering
    {
        // TODO: ignore strokes that were used to highlight the entire page, or more than half?
        public static List<StrokeCollection> ClusterStrokes(StrokeCollection strokes)
        {
            var allStrokePoints = strokes.SelectMany(s => s.StylusPoints).ToList();
            var processedPoints = OPTICS_Clustering(allStrokePoints, 1000, 2);

            if (!processedPoints.Any())
            {
                //ERROR?
                return new List<StrokeCollection>();
            }

            var normalizedReachabilityPlot = processedPoints.Select(p => new Point(0, p.ReachabilityDistance)).Skip(1).ToList();
            var rawData = new double[normalizedReachabilityPlot.Count][];
            for (var i = 0; i < rawData.Length; i++)
            {
                rawData[i] = new[] { 0.0, normalizedReachabilityPlot[i].Y};
            }
            
            var clustering = K_MEANS_Clustering(rawData, 3);
            
            var zeroCount = 0;
            var zeroTotal = 0.0;
            var oneCount = 0;
            var oneTotal = 0.0;
            var twoCount = 0;
            var twoTotal = 0.0;
            for (var i = 0; i < clustering.Length; i++)
            {
                if (clustering[i] == 0)
                {
                    zeroCount++;
                    zeroTotal += normalizedReachabilityPlot[i].Y;
                }
                if (clustering[i] == 1)
                {
                    oneCount++;
                    oneTotal += normalizedReachabilityPlot[i].Y;
                }
                if (clustering[i] == 2)
                {
                    twoCount++;
                    twoTotal += normalizedReachabilityPlot[i].Y;
                }
            }
            var zeroMean = zeroTotal / zeroCount;
            var oneMean = oneTotal / oneCount;
            var twoMean = twoTotal / twoCount;
            var clusterWithHighestMean = -1;
            if (zeroMean > oneMean)
            {
                clusterWithHighestMean = zeroMean > twoMean ? 0 : 2;
            }
            else
            {
                clusterWithHighestMean = oneMean > twoMean ? 1 : 2;
            }

            var pointClusters = new List<List<ClusterPoint>>();
            var currentCluster = new List<ClusterPoint>
                                 {
                                     processedPoints.First()
                                 };
            for (var i = 0; i < clustering.Length; i++)
            {
                if (clustering[i] != clusterWithHighestMean)
                {
                    currentCluster.Add(processedPoints[i + 1]);
                    continue;
                }

                var fullCluster = currentCluster.ToList();
                currentCluster.Clear();
                currentCluster.Add(processedPoints[i + 1]);
                pointClusters.Add(fullCluster);
            }
            if (currentCluster.Any())
            {
                currentCluster.Add(processedPoints.Last());
                var finalCluster = currentCluster.ToList();
                pointClusters.Add(finalCluster);
            }

            var strokeClusters = pointClusters.Select(pointCluster => new StrokeCollection()).ToList();
            foreach (var stroke in strokes)
            {
                var strokePoints = stroke.StylusPoints;
                var highestPercentageClusterID = 0;
                var percentageOfStrokeInHighestMatchingCluster = 0.0;
                for (var i = 0; i < pointClusters.Count; i++)
                {
                    var clusterPoints = pointClusters[i];
                    var numberOfSharedPoints = (from pointCluster in clusterPoints
                                                from strokePoint in strokePoints
                                                where pointCluster.IsEqualToStylusPoint(strokePoint)
                                                select pointCluster).Count();
                    var percentageOfStrokeInCurrentCluster = numberOfSharedPoints / strokePoints.Count;
                    if (percentageOfStrokeInCurrentCluster > percentageOfStrokeInHighestMatchingCluster)
                    {
                        percentageOfStrokeInHighestMatchingCluster = percentageOfStrokeInCurrentCluster;
                        highestPercentageClusterID = i;
                    }
                }
                strokeClusters[highestPercentageClusterID].Add(stroke);
            }

            return strokeClusters;
        }

        // https://en.wikipedia.org/wiki/OPTICS_algorithm
        // https://github.com/Euphoric/OpticsClustering
        //  epsilon needs to be figured out experimentally. It is the maximum distance (radius) away from a point to consider another point part of the current cluster.
        // probably needs to be adjusted based on average properties of the current page, or possibly the current student. epsilon could be infinity, meant to limit
        // complexity  of the scan, should be called max_epsilon. set to width/heigh of page?
        //
        // min cluster size should be the number of points in the shortest stroke
        public static List<ClusterPoint> OPTICS_Clustering(List<StylusPoint> points, double epsilon, int minClusterSize = 1)
        {
            var clusterPoints = points.Select(p => new ClusterPoint(p)).ToList();
            var epsilonSquared = epsilon * epsilon; //Squared to correctly compare against EuclideanDistanceSquared.
            var seeds = new HeapPriorityQueue<ClusterPoint>(clusterPoints.Count);
            var processedClusterPoints = new List<ClusterPoint>();

            foreach (var p in clusterPoints)
            {
                if (p.IsProcessed)
                {
                    continue;
                }

                p.ReachabilityDistanceSquared = ClusterPoint.UNDEFINED;
                var neighborhood = GetNeighbors(p, epsilonSquared, clusterPoints, minClusterSize);
                p.IsProcessed = true;
                processedClusterPoints.Add(p);

                if (p.CoreDistanceSquared == ClusterPoint.UNDEFINED)
                {
                    continue;
                }

                seeds.Clear();
                Update(p, neighborhood, seeds);

                var innerNeighborhood = new List<ClusterPoint>();
                while (seeds.Count > 0)
                {
                    var pInner = seeds.Dequeue();
                    innerNeighborhood = GetNeighbors(pInner, epsilonSquared, clusterPoints, minClusterSize);
                    pInner.IsProcessed = true;
                    processedClusterPoints.Add(pInner);

                    if (pInner.CoreDistanceSquared != ClusterPoint.UNDEFINED)
                    {
                        Update(pInner, innerNeighborhood, seeds);
                    }
                }
            }

            return processedClusterPoints;
        }

        private static void Update(ClusterPoint p1, List<ClusterPoint> neighborhood, HeapPriorityQueue<ClusterPoint> seeds)
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
                    seeds.Enqueue(p2, newReachabilityDistanceSquared);
                }
                else if (newReachabilityDistanceSquared < p2.ReachabilityDistanceSquared)
                {
                    p2.ReachabilityDistanceSquared = newReachabilityDistanceSquared;
                    seeds.UpdatePriority(p2, newReachabilityDistanceSquared);
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

        // https://visualstudiomagazine.com/articles/2013/12/01/k-means-data-clustering-using-c.aspx
        public static int[] K_MEANS_Clustering(double[][] rawData, int numberOfClusters)
        {
            var k = numberOfClusters;

            var random = new Random();
            var clustering = new int[rawData.Length];
            for (var i = 0; i < k; i++)
            {
                clustering[i] = i; // guarantees at least one data point is assigned to each cluster
            }
            for (var i = k; i < clustering.Length; i++)
            {
                clustering[i] = random.Next(0, k); // assigns rest of the data points to a random cluster.
            }

            var means = new double[k][];
            for (var i = 0; i < k; i++)
            {
                means[i] = new double[rawData[0].Length];
            }

            var maxIterations = rawData.Length * 10;
            var iteration = 0;
            var clusteringSuccessful = true;
            var clusteringChanged = true;
            
            while (clusteringSuccessful && 
                   clusteringChanged &&
                   iteration < maxIterations)
            {
                ++iteration;
                clusteringSuccessful = UpdateMeans(rawData, clustering, ref means);
                clusteringChanged = UpdateClustering(rawData, clustering, means);
            }

            return clustering;
        }

        private static bool UpdateMeans(double[][] data, int[] clustering, ref double[][] means)
        {
            var numberOfClusters = means.Length;
            var clusterCounts = new int[numberOfClusters];
            for (var i = 0; i < data.Length; i++)
            {
                var cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (var i = 0; i < numberOfClusters; i++)
            {
                if (clusterCounts[i] == 0)
                {
                    return false;
                }
                    
            }

            for (var i = 0; i < means.Length; i++)
            {
                for (var j = 0; j < means[i].Length; j++)
                {
                    means[i][j] = 0.0;
                }
            }
                
            for (var i = 0; i < data.Length; i++)
            {
                var cluster = clustering[i];
                for (var j = 0; j < data[i].Length; j++)
                {
                    means[cluster][j] += data[i][j]; // accumulate sum
                }
            }

            for (var i = 0; i < means.Length; i++)
            {
                for (var j = 0; j < means[i].Length; j++)
                {
                    means[i][j] /= clusterCounts[i]; // danger of div by 0
                }
            }

            return true;
        }

        private static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            var numberOfClusters = means.Length;
            var changed = false;

            var newClustering = new int[clustering.Length];
            Array.Copy(clustering, newClustering, clustering.Length);

            var distances = new double[numberOfClusters];

            for (var i = 0; i < data.Length; i++)
            {
                for (var j = 0; j < numberOfClusters; j++)
                {
                    distances[j] = Distance(data[i], means[j]);
                }

                int newClusterID = MinIndex(distances);
                if (newClusterID == newClustering[i])
                {
                    continue;
                }

                changed = true;
                newClustering[i] = newClusterID;
            }

            if (changed == false)
            {
                return false;
            }

            var clusterCounts = new int[numberOfClusters];
            for (var i = 0; i < data.Length; i++)
            {
                var cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (var i = 0; i < numberOfClusters; i++)
            {
                if (clusterCounts[i] == 0)
                {
                    return false;
                }
            }
                
            Array.Copy(newClustering, clustering, newClustering.Length);
            return true; // no zero-counts and at least one change
        }

        private static double Distance(double[] tuple, double[] mean)
        {
            var sumSquaredDiffs = tuple.Select((t, i) => Math.Pow(t - mean[i], 2)).Sum();
            return Math.Sqrt(sumSquaredDiffs);
        }

        private static int MinIndex(double[] distances)
        {
            var indexOfMin = 0;
            var smallDist = distances[0];
            for (var i = 0; i < distances.Length; i++)
            {
                if (distances[i] < smallDist)
                {
                    smallDist = distances[i];
                    indexOfMin = i;
                }
            }

            return indexOfMin;
        }
    }
}
