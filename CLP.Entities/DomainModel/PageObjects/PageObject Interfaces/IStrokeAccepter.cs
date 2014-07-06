using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities
{
    public interface IStrokeAccepter : IPageObject
    {
        bool CanAcceptStrokes { get; set; }
        StrokeCollection AcceptedStrokes { get; }
        List<string> AcceptedStrokeParentIDs { get; set; }

        void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes);
        void RefreshAcceptedStrokes();
    }
}