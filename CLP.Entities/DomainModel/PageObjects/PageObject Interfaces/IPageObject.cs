using System;

namespace CLP.Entities
{
    public interface IPageObject
    {
        string ID { get; set; }
        string OwnerID { get; set; }
        uint VersionIndex { get; set; }
        uint? LastVersionIndex { get; set; }
        string DifferentiationLevel { get; set; }
        DateTime CreationDate { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        double MinimumHeight { get; }
        double MinimumWidth { get; }
        bool IsBackgroundInteractable { get; }
        bool IsManipulatableByNonCreator { get; set; }

        string CreatorID { get; set; }
        string ParentPageID { get; set; }
        string ParentPageOwnerID { get; set; }
        uint ParentPageVersionIndex { get; set; }
        CLPPage ParentPage { get; set; }

        IPageObject Duplicate();
        void OnAdded();
        void OnDeleted();
        void OnMoving();
        void OnMoved();
        void OnResizing();
        void OnResized();
        bool PageObjectIsOver(IPageObject pageObject, double percentage);
    }
}