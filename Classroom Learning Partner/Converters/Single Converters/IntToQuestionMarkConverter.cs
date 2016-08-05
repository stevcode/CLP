using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class IntToQuestionMarkConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is int) ||
                !(parameter is int))
            {
                return ConverterHelper.UnsetValue;
            }

            var val = (int)value;
            var intToMatch = (int)parameter;

            if (val == intToMatch)
            {
                return "?";
            }

            return val;
        }
    }
}