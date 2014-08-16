using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="DivisionTemplateCorrectnessTag" /> from scratch.
        /// </summary>
        public DivisionTemplateCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateCorrectnessTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateCorrectnessTag" /> belongs to.</param>
        public DivisionTemplateCorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            Correctness = correctness;
        }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}", Correctness); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}