using System.Collections.Generic;

namespace CLP.MachineAnalysis
{
    public class PointsList
    {
        internal List<Point> _points;

        public PointsList()
        {
            _points = new List<Point>();
        }

        public void AddPoint(uint id, double[] vector)
        {
            var newPoint = new Point((uint)_points.Count, id, vector);
            _points.Add(newPoint);
        }
    }
}
