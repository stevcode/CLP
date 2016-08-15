using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(double))]
    public class WidthToHeightAspectRatioMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return 100.0;
            }

            var width = values[0].ToDouble();
            var aspectRatio = values[1].ToDouble();
            if (width == null ||
                aspectRatio == null ||
                aspectRatio == 0.0)
            {
                return 100.0;
            }

            return width / aspectRatio;
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