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
            var val = value.ToInt();
            var intToMatch = parameter.ToDouble();

            if (val == null ||
                intToMatch == null)
            {
                return ConverterHelper.UnsetValue;
            }

            if (val == (int)intToMatch)
            {
                return "?";
            }

            return (int)val;
        }
    }
}