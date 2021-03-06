﻿using System.Collections.Generic;

namespace CLP.Entities
{
    public interface ISemanticEvent
    {
        // ID
        string ID { get; set; }
        int SemanticEventIndex { get; set; }
        int SemanticPassNumber { get; set; }
        string SemanticPassName { get; set; }
        string CachedCodedValue { get; set; }

        // Coded Portion
        string CodedObject { get; set; }
        string CodedObjectID { get; set; }
        string CodedObjectIDIncrement { get; set; }
        string CodedObjectSubID { get; set; }
        string CodedObjectSubIDIncrement { get; set; }
        string EventType { get; set; }
        string EventInformation { get; set; }

        // Meta Data
        Dictionary<string, string> MetaData { get; set; }
        string ReferencePageObjectID { get; set; }
        bool IsManuallyModified { get; set; }

        // Backing
        List<string> HistoryActionIDs { get; set; }
        CLPPage ParentPage { get; set; }

        // Calculated
        IHistoryAction FirstHistoryAction { get; }
        IHistoryAction LastHistoryAction { get; }
        List<IHistoryAction> HistoryActions { get; }
        string CodedValue { get; }
        string FormattedPass { get; }

        bool ContainsHistoryActionID(string historyActionID);
        ISemanticEvent CreateCopy(bool isPureCopy = false);
    }
}