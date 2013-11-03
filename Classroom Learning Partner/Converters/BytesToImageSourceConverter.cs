using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Converters
{
    public class BytesToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageByteSource = value as byte[];
            if(imageByteSource == null)
            {
                return null;
            }

            var image = CLPImageViewModel.LoadImageFromByteSource(imageByteSource);
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}