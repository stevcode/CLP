using System;
using Catel.Data;

namespace CLP.Entities
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

        /// <summary>Initializes <see cref="StarredTag" /> from scratch.</summary>
        public StarredTag() { }

        /// <summary>Initializes <see cref="StarredTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StarredTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DottedTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public StarredTag(CLPPage parentPage, Origin origin, AcceptedValues value)
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

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(AcceptedValues));

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.OtherPageInformation;

        public override string FormattedName => "Starred";

        public override string FormattedValue
        {
            get { return Value.ToString(); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}