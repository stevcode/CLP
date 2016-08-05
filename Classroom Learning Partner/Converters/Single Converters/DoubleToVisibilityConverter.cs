using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(double), typeof(Visibility))]
    public class DoubleToVisibilityConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return (double)value > 0.0 ? Visibility.Visible : Visibility.Hidden;
        }
    }
}