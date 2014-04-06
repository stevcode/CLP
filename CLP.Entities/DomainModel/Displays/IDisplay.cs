using System;
using System.Collections.ObjectModel;

namespace CLP.Entities
{
    public interface IDisplay
    {
        string ID { get; set; }
        DateTime CreationDate { get; set; }
        int Index { get; set; }
        string CurrentPageID { get; set; }
        CLPPage CurrentPage { get; set; }
        ObservableCollection<string> PageIDs { get; set; } 
        ObservableCollection<CLPPage> Pages { get; set; } 

        void AddPageToDisplay(CLPPage page);
        void RemovePageFromDisplay(CLPPage page);
    }
}