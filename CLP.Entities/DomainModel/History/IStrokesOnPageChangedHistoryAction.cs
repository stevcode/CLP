using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IStrokesOnPageChangedHistoryAction : IHistoryAction
    {
        List<string> StrokeIDsAdded { get; }
        List<string> StrokeIDsRemoved { get; }
    }
}