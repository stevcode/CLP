using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPImageViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPImageViewModel(CLPImage image, CLPPageViewModel pageViewModel) : base(pageViewModel)
        {
            LoadImageFromByteSource(image.ByteSource);
            PageObject = image;
        }

        #region Binding

        /// <summary>
        /// The <see cref="SourceImage" /> property's name.
        /// </summary>
        public const string SourceImagePropertyName = "SourceImage";

        private ImageSource _sourceImage;

        /// <summary>
        /// Sets and gets the SourceImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ImageSource SourceImage
        {
            get
            {
                return _sourceImage;
            }

            set
            {
                if (_sourceImage == value)
                {
                    return;
                }

                _sourceImage = value;
                RaisePropertyChanged(SourceImagePropertyName);
            }
        }

        #endregion //Binding


        private void LoadImageFromByteSource(byte[] byteSource)
        {
            MemoryStream memoryStream = new MemoryStream(byteSource, 0, byteSource.Length, false, false);
            BitmapImage genBmpImage = new BitmapImage();

            genBmpImage.BeginInit();
            genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
            genBmpImage.StreamSource = memoryStream;
            genBmpImage.EndInit();
            genBmpImage.Freeze();

            memoryStream.Dispose();
            memoryStream.Close();
            memoryStream = null;

            _sourceImage = genBmpImage;
        }
    }
}