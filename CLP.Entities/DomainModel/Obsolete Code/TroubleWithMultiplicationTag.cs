using System;
using System.Linq;

namespace CLP.Entities
{
    [Serializable]
    public class TroubleWithMultiplicationTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="TroubleWithMultiplicationTag" /> from scratch.</summary>
        public TroubleWithMultiplicationTag() { }

        /// <summary>Initializes <see cref="TroubleWithMultiplicationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TroubleWithMultiplicationTag" /> belongs to.</param>
        public TroubleWithMultiplicationTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Stamp;

        public override string FormattedName => "Trouble With Multiplication";

        public override string FormattedValue
            => $"{(GetTroubleWithStampGroupingCount(ParentPage) == 0 ? string.Empty : $"Trouble with Stamp Grouping {GetTroubleWithStampGroupingCount(ParentPage)} time(s).\n")}".TrimEnd('\n');

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Static Methods

        public static int GetTroubleWithStampGroupingCount(CLPPage page)
        {
            return page.Tags.Count(tag => tag is StampTroubleWithGroupingTag);
        }

        #endregion //Static Methods
    }
}