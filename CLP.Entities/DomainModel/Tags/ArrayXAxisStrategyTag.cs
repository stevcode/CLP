using System;
using System.Runtime.Serialization;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    [Serializable]
    [RedirectType("CLP.Entities", "ArrayXAxisStrategyTag")]
    public class ArrayXAxisStrategyTag : ATagBase
    {
        public enum AcceptedValues
        {
            PlaceValue, // e.g. 43 -> 40 | 3
            Half, // e.g., 18 -> 9 | 9
            Tens, // e.g. 43 -> 10 | 10 | 10 | 10 | 3
            Fives,
            Twos,
            Singles,
            NoDividers,
            Other
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayXAxisStrategyTag" /> from scratch.
        /// </summary>
        public ArrayXAxisStrategyTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayXAxisStrategyTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayXAxisStrategyTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayXAxisStrategyTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayXAxisStrategyTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="ArrayXAxisStrategyTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayXAxisStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}