using System;
using System.Runtime.Serialization;

namespace CLP.Models
{

    [Serializable]
    public class CLPFactorCard : CLPArray
    {
        #region Constructors

        public CLPFactorCard(int rows, int columns, ICLPPage page)
            : base(rows, columns, page)
        {
            IsDivisionBehaviorOn = false;
            IsSnappable = false;
            IsGridOn = false;
        }
        
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override string PageObjectType
        {
            get { return "CLPFactorCard"; }
        }
    }
}
