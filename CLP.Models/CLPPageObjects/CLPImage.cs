using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [AllowNonSerializableMembers]
    public class CLPImage : CLPPageObjectBase
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPImage(string path)
            : base()
        {
            if (File.Exists(path))
            {
                ByteSource = File.ReadAllBytes(path);
            }

            XPosition = 10;
            YPosition = 10;
            Height = 300;
            Width = 300;
            LoadImageFromByteSource(ByteSource);

        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPImage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        //Parameterless constructor for Protobuf
        private CLPImage()
            : base()
        { }

        protected override void OnDeserialized()
        {
            LoadImageFromByteSource(ByteSource);
            base.OnDeserialized();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Byte source of the image that gets serialized.
        /// </summary>
        public byte[] ByteSource
        {
            get { return GetValue<byte[]>(ByteSourceProperty); }
            set { SetValue(ByteSourceProperty, value); }
        }

        /// <summary>
        /// Register the ByteSource property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ByteSourceProperty = RegisterProperty("ByteSource", typeof(byte[]), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            private set { SetValue(SourceImageProperty, value); }
        }

        /// <summary>
        /// Register the SourceImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof(ImageSource), null);

        #endregion

        #region Methods

        public void LoadImageFromByteSource(byte[] byteSource)
        {
            //MemoryStream memoryStream = new MemoryStream(byteSource, 0, byteSource.Length, false, false);
            //BitmapImage genBmpImage = new BitmapImage();

            //genBmpImage.BeginInit();
            //genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
            ////genBmpImage.DecodePixelHeight = Convert.ToInt32(this.Height);
            //genBmpImage.StreamSource = memoryStream;
            //genBmpImage.EndInit();
            //genBmpImage.Freeze();

            ////memoryStream.Close();
            //memoryStream.Dispose();
            //memoryStream = null;

            //SourceImage = genBmpImage;
        }

        public override string PageObjectType
        {
            get { return "CLPImage"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPImage newImage = this.Clone() as CLPImage;
            newImage.UniqueID = Guid.NewGuid().ToString();

            return newImage;
        }

        #endregion

    }
}
