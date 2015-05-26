using System;
using System.Windows.Data;
using Catel.Windows.Data.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (object), typeof (bool))]
    public class IsNullConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter) { return value == null; }
    }
}