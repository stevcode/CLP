using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class CLPImage : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPImage" /> from scratch.
        /// </summary>
        public CLPImage() { }

        /// <summary>
        /// Initializes <see cref="CLPImage" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="CLPImage" /> belongs to.</param>
        /// <param name="imageHashID">A unique hash of the image that corresponds to the fileName in the ImagePool.</param>
        /// <param name="height">Original height of the image.</param>
        /// <param name="width">Original width of the image.</param>
        public CLPImage(CLPPage parentPage, string imageHashID, double height, double width)
            : base(parentPage)
        {
            ImageHashID = imageHashID;
            XPosition = 50.0;
            YPosition = 50.0;
            Height = 300.0;
            Width = 300.0;

            //var aspectRatio = width / height;
            //EnforceAspectRatio(aspectRatio);

            //ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes <see cref="CLPImage" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPImage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override int ZIndex { get { return 10; } }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        /// <summary>
        /// The unique Hash of the image this <see cref="CLPImage" /> represents.
        /// </summary>
        public string ImageHashID
        {
            get { return GetValue<string>(ImageHashIDProperty); }
            set { SetValue(ImageHashIDProperty, value); }
        }

        public static readonly PropertyData ImageHashIDProperty = RegisterProperty("ImageHashID", typeof(string), string.Empty);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newCLPImage = Clone() as CLPImage;
            if(newCLPImage == null)
            {
                return null;
            }
            newCLPImage.CreationDate = DateTime.Now;
            newCLPImage.ID = Guid.NewGuid().ToCompactID();
            newCLPImage.VersionIndex = 0;
            newCLPImage.LastVersionIndex = null;
            newCLPImage.ParentPage = ParentPage;

            return newCLPImage;
        }

        public static BitmapImage GetImageFromPath(string filePath)
        {
            if(!File.Exists(filePath))
            {
                return null;
            }
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        #endregion //Methods
    }
}