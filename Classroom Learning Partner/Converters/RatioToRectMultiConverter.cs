using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class RatioToRectMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(values[0] is double) ||
               !(values[1] is double) ||
               !(values[2] is double))
            {
                return new Rect(0, 0, 0, 0);
            }
            var width = (double)values[0];
            var height = (double)values[1];
            var gridSquareSize = (double)values[2];
            return new Rect(0, 0, gridSquareSize / width, gridSquareSize / height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}