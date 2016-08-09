using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class IndexToMarginConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return (int)value % 2 == 0 ? "-3 7 7 -3" : "7 -3 -3 7";
        }
    }
}