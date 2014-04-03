using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Converters
{
    public class PanelLocationToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(parameter is double) ||
               !(value is PanelLocation))
            {
                return new Thickness(0, 0, 0, 0);
            }

            var thickness = (double)parameter;
            var location = (PanelLocation)value;
            switch(location)
            {
                case PanelLocation.Left:
                    return new Thickness(thickness, 0, 0, 0);
                case PanelLocation.Right:
                    return new Thickness(0, 0, thickness, 0);
                case PanelLocation.Top:
                    return new Thickness(0, thickness, 0, 0);
                case PanelLocation.Bottom:
                    return new Thickness(0, 0, 0, thickness);
                case PanelLocation.Floating:
                    return new Thickness(0, 0, 0, 0);
                default:
                    return new Thickness(0, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}