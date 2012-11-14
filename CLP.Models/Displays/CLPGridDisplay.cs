using System;
using System.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPGridDisplay : ACLPDisplayBase
    {
        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPGridDisplay()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPGridDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        public DisplayTypes DisplayType { get { return DisplayTypes.Grid; } }
    }
}
