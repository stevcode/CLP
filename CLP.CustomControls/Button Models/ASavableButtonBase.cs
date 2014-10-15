using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.CustomControls
{
    [Serializable]
    public abstract class ASavableButtonBase : ModelBase
    {
        #region Constructors

        /// <summary>Initializes a new object from scratch.</summary>
        protected ASavableButtonBase() { }

        protected ASavableButtonBase(string text, string packUri)
        {
            Text = text;
            LargeImageURI = packUri;
        }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected ASavableButtonBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>URI of the Image on the Button.</summary>
        public string LargeImageURI
        {
            get { return GetValue<string>(LargeImageURIProperty); }
            set { SetValue(LargeImageURIProperty, value); }
        }

        public static readonly PropertyData LargeImageURIProperty = RegisterProperty("LargeImageURI", typeof (string), string.Empty);

        /// <summary>Text of the Button.</summary>
        public string Text
        {
            get { return GetValue<string>(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof (string), string.Empty);

        #endregion //Properties
    }
}