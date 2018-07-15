using System.Windows;

namespace CLP.Entities
{
    public static class AdditionRelationAnalysis
    {
        public static void Analyze(CLPPage page) { AnalyzeRegion(page, new Rect(0, 0, page.Height, page.Width)); }

        public static void AnalyzeRegion(CLPPage page, Rect region)
        {

        }
    }
}