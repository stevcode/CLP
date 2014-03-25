using System.Collections.ObjectModel;
using System.Windows.Ink;

namespace CLP.Entities
{
    public interface IStrokeAccepter
    {
        bool CanAcceptStrokes { get; set; }
        StrokeCollection AcceptedStrokes { get; }
        ObservableCollection<string> AcceptedStrokeParentIDs { get; set; }

        void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes);
        void RefreshAcceptedStrokes();
    }
}