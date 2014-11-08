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
        public List<double> Factors
        {
            get { return GetValue<List<double>>(FactorsProperty); }
            set { SetValue(FactorsProperty, value); }
        }

        public static readonly PropertyData FactorsProperty = RegisterProperty("Factors", typeof (List<double>), () => new List<double>());

        /// <summary>Value of the product of the multiplication relation.</summary>
        public double Product
        {
            get { return GetValue<double>(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly PropertyData ProductProperty = RegisterProperty("Product", typeof (double), 0);

        /// <summary>Type of multiplication relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType",
                                                                                    typeof (RelationTypes),
                                                                                    RelationTypes.GeneralMultiplication);

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
                return RelationType == RelationTypes.EqualGroups
                           ? string.Format("Relation Type: {0}\n" + "{1} = {2}",
                                           RelationType,
                                           string.Join(", ",
                                                       Factors.Select((e, i) => new
                                                                                {
                                                                                    Index = i / 2,
                                                                                    Item = e
                                                                                })
                                                              .GroupBy(x => x.Index, x => x.Item)
                                                              .Select(x => string.Join(" groups of ", x))),
                                           Product)
                           : string.Format("Relation Type: {0}\n" + "{1} = {2}", RelationType, string.Join("x", Factors), Product);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}