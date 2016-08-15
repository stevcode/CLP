using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class ItemSelectedToColorConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;

            if (!(value is bool))
            {
                return dict["GrayBorderColor"];
            }

            if ((bool)value) //IsSelected
            {
                return dict["MainColor"];
            }
            return dict["GrayBorderColor"];
        }
    }
}