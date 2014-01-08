using System;
using System.Globalization;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class BoolToHighlighterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Double.Parse((string)parameter) : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}