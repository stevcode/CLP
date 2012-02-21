using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class  CLPStamp : CLPPageObjectBase
    {
        public CLPStamp() : base()
        {
            StampConstruct();
            Height = 150;
        }

        public CLPStamp(string path) : base()
        {
            StampConstruct();
            if (File.Exists(path))
            {
                ByteSource = File.ReadAllBytes(path);
            }
            LoadImageFromByteSource();
            InitializeBase();
        }

        public CLPStamp(byte[] imgSource)
        {
            StampConstruct();
            ByteSource = imgSource;
            LoadImageFromByteSource();
            InitializeBase();
        }

        #region Methods

        private void StampConstruct(){
            MetaData.SetValue("IsAnchored", "True");
            MetaData.SetValue("Parts", "0");
            base.Position = new Point(10, 10);
            Width = 150;
            IsAnchored = true;
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
            CLPStamp newStamp;
            if (ByteSource == null)
            {
                newStamp = new CLPStamp();
            }
            else
            {
                newStamp = new CLPStamp(ByteSource);
            }
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

        #endregion Methods

        #region Properties

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

        //Non-Serialized
        [NonSerialized]
        private ImageSource _sourceImage = null;
        public ImageSource SourceImage
        {
            get
            {
//                if (_sourceImage == null)
//                {
//                    LoadImageFromByteSource();
//                }
                return _sourceImage;
            }
            set
            {
                _sourceImage = value;
            }
        }

        private byte[] _byteSource = null;
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