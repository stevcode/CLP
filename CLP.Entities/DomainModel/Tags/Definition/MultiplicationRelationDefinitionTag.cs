using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class MultiplicationRelationDefinitionTag : ATagBase
    {
        public enum RelationTypes
        {
            GeneralMultiplication,
            Area,
            Volume,
            EqualGroups,
            OrderedEqualGroups
        }

        #region Constructors

        /// <summary>Initializes <see cref="MultiplicationRelationDefinitionTag" /> from scratch.</summary>
        public MultiplicationRelationDefinitionTag() { }

        /// <summary>Initializes <see cref="MultiplicationRelationDefinitionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="MultiplicationRelationDefinitionTag" /> belongs to.</param>
        public MultiplicationRelationDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>Initializes <see cref="MultiplicationRelationDefinitionTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public MultiplicationRelationDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

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

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType",
                                                                                    typeof(RelationTypes),
                                                                                    RelationTypes.GeneralMultiplication);

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
                        return string.Join(" groups of ", Factors.Select(x => x.RelationPartAnswerValue));
                    //return string.Join(", ",
                    //                                 Factors.Select((e, i) => new
                    //                                                          {
                    //                                                              Index = i / 2,
                    //                                                              Item = e
                    //                                                          }).GroupBy(x => x.Index, x => x.Item).Select(x => string.Join(" groups of ", x)));
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
                        return string.Join(" groups of ", Factors.Select(x => x is NumericValueDefinitionTag ? x.FormattedRelation : "(" + x.FormattedRelation + ")"));
                    default:
                        return string.Join("x", Factors.Select(x => x is NumericValueDefinitionTag ? x.FormattedRelation : "(" + x.FormattedRelation + ")"));
                }
            }
        }

        #endregion //IRelationPartImplementation

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Definition; }
        }

        public override string FormattedName
        {
            get { return "Multiplication Relation Definition"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Relation Type: {0}\n" + "{1} = {2}\n" + "Expanded Relation:\n" + "{3} = {2}", RelationType, FormattedRelation, Product, ExpandedFormattedRelation);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}