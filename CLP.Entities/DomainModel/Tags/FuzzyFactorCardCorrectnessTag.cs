using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class FuzzyFactorCardCorrectnessTag : ATagBase
    {
        public enum AcceptedValues
        {
            NoArrays,
            NotEnoughArrays,
            Complete
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardCorrectnessTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardCorrectnessTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardCorrectnessTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardCorrectnessTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardCorrectnessTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}