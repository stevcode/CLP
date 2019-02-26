using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class ListContainsItemToVisibilityMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var doesNotContainVisibility = Visibility.Visible;
            switch (parameter as string)
            {
                case "Hidden":
                    doesNotContainVisibility = Visibility.Hidden;
                    break;
                case "Collapsed":
                    doesNotContainVisibility = Visibility.Collapsed;
                    break;
            }
            if (values.Length != 2)
            {
                return doesNotContainVisibility;
            }

            if (!(values[0] is IEnumerable<object> list))
            {
                return doesNotContainVisibility;
            }
            var item = values[1];
            var isItemInList = list.Contains(item);
            return isItemInList ? Visibility.Visible : doesNotContainVisibility;
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