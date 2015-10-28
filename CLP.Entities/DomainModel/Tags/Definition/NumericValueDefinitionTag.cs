using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumericValueDefinitionTag : ATagBase, IRelationPart
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="NumericValueDefinitionTag" /> from scratch.
        /// </summary>
        public NumericValueDefinitionTag() { }

        /// <summary>
        /// Initializes <see cref="NumericValueDefinitionTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="NumericValueDefinitionTag" /> belongs to.</param>
        public NumericValueDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="NumericValueDefinitionTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public NumericValueDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Value of the numberic answer
        /// </summary>
        public double NumericValue
        {
            get { return GetValue<double>(NumericValueProperty); }
            set { SetValue(NumericValueProperty, value); }
        }

        public static readonly PropertyData NumericValueProperty = RegisterProperty("NumericValue", typeof(double), 0);

        #region IRelationPartImplementation

        public double RelationPartAnswerValue
        {
            get { return NumericValue; }
        }

        public string FormattedRelation
        {
            get { return string.Format("{0}", RelationPartAnswerValue); }
        }

        public string ExpandedFormattedRelation
        {
            get { return string.Format("{0}", RelationPartAnswerValue); }
        }

        #endregion //IRelationPartImplementation

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Definition; }
        }

        public override string FormattedName
        {
            get { return "Numeric Value Definition"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Value: {0}", NumericValue); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}