using System;

namespace CLP.Entities
{
    public interface IHistoryAction
    {
        // ID
        string ID { get; set; }
        int HistoryActionIndex { get; set; }
        string CachedFormattedValue { get; set; }

        // Backing
        string OwnerID { get; set; }
        DateTime CreationTime { get; set; }
        CLPPage ParentPage { get; set; }

        // Calculated
        int AnimationDelay { get; }
        string FormattedValue { get; }
        
        // Methods
        void Undo(bool isAnimationUndo);
        void Redo(bool isAnimationRedo);
        IHistoryAction CreatePackagedHistoryAction();
        void UnpackHistoryAction();
        bool IsUsingTrashedPageObject(string id);
        bool IsUsingTrashedInkStroke(string id);
    }
}