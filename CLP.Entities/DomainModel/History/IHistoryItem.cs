namespace CLP.Entities
{
    public interface IHistoryItem
    {
        int HistoryIndex { get; set; }
        string CachedFormattedValue { get; set; }
        string ID { get; set; }
        string OwnerID { get; set; }
        string ParentPageID { get; set; }
        string ParentPageOwnerID { get; set; }
        uint ParentPageVersionIndex { get; set; }
        CLPPage ParentPage { get; set; }
        int AnimationDelay { get; }
        string FormattedValue { get; }
        
        void ConversionUndo();
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
        IHistoryItem CreatePackagedHistoryItem();
        void UnpackHistoryItem();
        bool IsUsingTrashedPageObject(string id);
        bool IsUsingTrashedInkStroke(string id);
    }
}