using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class TroubleWithGroupingTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="TroubleWithGroupingTag" /> from scratch.
        /// </summary>
        public TroubleWithGroupingTag() { }

        /// <summary>
        /// Initializes <see cref="TroubleWithGroupingTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TroubleWithGroupingTag" /> belongs to.</param>
        public TroubleWithGroupingTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="TroubleWithGroupingTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TroubleWithGroupingTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Stamp; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}", Category); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}