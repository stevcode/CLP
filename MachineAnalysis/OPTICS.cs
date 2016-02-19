using System;
using System.Collections.Generic;
using System.Linq;

namespace CLP.MachineAnalysis
{
    public class OPTICS<T>
    {
        private struct ItemRelation
        {
            public readonly uint OtherItemIndex;
            public readonly double DistanceToOtherItem;

            public ItemRelation(uint otherItemIndex, double distanceToOtherItem)
            {
                OtherItemIndex = otherItemIndex;
                DistanceToOtherItem = distanceToOtherItem;
            }
        }

        private class ItemRelationComparer : IComparer<ItemRelation>
        {
            public int Compare(ItemRelation x, ItemRelation y)
            {
                if (Math.Abs(x.DistanceToOtherItem - y.DistanceToOtherItem) < 0.0001)
                {
                    return 0;
                }
                return x.DistanceToOtherItem > y.DistanceToOtherItem ? 1 : -1;
            }
        }

        readonly Item<T>[] _items;
        readonly double _maximumEpsilon;
        readonly int _minimumItemsInCluster;
        readonly List<uint> _outputIndexes;
        readonly HeapPriorityQueue<Item<T>> _seeds;
        readonly Func<T, T, double> _distanceEquation;

        private static readonly ItemRelationComparer ItemComparer = new ItemRelationComparer();

        public OPTICS(double maximumEpsilon, int minimumItemsInCluster, List<T> internalItems, Func<T, T, double> distanceEquation)
        {
            var items = new List<Item<T>>();
            foreach (var internalItem in internalItems)
            {
                var item = new Item<T>((uint)items.Count, (uint)items.Count, internalItem);
                items.Add(item);
            }

            _items = items.ToArray();
            _maximumEpsilon = maximumEpsilon;
            _minimumItemsInCluster = minimumItemsInCluster;
            _distanceEquation = distanceEquation;

            _outputIndexes = new List<uint>(_items.Length);
            _seeds = new HeapPriorityQueue<Item<T>>(_items.Length);
        }

        private void GetNeighborhood(uint itemIndex, List<ItemRelation> neighborhood)
        {
            neighborhood.Clear();

            for (uint otherItemIndex = 0; otherItemIndex < _items.Length; otherItemIndex++)
            {
                var distance = _distanceEquation(_items[itemIndex].InternalItem, _items[otherItemIndex].InternalItem);

                if (distance <= _maximumEpsilon)
                {
                    neighborhood.Add(new ItemRelation(otherItemIndex, distance));
                }
            }
        }

        private double CoreDistance(List<ItemRelation> neighbors)
        {
            if (neighbors.Count < _minimumItemsInCluster)
            {
                return double.NaN;
            }

            neighbors.Sort(ItemComparer);
            return neighbors[_minimumItemsInCluster - 1].DistanceToOtherItem;
        }

        public void BuildReachability()
        {
            for (uint itemIndex = 0; itemIndex < _items.Length; itemIndex++)
            {
                if (_items[itemIndex].WasProcessed)
                {
                    continue;
                }

                var neighborhoodOfPoint = new List<ItemRelation>();
                GetNeighborhood(itemIndex, neighborhoodOfPoint);

                _items[itemIndex].WasProcessed = true;

                _outputIndexes.Add(itemIndex);

                var coreDistance = CoreDistance(neighborhoodOfPoint);

                if (double.IsNaN(coreDistance))
                {
                    continue;
                }

                _seeds.Clear();
                Update(neighborhoodOfPoint, coreDistance);

                var innerNeighborhood = new List<ItemRelation>();
                while (_seeds.Count > 0)
                {
                    var innerItemIndex = _seeds.Dequeue().OriginalIndex;

                    GetNeighborhood(innerItemIndex, innerNeighborhood);

                    _items[innerItemIndex].WasProcessed = true;

                    _outputIndexes.Add(innerItemIndex);

                    var innerCoreDistance = CoreDistance(innerNeighborhood);

                    if (!double.IsNaN(innerCoreDistance))
                    {
                        Update(innerNeighborhood, innerCoreDistance);
                    }
                }
            }
        }

        private void Update(List<ItemRelation> neighborhood, double coreDistance)
        {
            for (var i = 0; i < neighborhood.Count; i++)
            {
                var otherItemIndex = neighborhood[i].OtherItemIndex;

                if (_items[otherItemIndex].WasProcessed)
                {
                    continue;
                }

                var newReachabilityDistance = Math.Max(coreDistance, neighborhood[i].DistanceToOtherItem);

                if (double.IsNaN(_items[otherItemIndex].ReachabilityDistance))
                {
                    _items[otherItemIndex].ReachabilityDistance = newReachabilityDistance;
                    _seeds.Enqueue(_items[otherItemIndex], newReachabilityDistance);
                }
                else if (newReachabilityDistance < _items[otherItemIndex].ReachabilityDistance)
                {
                    _items[otherItemIndex].ReachabilityDistance = newReachabilityDistance;
                    _seeds.UpdatePriority(_items[otherItemIndex], newReachabilityDistance);
                }
            }
        }

        public IEnumerable<ItemReachability> ReachabilityDistances()
        {
            return _outputIndexes.Select(item => new ItemReachability(_items[item].OriginalIndex, _items[item].ReachabilityDistance));
        }
    }
}