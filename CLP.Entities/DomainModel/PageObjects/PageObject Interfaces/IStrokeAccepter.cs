using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;

namespace CLP.Entities
{
    public interface IStrokeAccepter : IPageObject
    {
        int StrokeHitTestPercentage { get; }
        Rect StrokeAcceptanceBoundingBox { get; }
        bool CanAcceptStrokes { get; set; }
        List<Stroke> AcceptedStrokes { get; set; }
        List<string> AcceptedStrokeParentIDs { get; set; }

        void LoadAcceptedStrokes();
        void ChangeAcceptedStrokes(IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes);
        bool IsStrokeOverPageObject(Stroke stroke);
        double PercentageOfStrokeOverPageObject(Stroke stroke);
        StrokeCollection GetStrokesOverPageObject(); 
    }
}