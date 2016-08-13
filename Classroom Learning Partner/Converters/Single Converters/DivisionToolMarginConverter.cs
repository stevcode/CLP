using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(double), typeof(Thickness))]
    public class DivisionTemplateMarginConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var leftMargin = value.ToDouble();
            if (leftMargin == null)
            {
                return new Thickness(0, 0, 0, 0);
            }

            //ActualWidth of Expression StackGrid is 69.0, subtract half from the middle of LastDivisionPosition to get Margin
            var adjustedLeftMargin = (double)leftMargin / 2.0 - 34.5;
            return adjustedLeftMargin <= 0.1 ? new Thickness(0, 0, 0, 0) : new Thickness(adjustedLeftMargin, 0, 0, 0);
        }
    }
}