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
        string CodedObject { get; set; }
        string CodedObjectSubType { get; set; }
        bool IsSubTypeVisisble { get; set; }
        bool IsSubTypeForcedVisible { get; set; }
        string CodedObjectAction { get; set; }
        bool IsObjectActionVisible { get; set; }
        bool IsObjectActionForcedVisible { get; set; }
        string CodedObjectID { get; set; }
        string CodedObjectIDIncrement { get; set; }
        string CodedObjectSubID { get; set; }
        string CodedObjectSubIDIncrement { get; set; }
        string CodedObjectActionID { get; set; }
        Dictionary<string, string> MetaData { get; set; }
        List<string> HistoryItemIDs { get; set; }
        List<IHistoryAction> HistoryActions { get; set; }
        string CachedCodedValue { get; set; }
        string CodedValue { get; }
        List<IHistoryItem> HistoryItems { get; }
    }
}