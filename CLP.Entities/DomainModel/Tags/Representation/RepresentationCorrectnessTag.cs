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
        public IDefinition PageDefinition;
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

        #region Page Answer Definition Relation Generation

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
                simplifiedRelation.PageDefinition = relationDefinitionTag;

                return simplifiedRelation;
            }

            var multiplicationDefinition = relationDefinitionTag as MultiplicationRelationDefinitionTag;
            if (multiplicationDefinition != null)
            {
                simplifiedRelation.NumberOfGroups = multiplicationDefinition.Factors.First().RelationPartAnswerValue;
                simplifiedRelation.GroupSize = multiplicationDefinition.Factors.Last().RelationPartAnswerValue;
                simplifiedRelation.Product = multiplicationDefinition.Product;
                simplifiedRelation.IsOrderedGroup = multiplicationDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups;
                simplifiedRelation.PageDefinition = relationDefinitionTag;

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
                simplifiedRelation.PageDefinition = relationDefinitionTag;

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
                    simplifiedRelation.PageDefinition = relationDefinitionTag;

                    return simplifiedRelation;
                }

                var leftDivisionDefinition = leftDefinition as DivisionRelationDefinitionTag;
                if (leftDivisionDefinition != null)
                {
                    simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                    simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                    simplifiedRelation.IsOrderedGroup = false;
                    simplifiedRelation.Product = divisionDefinition.Dividend.RelationPartAnswerValue;
                    simplifiedRelation.PageDefinition = relationDefinitionTag;

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
                simplifiedRelation.PageDefinition = relationDefinitionTag;

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
                    simplifiedRelation.PageDefinition = relationDefinitionTag;

                    return simplifiedRelation;
                }

                var rightDivisionDefinition = rightDefinition as DivisionRelationDefinitionTag;
                if (rightDivisionDefinition != null)
                {
                    simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                    simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                    simplifiedRelation.IsOrderedGroup = false;
                    simplifiedRelation.Product = divisionDefinition.Dividend.RelationPartAnswerValue;
                    simplifiedRelation.PageDefinition = relationDefinitionTag;

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
                IsOrderedGroup = partOneDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
            };

            var rightSimplifiedRelation = new SimplifiedRelation
            {
                GroupSize = partTwoDefinition.Factors.Last().RelationPartAnswerValue,
                NumberOfGroups = partTwoDefinition.Factors.First().RelationPartAnswerValue,
                Product = partTwoDefinition.Product,
                IsOrderedGroup = partTwoDefinition.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
            };

            if (!(Math.Abs(leftSimplifiedRelation.GroupSize - rightSimplifiedRelation.GroupSize) < 0.0001))
            {
                return null;
            }

            var simplifiedRelation = new SimplifiedRelation
            {
                GroupSize = leftSimplifiedRelation.GroupSize,
                NumberOfGroups = leftSimplifiedRelation.NumberOfGroups + rightSimplifiedRelation.NumberOfGroups,
                IsOrderedGroup = true
            };

            simplifiedRelation.Product = simplifiedRelation.GroupSize * simplifiedRelation.NumberOfGroups;
            simplifiedRelation.PageDefinition = relationDefinitionTag;

            return simplifiedRelation;
        }

        #endregion // Page Answer Definition Relation Generation

        #region PageObject Relation Generation

        public static SimplifiedRelation GenerateArrayRelation(CLPArray array, int historyIndex)
        {
            if (array == null)
            {
                return null;
            }

            var colsAndRows = array.GetColumnsAndRowsAtHistoryIndex(historyIndex);
            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = colsAndRows.X,
                                         NumberOfGroups = colsAndRows.Y,
                                         Product = colsAndRows.X * colsAndRows.Y,
                                         IsOrderedGroup = false
                                     };

            return simplifiedRelation;
        }

        public static SimplifiedRelation GenerateNumberLineRelation(NumberLine numberLine, int historyIndex)
        {
            // TODO: Get jumpsizes at historyIndex
            // TODO: Have bias towards majority of jump sizes being the same and modify SimplifiedRelation to have COR biased to PAR in that scenario.

            var firstGroupSize = -1;
            var firstJump = numberLine.JumpSizes.FirstOrDefault();
            if (firstJump != null)
            {
                firstGroupSize = firstJump.JumpSize;
            }
            var isEqualGroups = numberLine.JumpSizes.All(j => j.JumpSize == firstGroupSize);

            var product = -1;
            var lastJump = numberLine.JumpSizes.LastOrDefault();
            if (lastJump != null)
            {
                product = lastJump.StartingTickIndex + lastJump.JumpSize;
            }

            var jumpSizesIgnoringOverlaps = numberLine.JumpSizes.GroupBy(j => j.StartingTickIndex).Select(g => g.First()).ToList();

            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = isEqualGroups ? firstGroupSize : -1,
                                         NumberOfGroups = jumpSizesIgnoringOverlaps.Count,
                                         Product = product,
                                         IsOrderedGroup = true
                                     };

            return simplifiedRelation;
        }

        public static SimplifiedRelation GenerateNumberLineRelation(List<NumberLineJumpSize> jumpSizes)
        {
            var firstGroupSize = -1;
            var firstJump = jumpSizes.FirstOrDefault();
            if (firstJump != null)
            {
                firstGroupSize = firstJump.JumpSize;
            }
            var isEqualGroups = jumpSizes.All(j => j.JumpSize == firstGroupSize);

            var product = -1;
            var lastJump = jumpSizes.LastOrDefault();
            if (lastJump != null)
            {
                product = lastJump.StartingTickIndex + lastJump.JumpSize;
            }

            var jumpSizesIgnoringOverlaps = jumpSizes.GroupBy(j => j.StartingTickIndex).Select(g => g.First()).ToList();

            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = isEqualGroups ? firstGroupSize : -1,
                                         NumberOfGroups = jumpSizesIgnoringOverlaps.Count,
                                         Product = product,
                                         IsOrderedGroup = true
                                     };

            return simplifiedRelation;
        }

        // numberOfStampedObjects = number of stamped objects that share the same parts values and parentStampIDs (though, is the parentStampID necessary?)
        public static SimplifiedRelation GenerateStampedObjectsRelation(int parentStampParts, int numberOfStampedObjects)
        {
            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = parentStampParts,
                                         NumberOfGroups = numberOfStampedObjects,
                                         Product = numberOfStampedObjects * parentStampParts,
                                         IsOrderedGroup = parentStampParts != 1
                                     };

            return simplifiedRelation;
        }

        public static SimplifiedRelation GenerateBinsRelation(CLPPage page, int historyIndex)
        {
            // TODO: Modify to use historyIndex, modelled after StampsRelation method above.
            var binsOnPage = page.PageObjects.OfType<Bin>().Where(b => b.Parts > 0).ToList();
            if (!binsOnPage.Any())
            {
                return null;
            }

            var numberOfGroups = binsOnPage.Count();
            var product = binsOnPage.Select(b => b.Parts).Sum();
            var firstGroupSize = binsOnPage.First().Parts;
            var isEqualGroups = binsOnPage.All(b => b.Parts == firstGroupSize);

            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = isEqualGroups ? firstGroupSize : -1,
                                         NumberOfGroups = numberOfGroups,
                                         Product = product,
                                         IsOrderedGroup = true
                                     };

            return simplifiedRelation;
        }

        #endregion // PageObject Relation Generation

        #region Relation Comparison

        public static Correctness CompareSimplifiedRelations(SimplifiedRelation representationRelation, SimplifiedRelation definitionRelation)
        {
            if (representationRelation == null ||
                definitionRelation == null)
            {
                return Correctness.Unknown;
            }

            if (representationRelation.IsOrderedGroup &&
                definitionRelation.IsOrderedGroup)
            {
                if (representationRelation.GroupSize == definitionRelation.GroupSize &&
                    representationRelation.NumberOfGroups == definitionRelation.NumberOfGroups)
                {
                    return representationRelation.Product == definitionRelation.Product ? Correctness.Correct : Correctness.PartiallyCorrect;
                }

                if (representationRelation.GroupSize == definitionRelation.GroupSize ||
                    representationRelation.NumberOfGroups == definitionRelation.NumberOfGroups ||
                    representationRelation.GroupSize == definitionRelation.NumberOfGroups ||
                    representationRelation.NumberOfGroups == definitionRelation.GroupSize)
                {
                    return Correctness.PartiallyCorrect;
                }

                return Correctness.Incorrect;
            }

            if ((representationRelation.GroupSize == definitionRelation.GroupSize && representationRelation.NumberOfGroups == definitionRelation.NumberOfGroups) ||
                (representationRelation.GroupSize == definitionRelation.NumberOfGroups && representationRelation.NumberOfGroups == definitionRelation.GroupSize))
            {
                return representationRelation.Product == definitionRelation.Product ? Correctness.Correct : Correctness.PartiallyCorrect;
            }

            if (representationRelation.GroupSize == definitionRelation.GroupSize ||
                representationRelation.NumberOfGroups == definitionRelation.NumberOfGroups ||
                representationRelation.GroupSize == definitionRelation.NumberOfGroups ||
                representationRelation.NumberOfGroups == definitionRelation.GroupSize)
            {
                return Correctness.PartiallyCorrect;
            }

            return Correctness.Incorrect;
        }

        #endregion // Relation Comparison

        #endregion // Static Methods
    }
}