namespace CLP.Entities
{
    public interface IReporter
    {
        string FormattedReport { get; }
        void UpdateReport();
    }
}