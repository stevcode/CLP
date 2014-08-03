using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateCompletenessTag : ATagBase
    {
        public enum AcceptedValues
        {
            NoArrays,
            NotEnoughArrays,
            Complete
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="DivisionTemplateCompletenessTag" /> from scratch.
        /// </summary>
        public DivisionTemplateCompletenessTag() { }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateCompletenessTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateCompletenessTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DivisionTemplateCompletenessTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public DivisionTemplateCompletenessTag(CLPPage parentPage, Origin origin, AcceptedValues value)
            : base(parentPage, origin) { Value = value; }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateCompletenessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateCompletenessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Value of the Starred Tag.
        /// </summary>
        public AcceptedValues Value
        {
            get { return GetValue<AcceptedValues>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(AcceptedValues));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get { return Value.ToString(); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}