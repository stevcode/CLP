using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class MarginSingleSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return new Thickness(0, 0, 50, 0);
            var marginSide = parameter as string;
            var margin = (double)value;
            if(marginSide == null || margin == 0.0)
            {
                return new Thickness(0, 0, 0, 0);
            }

            switch(marginSide)
            {
                case "Left":
                    return new Thickness(margin, 0, 0, 0);
                case "Right":
                    return new Thickness(0, 0, margin, 0);
                case "Top":
                    return new Thickness(0, margin, 0, 0);
                case "Bottom":
                    return new Thickness(0, 0, 0, margin);
                default:
                    return new Thickness(0, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
