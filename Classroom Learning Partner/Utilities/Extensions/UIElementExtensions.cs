using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel;

namespace Classroom_Learning_Partner
{
    public static class UIElementExtensions
    {
        public static byte[] ToImageByteArray(this UIElement sourceElement, double imageWidth = -1.0, double scale = 1.0, double dpi = 96, BitmapEncoder encoder = null)
        {
            Argument.IsNotNull("sourceElement", sourceElement);
            Argument.IsNotNull("imageWidth", imageWidth);
            Argument.IsNotNull("scale", scale);
            Argument.IsNotNull("dpi", dpi);

            const double SCREEN_DPI = 96.0;
            var actualWidth = imageWidth <= 0.0 ? sourceElement.RenderSize.Width : imageWidth;
            var actualHeight = actualWidth * sourceElement.RenderSize.Height / sourceElement.RenderSize.Width;

            var renderWidth = actualWidth * scale * dpi / SCREEN_DPI;
            var renderHeight = actualHeight * scale * dpi / SCREEN_DPI;

            var renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, dpi, dpi, PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(sourceElement);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            byte[] imageArray;
            if (encoder == null)
            {
                encoder = new PngBitmapEncoder();
            }

            encoder.Frames.Add(BitmapFrame.Create(renderTarget));
            using (var outputStream = new MemoryStream())
            {
                encoder.Save(outputStream);
                imageArray = outputStream.ToArray();
            }

            return imageArray;
        }

        public static BitmapImage ToBitmapImage(this UIElement sourceElement, double imageWidth = -1.0, double scale = 1.0, double dpi = 96, BitmapEncoder encoder = null)
        {
            Argument.IsNotNull("sourceElement", sourceElement);
            Argument.IsNotNull("imageWidth", imageWidth);
            Argument.IsNotNull("scale", scale);
            Argument.IsNotNull("dpi", dpi);

            var imageByteArray = sourceElement.ToImageByteArray(imageWidth, scale, dpi, encoder);

            var bitmapImage = imageByteArray?.ToBitmapImage();
            return bitmapImage;
        }
    }
}