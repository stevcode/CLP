using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ServiceModelEx;

namespace Classroom_Learning_Partner.Converters
{
    public class MultiplyAllMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Where(value => value is int || value is double).Aggregate(1.0, (current, value) => current * System.Convert.ToDouble(value));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

