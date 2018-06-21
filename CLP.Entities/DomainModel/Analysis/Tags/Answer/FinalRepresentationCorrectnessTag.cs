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
        public bool IsSwapped;
    }

    [Serializable]
    public class FinalRepresentationCorrectnessTag : AAnalysisTagBase
    {
        #region Constructors

        public FinalRepresentationCorrectnessTag() { }

        public FinalRepresentationCorrectnessTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Overall correctness of the final Representations on the page.</summary>
        public Correctness RepresentationCorrectness
        {
            get => GetValue<Correctness>(RepresentationCorrectnessProperty);
            set => SetValue(RepresentationCorrectnessProperty, value);
        }

        public static readonly PropertyData RepresentationCorrectnessProperty = RegisterProperty(nameof(RepresentationCorrectness), typeof(Correctness), Correctness.Unknown);

        #endregion // Properties
        
        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Answer;

        public override string FormattedName => "Final Representation Correctness";

        public override string FormattedValue
        {
            get
            {
                var overallCorrectness = Codings.CorrectnessToFriendlyCorrectness(RepresentationCorrectness);
                var analysisCodes = string.Join("  - ", SpreadSheetCodes);
                var representations = SpreadSheetCodes.Any() ? $"Representations:\n  - {analysisCodes}" : "No Representations";
                return $"Overall Correctness: {overallCorrectness}\n{representations}";
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static FinalRepresentationCorrectnessTag AttemptTagGeneration(CLPPage page, RepresentationsUsedTag representationsUsedTag)
        {
            var isAnswerDefinitionOnPage = page.Tags.OfType<IDefinition>().Any();
            if (!isAnswerDefinitionOnPage)
            {
                return null;
            }

            var tag = new FinalRepresentationCorrectnessTag(page, Origin.StudentPageGenerated);

            var finalRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.IsFinalRepresentation).ToList();
            if (finalRepresentations.Any() &&
                finalRepresentations.All(r => r.Correctness == Correctness.Correct))
            {
                tag.RepresentationCorrectness = Correctness.Correct;
            }
            else if (finalRepresentations.Any() &&
                     finalRepresentations.All(r => r.Correctness == Correctness.Incorrect))
            {
                tag.RepresentationCorrectness = Correctness.Incorrect;
            }
            else if (finalRepresentations.Any(r => r.Correctness == Correctness.PartiallyCorrect) ||
                     finalRepresentations.Any(r => r.Correctness == Correctness.Correct))
            {
                tag.RepresentationCorrectness = Correctness.PartiallyCorrect;
            }

            tag.SpreadSheetCodes = finalRepresentations
                                .Select(r => $"{r.CodedObject} [{r.CodedID}] {r.RepresentationInformation}, {Codings.CorrectnessToCodedCorrectness(tag.RepresentationCorrectness)}")
                                .ToList();

            return tag;
        }

        // TODO: All the below static methods are only used for RepsUsed Tag now. Refactor to better location.

        #region Page Answer Definition Relation Generation

        public static SimplifiedRelation GenerateLeftRelationFromPageAnswerDefinition(CLPPage page)
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
                        simplifiedRelation.GroupSize = divisionDefinition.Quotient;
                        simplifiedRelation.NumberOfGroups = divisionDefinition.Divisor.RelationPartAnswerValue;
                        simplifiedRelation.IsOrderedGroup = true;
                        break;
                    case DivisionRelationDefinitionTag.RelationTypes.Quotative:
                        simplifiedRelation.GroupSize = divisionDefinition.Divisor.RelationPartAnswerValue;
                        simplifiedRelation.NumberOfGroups = divisionDefinition.Quotient;
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
                    simplifiedRelation.GroupSize = leftDivisionDefinition.Quotient;
                    simplifiedRelation.NumberOfGroups = leftDivisionDefinition.Divisor.RelationPartAnswerValue;
                    simplifiedRelation.IsOrderedGroup = false;
                    simplifiedRelation.Product = leftDivisionDefinition.Dividend.RelationPartAnswerValue;
                    simplifiedRelation.PageDefinition = relationDefinitionTag;

                    return simplifiedRelation;
                }
            }

            return null;
        }

        public static SimplifiedRelation GenerateRightRelationFromPageAnswerDefinition(CLPPage page)
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
                    simplifiedRelation.GroupSize = rightDivisionDefinition.Quotient;
                    simplifiedRelation.NumberOfGroups = rightDivisionDefinition.Divisor.RelationPartAnswerValue;
                    simplifiedRelation.IsOrderedGroup = false;
                    simplifiedRelation.Product = rightDivisionDefinition.Dividend.RelationPartAnswerValue;
                    simplifiedRelation.PageDefinition = relationDefinitionTag;

                    return simplifiedRelation;
                }
            }

            return null;
        }

        public static SimplifiedRelation GenerateAlternativeRelationFromPageAnswerDefinition(CLPPage page)
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
                                              IsOrderedGroup = partTwoDefinition.RelationType ==
                                                               MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
                                          };

            var groupSize = -1.0;
            var numberOfGroups = -1.0;
            var isOrderedGroup = false;

            if (Math.Abs(leftSimplifiedRelation.GroupSize - rightSimplifiedRelation.GroupSize) < 0.001)
            {
                groupSize = leftSimplifiedRelation.GroupSize;
                numberOfGroups = leftSimplifiedRelation.NumberOfGroups + rightSimplifiedRelation.NumberOfGroups;
                isOrderedGroup = true;
            }
            else if (Math.Abs(leftSimplifiedRelation.NumberOfGroups - rightSimplifiedRelation.NumberOfGroups) < 0.001)
            {
                groupSize = leftSimplifiedRelation.GroupSize + rightSimplifiedRelation.GroupSize;
                numberOfGroups = leftSimplifiedRelation.NumberOfGroups;
                isOrderedGroup = true;
            }
            else if (Math.Abs(leftSimplifiedRelation.NumberOfGroups - rightSimplifiedRelation.GroupSize) < 0.001)
            {
                groupSize = leftSimplifiedRelation.GroupSize + rightSimplifiedRelation.NumberOfGroups;
                numberOfGroups = leftSimplifiedRelation.NumberOfGroups;
            }
            else if (Math.Abs(leftSimplifiedRelation.GroupSize - rightSimplifiedRelation.NumberOfGroups) < 0.001)
            {
                groupSize = leftSimplifiedRelation.GroupSize;
                numberOfGroups = leftSimplifiedRelation.NumberOfGroups + rightSimplifiedRelation.GroupSize;
            }
            else
            {
                return null;
            }

            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = groupSize,
                                         NumberOfGroups = numberOfGroups,
                                         IsOrderedGroup = isOrderedGroup
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
            var lastJump = jumpSizes.OrderBy(j => j.StartingTickIndex).LastOrDefault();
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

        // numberOfStampedObjects = number of stamped objects that share the same parts values
        public static SimplifiedRelation GenerateStampedObjectsRelation(int parts, int numberOfStampedObjects)
        {
            var simplifiedRelation = new SimplifiedRelation
                                     {
                                         GroupSize = parts,
                                         NumberOfGroups = numberOfStampedObjects,
                                         Product = numberOfStampedObjects * parts,
                                         IsOrderedGroup = parts != 1
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

                if (representationRelation.GroupSize == definitionRelation.NumberOfGroups &&
                    representationRelation.NumberOfGroups == definitionRelation.GroupSize)
                {
                    definitionRelation.IsSwapped = true;
                    return Correctness.PartiallyCorrect;
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