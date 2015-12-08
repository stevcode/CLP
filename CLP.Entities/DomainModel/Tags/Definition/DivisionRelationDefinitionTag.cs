using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionRelationDefinitionTag : ATagBase, IRepresentationComparer
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

        /// <summary>Initializes <see cref="DivisionRelationDefinitionTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionRelationDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Dividend of the division relation.</summary>
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (double), 0);

        /// <summary>Divisor of the division relation.</summary>
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof (double), 0);

        /// <summary>Quotient of the division relation.</summary>
        public double Quotient
        {
            get { return GetValue<double>(QuotientProperty); }
            set { SetValue(QuotientProperty, value); }
        }

        public static readonly PropertyData QuotientProperty = RegisterProperty("Quotient", typeof (double), 0);

        /// <summary>Remainder of the division relation.</summary>
        public double Remainder
        {
            get { return GetValue<double>(RemainderProperty); }
            set { SetValue(RemainderProperty, value); }
        }

        public static readonly PropertyData RemainderProperty = RegisterProperty("Remainder", typeof (double), 0);

        /// <summary>Type of division relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType",
                                                                                    typeof (RelationTypes),
                                                                                    RelationTypes.GeneralDivision);

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Definition; }
        }

        public override string FormattedName
        {
            get { return "Division Relation Definition"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Relation Type: {0}\n" + "{1} / {2} = {3} R{4}", RelationType, Dividend, Divisor, Quotient, Remainder); }
        }

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