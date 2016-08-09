using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class EnumToBoolConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return value != null && value.Equals(parameter);
        }

        protected override object ConvertBack(object value, Type targetType, object parameter)
        {
            return value.Equals(true) ? parameter : null;
        }
    }
}