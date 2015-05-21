using System;
using System.Windows.Data;
using Catel.Windows.Data.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class IncreaseDoubleConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var originalValue = 0d;

            if (value is int)
            {
                originalValue = System.Convert.ToDouble(value);
            }
            else if (value is double)
            {
                originalValue = (double)value;
            }

            double increasedAmount;
            if (parameter is int)
            {
                increasedAmount = System.Convert.ToDouble(parameter);
            }
            else if (parameter is double)
            {
                increasedAmount = (double)parameter;
            }
            else if (!double.TryParse(parameter as string, out increasedAmount))
            {
                return originalValue;
            }

            return originalValue + increasedAmount;
        }
    }
}
