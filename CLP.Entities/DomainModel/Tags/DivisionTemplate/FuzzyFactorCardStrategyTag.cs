using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class FuzzyFactorCardStrategyTag : ATagBase
    {
        public enum AcceptedValues
        {
            OneArray, // Only one array snapped in
            Repeat, // e.g. 28 / 4 -> 4 x 3 | 4 x 3 | 4 x 1
            Tens, // e.g. 93 / 4 -> 4 x 10 | 4 x 10 | 4 x 3
            Fives, // e.g. 44 / 4 -> 4 x 5 | 4 x 5 | 4 x 1
            Twos,
            Singles, // e.g. 43 / 4 -> 4 x 1 | 4 x 1 | ... | 4 x 1
            Other
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardStrategyTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardStrategyTag() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardStrategyTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardStrategyTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardStrategyTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardStrategyTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardStrategyTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override Category Category { get { return Category.DivisionTemplate; } }
    }
}