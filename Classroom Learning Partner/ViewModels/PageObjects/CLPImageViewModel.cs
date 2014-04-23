using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPImageViewModel : APageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image)
        {
            PageObject = image;
            if(App.MainWindowViewModel.ImagePool.ContainsKey(image.ImageHashID))
            {
                SourceImage = App.MainWindowViewModel.ImagePool[image.ImageHashID];
            }
            else
            {
                var filePath = string.Empty;
                var imageFilePaths = Directory.EnumerateFiles(App.ImageCacheDirectory);
                foreach(var imageFilePath in from imageFilePath in imageFilePaths
                                             let imageHashID = Path.GetFileNameWithoutExtension(imageFilePath)
                                             where imageHashID == image.ImageHashID
                                             select imageFilePath) 
                                             {
                                                 filePath = imageFilePath;
                                                 break;
                                             }

                var bitmapImage = CLPImage.GetImageFromPath(filePath);
                if(bitmapImage == null)
                {
                    return;
                }
                SourceImage = bitmapImage;
                App.MainWindowViewModel.ImagePool.Add(image.ImageHashID, bitmapImage);
            }

            ResizeImageCommand = new Command<DragDeltaEventArgs>(OnResizeImageCommandExecute);
        }

        public override string Title { get { return "ImageVM"; } }

        #region Binding

        /// <summary>
        /// The visible image, loaded from the ImageCache.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof (ImageSource));

        #endregion //Binding

        public static BitmapImage LoadImageFromByteSource(byte[] byteSource)
        {
            var memoryStream = new MemoryStream(byteSource, 0, byteSource.Length, false, false);
            var genBmpImage = new BitmapImage();

            genBmpImage.BeginInit();
            genBmpImage.CacheOption = BitmapCacheOption.OnDemand;
            //genBmpImage.DecodePixelHeight = Convert.ToInt32(this.Height);
            genBmpImage.StreamSource = memoryStream;
            genBmpImage.EndInit();
            genBmpImage.Freeze();

            memoryStream.Dispose();

            return genBmpImage;
        }

        /// <summary>
        /// Gets the CLPImageResize command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeImageCommand { get; set; }

        private void OnResizeImageCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = PageObject.ParentPage;

            PageObject.Height = PageObject.Height + e.VerticalChange;
            PageObject.Width = PageObject.Width + e.HorizontalChange;
            if(PageObject.Height < 10)
            {
                PageObject.Height = 10;
            }
            if(PageObject.Width < 10)
            {
                PageObject.Width = 10;
            }
            if(PageObject.Height + PageObject.YPosition > parentPage.Height)
            {
                PageObject.Height = PageObject.Height;
            }
            if(PageObject.Width + PageObject.XPosition > parentPage.Width)
            {
                PageObject.Width = PageObject.Width;
            }

            var aspectRatio = 1.0;
            if(SourceImage.Width > 0)
            {
                aspectRatio = SourceImage.Width / SourceImage.Height;
            }
            //PageObject.EnforceAspectRatio(aspectRatio);

            ChangePageObjectDimensions(PageObject, PageObject.Height, PageObject.Width);
        }
    }
}