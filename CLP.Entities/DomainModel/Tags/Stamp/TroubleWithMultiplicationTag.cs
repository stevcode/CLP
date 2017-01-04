using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Entities.Demo
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
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;
        }

        /// <summary>Initializes <see cref="TroubleWithMultiplicationTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TroubleWithMultiplicationTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Stamp; }
        }

        public override string FormattedName
        {
            get { return "Trouble With Multiplication"; }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format("{0}",
                                  GetTroubleWithStampGroupingCount(ParentPage) == 0
                                      ? string.Empty
                                      : string.Format("Trouble with Stamp Grouping {0} time(s).\n", GetTroubleWithStampGroupingCount(ParentPage))).TrimEnd('\n');
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Static Methods

        public static int GetTroubleWithStampGroupingCount(CLPPage page) { return page.Tags.Count(tag => tag is StampTroubleWithGroupingTag); }

        #endregion //Static Methods
    }
}