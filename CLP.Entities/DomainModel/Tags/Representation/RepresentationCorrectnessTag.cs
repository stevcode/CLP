using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    public class SimplifiedRelation
    {
        public double GroupSize;
        public double NumberOfGroups;
        public double Product;
        public bool IsOrderedGroup;
        public bool IsProductImportant;
    }

    [Serializable]
    public class RepresentationCorrectnessTag : AAnalysisTagBase
    {
        #region Constructors

        public RepresentationCorrectnessTag() { }

        public RepresentationCorrectnessTag(CLPPage parentPage, Origin origin, List<string> analysisCodes)
            : base(parentPage, origin)
        {
            AnalysisCodes = analysisCodes;
        }

        #endregion //Constructors

        #region ATagBase Overrides

        public override Category Category => Category.Representation;

        public override string FormattedName => "Representation Correctness";

        public override string FormattedValue => string.Join("\n", AnalysisCodes);

        #endregion //ATagBase Overrides

        #region Static Methods

        public SimplifiedRelation GenerateLeftRelationFromPageAnswerDefinition(CLPPage page)
        {
            var relationDefinitionTag = page.Tags.OfType<IDefinition>().FirstOrDefault();
            if (relationDefinitionTag == null)
            {
                return null;
            }

            var simplifiedRelation = new SimplifiedRelation();

            var divisionDefinition = relationDefinitionTag as DivisionRelationDefinitionTag;
            if (divisionDefinition != null)
            {
                // Dividend / Divisor = Quotient

                // Partitive = Sharing/Dealing Out/How Many In Each Group?, Divisor = Number Of Groups, Quotient = Group Size, Dividend = Product
                // Quotative = Measuring/Bags/How Many Groups You Have?, Divisor = Group Size, Quotient = Number of Groups, Dividend = Product

                switch (divisionDefinition.RelationType)
                {
                    case DivisionRelationDefinitionTag.RelationTypes.Partitive:
                        simplifiedRelation.GroupSize = divisionDefinition.Divisor.RelationPartAnswerValue;
                        simplifiedRelation.NumberOfGroups = divisionDefinition.Quotient;
                        simplifiedRelation.IsOrderedGroup = true;
                        break;
                    case DivisionRelationDefinitionTag.RelationTypes.Quotative:
                        simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                        simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                        simplifiedRelation.IsOrderedGroup = true;
                        break;
                    default:
                        simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                        simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                        simplifiedRelation.IsOrderedGroup = false;
                        break;
                }

                
                simplifiedRelation.Product = divisionDefinition.Dividend.RelationPartAnswerValue;
                simplifiedRelation.IsProductImportant = true;

                return simplifiedRelation;
            }

            var multiplicationDefinition = relationDefinitionTag as MultiplicationRelationDefinitionTag;
            if (multiplicationDefinition != null)
            {
                simplifiedRelation.NumberOfGroups = multiplicationDefinition.Factors.First().RelationPartAnswerValue;
                simplifiedRelation.GroupSize = multiplicationDefinition.Factors.Last().RelationPartAnswerValue;
                simplifiedRelation.Product = multiplicationDefinition.Product;
                simplifiedRelation.IsOrderedGroup = multiplicationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;
                simplifiedRelation.IsProductImportant = true;

                return simplifiedRelation;
            }

            var additionDefinition = relationDefinitionTag as AdditionRelationDefinitionTag;
            if (additionDefinition != null)
            {
                var partOneDefinition = additionDefinition.Addends.First() as MultiplicationRelationDefinitionTag;
                if (partOneDefinition == null)
                {
                    return null;
                }

                simplifiedRelation.GroupSize = partOneDefinition.Factors.Last().RelationPartAnswerValue;
                simplifiedRelation.NumberOfGroups = partOneDefinition.Factors.First().RelationPartAnswerValue;
                simplifiedRelation.Product = partOneDefinition.Product;
                simplifiedRelation.IsOrderedGroup = partOneDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;
                simplifiedRelation.IsProductImportant = true;

                return simplifiedRelation;
            }

            var equivalenceDefinition = relationDefinitionTag as EquivalenceRelationDefinitionTag;
            if (equivalenceDefinition != null)
            {
                var leftDefinition = equivalenceDefinition.LeftRelationPart;

                var leftMultiplicationDefinition = leftDefinition as MultiplicationRelationDefinitionTag;
                if (leftMultiplicationDefinition != null)
                {
                    simplifiedRelation.NumberOfGroups = leftMultiplicationDefinition.Factors.First().RelationPartAnswerValue;
                    simplifiedRelation.GroupSize = leftMultiplicationDefinition.Factors.Last().RelationPartAnswerValue;
                    simplifiedRelation.Product = leftMultiplicationDefinition.Product;
                    simplifiedRelation.IsOrderedGroup = leftMultiplicationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;
                    simplifiedRelation.IsProductImportant = true;

                    return simplifiedRelation;
                }

                var leftDivisionDefinition = leftDefinition as DivisionRelationDefinitionTag;
                if (leftDivisionDefinition != null)
                {
                    simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                    simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                    simplifiedRelation.IsOrderedGroup = false;
                    simplifiedRelation.Product = divisionDefinition.Dividend.RelationPartAnswerValue;
                    simplifiedRelation.IsProductImportant = true;

                    return simplifiedRelation;
                }
            }

            return null;
        }

        public SimplifiedRelation GenerateRightRelationFromPageAnswerDefinition(CLPPage page)
        {
            var relationDefinitionTag = page.Tags.OfType<IDefinition>().FirstOrDefault();
            if (relationDefinitionTag == null)
            {
                return null;
            }

            var simplifiedRelation = new SimplifiedRelation();

            var divisionDefinition = relationDefinitionTag as DivisionRelationDefinitionTag;
            if (divisionDefinition != null)
            {
                return null;
            }

            var multiplicationDefinition = relationDefinitionTag as MultiplicationRelationDefinitionTag;
            if (multiplicationDefinition != null)
            {
                return null;
            }

            var additionDefinition = relationDefinitionTag as AdditionRelationDefinitionTag;
            if (additionDefinition != null)
            {
                var partTwoDefinition = additionDefinition.Addends.Last() as MultiplicationRelationDefinitionTag;
                if (partTwoDefinition == null)
                {
                    return null;
                }

                simplifiedRelation.GroupSize = partTwoDefinition.Factors.Last().RelationPartAnswerValue;
                simplifiedRelation.NumberOfGroups = partTwoDefinition.Factors.First().RelationPartAnswerValue;
                simplifiedRelation.Product = partTwoDefinition.Product;
                simplifiedRelation.IsOrderedGroup = partTwoDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;
                simplifiedRelation.IsProductImportant = true;

                return simplifiedRelation;
            }

            var equivalenceDefinition = relationDefinitionTag as EquivalenceRelationDefinitionTag;
            if (equivalenceDefinition != null)
            {
                var rightDefinition = equivalenceDefinition.RightRelationPart;

                var rightMultiplicationDefinition = rightDefinition as MultiplicationRelationDefinitionTag;
                if (rightMultiplicationDefinition != null)
                {
                    simplifiedRelation.NumberOfGroups = rightMultiplicationDefinition.Factors.First().RelationPartAnswerValue;
                    simplifiedRelation.GroupSize = rightMultiplicationDefinition.Factors.Last().RelationPartAnswerValue;
                    simplifiedRelation.Product = rightMultiplicationDefinition.Product;
                    simplifiedRelation.IsOrderedGroup = rightMultiplicationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;
                    simplifiedRelation.IsProductImportant = true;

                    return simplifiedRelation;
                }

                var rightDivisionDefinition = rightDefinition as DivisionRelationDefinitionTag;
                if (rightDivisionDefinition != null)
                {
                    simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                    simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                    simplifiedRelation.IsOrderedGroup = false;
                    simplifiedRelation.Product = divisionDefinition.Dividend.RelationPartAnswerValue;
                    simplifiedRelation.IsProductImportant = true;

                    return simplifiedRelation;
                }
            }

            return null;
        }

        public SimplifiedRelation GenerateAlternativeRelationFromPageAnswerDefinition(CLPPage page)
        {
            var relationDefinitionTag = page.Tags.OfType<IDefinition>().FirstOrDefault();
            if (relationDefinitionTag == null)
            {
                return null;
            }

            var additionDefinition = relationDefinitionTag as AdditionRelationDefinitionTag;
            if (additionDefinition == null)
            {
                return null;
            }

            var partOneDefinition = additionDefinition.Addends.First() as MultiplicationRelationDefinitionTag;
            var partTwoDefinition = additionDefinition.Addends.Last() as MultiplicationRelationDefinitionTag;
            if (partOneDefinition == null ||
                partTwoDefinition == null)
            {
                return null;
            }

            var leftSimplifiedRelation = new SimplifiedRelation
                                         {
                                             GroupSize = partOneDefinition.Factors.Last().RelationPartAnswerValue,
                                             NumberOfGroups = partOneDefinition.Factors.First().RelationPartAnswerValue,
                                             Product = partOneDefinition.Product,
                                             IsOrderedGroup = partOneDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups,
                                             IsProductImportant = true
                                         };

            var rightSimplifiedRelation = new SimplifiedRelation
                                          {
                                              GroupSize = partTwoDefinition.Factors.Last().RelationPartAnswerValue,
                                              NumberOfGroups = partTwoDefinition.Factors.First().RelationPartAnswerValue,
                                              Product = partTwoDefinition.Product,
                                              IsOrderedGroup = partTwoDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups,
                                              IsProductImportant = true
                                          };

            if (!(Math.Abs(leftSimplifiedRelation.GroupSize - rightSimplifiedRelation.GroupSize) < 0.0001))
            {
                return null;
            }

            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = leftSimplifiedRelation.GroupSize,
                                         NumberOfGroups = leftSimplifiedRelation.NumberOfGroups + rightSimplifiedRelation.NumberOfGroups,
                                         IsOrderedGroup = true,
                                         IsProductImportant = true
                                     };

            simplifiedRelation.Product = simplifiedRelation.GroupSize * simplifiedRelation.NumberOfGroups;

            return simplifiedRelation;
        }

        #endregion // Static Methods
    }
}