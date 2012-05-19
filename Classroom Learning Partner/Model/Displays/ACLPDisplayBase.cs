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
        string UniqueID { get; set; }
        DisplayTypes DisplayType { get; }
        string ParentNotebookUniqueID { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastProjectionTime { get; set; }
        List<DateTime> ProjectionTimesHistory { get; set; }

        ObservableCollection<CLPPage> ForeignPages { get; set; }
        ObservableCollection<Tuple<bool, string>> DisplayPages { get; set; }
    }

    abstract public class ACLPDisplayBase : DataObjectBase<ACLPDisplayBase>
    {
    }
}
