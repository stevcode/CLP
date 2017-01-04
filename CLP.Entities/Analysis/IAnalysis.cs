using System.Windows;

namespace CLP.Entities.Demo.Demo.Analysis
{
    public interface IAnalysis
    {
        string FormattedAnalysis { get; }
        void Analyze(CLPPage page);
        void Analyze(CLPPage page, Rect region);
    }
}