namespace CLP.Models
{
    public interface IHistoryBatch : ICLPHistoryItem
    {
        int BatchDelay { get; }
        int NumberOfBatchTicks { get; }
        int CurrentBatchTickIndex { get; set; }
        void ClearBatchAfterCurrentIndex();
    }
}
