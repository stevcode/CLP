using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class BoolZeroToQuestionMarkMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(values[0] is int) || !(values[1] is bool))
            {
                return DependencyProperty.UnsetValue;
            }

            var val = (int)values[0];
            var showZero = (bool)values[1];
            return !showZero && val == 0 ? "?" : val.ToString(CultureInfo.InvariantCulture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
