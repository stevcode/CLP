using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
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

        ICLPPageObject Duplicate();
        void OnRemoved();

        void RefreshStrokeParentIDs();
        void AcceptStrokes(List<string> addedStrokeIDs, List<string> removedStrokeIDs);
        StrokeCollection GetStrokesOverPageObject();

        bool PageObjectIsOver(ICLPPageObject pageObject, double percentage);
        void AcceptObjects(List<string> addedPageObjectIDs, List<string> removedPageObjectIDs);
        ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject();
    }
}
