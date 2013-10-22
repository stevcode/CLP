using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ServiceModelEx;

namespace Classroom_Learning_Partner.Converters
{
    public class MultiplyAllIntToTextMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Where(value => value is int).Aggregate(1, (current, value) => current * (int)value).ToString(CultureInfo.InvariantCulture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

