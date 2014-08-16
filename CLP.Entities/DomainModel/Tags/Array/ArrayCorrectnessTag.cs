using System;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayCorrectnessTag" /> from scratch.
        /// </summary>
        public ArrayCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayCorrectnessTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayCorrectnessTag" /> belongs to.</param>
        public ArrayCorrectnessTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="ArrayCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}", Category); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}