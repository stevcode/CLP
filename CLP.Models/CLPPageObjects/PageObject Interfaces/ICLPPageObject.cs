using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Ink;
using System.Windows.Media;

namespace CLP.Models
{
    public interface ICLPPageObject
    {
        ICLPPage ParentPage { get; set; }
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
        string BackgroundColor { get; set; }

        ICLPPageObject Duplicate();
        void OnAdded();
        void OnRemoved();
        void OnMoved();
        void OnResized();

        void AcceptStrokes(StrokeCollection addedStrokeIDs, StrokeCollection removedStrokeIDs);
        StrokeCollection GetStrokesOverPageObject();
        void RefreshStrokeParentIDs();
        void AcceptObjects(IEnumerable<ICLPPageObject> addedPageObjects, IEnumerable<ICLPPageObject> removedPageObjects);
        ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject();
        void RefreshPageObjectIDs();
        bool PageObjectIsOver(ICLPPageObject pageObject, double percentage);
        
        List<ICLPPageObject> Cut(Stroke cuttingStroke);
        void EnforceAspectRatio(double aspectRatio);
    }
}
