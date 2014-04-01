using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class FFCMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //ActualWidth of Expression StackGrid is 69.0, subtract half from the middle of LastDivisionPosition to get Margin
            var margin = (double)value / 2 - 34.5;
            if(margin <= 0.1)
            {
                return new Thickness(0, 0, 0, 0);
            }

            return new Thickness(margin, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
