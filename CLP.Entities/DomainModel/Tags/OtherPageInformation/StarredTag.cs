using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class StarredTag : ATagBase
    {
        public enum AcceptedValues
        {
            Starred,
            Unstarred
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="StarredTag" /> from scratch.
        /// </summary>
        public StarredTag() { }

        /// <summary>
        /// Initializes <see cref="StarredTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StarredTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DottedTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public StarredTag(CLPPage parentPage, Origin origin, AcceptedValues value)
            : base(parentPage, origin)
        {
            Value = value;
            IsSingleValueTag = true;
        }

        /// <summary>
        /// Initializes <see cref="StarredTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public StarredTag(SerializationInfo info, StreamingContext context)
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
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedName
        {
            get { return "Starred"; }
        }

        public override string FormattedValue
        {
            get { return Value.ToString(); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}