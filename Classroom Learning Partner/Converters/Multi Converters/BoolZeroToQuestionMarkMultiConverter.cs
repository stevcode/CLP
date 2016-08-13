using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class BoolZeroToQuestionMarkMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return ConverterHelper.UnsetValue;
            }

            if (!(values[0] is int) ||
                !(values[1] is bool))
            {
                return ConverterHelper.UnsetValue;
            }

            var val = (int)values[0];
            var showZero = (bool)values[1];
            return !showZero && val == 0 ? "?" : val.ToString();
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