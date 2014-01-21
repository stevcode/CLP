using System;
using System.Globalization;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class ByteArrayToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var byteArray = value as byte[];
            return byteArray != null && byteArray.Length != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}