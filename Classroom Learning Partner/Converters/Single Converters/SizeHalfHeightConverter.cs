using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (double), typeof (Size))]
    public class SizeHalfHeightConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var height = value.ToDouble() ?? 0.0; // BUG: Ann's Cache, page 24, some heights set to zero?
            var halvedHight = height / 2.0;
            var adjustedHeight = halvedHight - 2.0 >= 0.0 ? halvedHight : 2.0;

            var width = parameter.ToDouble() ?? 0.0;

            return new Size(adjustedHeight - 2.0, width);
        }
    }
}