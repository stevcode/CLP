using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(int), typeof(Thickness))]
    public class IndexToMarginConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var originalValue = value.ToInt();
            if (originalValue == null)
            {
                return new Thickness(0, 0, 0, 0);
            }

            return (int)originalValue % 2 == 0 ? new Thickness(-3, 7, 7, -3) : new Thickness(7, -3, -3, 7);
        }
    }
}