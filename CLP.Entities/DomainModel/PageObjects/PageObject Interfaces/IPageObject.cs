using System;
using System.Windows;

namespace CLP.Entities
{
    public interface IPageObject
    {
        string ID { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        string OwnerID { get; set; }
        string CreatorID { get; set; }
        DateTime CreationDate { get; set; }
        string PageObjectFunctionalityVersion { get; set; }
        bool IsManipulatableByNonCreator { get; set; }
        CLPPage ParentPage { get; set; }

        int ZIndex { get; }
        double MinimumHeight { get; }
        double MinimumWidth { get; }
        string FormattedName { get; }
        string CodedName { get; }
        string CodedID { get; }
        bool IsBackgroundInteractable { get; }

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
        bool IsOnPageAtHistoryIndex(int historyIndex);
        string GetCodedIDAtHistoryIndex(int historyIndex);
        Point GetPositionAtHistoryIndex(int historyIndex);
        Point GetDimensionsAtHistoryIndex(int historyIndex);
        Rect GetBoundsAtHistoryIndex(int historyIndex);
    }
}