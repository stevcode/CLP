using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Entities.Ann
{
    public interface IDisplay
    {
        string ID { get; set; }
        DateTime CreationDate { get; set; }
        int DisplayNumber { get; set; }
        string NotebookID { get; set; }
        List<string> CompositePageIDs { get; set; } 
        ObservableCollection<CLPPage> Pages { get; set; } 

        void AddPageToDisplay(CLPPage page);
        void RemovePageFromDisplay(CLPPage page);

        void ToXML(string filePath);
        void Save(string folderPath);
    }
}