using System;
using System.Globalization;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class WidthToHeightAspectRatioMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var width = (double)values[0];
                var aspectRatio = (double)values[1];
                return width/aspectRatio;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return 100.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
