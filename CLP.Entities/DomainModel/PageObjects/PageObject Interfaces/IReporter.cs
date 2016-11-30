namespace CLP.Entities.Ann
{
    public interface IReporter
    {
        string FormattedReport { get; }
        void UpdateReport();
    }
}