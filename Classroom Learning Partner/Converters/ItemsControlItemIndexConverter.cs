using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Catel.Data;
using Catel.Windows;

namespace Classroom_Learning_Partner.Converters
{
    public class ItemsControlItemIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (FrameworkElement)value;
            var itemsControl = item.FindVisualAncestorByType<ItemsControl>();
            var index = itemsControl.Items.IndexOf(item.DataContext);

            return index + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
