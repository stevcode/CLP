namespace CLP.Models
{
    public interface IHistoryBatch : ICLPHistoryItem
    {
        int NumberOfBatchTicks { get; }
        int CurrentBatchTickIndex { get; set; }
        void ClearBatchAfterCurrentIndex();
    }
}
