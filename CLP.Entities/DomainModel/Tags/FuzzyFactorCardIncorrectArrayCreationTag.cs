using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class FuzzyFactorCardIncorrectArrayCreationTag : ATagBase
    {
        public enum AcceptedValues
        {
            TooMany,
            IncorrectDimension,
            WrongOrientation,
            ProductAsDimension
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardIncorrectArrayCreationTag" /> from scratch.
        /// </summary>
        public FuzzyFactorCardIncorrectArrayCreationTag() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardIncorrectArrayCreationTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCardIncorrectArrayCreationTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="FuzzyFactorCardIncorrectArrayCreationTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public FuzzyFactorCardIncorrectArrayCreationTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCardIncorrectArrayCreationTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCardIncorrectArrayCreationTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}