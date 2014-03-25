using System.Collections.ObjectModel;
using System.Windows.Ink;

namespace CLP.Models
{
    public interface IInkStrokeAccepter
    {
        bool CanAcceptStrokes { get; set; }
        ObservableCollection<string> AcceptedStrokeIDs { get; set; }
        
        void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes);
        void RefreshStrokeParentIDs();
        StrokeCollection GetStrokesOverThisPageObject();
    }
}
