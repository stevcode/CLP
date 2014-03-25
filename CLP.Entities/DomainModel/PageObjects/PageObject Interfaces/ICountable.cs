namespace CLP.Entities
{
    public interface ICountable
    {
        int Parts { get; set; }
        bool IsInnerPart { get; set; }
    }
}