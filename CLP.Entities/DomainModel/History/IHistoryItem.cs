namespace CLP.Entities
{
    public interface IHistoryItem
    {
        int HistoryIndex { get; set; }
        string ID { get; set; }
        string OwnerID { get; set; }
        uint VersionIndex { get; set; }
        uint? LastVersionIndex { get; set; }
        string DifferentiationGroup { get; set; }
        int AnimationDelay { get; }
        string ParentPageID { get; set; }
        string ParentPageOwnerID { get; set; }
        uint ParentPageVersionIndex { get; set; }
        string FormattedValue { get; }
        CLPPage ParentPage { get; set; }
        void ConversionUndo();
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
        IHistoryItem CreatePackagedHistoryItem();
        void UnpackHistoryItem();
        bool IsUsingTrashedPageObject(string id, bool isUndoItem);
        bool IsUsingTrashedInkStroke(string id, bool isUndoItem);
    }
}