using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(Rect))]
    public class RatioToRectMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
            {
                return new Rect(0, 0, 0, 0);
            }

            var width = values[0].ToDouble();
            var height = values[1].ToDouble();
            var gridSquareSize = values[2].ToDouble();

            if (width == null ||
                width == 0.0 ||
                height == null ||
                height == 0.0 ||
                gridSquareSize == null)
            {
                return new Rect(0, 0, 0, 0);
            }

            return new Rect(0, 0, (double)gridSquareSize / (double)width, (double)gridSquareSize / (double)height);
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