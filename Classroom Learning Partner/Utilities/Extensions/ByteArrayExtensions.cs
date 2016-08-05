using System.IO;
using System.Windows.Media.Imaging;
using Catel;

namespace Classroom_Learning_Partner
{
    public static class ByteArrayExtensions
    {
        public static BitmapImage ToBitmapImage(this byte[] imageByteArray)
        {
            Argument.IsNotNull("imageByteArray", imageByteArray);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.StreamSource = new MemoryStream(imageByteArray);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}