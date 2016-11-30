using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities.Ann
{
    public interface IStrokeAccepter : IPageObject
    {
        bool CanAcceptStrokes { get; set; }
        List<Stroke> AcceptedStrokes { get; }
        List<string> AcceptedStrokeParentIDs { get; set; }

        void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes);
        void RefreshAcceptedStrokes();
    }
}