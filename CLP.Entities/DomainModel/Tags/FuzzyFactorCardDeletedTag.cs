using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class FuzzyFactorCardDeletedTag : ATagBase
    {

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardDeletedTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardDeletedTag() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardDeletedTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardDeletedTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardDeletedTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardDeletedTag(CLPPage parentPage, string value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardDeletedTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardDeletedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}