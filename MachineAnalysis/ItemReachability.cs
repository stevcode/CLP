namespace CLP.MachineAnalysis
{
    public struct ItemReachability
    {
        public ItemReachability(uint originalIndex, double reachabilityDistance)
        {
            OriginalIndex = originalIndex;
            ReachabilityDistance = reachabilityDistance;
        }

        public readonly uint OriginalIndex;
        public readonly double ReachabilityDistance;
    }
}