using System;
using System.Globalization;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class HeightToWidthAspectRatioMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(values[0] is double) ||
               !(values[1] is double))
            {
                return 10.0;
            }
            var height = (double)values[0];
            var aspectRatio = (double)values[1];
            return height*aspectRatio;

            //try
            //{
            //    var height = (double)values[0];
            //    var aspectRatio = (double)values[1];
            //    return height*aspectRatio;
            //}
            //catch(Exception)
            //{
            //    return 10.0;
            //}
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}