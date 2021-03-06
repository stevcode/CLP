﻿using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionRelationDefinitionTag : ATagBase, IRelationPart, IDefinition
    {
        public enum RelationTypes
        {
            GeneralDivision,
            Partitive,
            Quotative
        }

        #region Constructors

        public DivisionRelationDefinitionTag() { }

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

        public static readonly PropertyData QuotientProperty = RegisterProperty("Quotient", typeof(double), 0.0);

        /// <summary>Remainder of the division relation. Dividend / Divisor = Quotient R Remainder.</summary>
        public double Remainder
        {
            get { return GetValue<double>(RemainderProperty); }
            set { SetValue(RemainderProperty, value); }
        }

        public static readonly PropertyData RemainderProperty = RegisterProperty("Remainder", typeof(double), 0.0);

        /// <summary>Type of division relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(RelationTypes), RelationTypes.GeneralDivision);

        #endregion //Properties

        #region ATagBase Overrides

        public override Category Category => Category.Definition;

        public override string FormattedName => "Division Relation Definition";

        public override string FormattedValue => $"Relation Type: {RelationType}\n" + $"{Dividend.RelationPartAnswerValue} / {Divisor.RelationPartAnswerValue} = {Quotient} R{Remainder}";

        #endregion // ATagBase Overrides

        #region IRelationPart Implementation

        public double RelationPartAnswerValue => Math.Abs(Remainder) < 0.0001 ? Quotient : Quotient + (Dividend.RelationPartAnswerValue / Remainder);

        public string FormattedAnswerValue => RelationPartAnswerValue.ToString();

        public string FormattedRelation => $"{Dividend.FormattedAnswerValue} / {Divisor.FormattedAnswerValue}";

        public string ExpandedFormattedRelation => $"{Dividend.ExpandedFormattedRelation} / {Divisor.ExpandedFormattedRelation}";

        #endregion // IRelationPart Implementation

        #region IDefinition Implementation

        public double Answer => RelationPartAnswerValue;

        #endregion // IDefinition Implementation
    }
}