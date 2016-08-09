using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(IList), typeof(Visibility))]
    public class EmptyListToVisibilityConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var list = value as IList;
            if (list == null ||
                list.Count == 0)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }
    }
}