using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class StringToEmptyStringConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var val = value as string;
            var stringToMatch = parameter as string;

            if (val == null ||
                stringToMatch == null ||
                val == stringToMatch)
            {
                return string.Empty;
            }

            return val;
        }
    }
}