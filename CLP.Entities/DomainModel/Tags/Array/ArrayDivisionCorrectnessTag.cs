using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayDivisionCorrectnessTag : ATagBase
    {
        public enum AcceptedValues
        {
            Correct,
            Incorrect,
            Unfinished
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayDivisionCorrectnessTag" /> from scratch.
        /// </summary>
        public ArrayDivisionCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayDivisionCorrectnessTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayDivisionCorrectnessTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayDivisionCorrectnessTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayDivisionCorrectnessTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="ArrayDivisionCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayDivisionCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override Category Category { get { return Category.Array; } }
    }
}