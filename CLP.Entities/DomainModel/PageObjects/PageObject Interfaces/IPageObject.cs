using System;

namespace CLP.Entities
{
    public interface IPageObject
    {
        string FormattedName { get; }
        string ID { get; set; }
        string OwnerID { get; set; }
        uint VersionIndex { get; set; }
        uint? LastVersionIndex { get; set; }
        string DifferentiationLevel { get; set; }
        string PageObjectFunctionalityVersion { get; set; }
        DateTime CreationDate { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        int ZIndex { get; }
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
        void OnAdded(bool fromHistory = false);
        void OnDeleted(bool fromHistory = false);
        void OnMoving(double oldX, double oldY, bool fromHistory = false);
        void OnMoved(double oldX, double oldY, bool fromHistory = false);
        void OnResizing(double oldWidth, double oldHeight, bool fromHistory = false);
        void OnResized(double oldWidth, double oldHeight, bool fromHistory = false);
        void OnRotating(double oldAngle, bool fromHistory = false);
        void OnRotated(double oldAngle, bool fromHistory = false);
        bool PageObjectIsOver(IPageObject pageObject, double percentage);
    }
}