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
    public class CLPImageStamp : CLPPageObjectBase, ICLPStamp
    {

        public CLPImageStamp(string path) : base()
        {
            if (File.Exists(path))
            {
                _byteSource = File.ReadAllBytes(path);
            }

            LoadImageFromByteSource();
            InitializeBase();

            MetaData.SetValue("IsAnchored", "True");
            MetaData.SetValue("Parts", "0");
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
                Width = 150;
                double ratio = _sourceImage.Height / _sourceImage.Width;
                Height = Width * ratio;

                base.Position = new Point(10, 10);
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

        public override CLPPageObjectBase Copy()
        {
            CLPImageStamp newStamp = new CLPImageStamp(_byteSource);
            //copy all metadata and create new unique ID/creation date for the moved stamp
            newStamp.IsAnchored = IsAnchored;
            newStamp.Parts = Parts;
            newStamp.Position = Position;
            newStamp.Height = Height;
            newStamp.Width = Width;

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
        public bool IsAnchored
        {
            get
            {
                if (MetaData.GetValue("IsAnchored") == "True")
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
                    MetaData.SetValue("IsAnchored", "True");
                }
                else
                {
                    MetaData.SetValue("IsAnchored", "False");
                }
            }
        }

        public int Parts
        {
            get
            {
                if (MetaData.GetValue("Parts") == "")
                {
                    return 0;
                }
                else
                {
                    return Int32.Parse(MetaData.GetValue("Parts"));
                }   
            }
            set
            {
                MetaData.SetValue("Parts", value.ToString());
            }
        }

        #endregion //Properties
    }
}
