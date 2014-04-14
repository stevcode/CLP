using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class FuzzyFactorCardDeletedDivisionTag : ATagBase
    {
        public enum AcceptedValues
        {
            CorrectDeletion,
            IncorrectDeletion
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardDeletedDivisionTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardDeletedDivisionTag() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardDeletedDivisionTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardDeletedDivisionTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardDeletedDivisionTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardDeletedDivisionTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardDeletedDivisionTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardDeletedDivisionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}