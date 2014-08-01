using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class DottedTag : ATagBase
    {
        public enum AcceptedValues
        {
            Dotted,
            Undotted,
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="DottedTag" /> from scratch.
        /// </summary>
        public DottedTag() { }

        /// <summary>
        /// Initializes <see cref="DottedTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DottedTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DottedTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public DottedTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage)
        {
            Value = value.ToString();
            IsSingleValueTag = true;
        }

        /// <summary>
        /// Initializes <see cref="DottedTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DottedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override Category Category { get { return Category.OtherPageInformation; } }
    }
}