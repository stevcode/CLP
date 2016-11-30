using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Ann
{
    [Serializable]
    public class ArrayOrientationTag : ATagBase
    {
        public enum AcceptedValues
        {
            FirstFactorWidth,
            FirstFactorHeight,
            Unknown
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayOrientationTag" /> from scratch.
        /// </summary>
        public ArrayOrientationTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayOrientationTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayOrientationTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayOrientationTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayOrientationTag(CLPPage parentPage, Origin origin, AcceptedValues value)
            : base(parentPage, origin) { Value = value; }

        /// <summary>
        /// Initializes <see cref="ArrayOrientationTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayOrientationTag(SerializationInfo info, StreamingContext context)
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
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Orientation"; }
        }

        public override string FormattedValue
        {
            get { return Value.ToString(); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}