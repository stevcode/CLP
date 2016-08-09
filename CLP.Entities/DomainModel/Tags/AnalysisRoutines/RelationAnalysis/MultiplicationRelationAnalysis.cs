using System.Linq;
using System.Windows;

namespace CLP.Entities
{
    public static class MultiplicationRelationAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {
            var multiplicationRelationDefinitionTags = page.Tags.OfType<MultiplicationRelationDefinitionTag>().ToList();
            if (!multiplicationRelationDefinitionTags.Any())
            {
                return;
            }

            // First, clear out any old Tags generated via Analysis.

            //foreach (var tag in
            //    page.Tags.ToList()
            //        .Where(
            //               tag =>
            //               tag is DivisionToolRepresentationCorrectnessTag || tag is DivisionToolCompletenessTag || tag is DivisionToolCorrectnessSummaryTag ||
            //               tag is TroubleWithDivisionTag))
            //{
            //    page.RemoveTag(tag);
            //}

            foreach (var multiplicationRelationDefinitionTag in multiplicationRelationDefinitionTags)
            {
                AnalyzeRepresentationCorrectness(page, multiplicationRelationDefinitionTag);
            }

            //AnalyzeDivisionToolTroubleWithDivision(page);
            //AnalyzeDivisionToolCorrectness(page);
        }

        public static void AnalyzeRepresentationCorrectness(CLPPage page, MultiplicationRelationDefinitionTag multiplicationRelationDefinitionTag)
        {
            
        }
    }
}