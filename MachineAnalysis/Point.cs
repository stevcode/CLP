namespace CLP.MachineAnalysis
{
    internal class Point : PriorityQueueNode
    {
        public Point(uint index, uint id, double[] vector)
        {
            Index = index;
            Id = id;
            Vector = vector;

            WasProcessed = false;
            ReachabilityDistance = double.NaN;
        }

        public readonly uint Id;
        public readonly double[] Vector;
        public readonly uint Index;

        internal double ReachabilityDistance;
        internal bool WasProcessed;
    }
}