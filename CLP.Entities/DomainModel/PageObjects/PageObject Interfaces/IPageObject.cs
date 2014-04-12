﻿using System;

namespace CLP.Entities
{
    public interface IPageObject
    {
        string ID { get; set; }
        string OwnerID { get; set; }
        uint VersionIndex { get; set; }
        uint? LastVersionIndex { get; set; }
        DateTime CreationDate { get; set; }
        double XPosition { get; set; }
        double YPosition { get; set; }
        double Height { get; set; }
        double Width { get; set; }
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
        void OnMoved();
        void OnResized();
    }
}