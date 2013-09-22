using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Converters
{
    public class ProjectedDisplayBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if((bool)value)
            {
                return new SolidColorBrush(Colors.PaleGreen);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
