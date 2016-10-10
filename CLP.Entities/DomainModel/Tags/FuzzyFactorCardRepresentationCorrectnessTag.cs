using System;
using System.Runtime.Serialization;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    [Serializable]
    [RedirectType("CLP.Entities", "FuzzyFactorCardRepresentationCorrectnessTag")]
    public class FuzzyFactorCardRepresentationCorrectnessTag : ATagBase
    {
        public enum AcceptedValues
        {
            Correct,
            ErrorMisusedGivens,
            ErrorOther
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardRepresentationCorrectnessTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardRepresentationCorrectnessTag()
        {
        }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardRepresentationCorrectnessTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardRepresentationCorrectnessTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardRepresentationCorrectnessTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardRepresentationCorrectnessTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage)
        {
            Value = value.ToString();
        }

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors
    }
}