using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class CorrectnessTag : ATagBase
    {
        public enum AcceptedValues
        {
            Correct,
            AlmostCorrect,
            Incorrect,
            Unknown
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" /> from scratch.
        /// </summary>
        public CorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="value">The value of the <see cref="CorrectnessTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public CorrectnessTag(CLPPage parentPage, Origin origin, AcceptedValues value)
            : base(parentPage)
        {
            Origin = origin;
            IsSingleValueTag = true;
            Value = value.ToString();
        }

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override Category Category { get { return Category.OtherPageInformation; } }

        public override string FormattedValue
        {
            get { return Value; }
        }
    }
}