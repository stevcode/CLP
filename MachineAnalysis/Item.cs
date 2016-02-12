namespace CLP.MachineAnalysis
{
    internal class Item<T> : PriorityQueueNode
    {
        public Item(uint currentIndex, uint originalIndex, T internalItem)
        {
            CurrentIndex = currentIndex;
            OriginalIndex = originalIndex;
            InternalItem = internalItem;

            WasProcessed = false;
            ReachabilityDistance = double.NaN;
        }

        public readonly uint OriginalIndex;
        public readonly T InternalItem;
        public readonly uint CurrentIndex;

        internal double ReachabilityDistance;
        internal bool WasProcessed;
    }
}