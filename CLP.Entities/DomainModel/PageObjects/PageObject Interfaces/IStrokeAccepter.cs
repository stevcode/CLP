using System.Collections.Generic;
using System.Windows.Ink;

namespace CLP.Entities
{
    public interface IStrokeAccepter : IPageObject
    {
        bool CanAcceptStrokes { get; set; }
        List<Stroke> AcceptedStrokes { get; set; }
        List<string> AcceptedStrokeParentIDs { get; set; }

        void AcceptStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes);
        StrokeCollection GetStrokesOverPageObject(); 
        void RefreshAcceptedStrokes();
    }
}