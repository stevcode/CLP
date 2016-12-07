using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Initializes <see cref="AdditionRelationDefinitionTag" /> from scratch.</summary>
        public AdditionRelationDefinitionTag() { }

        /// <summary>Initializes <see cref="AdditionRelationDefinitionTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AdditionRelationDefinitionTag" /> belongs to.</param>
        /// <param name="origin">The origin that created the Tag.</param>
        public AdditionRelationDefinitionTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of the Addends</summary>
        public List<IRelationPart> Addends
        {
            get { return GetValue<List<IRelationPart>>(AddendsProperty); }
            set { SetValue(AddendsProperty, value); }
        }

        public static readonly PropertyData AddendsProperty = RegisterProperty("Addends", typeof(List<IRelationPart>), () => new List<IRelationPart>());

        /// <summary>Final sum of the addition relation.</summary>
        public double Sum
        {
            get { return GetValue<double>(SumProperty); }
            set { SetValue(SumProperty, value); }
        }

        public static readonly PropertyData SumProperty = RegisterProperty("Sum", typeof(double), 0);

        /// <summary>Type of multiplication relationship the relation defines.</summary>
        public RelationTypes RelationType
        {
            get { return GetValue<RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(RelationTypes), RelationTypes.GeneralAddition);

        #endregion //Properties

        #region ATagBase Overrides

        public override Category Category => Category.Definition;

        public override string FormattedName => "Addition Relation Definition";

        public override string FormattedValue
        {
            get
            {
                var expandedRelation = IsExpandedFormatRelationVisible ? $"\nExpanded Relation:\n{ExpandedFormattedRelation} = {Sum}" : string.Empty;
                var alternateRelation = string.IsNullOrWhiteSpace(AlternateFormattedRelation) ? string.Empty : $"\nAlternate Relation(s):\n{AlternateFormattedRelation}";

                return $"Relation Type: {RelationType}\n{FormattedRelation} = {Sum}{expandedRelation}{alternateRelation}";
            }
        }

        #endregion //ATagBase Overrides

        #region IRelationPartImplementation

        public double RelationPartAnswerValue => Sum;

        public string FormattedRelation
        {
            get { return string.Join(" + ", Addends.Select(x => x.FormattedRelation)); }
        }

        public string ExpandedFormattedRelation
        {
            get { return string.Join(" + ", Addends.Select(x => x is NumericValueDefinitionTag ? x.FormattedRelation : "(" + x.ExpandedFormattedRelation + ")")); }
        }

        #region Support

        public bool IsExpandedFormatRelationVisible
        {
            get { return !Addends.All(r => r is NumericValueDefinitionTag); }
        }

        public string AlternateFormattedRelation
        {
            get
            {
                if (!Addends.All(r => r is MultiplicationRelationDefinitionTag))
                {
                    return string.Empty;
                }

                var multiplicationRelations = Addends.Cast<MultiplicationRelationDefinitionTag>().ToList();
                if (!multiplicationRelations.All(m => m.Factors.All(f => f is NumericValueDefinitionTag)) &&
                    multiplicationRelations.Count != 2)
                {
                    return string.Empty;
                }

                var firstAddend = multiplicationRelations.First();
                var secondAddend = multiplicationRelations.Last();

                if (firstAddend.RelationType != secondAddend.RelationType)
                {
                    return string.Empty;
                }

                var alternateRelations = new List<string>();

                // Group Size Matches
                if (Math.Abs(firstAddend.Factors.Last().RelationPartAnswerValue - secondAddend.Factors.Last().RelationPartAnswerValue) < 0.001)
                {
                    var groupSize = firstAddend.Factors.Last().RelationPartAnswerValue;
                    var numberOfGroups = firstAddend.Factors.First().RelationPartAnswerValue + secondAddend.Factors.First().RelationPartAnswerValue;
                    var delimiter = firstAddend.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups ||
                                    firstAddend.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups
                                        ? " group(s) of "
                                        : "x";
                    var alternateRelation = $"{numberOfGroups}{delimiter}{groupSize} = {Sum}";
                    alternateRelations.Add(alternateRelation);
                }

                // Number of Groups Matches
                if (Math.Abs(firstAddend.Factors.First().RelationPartAnswerValue - secondAddend.Factors.First().RelationPartAnswerValue) < 0.001 &&
                    firstAddend.RelationType != MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups)
                {
                    var groupSize = firstAddend.Factors.Last().RelationPartAnswerValue + secondAddend.Factors.Last().RelationPartAnswerValue;
                    var numberOfGroups = firstAddend.Factors.First().RelationPartAnswerValue;
                    var delimiter = firstAddend.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups ? " group(s) of " : "x";
                    var alternateRelation = $"{numberOfGroups}{delimiter}{groupSize} = {Sum}";
                    alternateRelations.Add(alternateRelation);
                }

                return !alternateRelations.Any() ? string.Empty : string.Join("\n", alternateRelations);
            }
        }

        #endregion // Support

        #endregion //IRelationPartImplementation
    }
}