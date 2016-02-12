namespace CLP.MachineAnalysis
{
    public struct ItemReachability
    {
        public ItemReachability(uint itemID, double reachabilityDistance)
        {
            ItemID = itemID;
            ReachabilityDistance = reachabilityDistance;
        }

        public readonly uint ItemID;
        public readonly double ReachabilityDistance;
    }
}