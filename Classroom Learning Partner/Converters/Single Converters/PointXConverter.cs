using System;
using System.Windows;
using System.Windows.Data;
using Catel.Windows.Data.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (double), typeof (Point))]
    public class PointXConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            double x;
            try
            {
                x = System.Convert.ToDouble(value);
            }
            catch (Exception)
            {
                x = 0;
            }

            double y;
            try
            {
                y = System.Convert.ToDouble(parameter);
            }
            catch (Exception)
            {
                y = 0;
            }

            return new Point(x - 2.0, y);
        }
    }
}
