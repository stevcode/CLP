using Catel.Data;

namespace CLP.Entities
{
    public class EquivalenceRelationDefinitionTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="EquivalenceRelationDefinitionTag" /> from scratch.</summary>
        public EquivalenceRelationDefinitionTag() { }

        /// <summary>Initializes <see cref="EquivalenceRelationDefinitionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="EquivalenceRelationDefinitionTag" /> belongs to.</param>
        public EquivalenceRelationDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>The definition for the left hand side of the equivalence definition.</summary>
        public IRelationPart LeftRelationPart
        {
            get { return GetValue<IRelationPart>(LeftRelationPartProperty); }
            set { SetValue(LeftRelationPartProperty, value); }
        }

        public static readonly PropertyData LeftRelationPartProperty = RegisterProperty("LeftRelationPart", typeof(IRelationPart));

        /// <summary>The definition for the right hand side of the equivalence definition.</summary>
        public IRelationPart RightRelationPart
        {
            get { return GetValue<IRelationPart>(RightRelationPartProperty); }
            set { SetValue(RightRelationPartProperty, value); }
        }

        public static readonly PropertyData RightRelationPartProperty = RegisterProperty("RightRelationPart", typeof(IRelationPart));

        #endregion // Properties

        #region ATagBase Overrides

        public override Category Category => Category.Definition;

        public override string FormattedName => "Equivalence Relation Definition";

        public override string FormattedValue => $"{LeftRelationPart.FormattedRelation} = {RightRelationPart.FormattedRelation}";

        #endregion //ATagBase Overrides
    }
}