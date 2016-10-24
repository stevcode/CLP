using System.IO;
using System.IO.Compression;
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
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream(imageByteArray);
            bitmapImage.EndInit();

            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static byte[] CompressWithGZip(this byte[] raw)
        {
            Argument.IsNotNull("raw", raw);

            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public static byte[] DecompressFromGZip(this byte[] gzip)
        {
            Argument.IsNotNull("gzip", gzip);

            using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int SIZE = 4096;
                var buffer = new byte[SIZE];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, SIZE);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}