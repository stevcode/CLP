using System.Collections.ObjectModel;

namespace CLP.Models
{
    public interface IHistoryBatch
    {
        ICLPPage ParentPage { get; set; }
        ObservableCollection<ICLPHistoryItem> HistoryItems { get; }
        bool IsEmptyBatch { get; }
        bool IsSingleBatch { get; }
        void Undo();
        void Redo();
        void AddToBatch(object target, object tag);
    }
}
