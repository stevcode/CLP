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
        ObservableCollection<List<byte>> PageObjectByteStrokes { get; set; }
        bool CanAcceptStrokes { get; set; }
        ObservableCollection<ICLPPageObject> PageObjectObjects { get; set; }
        bool CanAcceptObjects { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        bool IsBackground { get; set; }

        ICLPPageObject Duplicate();
        void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes);
    }
}
