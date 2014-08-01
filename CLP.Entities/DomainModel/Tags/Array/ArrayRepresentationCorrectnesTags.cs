using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayRepresentationCorrectnessTag : ATagBase
    {
        public enum AcceptedValues
        {
            Correct,
            ErrorSwappedFactors,
            ErrorMisusedGivens,
            ErrorOther
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" /> from scratch.
        /// </summary>
        public ArrayRepresentationCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayRepresentationCorrectnessTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayRepresentationCorrectnessTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayRepresentationCorrectnessTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override Category Category { get { return Category.Array; } }
    }
}