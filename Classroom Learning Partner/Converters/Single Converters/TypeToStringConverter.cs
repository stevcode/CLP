using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    //TODO: Use type equals converter instead of magic strings
    [ValueConversion(typeof(object), typeof(string))]
    public class TypeToStringConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return value == null ? ConverterHelper.UnsetValue : value.GetType().Name;
        }
    }
}