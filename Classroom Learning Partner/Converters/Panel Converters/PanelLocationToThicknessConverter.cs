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
               !(value is PanelLocations))
            {
                return new Thickness(0, 0, 0, 0);
            }

            var thickness = (double)parameter;
            var location = (PanelLocations)value;
            switch(location)
            {
                case PanelLocations.Left:
                    return new Thickness(thickness, 0, 0, 0);
                case PanelLocations.Right:
                    return new Thickness(0, 0, thickness, 0);
                case PanelLocations.Top:
                    return new Thickness(0, thickness, 0, 0);
                case PanelLocations.Bottom:
                    return new Thickness(0, 0, 0, thickness);
                case PanelLocations.Floating:
                    return new Thickness(0, 0, 0, 0);
                default:
                    return new Thickness(0, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}