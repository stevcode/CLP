using System;
using System.Runtime.Serialization;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    [Serializable]
    [RedirectType("CLP.Entities", "ArrayYAxisStrategyTag")]
    public class ArrayYAxisStrategyTag : ATagBase
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
        /// Initializes <see cref="ArrayYAxisStrategyTag" /> from scratch.
        /// </summary>
        public ArrayYAxisStrategyTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayYAxisStrategyTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayYAxisStrategyTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayYAxisStrategyTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayYAxisStrategyTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="ArrayYAxisStrategyTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayYAxisStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}