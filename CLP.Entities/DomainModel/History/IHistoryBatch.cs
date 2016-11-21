namespace CLP.Entities
{
    public interface IHistoryBatch : IHistoryAction
    {
        int NumberOfBatchTicks { get; }
        int CurrentBatchTickIndex { get; set; }

        void ClearBatchAfterCurrentIndex();
    }
}