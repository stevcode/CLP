using System;

namespace CLP.Entities
{
    public interface IPageObject
    {
        string ID { get; set; }
        DateTime CreationDate { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        bool IsManipulatableByNonCreator { get; set; }

        string CreatorID { get; set; }
        Person Creator { get; set; }
        string ParentPageID { get; set; }
        CLPPage ParentPage { get; set; }

        IPageObject Duplicate();
        void OnAdded();
        void OnDeleted();
        void OnMoved();
        void OnResized();
    }
}