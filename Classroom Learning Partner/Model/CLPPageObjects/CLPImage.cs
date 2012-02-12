using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;
using Catel.Data;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CLPImage : CLPPageObjectBase, ICLPPageObject
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

            Position = new System.Windows.Point(10, 10);
            Height = 300;
            Width = 300;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPImage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Byte source of the image that gets serialized.
        /// </summary>
        public byte[] ByteSource
        {
            get { return GetValue<byte[]>(ByteSourceProperty); }
            private set { SetValue(ByteSourceProperty, value); }
        }

        /// <summary>
        /// Register the ByteSource property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ByteSourceProperty = RegisterProperty("ByteSource", typeof(byte[]), null);

        #endregion

        #region Methods

        /// <summary>
        /// Validates the fields.
        /// </summary>
        protected override void ValidateFields()
        {
            // TODO: Implement any field validation of this object. Simply set any error by using the SetFieldError method
        }

        /// <summary>
        /// Validates the business rules.
        /// </summary>
        protected override void ValidateBusinessRules()
        {
            // TODO: Implement any business rules of this object. Simply set any error by using the SetBusinessRuleError method
        }

        public ICLPPageObject Duplicate()
        {
            CLPImage newImage = this.Clone() as CLPImage;
            newImage.UniqueID = Guid.NewGuid().ToString();

            return newImage;
        }

        #endregion

        //#region Constructors

        //public CLPImage(string path) : base()
        //{
        //    if (File.Exists(path))
        //    {
        //        _byteSource = File.ReadAllBytes(path);
        //    }

        //    LoadImageFromByteSource();
        //    InitializeBase();
        //}

        //private void InitializeBase()
        //{
        //    if (_sourceImage != null)
        //    {
        //        if (_sourceImage.Height > 1000)
        //        {
        //            Height = 1000;
        //            double ratio = _sourceImage.Height / _sourceImage.Width;
        //            Width = 1000 * ratio;
        //        }
        //        else
        //        {
        //            Height = _sourceImage.Height;
        //        }

        //        if (_sourceImage.Width > 800)
        //        {
        //            Width = 800;
        //            double ratio = _sourceImage.Width / _sourceImage.Height;
        //            Height = 800 * ratio;
        //        }
        //        else
        //        {
        //            Width = _sourceImage.Width;
        //        }

        //        base.Position = new System.Windows.Point(10, 10);
        //    }
        //}

        //[OnDeserialized]
        //private void OnDeserialized(StreamingContext context)
        //{
        //    LoadImageFromByteSource();
        //}

        //private void LoadImageFromByteSource()
        //{
        //    MemoryStream memoryStream = new MemoryStream(ByteSource, 0, ByteSource.Length, false, false);
        //    BitmapImage genBmpImage = new BitmapImage();

        //    genBmpImage.BeginInit();
        //    genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
        //    genBmpImage.StreamSource = memoryStream;
        //    genBmpImage.EndInit();
        //    genBmpImage.Freeze();

        //    memoryStream.Dispose();
        //    memoryStream.Close();
        //    memoryStream = null;

        //    _sourceImage = genBmpImage;
        //}

        //public CLPImage(byte[] imgSource)
        //{
        //    _byteSource = imgSource;
        //    LoadImageFromByteSource();
        //    InitializeBase();
        //}

        //#endregion //Constructors

        //#region Properties

        ////Non-Serialized
        //[NonSerialized]
        //private ImageSource _sourceImage;
        //public ImageSource SourceImage
        //{
        //    get
        //    {
        //        if (_sourceImage == null)
        //        {
        //            LoadImageFromByteSource();
        //        }
        //        return _sourceImage;
        //    }
        //}

        //private byte[] _byteSource;
        //public byte[] ByteSource
        //{
        //    get
        //    {
        //        return _byteSource;
        //    }
        //}

        //#endregion //Properties

        
    }
}
