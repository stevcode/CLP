using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CLP.Models
{
    public interface ICLPDisplay
    {
        string UniqueID { get; set; }
        string ParentNotebookID { get; set; }
        DateTime CreationDate { get; set; }
        ObservableCollection<DateTime> ProjectionTimesHistory { get; set; }
        ObservableCollection<string> DisplayPageIDs { get; set; }
        List<ICLPPage> ForeignPages { get; set; }
        void AddPageToDisplay(ICLPPage page);
        void RemovePageFromDisplay(ICLPPage page);
    }
}
