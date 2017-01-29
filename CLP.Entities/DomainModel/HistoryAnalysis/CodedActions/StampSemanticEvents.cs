using Catel;

namespace CLP.Entities
{
    public static class StampSemanticEvents
    {
        #region Static Methods

        public static ISemanticEvent PartsValueChanged(CLPPage page, PartsValueChangedHistoryAction partsValueChangedHistoryAction)
        {
            Argument.IsNotNull(nameof(page), page);
            Argument.IsNotNull(nameof(partsValueChangedHistoryAction), partsValueChangedHistoryAction);

            var stampID = partsValueChangedHistoryAction.PageObjectID;
            var stamp = page.GetPageObjectByIDOnPageOrInHistory(stampID) as Stamp;
            if (stamp == null)
            {
                return SemanticEvent.GetErrorSemanticEvent(page, partsValueChangedHistoryAction, Codings.ERROR_TYPE_NULL_PAGE_OBJECT, "PartsValueChanged, Stamp NULL");
            }

            var codedObject = Codings.OBJECT_STAMP;
            var eventType = Codings.EVENT_PARTS_VALUE_CHANGED;
            var codedID = stamp.GetCodedIDAtHistoryIndex(partsValueChangedHistoryAction.HistoryActionIndex);
            var incrementID = ObjectSemanticEvents.GetCurrentIncrementIDForPageObject(stamp.ID, codedObject, codedID);

            var eventInfo = $"{partsValueChangedHistoryAction.PreviousValue} to {partsValueChangedHistoryAction.NewValue}";

            var semanticEvent = new SemanticEvent(page, partsValueChangedHistoryAction)
                                {
                                    CodedObject = codedObject,
                                    EventType = eventType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = eventInfo,
                                    ReferencePageObjectID = stampID
                                };

            return semanticEvent;
        }

        #endregion // Static Methods
    }
}