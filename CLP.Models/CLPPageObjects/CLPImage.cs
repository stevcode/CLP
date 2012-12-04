using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using System.Collections.Generic;

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
        public CLPImage(string imageID, CLPPage page)
            : base(page)
        {
            ImageID = imageID;
            XPosition = 10;
            YPosition = 10;
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
        /// Gets or sets the property value.
        /// </summary>
        public string ImageID
        {
            get { return GetValue<string>(ImageIDProperty); }
            set { SetValue(ImageIDProperty, value); }
        }

        /// <summary>
        /// Register the ImageID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ImageIDProperty = RegisterProperty("ImageID", typeof(string), null);

        #endregion

        #region Methods

        public override string PageObjectType
        {
            get { return "CLPImage"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPImage newImage = this.Clone() as CLPImage;
            newImage.UniqueID = Guid.NewGuid().ToString();
            newImage.ParentPage = ParentPage;

            return newImage;
        }

        #endregion

    }
}
