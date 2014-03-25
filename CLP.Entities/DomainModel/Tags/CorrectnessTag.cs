using System.Runtime.Serialization;

namespace CLP.Entities
{
    public class CorrectnessTag : ATagBase
    {
        public enum AcceptedValues
        {
            Correct,
            AlmostCorrect,
            Incorrect,
            Uknown
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
        public CorrectnessTag(AcceptedValues value)
        {
            Value = value.ToString();
            IsSingleValueTag = true;
        }

        /// <summary>
        /// Initializes <see cref="CorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}