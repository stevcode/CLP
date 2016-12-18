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

        public static readonly PropertyData NumericValueProperty = RegisterProperty("NumericValue", typeof(double), 0.0);

        /// <summary>Determines whether the numeric value is a given and as such should be displayed.</summary>
        public bool IsNotGiven
        {
            get { return GetValue<bool>(IsNotGivenProperty); }
            set { SetValue(IsNotGivenProperty, value); }
        }

        public static readonly PropertyData IsNotGivenProperty = RegisterProperty("IsNotGiven", typeof(bool), false);

        #endregion //Properties

        #region ATagBase Overrides

        public override Category Category => Category.Definition;

        public override string FormattedName => "Numeric Value Definition";

        public override string FormattedValue => $"Value: {NumericValue}";

        #endregion //ATagBase Overrides

        #region IRelationPartImplementation

        public double RelationPartAnswerValue => NumericValue;

        public string FormattedAnswerValue => IsNotGiven ? $"?({RelationPartAnswerValue})" : RelationPartAnswerValue.ToString();

        public string FormattedRelation => IsNotGiven ? $"?({RelationPartAnswerValue})" : RelationPartAnswerValue.ToString();

        public string ExpandedFormattedRelation => IsNotGiven ? $"?({RelationPartAnswerValue})" : RelationPartAnswerValue.ToString();

        #endregion //IRelationPartImplementation
    }
}