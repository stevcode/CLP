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
            var newX = value.ToDouble();
            var newY = parameter.ToDouble();

            if (newX == null ||
                newY == null)
            {
                return ConverterHelper.UnsetValue;
            }

            return new Point((double)newX - 2.0, (double)newY);
        }
    }
}
