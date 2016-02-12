namespace CLP.MachineAnalysis
{
    public struct PointReachability
    {
        public PointReachability(uint pointId, double reachability)
        {
            PointId = pointId;
            Reachability = reachability;
        }

        public readonly uint PointId;
        public readonly double Reachability;
    }
}