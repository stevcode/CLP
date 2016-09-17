using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class PageTopicTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="PageTopicTag" /> from scratch.</summary>
        public PageTopicTag() { }

        /// <summary>Initializes <see cref="PageTopicTag" /> from a value.</summary>
        /// <param name="value">The value of the <see cref="PageTopicTag" />.</param>
        public PageTopicTag(CLPPage parentPage, Origin origin, string value)
            : base(parentPage, origin)
        {
            Value = value;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the Starred Tag.</summary>
        public string Value
        {
            get { return GetValue<string>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(string), string.Empty);

        #region ATagBase Overrides

        public override Category Category => Category.CurriculumInformation;

        public override string FormattedName => "Page Topic";

        public override string FormattedValue
        {
            get { return Value; }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}