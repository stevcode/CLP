using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class StarredTag : ATagBase
    {
        public enum AcceptedValues
        {
            Starred,
            Unstarred,
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="StarredTag" /> from scratch.
        /// </summary>
        public StarredTag() { }

        /// <summary>
        /// Initializes <see cref="StarredTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StarredTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="StarredTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public StarredTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage)
        {
            Value = value.ToString();
            IsSingleValueTag = true;
        }

        /// <summary>
        /// Initializes <see cref="StarredTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public StarredTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}