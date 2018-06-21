using System;
using Catel.Data;

namespace CLP.Entities
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

        /// <summary>Initializes <see cref="ArrayOrientationTag" /> from scratch.</summary>
        public ArrayOrientationTag() { }

        /// <summary>Initializes <see cref="ArrayOrientationTag" /> from <see cref="AcceptedValues" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayOrientationTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="ArrayOrientationTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public ArrayOrientationTag(CLPPage parentPage, Origin origin, AcceptedValues value)
            : base(parentPage, origin)
        {
            Value = value;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public AcceptedValues Value
        {
            get { return GetValue<AcceptedValues>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(AcceptedValues), AcceptedValues.Unknown);

        #region ATagBase Overrides

        public override Category Category => Category.Array;

        public override string FormattedName => "Array Orientation";

        public override string FormattedValue
        {
            get { return Value.ToString(); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}