using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPImageStamp : CLPPageObjectBase
    {

        public CLPImageStamp(string path)
        {
            if (File.Exists(path))
            {
                _byteSource = File.ReadAllBytes(path);
            }

            LoadImageFromByteSource();
            InitializeBase();

            MetaData.Add("IsAnchored", new CLPAttribute("IsAnchored", "true"));
        }

        public CLPImageStamp(byte[] imgSource)
        {
            _byteSource = imgSource;
            LoadImageFromByteSource();
            InitializeBase();
        }

        private void InitializeBase()
        {
            if (_sourceImage != null)
            {
                if (_sourceImage.Height > 1000)
                {
                    base.Height = 1000;
                    double ratio = _sourceImage.Width / _sourceImage.Height;
                    base.Width = 1000 * ratio;
                }
                else
                {
                    base.Height = _sourceImage.Height;
                }

                if (_sourceImage.Width > 800)
                {
                    base.Width = 800;
                    double ratio = _sourceImage.Height / _sourceImage.Width;
                    base.Height = 800 * ratio;
                }
                else
                {
                    base.Width = _sourceImage.Width;
                }

                base.Position = new System.Windows.Point(150, 150);
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

        //returns true if stamp is anchor/placed by teacher
        //returns false if stamp is a copy of the anchor; moved by the student
        public bool IsAnchor
        {
            get
            {
                if (MetaData["IsAnchored"].SelectedValue == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    MetaData["IsAnchored"].AttributeValues[0] = "true";
                }
                else
                {
                    MetaData["IsAnchored"].AttributeValues[0] = "false";
                }
            }
        }

        #endregion //Properties
    }
}
