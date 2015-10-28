using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class AllBoolToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = false;
            foreach (var value in values.Where(value => value is bool))
            {
                if ((bool)value)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    break;
                }
            }

            var nonVisibleType = parameter is Visibility ? parameter : Visibility.Collapsed;

            return result ? Visibility.Visible : nonVisibleType;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) { return null; }
    }
}