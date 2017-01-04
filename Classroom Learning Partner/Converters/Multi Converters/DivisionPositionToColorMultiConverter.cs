using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.Converters
{
    [ValueConversion(typeof(object), typeof(SolidColorBrush))]
    public class DivisionPositionToColorMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            var position = values[0].ToDouble();
            var dividerRegions = values[1] as ObservableCollection<CLPArrayDivision>;
            if (position == null ||
                dividerRegions == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            var index = 0;
            foreach (var arrayDivision in dividerRegions)
            {
                index += 1;
                if (position == arrayDivision.Position)
                {
                    break;
                }
            }

            if (index == dividerRegions.Count)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            return index % 2 == 0 ? new SolidColorBrush(Colors.MediumPurple) : new SolidColorBrush(Colors.SpringGreen);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}