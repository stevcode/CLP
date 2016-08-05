using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(Type), typeof(bool))]
    public class TypeMatchToBooleanConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var desiredType = parameter as Type;
            return value != null && value.GetType() == desiredType;
        }
    }
}