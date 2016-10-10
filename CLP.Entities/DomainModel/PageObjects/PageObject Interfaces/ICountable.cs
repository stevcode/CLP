namespace CLP.Entities.Old
{
    public interface ICountable : IPageObject
    {
        int Parts { get; set; }
        bool IsInnerPart { get; set; }
    }
}