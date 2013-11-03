using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Converters
{
    public class AllBoolToColorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = false;
            foreach(var value in values.Where(value => value is bool))
            {
                if((bool)value)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    break;
                }
            }

            // TODO: Generalize this so that parameter can be any color
            return result ? new SolidColorBrush(Colors.PaleGreen) : new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}