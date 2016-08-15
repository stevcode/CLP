using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class AllBoolToVisibilityMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!values.All(v => v is bool))
            {
                return Visibility.Visible;
            }

            var result = values.All(v => (bool)v);

            var nonVisibleType = parameter is Visibility ? parameter : Visibility.Collapsed;

            return result ? Visibility.Visible : nonVisibleType;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}