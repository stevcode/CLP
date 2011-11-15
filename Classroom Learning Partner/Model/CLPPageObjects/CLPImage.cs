using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CLPImage : CLPPageObjectBase
    {
        #region Constructors

        public CLPImage(string path) : base()
        {
            if (File.Exists(path))
            {
                _byteSource = File.ReadAllBytes(path);
            }

            LoadImageFromByteSource();
            InitializeBase();
        }

        private void InitializeBase()
        {
            if (_sourceImage != null)
            {
                if (_sourceImage.Height > 1000)
                {
                    Height = 1000;
                    double ratio = _sourceImage.Height / _sourceImage.Width;
                    Width = 1000 * ratio;
                }
                else
                {
                    Height = _sourceImage.Height;
                }

                if (_sourceImage.Width > 800)
                {
                    Width = 800;
                    double ratio = _sourceImage.Width / _sourceImage.Height;
                    Height = 800 * ratio;
                }
                else
                {
                    Width = _sourceImage.Width;
                }

                base.Position = new System.Windows.Point(10, 10);
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            LoadImageFromByteSource();
        }

        private void LoadImageFromByteSource()
        {
            MemoryStream memoryStream = new MemoryStream(ByteSource, 0, ByteSource.Length, false, false);
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

        public CLPImage(byte[] imgSource)
        {
            _byteSource = imgSource;
            LoadImageFromByteSource();
            InitializeBase();
        }

        #endregion //Constructors

        #region Properties

        //Non-Serialized
        [NonSerialized]
        private ImageSource _sourceImage;
        public ImageSource SourceImage
        {
            get
            {
                if (_sourceImage == null)
                {
                    LoadImageFromByteSource();
                }
                return _sourceImage;
            }
        }

        private byte[] _byteSource;
        public byte[] ByteSource
        {
            get
            {
                return _byteSource;
            }
        }

        #endregion //Properties
    }
}
