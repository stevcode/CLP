namespace CLP.Entities.Ann
{
    public interface IHistoryBatch : IHistoryItem
    {
        int NumberOfBatchTicks { get; }
        int CurrentBatchTickIndex { get; set; }
        void ClearBatchAfterCurrentIndex();
    }
}