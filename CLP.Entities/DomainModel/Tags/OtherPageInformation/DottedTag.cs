using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class DottedTag : ATagBase
    {
        public enum AcceptedValues
        {
            Dotted,
            Undotted,
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="DottedTag" /> from scratch.
        /// </summary>
        public DottedTag() { }

        /// <summary>
        /// Initializes <see cref="DottedTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DottedTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DottedTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public DottedTag(CLPPage parentPage, Origin origin, AcceptedValues value)
            : base(parentPage, origin)
        {
            Value = value;
            IsSingleValueTag = true;
        }

        /// <summary>
        /// Initializes <see cref="DottedTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DottedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Value of the Dotted Tag.
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
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedName
        {
            get { return "Had Help"; }
        }

        public override string FormattedValue
        {
            get { return Value == AcceptedValues.Dotted ? "True" : "False"; }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}