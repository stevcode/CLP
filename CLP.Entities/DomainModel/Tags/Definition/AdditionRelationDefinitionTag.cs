using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AdditionRelationDefinitionTag : ATagBase, IRelationPart
    {
        public enum RelationTypes
        {
            GeneralAddition
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="AdditionRelationDefinitionTag" /> from scratch.
        /// </summary>
        public AdditionRelationDefinitionTag() { }

        /// <summary>
        /// Initializes <see cref="AdditionRelationDefinitionTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AdditionRelationDefinitionTag" /> belongs to.</param>
        public AdditionRelationDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>
        /// Initializes <see cref="AdditionRelationDefinitionTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public AdditionRelationDefinitionTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// List of the Addends
        /// </summary>
        public List<IRelationPart> Addends
        {
            get { return GetValue<List<IRelationPart>>(AddendsProperty); }
            set { SetValue(AddendsProperty, value); }
        }

        public static readonly PropertyData AddendsProperty = RegisterProperty("Addends", typeof(List<IRelationPart>), () => new List<IRelationPart>());

        /// <summary>
        /// Final sum of the addition relation.
        /// </summary>
        public double Sum
        {
            get { return GetValue<double>(SumProperty); }
            set { SetValue(SumProperty, value); }
        }

        public static readonly PropertyData SumProperty = RegisterProperty("Sum", typeof (double), 0);

        /// <summary>Type of multiplication relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType",
                                                                                    typeof(RelationTypes),
                                                                                    RelationTypes.GeneralAddition);

        #region IRelationPartImplementation

        public double RelationPartAnswerValue
        {
            get { return Sum; }
        }

        public string FormattedRelation
        {
            get { return string.Join("+", Addends.Select(x => x.RelationPartAnswerValue)); }
        }

        public string ExpandedFormattedRelation
        {
            get { return string.Join("+", Addends.Select(x => x is NumericValueDefinitionTag ? x.FormattedRelation : "(" + x.FormattedRelation + ")")); }
        }

        #endregion //IRelationPartImplementation

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Definition; }
        }

        public override string FormattedName
        {
            get { return "Addition Relation Definition"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("Relation Type: {0}\n" + "{1} = {2}\n" + "Expanded Relation:\n" + "{3} = {2}", RelationType, FormattedRelation, Sum, ExpandedFormattedRelation); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties

        
    }
}