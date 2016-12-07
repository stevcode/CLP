using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionRelationDefinitionTag : ATagBase
    {
        public enum RelationTypes
        {
            GeneralDivision
        }

        #region Constructors

        /// <summary>Initializes <see cref="DivisionRelationDefinitionTag" /> from scratch.</summary>
        public DivisionRelationDefinitionTag() { }

        /// <summary>Initializes <see cref="DivisionRelationDefinitionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionRelationDefinitionTag" /> belongs to.</param>
        public DivisionRelationDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Dividend of the division relation. Dividend / Divisor = Quotient R Remainder.</summary>
        public IRelationPart Dividend
        {
            get { return GetValue<IRelationPart>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(IRelationPart));

        /// <summary>Divisor of the division relation. Dividend / Divisor = Quotient R Remainder.</summary>
        public IRelationPart Divisor
        {
            get { return GetValue<IRelationPart>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof(IRelationPart));

        /// <summary>Quotient of the division relation. Dividend / Divisor = Quotient R Remainder.</summary>
        public double Quotient
        {
            get { return GetValue<double>(QuotientProperty); }
            set { SetValue(QuotientProperty, value); }
        }

        public static readonly PropertyData QuotientProperty = RegisterProperty("Quotient", typeof(double), 0);

        /// <summary>Remainder of the division relation. Dividend / Divisor = Quotient R Remainder.</summary>
        public double Remainder
        {
            get { return GetValue<double>(RemainderProperty); }
            set { SetValue(RemainderProperty, value); }
        }

        public static readonly PropertyData RemainderProperty = RegisterProperty("Remainder", typeof(double), 0);

        /// <summary>Type of division relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(RelationTypes), RelationTypes.GeneralDivision);

        #region ATagBase Overrides

        public override Category Category => Category.Definition;

        public override string FormattedName => "Division Relation Definition";

        public override string FormattedValue => $"Relation Type: {RelationType}\n" + $"{Dividend.RelationPartAnswerValue} / {Divisor.RelationPartAnswerValue} = {Quotient} R{Remainder}";

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region IRepresentationComparer Implementation

        public Correctness CompareRelationToRepresentations(List<IPageObject> pageObjects)
        {
            return Correctness.Correct;
        }

        #endregion // IRepresentationComparer Implementation
    }
}