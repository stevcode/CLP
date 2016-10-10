using System;
using System.Runtime.Serialization;
using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    [Serializable]
    [RedirectType("CLP.Entities", "PageDefinitionTag")]
    public class PageDefinitionTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="PageDefinitionTag" /> from scratch.
        /// </summary>
        public PageDefinitionTag() { }

        /// <summary>
        /// Initializes <see cref="PageDefinitionTag" /> from <see cref="ProductRelation" />.
        /// </summary>
        /// <param name="relation">The value of the <see cref="PageDefinitionTag" />, parsed from <see cref="ProductRelation" />.</param>
        public PageDefinitionTag(CLPPage parentPage, ProductRelation relation)
            : base(parentPage)
        {
            Value = ProductRelation.toString(relation);
            IsSingleValueTag = true;
        }

        /// <summary>
        /// Initializes <see cref="PageDefinitionTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public PageDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}