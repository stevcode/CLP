using System.Collections.Generic;

namespace CLP.Entities
{
    public interface IHistoryAction
    {
        int HistoryActionIndex { get; set; }
        string ID { get; set; }
        string ParentPageID { get; set; }
        string ParentPageOwnerID { get; set; }
        uint ParentPageVersionIndex { get; set; }
        CLPPage ParentPage { get; set; }
        List<string> HistoryItemIDs { get; set; }
        List<string> HistoryActionIDs { get; set; }
        string CachedCodedValue { get; set; }
        string CachedFormattedValue { get; set; }
        string CodedValue { get; }
        List<IHistoryItem> HistoryItems { get; }
        List<IHistoryAction> HistoryActions { get; }
        void GenerateValues();
    }
}