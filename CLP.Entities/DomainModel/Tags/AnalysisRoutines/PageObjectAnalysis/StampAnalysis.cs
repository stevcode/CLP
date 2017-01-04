using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CLP.Entities.Demo
{
    public static class StampAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            // First, clear out any old StampTags generated via Analysis.
            foreach (var tag in
                page.Tags.ToList().Where(tag => tag is StampTroubleWithGroupingTag || tag is TroubleWithMultiplicationTag))
            {
                page.RemoveTag(tag);
            }

            var multiplicationDefinitionTags = page.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
            var stampedObjects = page.PageObjects.OfType<StampedObject>().ToList();
            if (!multiplicationDefinitionTags.Any() ||
                !stampedObjects.Any())
            {
                return;
            }

            foreach (var multiplicationDefinitionTag in multiplicationDefinitionTags.Where(x => x.RelationType == MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups))
            {
                AnalyzeParentStampGroupings(page, multiplicationDefinitionTag, stampedObjects);
            }
            
            AnalyzeStampTroubleWithMultiplication(page);
        }

        public static void AnalyzeParentStampGroupings(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinitionTag, List<StampedObject> stampedObjects)
        {
            var parentStampIDs = stampedObjects.Select(x => x.ParentStampID).Distinct().ToList();
            foreach (var parentStampID in parentStampIDs)
            {
                var id = parentStampID;
                var distinctPartsValues = stampedObjects.Where(x => x.ParentStampID == id).Select(x => x.Parts).Distinct().ToList();
                foreach (var distinctPartsValue in distinctPartsValues)
                {
                    var parts = distinctPartsValue;
                    var stampedObjectIDs = stampedObjects.Where(x => x.ParentStampID == id && x.Parts == parts).Select(x => x.ID).ToList();
                    //var stampGroupTag = new StampGroupTag(page, Origin.StudentPageGenerated, parentStampID, parts, stampedObjectIDs);
                    //page.AddTag(stampGroupTag);
                    //AnalyzeStampTroubleWithGrouping(page, multiplicationRelationDefinitionTag, stampGroupTag);
                }
            }
        }

        //public static void AnalyzeStampTroubleWithGrouping(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinitionTag, StampGroupTag groupTag)
        //{
        //    var groups = groupTag.StampedObjectIDs.Count;
        //    var groupSize = groupTag.Parts;

        //    if (groups == multiplicationRelationDefinitionTag.Factors[0].RelationPartAnswerValue && groupSize == multiplicationRelationDefinitionTag.Factors[1].RelationPartAnswerValue)
        //    {
        //        return;
        //    }

        //    var existingTroubleWithGroupingTag = page.Tags.OfType<StampTroubleWithGroupingTag>().FirstOrDefault();
        //    if (existingTroubleWithGroupingTag == null)
        //    {
        //        existingTroubleWithGroupingTag = new StampTroubleWithGroupingTag(page, Origin.StudentPageGenerated);
        //        page.AddTag(existingTroubleWithGroupingTag);
        //    }

        //    if (groups == multiplicationRelationDefinitionTag.Factors[1].RelationPartAnswerValue && groupSize == multiplicationRelationDefinitionTag.Factors[0].RelationPartAnswerValue)
        //    {
        //        existingTroubleWithGroupingTag.NumberOfGroupsAndGroupSizeSwappedCount++;
        //        return;
        //    }

        //    if (groups == multiplicationRelationDefinitionTag.Factors[0].RelationPartAnswerValue && groupSize != multiplicationRelationDefinitionTag.Factors[1].RelationPartAnswerValue)
        //    {
        //        existingTroubleWithGroupingTag.GroupSizeWrongCount++;
        //        return;
        //    }

        //    if (groups != multiplicationRelationDefinitionTag.Factors[0].RelationPartAnswerValue && groupSize == multiplicationRelationDefinitionTag.Factors[1].RelationPartAnswerValue)
        //    {
        //        existingTroubleWithGroupingTag.NumberOfGroupsWrongCount++;
        //        return;
        //    }

        //    existingTroubleWithGroupingTag.NumberOfGroupsWrongAndGroupSizeWrongCount++;
        //}

        public static void AnalyzeStampTroubleWithMultiplication(CLPPage page)
        {
            var errorSum = TroubleWithMultiplicationTag.GetTroubleWithStampGroupingCount(page);

            if (errorSum == 0)
            {
                return;
            }

            var troubleWithMultiplicationTag = new TroubleWithMultiplicationTag(page, Origin.StudentPageGenerated);
            page.AddTag(troubleWithMultiplicationTag);
        }
    }
}