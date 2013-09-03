using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;
using Catel.Data;
using System.Diagnostics;
using System.Collections.Generic;

namespace CLP.Models
{
    [Serializable]
    public class CLPAudio : ACLPPageObjectBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPAudio(ICLPPage page)
            : base(page)
        {
            ByteSource = new Byte[0];
            Height = 70;
            Width = 200;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPAudio(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties
        
        public byte[] ByteSource
        {
            get { return GetValue<byte[]>(ByteSourceProperty); }
            set { SetValue(ByteSourceProperty, value); }
        }

        public static readonly PropertyData ByteSourceProperty = RegisterProperty("ByteSource", typeof(byte[]), () => new Byte[0]);

        #endregion

        #region Methods

        public override string PageObjectType
        {
            get { return "CLPAudio"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPAudio newAudio = this.Clone() as CLPAudio;
            newAudio.UniqueID = Guid.NewGuid().ToString();
            newAudio.ParentPage = ParentPage;

            return newAudio;
        }

        #endregion
    }
}
