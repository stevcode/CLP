using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Ink;

namespace CLP.Models
{
    public interface ICLPPageObject
    {
        CLPPage ParentPage { get; set; }
        string ParentPageID { get; set; }
        string ParentID { get; set; }
        string UniqueID { get; set; }
        DateTime CreationDate { get; set; }
        string PageObjectType { get; }
        ObservableCollection<string> PageObjectStrokeParentIDs { get; set; }
        bool CanAcceptStrokes { get; set; }
        ObservableCollection<string> PageObjectObjectParentIDs { get; set; }
        bool CanAcceptPageObjects { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        bool IsBackground { get; set; }
        int Parts { get; set; }
        bool CanAdornersShow { get; set; }
        bool IsInternalPageObject { get; set; }

        ICLPPageObject Duplicate();
        void OnRemoved();

        void RefreshStrokeParentIDs();
        void AcceptStrokes(StrokeCollection addedStrokeIDs, StrokeCollection removedStrokeIDs);
        StrokeCollection GetStrokesOverPageObject();

        bool PageObjectIsOver(ICLPPageObject pageObject, double percentage);
        void AcceptObjects(ObservableCollection<ICLPPageObject> addedPageObjects, ObservableCollection<ICLPPageObject> removedPageObjects);
        ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject();
        void EnforceAspectRatio(double aspectRatio);
    }
}
