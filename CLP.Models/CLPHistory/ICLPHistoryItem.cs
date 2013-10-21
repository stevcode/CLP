namespace CLP.Models
{
    public interface ICLPHistoryItem
    {
        int AnimationDelay { get; }
        ICLPPage ParentPage { get; set; }
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
        ICLPHistoryItem UndoRedoCompleteClone();
    }
}
