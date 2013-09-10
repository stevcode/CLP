using System;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class AspectRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            var side = (double)value;
            var aspectRatio = (double)parameter;
            return side;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}