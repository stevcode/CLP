using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Classroom_Learning_Partner.Model.Displays
{
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
