using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayOrientationTag : ATagBase
    {
        public enum AcceptedValues
        {
            FirstFactorWidth,
            FirstFactorHeight,
            Unknown
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayOrientationTag" /> from scratch.
        /// </summary>
        public ArrayOrientationTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayOrientationTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayOrientationTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayOrientationTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayOrientationTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="ArrayOrientationTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayOrientationTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override Category Category { get { return Category.Array; } }
    }
}