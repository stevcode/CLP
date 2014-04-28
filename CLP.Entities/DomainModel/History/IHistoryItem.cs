namespace CLP.Entities
{
    public interface IHistoryItem
    {
        string ID { get; set; }
        string OwnerID { get; set; }
        uint VersionIndex { get; set; }
        uint? LastVersionIndex { get; set; }
        int AnimationDelay { get; }
        string ParentPageID { get; set; }
        string ParentPageOwnerID { get; set; }
        uint ParentPageVersionIndex { get; set; }
        CLPPage ParentPage { get; set; }
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
        IHistoryItem CreatePackagedHistoryItem();
    }
}