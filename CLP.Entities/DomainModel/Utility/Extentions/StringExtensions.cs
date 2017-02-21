using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Catel;

namespace CLP.Entities
{
    public static class StringExtensions
    {
        /// <summary>Tests a string to see if it is a numeric value (int, double).</summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumber(this string s)
        {
            Argument.IsNotNull("s", s);

            int intValue;
            var isInt = int.TryParse(s, out intValue);
            if (isInt)
            {
                return true;
            }

            double doubleValue;
            var isDouble = double.TryParse(s, out doubleValue);
            if (isDouble)
            {
                return true;
            }

            return false;
        }

        public static string TrimAll(this string s)
        {
            Argument.IsNotNull("s", s);

            return s.Replace(" ", string.Empty);
        }

        public static string[] Split(this string s, string delimiter)
        {
            return s.Split(new[] { delimiter }, StringSplitOptions.None);
        }

        public static string[] Split(this string s, string delimiter, StringSplitOptions options)
        {
            return s.Split(new[] { delimiter }, options);
        }

        public static string CompressWithGZip(this string text)
        {
            Argument.IsNotNull("text", text);

            var buffer = Encoding.Unicode.GetBytes(text);
            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;

            var compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            var gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string DecompressFromGZip(this string compressedText)
        {
            Argument.IsNotNull("compressedText", compressedText);

            var gzBuffer = Convert.FromBase64String(compressedText);
            using (var ms = new MemoryStream())
            {
                var msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                var buffer = new byte[msgLength];

                ms.Position = 0;
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.Unicode.GetString(buffer, 0, buffer.Length);
            }
        }
    }
}