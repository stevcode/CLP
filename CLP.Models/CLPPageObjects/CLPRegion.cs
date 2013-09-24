using System;
using System.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPRegion : ACLPPageObjectBase
    {
        #region Constructors

        public CLPRegion(ICLPPage page)
            : base(page)
        {
            CanAcceptStrokes = true;
            CanAcceptPageObjects = true;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Overrides of ACLPPageObjectBase

        public override string PageObjectType
        {
            get { throw new NotImplementedException(); }
        }

        public override ICLPPageObject Duplicate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
