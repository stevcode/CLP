using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class MultiplicationRelationDefinitionTag : ATagBase, IRelationPart
    {
        public enum RelationTypes
        {
            GeneralMultiplication,
            Area,
            EqualGroups,
            OrderedEqualGroups,
            Commutativity,
            Associativity
        }

        #region Constructors

        /// <summary>Initializes <see cref="MultiplicationRelationDefinitionTag" /> from scratch.</summary>
        public MultiplicationRelationDefinitionTag() { }

        /// <summary>Initializes <see cref="MultiplicationRelationDefinitionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="MultiplicationRelationDefinitionTag" /> belongs to.</param>
        public MultiplicationRelationDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the factors in the multiplication relation.</summary>
        public List<IRelationPart> Factors
        {
            get { return GetValue<List<IRelationPart>>(FactorsProperty); }
            set { SetValue(FactorsProperty, value); }
        }

        public static readonly PropertyData FactorsProperty = RegisterProperty("Factors", typeof(List<IRelationPart>), () => new List<IRelationPart>());

        /// <summary>Value of the product of the multiplication relation.</summary>
        public double Product
        {
            get { return GetValue<double>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof(double), 0);

        /// <summary>Type of multiplication relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(RelationTypes), RelationTypes.GeneralMultiplication);

        #region IRelationPartImplementation

        public double RelationPartAnswerValue
        {
            get { return Product; }
        }

        public string FormattedRelation
        {
            get
            {
                switch (RelationType)
                {
                    case RelationTypes.EqualGroups:
                    case RelationTypes.OrderedEqualGroups:
                        return string.Join(" group(s) of ", Factors.Select(x => x.RelationPartAnswerValue));
                    default:
                        return string.Join("x", Factors.Select(x => x.RelationPartAnswerValue));
                }
            }
        }

        public string ExpandedFormattedRelation
        {
            get
            {
                switch (RelationType)
                {
                    case RelationTypes.EqualGroups:
                    case RelationTypes.OrderedEqualGroups:
                        return string.Join(" group(s) of ", Factors.Select(x => x is NumericValueDefinitionTag ? x.FormattedRelation : "(" + x.FormattedRelation + ")"));
                    default:
                        return string.Join("x", Factors.Select(x => x is NumericValueDefinitionTag ? x.FormattedRelation : "(" + x.FormattedRelation + ")"));
                }
            }
        }

        public bool IsExpandedFormatRelationVisible
        {
            get { return !Factors.All(r => r is NumericValueDefinitionTag); }
        }

        #endregion //IRelationPartImplementation

        #region ATagBase Overrides

        public override Category Category => Category.Definition;

        public override string FormattedName => "Multiplication Relation Definition";

        public override string FormattedValue
        {
            get
            {
                var expandedRelation = !IsExpandedFormatRelationVisible ? string.Empty : string.Format("\nExpanded Relation:\n{0} = {1}", ExpandedFormattedRelation, Product);
                var alternateRelation = string.Empty;

                return string.Format("Relation Type: {0}\n{1} = {2}{3}{4}", RelationType, FormattedRelation, Product, expandedRelation, alternateRelation);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}