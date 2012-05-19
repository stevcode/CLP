using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Catel.Data;

namespace Classroom_Learning_Partner.Model.Displays
{
    public enum DisplayTypes
    {
        Mirror,
        Grid,
        Column,
        Canvas
    }

    public interface ICLPDisplay
    {
        string UniqueID;
        DisplayTypes DisplayType;
        string ParentNotebookUniqueID;
        DateTime CreationDate;
        DateTime LastProjectionTime;
        List<DateTime> ProjectionTimesHistory;

        ObservableCollection<CLPPage> ForeignPages;
        ObservableCollection<Tuple<bool,string>> DisplayPages;
    }

    abstract public class ACLPDisplayBase : DataObjectBase<ACLPDisplayBase>, ICLPDisplay
    {
    }
}
