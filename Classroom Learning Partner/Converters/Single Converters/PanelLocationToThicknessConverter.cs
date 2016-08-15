using System;
using System.Windows;
using System.Windows.Data;
using Catel.MVVM.Converters;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(Thickness))]
    public class PanelLocationToThicknessConverter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var thickness = parameter.ToDouble();
            if (!(value is PanelLocations) ||
                thickness == null)
            {
                return new Thickness(0, 0, 0, 0);
            }

            var location = (PanelLocations)value;
            switch (location)
            {
                case PanelLocations.Left:
                    return new Thickness(0, 0, (double)thickness, 0);
                case PanelLocations.Right:
                    return new Thickness((double)thickness, 0, 0, 0);
                case PanelLocations.Top:
                    return new Thickness(0, 0, 0, (double)thickness);
                case PanelLocations.Bottom:
                    return new Thickness(0, (double)thickness, 0, 0);
                case PanelLocations.Floating:
                    return new Thickness(0, 0, 0, 0);
                default:
                    return new Thickness(0, 0, 0, 0);
            }
        }
    }
}