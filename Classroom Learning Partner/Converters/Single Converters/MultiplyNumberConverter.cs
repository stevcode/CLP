using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof (double), typeof (double))]
    public class MultiplyNumberConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var originalValue = value.ToDouble();
            if (originalValue == null)
            {
                return 0.0;
            }

            var factor = parameter.ToDouble();
            if (factor == null)
            {
                return originalValue;
            }

            return originalValue * factor;
        }
    }
}