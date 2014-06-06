using System;
using System.Globalization;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class NumberIncrementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = System.Convert.ToInt32(value);
            var increment = System.Convert.ToInt32(parameter);
            return number + increment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}