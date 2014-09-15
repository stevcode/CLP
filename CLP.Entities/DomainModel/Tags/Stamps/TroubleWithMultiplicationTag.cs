using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class TroubleWithMultiplicationTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="TroubleWithMultiplicationTag" /> from scratch.
        /// </summary>
        public TroubleWithMultiplicationTag() { }

        /// <summary>
        /// Initializes <see cref="TroubleWithMultiplicationTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TroubleWithMultiplicationTag" /> belongs to.</param>
        public TroubleWithMultiplicationTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="TroubleWithMultiplicationTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
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

        public override string FormattedValue
        {
            get { return string.Format("{0}", Category); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}