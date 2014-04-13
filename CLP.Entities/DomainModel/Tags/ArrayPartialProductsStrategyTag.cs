using System.Runtime.Serialization;

namespace CLP.Entities
{
    public class ArrayPartialProductsStrategyTag : ATagBase
    {
        public enum AcceptedValues
        {
            FriendlyNumbers,
            SomeRepeated,
            AllRepeated
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayPartialProductsStrategyTag" /> from scratch.
        /// </summary>
        public ArrayPartialProductsStrategyTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayPartialProductsStrategyTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayPartialProductsStrategyTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayPartialProductsStrategyTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayPartialProductsStrategyTag(CLPPage parentPage, AcceptedValues value)
            : base(parentPage) { Value = value.ToString(); }

        /// <summary>
        /// Initializes <see cref="ArrayPartialProductsStrategyTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayPartialProductsStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}