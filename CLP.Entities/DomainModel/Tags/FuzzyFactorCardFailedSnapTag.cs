using System.Runtime.Serialization;

namespace CLP.Entities
{
    public class FuzzyFactorCardFailedSnapTag : ATagBase
    {
        public enum AcceptedValues
        {
            TooMany,
            TooManyMultipleTimes,
            SnappedIncorrectDimension,
            SnappedIncorrectDimensionMultipleTimes,
            SnappedWrongOrientation,
            SnappedWrongOrientationMultipleTimes
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardFailedSnapTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardFailedSnapTag() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardFailedSnapTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardFailedSnapTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardFailedSnapTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardFailedSnapTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardFailedSnapTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardFailedSnapTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}