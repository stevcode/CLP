namespace CLP.Entities
{
    public interface IHistoryAction
    {
        // ID
        string ID { get; set; }
        int HistoryIndex { get; set; }
        string CachedFormattedValue { get; set; }

        // Backing
        string OwnerID { get; set; }
        CLPPage ParentPage { get; set; }

        // Calculated
        int AnimationDelay { get; }
        string FormattedValue { get; }
        
        // Methods
        void ConversionUndo();
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
        IHistoryAction CreatePackagedHistoryItem();
        void UnpackHistoryItem();
        bool IsUsingTrashedPageObject(string id);
        bool IsUsingTrashedInkStroke(string id);
    }
}