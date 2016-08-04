using System;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class HalfNumberConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            double halfOriginalValue;
            try
            {
                halfOriginalValue = System.Convert.ToDouble(value) / 2.0;
            }
            catch (Exception)
            {
                halfOriginalValue = 0;
            }

            return halfOriginalValue;
        }
    }
}
