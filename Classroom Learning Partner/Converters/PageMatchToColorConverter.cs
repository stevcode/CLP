using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.Converters
{
    public class PageMatchToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;

            CLPPage current = values[0] as CLPPage;
            CLPPage compared = values[1] as CLPPage;

            if(current != null && compared != null && current.ID == compared.ID && current.OwnerID == compared.OwnerID && current.DifferentiationLevel == compared.DifferentiationLevel)
            {
                 return dict["MainColor"];
            }
            return dict["GrayBorderColor"];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
