using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (double), typeof (Point))]
    public class PointXConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is double) ||
                !(parameter is double))
            {
                return ConverterHelper.UnsetValue;
            }

            var x = (double)value;
            var y = (double)parameter;

            return new Point(x - 2.0, y);
        }
    }
}
