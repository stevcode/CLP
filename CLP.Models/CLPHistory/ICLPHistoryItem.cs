namespace CLP.Models
{
    public interface ICLPHistoryItem
    {
        ICLPPage ParentPage { get; set; }
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
    }
}
