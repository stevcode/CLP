using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumericValueDefinitionTag : ATagBase, IRelationPart
    {
        #region Constructors

        /// <summary>Initializes <see cref="NumericValueDefinitionTag" /> from scratch.</summary>
        public NumericValueDefinitionTag() { }

        /// <summary>Initializes <see cref="NumericValueDefinitionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="NumericValueDefinitionTag" /> belongs to.</param>
        public NumericValueDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Value of the numberic answer</summary>
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

        public override Category Category => Category.Definition;

        public override string FormattedName => "Numeric Value Definition";

        public override string FormattedValue
        {
            get { return string.Format("Value: {0}", NumericValue); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}