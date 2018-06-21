using System;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class EquivalenceRelationDefinitionTag : ATagBase, IDefinition
    {
        #region Constructors

        public EquivalenceRelationDefinitionTag() { }

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

        #region IDefinition Implementation

        public double Answer
        {
            get
            {
                var answer = GetHiddenValueOfRelation(LeftRelationPart) ?? GetHiddenValueOfRelation(RightRelationPart);
                return answer.Value;
            }
        }

        public double? GetHiddenValueOfRelation(IRelationPart relationPart)
        {
            var multiplicationDefinition = relationPart as MultiplicationRelationDefinitionTag;
            if (multiplicationDefinition != null)
            {
                var firstFactor = multiplicationDefinition.Factors.First() as NumericValueDefinitionTag;
                if (firstFactor.IsNotGiven)
                {
                    return firstFactor.Answer;
                }

                var secondFactor = multiplicationDefinition.Factors.Last() as NumericValueDefinitionTag;
                if (secondFactor.IsNotGiven)
                {
                    return secondFactor.Answer;
                }
            }

            var additionDefinition = relationPart as AdditionRelationDefinitionTag;
            if (additionDefinition != null)
            {
                var firstAddend = additionDefinition.Addends.First() as NumericValueDefinitionTag;
                if (firstAddend.IsNotGiven)
                {
                    return firstAddend.Answer;
                }

                var secondAddend = additionDefinition.Addends.Last() as NumericValueDefinitionTag;
                if (secondAddend.IsNotGiven)
                {
                    return secondAddend.Answer;
                }
            }

            var divisionDefinition = relationPart as DivisionRelationDefinitionTag;
            if (divisionDefinition != null)
            {
                var dividend = divisionDefinition.Dividend as NumericValueDefinitionTag;
                if (dividend.IsNotGiven)
                {
                    return dividend.Answer;
                }

                var divisor = divisionDefinition.Divisor as NumericValueDefinitionTag;
                if (divisor.IsNotGiven)
                {
                    return divisor.Answer;
                }
            }

            return null;
        }

        #endregion // IDefinition Implementation
    }
}