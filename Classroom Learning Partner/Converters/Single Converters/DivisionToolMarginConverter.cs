using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(double), typeof(Thickness))]
    public class DivisionToolMarginConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            //ActualWidth of Expression StackGrid is 69.0, subtract half from the middle of LastDivisionPosition to get Margin
            var margin = (double)value / 2.0 - 34.5;
            return margin <= 0.1 ? new Thickness(0, 0, 0, 0) : new Thickness(margin, 0, 0, 0);
        }
    }
}