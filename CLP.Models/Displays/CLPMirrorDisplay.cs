using System.Runtime.Serialization;

namespace CLP.Models
{
    public class CLPMirrorDisplay : ACLPDisplayBase
    {
        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPMirrorDisplay()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPMirrorDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        public DisplayTypes DisplayType { get { return DisplayTypes.Mirror; } }
    }
}
