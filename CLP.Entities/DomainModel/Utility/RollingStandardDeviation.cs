using System;

namespace CLP.Entities.Demo
{
    // Derived from: http://www.johndcook.com/blog/standard_deviation/
    public class RollingStandardDeviation
    {
        private int _numberOfDataValues;
        private double _oldMean;
        private double _newMean;
        private double _oldSum;
        private double _newSum;

        public RollingStandardDeviation() { _numberOfDataValues = 0; }

        public double Mean
        {
            get { return _numberOfDataValues > 0 ? _newMean : 0.0; }
        }

        public double Variance
        {
            get { return _numberOfDataValues > 1 ? _newSum / (_numberOfDataValues - 1) : 0.0; }
        }

        public double StandardDeviation
        {
            get { return Math.Sqrt(Variance); }
        }

        public void Clear() { _numberOfDataValues = 0; }

        public void Update(double x)
        {
            _numberOfDataValues++;

            if (_numberOfDataValues == 1)
            {
                _oldMean = x;
                _newMean = x;
                _oldSum = 0.0;
                return;
            }

            _newMean = _oldMean + (x - _oldMean) / _numberOfDataValues;
            _newSum = _oldSum + (x - _oldMean) * (x - _oldMean);

            _oldMean = _newMean;
            _oldSum = _newSum;
        }
    }
}