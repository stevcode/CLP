using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPImageStamp : CLPStampBase
    {
        public CLPImageStamp(string path) : base()
        {
            if (File.Exists(path))
            {
                ByteSource = File.ReadAllBytes(path);
            }
            LoadImageFromByteSource();
            InitializeBase();
        }

        public CLPImageStamp(byte[] imgSource)
        {
            ByteSource = imgSource;
            LoadImageFromByteSource();
            InitializeBase();
        }

        private void InitializeBase()
        {
            if (_sourceImage != null)
            {
                double ratio = _sourceImage.Height / _sourceImage.Width;
                Height = Width * ratio;
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

            SourceImage = genBmpImage;
        }

        public override CLPPageObjectBase Copy()
        {
            CLPImageStamp newStamp = new CLPImageStamp(_byteSource);
            //copy all metadata and create new unique ID/creation date for the moved stamp
            newStamp.IsAnchored = IsAnchored;
            newStamp.Parts = Parts;
            newStamp.Position = Position;
            newStamp.Height = Height;
            newStamp.Width = Width;

            foreach (var stringStroke in PageObjectStrokes)
            {
                newStamp.PageObjectStrokes.Add(stringStroke);
            }

            return newStamp;
        }

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
            set
            {
                _sourceImage = value;
            }
        }

        private byte[] _byteSource;
        public byte[] ByteSource
        {
            get
            {
                return _byteSource;
            }
            set
            {
                _byteSource = value;
            }
        }

        #endregion //Properties
    }
}
