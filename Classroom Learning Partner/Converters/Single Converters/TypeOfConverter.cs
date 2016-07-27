using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (object), typeof (Type))]
    public class TypeOfConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter) { return value == null ? null : value.GetType(); }
    }
}