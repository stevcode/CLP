using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    /// <summary>
    /// MathRelation Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public abstract class MathRelation : ModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public MathRelation() { }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        protected MathRelation(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Methods

        public abstract String GetExampleNumberSentence();

        #endregion
    }
}