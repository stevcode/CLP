using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IHistoryAction
    {
        string ID { get; set; }
        string ParentPageID { get; set; }
        string ParentPageOwnerID { get; set; }
        uint ParentPageVersionIndex { get; set; }
        CLPPage ParentPage { get; set; }
        List<string> HistoryItemIDs { get; set; }
        List<string> HistoryActionIDs { get; set; }
        List<IHistoryItem> HistoryItems { get; }
        List<IHistoryAction> HistoryActions { get; }
        string CodedValue { get; }
    }
}