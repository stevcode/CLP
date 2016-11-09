using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Entities
{
    public interface IDisplay
    {
        string ID { get; set; }
        DateTime CreationDate { get; set; }
        int DisplayNumber { get; set; }
        string NotebookID { get; set; }
        Notebook ParentNotebook { get; set; }
        List<string> CompositePageIDs { get; set; } 
        ObservableCollection<CLPPage> Pages { get; set; }
        bool IsHidden { get; set; }
    }
}