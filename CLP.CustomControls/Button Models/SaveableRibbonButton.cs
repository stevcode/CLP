using System;
using System.Runtime.Serialization;

namespace CLP.CustomControls
{
    [Serializable]
    public class SaveableRibbonButton : ASavableButtonBase
    {
        #region Constructor

        public SaveableRibbonButton() { }

        /// <summary>Initializes a new object from scratch.</summary>
        public SaveableRibbonButton(string text, string packUri)
            : base(text, packUri) { }

        /// <summary>Initializes a new object based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        private SaveableRibbonButton(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor
    }
}