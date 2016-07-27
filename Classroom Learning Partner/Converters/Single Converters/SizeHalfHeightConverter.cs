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
            double height;
            try
            {
                height = System.Convert.ToDouble(value) / 2;  // BUG: Ann's Cache, page 24, some heights set to zero?
            }
            catch (Exception)
            {
                height = 0;
            }

            double width;
            try
            {
                width = System.Convert.ToDouble(parameter);
            }
            catch (Exception)
            {
                width = 0;
            }

            var finalHeight = height - 2.0 >= 0.0 ? height : 2.0;

            return new Size(finalHeight - 2.0, width);
        }
    }
}

