﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class SemanticEvent : ASerializableBase, ISemanticEvent
    {
        #region Constants

        private const string META_REFERENCE_PAGE_OBJECT_ID = "REFERENCE_PAGE_OBJECT_ID";

        #endregion // Constants

        #region Constructors

        public SemanticEvent()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        public SemanticEvent(CLPPage parentPage, IHistoryAction historyAction)
            : this(parentPage,
                   new List<IHistoryAction>
                   {
                       historyAction
                   },
                   new List<ISemanticEvent>()) { }

        public SemanticEvent(CLPPage parentPage, List<IHistoryAction> historyActions)
            : this(parentPage, historyActions, new List<ISemanticEvent>()) { }

        public SemanticEvent(CLPPage parentPage, ISemanticEvent semanticEvent)
            : this(parentPage,
                   new List<IHistoryAction>(),
                   new List<ISemanticEvent>
                   {
                       semanticEvent
                   }) { }

        public SemanticEvent(CLPPage parentPage, List<ISemanticEvent> semanticEvents)
            : this(parentPage, new List<IHistoryAction>(), semanticEvents) { }

        public SemanticEvent(CLPPage parentPage, List<IHistoryAction> historyActions, List<ISemanticEvent> semanticEvents)
            : this()
        {
            ParentPage = parentPage;
            HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
            SemanticEvents = semanticEvents;
        }

        #endregion //Constructors

        #region Properties

        #region ID Properties

        /// <summary>Unique Identifier for the <see cref="SemanticEvent" />.</summary>
        public string ID
        {
            get => GetValue<string>(IDProperty);
            set => SetValue(IDProperty, value);
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), string.Empty);

        /// <summary>The pass the semantic event is part of.</summary>
        public int SemanticPassNumber
        {
            get => GetValue<int>(SemanticPassNumberProperty);
            set => SetValue(SemanticPassNumberProperty, value);
        }

        public static readonly PropertyData SemanticPassNumberProperty = RegisterProperty("SemanticPassNumber", typeof(int), -1);

        /// <summary>Location of the <see cref="ISemanticEvent" /> in the list of <see cref="ISemanticEvent" />s.</summary>
        public int SemanticEventIndex
        {
            get => GetValue<int>(SemanticEventIndexProperty);
            set => SetValue(SemanticEventIndexProperty, value);
        }

        public static readonly PropertyData SemanticEventIndexProperty = RegisterProperty("SemanticEventIndex", typeof(int), -1);

        /// <summary>Cached value of CodedValue with correct page state.</summary>
        public string CachedCodedValue
        {
            get => GetValue<string>(CachedCodedValueProperty);
            set => SetValue(CachedCodedValueProperty, value);
        }

        public static readonly PropertyData CachedCodedValueProperty = RegisterProperty("CachedCodedValue", typeof(string), string.Empty);

        #endregion // ID Properties

        #region Coded Portion Properties

        /// <summary>Coded Object portion of the SemanticEvent report.</summary>
        public string CodedObject
        {
            get => GetValue<string>(CodedObjectProperty);
            set => SetValue(CodedObjectProperty, value);
        }

        public static readonly PropertyData CodedObjectProperty = RegisterProperty("CodedObject", typeof(string), string.Empty);

        /// <summary>Coded Object ID portion of the SemanticEvent report.</summary>
        public string CodedObjectID
        {
            get => GetValue<string>(CodedObjectIDProperty);
            set => SetValue(CodedObjectIDProperty, value);
        }

        public static readonly PropertyData CodedObjectIDProperty = RegisterProperty("CodedObjectID", typeof(string), string.Empty);

        /// <summary>Coded Object ID Increment portion of the SemanticEvent report.</summary>
        public string CodedObjectIDIncrement
        {
            get => GetValue<string>(CodedObjectIDIncrementProperty);
            set => SetValue(CodedObjectIDIncrementProperty, value);
        }

        public static readonly PropertyData CodedObjectIDIncrementProperty = RegisterProperty("CodedObjectIDIncrement", typeof(string), string.Empty);

        /// <summary>Coded Object SubID portion of the SemanticEvent report.</summary>
        public string CodedObjectSubID
        {
            get => GetValue<string>(CodedObjectSubIDProperty);
            set => SetValue(CodedObjectSubIDProperty, value);
        }

        public static readonly PropertyData CodedObjectSubIDProperty = RegisterProperty("CodedObjectSubID", typeof(string), string.Empty);

        /// <summary>Coded Object SubID Increment portion of the SemanticEvent report.</summary>
        public string CodedObjectSubIDIncrement
        {
            get => GetValue<string>(CodedObjectSubIDIncrementProperty);
            set => SetValue(CodedObjectSubIDIncrementProperty, value);
        }

        public static readonly PropertyData CodedObjectSubIDIncrementProperty = RegisterProperty("CodedObjectSubIDIncrement", typeof(string), string.Empty);

        /// <summary>Event Type portion of the SemanticEvent report.</summary>
        public string EventType
        {
            get => GetValue<string>(EventTypeProperty);
            set => SetValue(EventTypeProperty, value);
        }

        public static readonly PropertyData EventTypeProperty = RegisterProperty("EventType", typeof(string), string.Empty);

        /// <summary>Event Information portion of the SemanticEvent report.</summary>
        public string EventInformation
        {
            get => GetValue<string>(EventInformationProperty);
            set => SetValue(EventInformationProperty, value);
        }

        public static readonly PropertyData EventInformationProperty = RegisterProperty("EventInformation", typeof(string), string.Empty);

        #endregion // Coded Portion Properties

        #region Meta Data Properties

        /// <summary>Storage dictionary for all meta data.</summary>
        public Dictionary<string, string> MetaData
        {
            get => GetValue<Dictionary<string, string>>(MetaDataProperty);
            set => SetValue(MetaDataProperty, value);
        }

        public static readonly PropertyData MetaDataProperty = RegisterProperty("MetaData", typeof(Dictionary<string, string>), () => new Dictionary<string, string>());

        /// <summary> Used if the CodedObject acts upon another PageObject. </summary>
        public string ReferencePageObjectID
        {
            get => MetaData.ContainsKey(META_REFERENCE_PAGE_OBJECT_ID) ? MetaData[META_REFERENCE_PAGE_OBJECT_ID] : null;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                if (!MetaData.ContainsKey(META_REFERENCE_PAGE_OBJECT_ID))
                {
                    MetaData.Add(META_REFERENCE_PAGE_OBJECT_ID, value);
                }
                else
                {
                    MetaData[META_REFERENCE_PAGE_OBJECT_ID] = value;
                }
            }
        }

        #endregion // Meta Data Properties

        #region Backing Properties

        /// <summary>List of the IDs of the HistoryActionss that make up this SemanticEvent.</summary>
        public List<string> HistoryActionIDs
        {
            get => GetValue<List<string>>(HistoryActionIDsProperty);
            set => SetValue(HistoryActionIDsProperty, value);
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof(List<string>), () => new List<string>());

        /// <summary>List of any SemanticEvents that make up this SemanticEvent.</summary>
        public List<ISemanticEvent> SemanticEvents
        {
            get => GetValue<List<ISemanticEvent>>(SemanticEventsProperty);
            set => SetValue(SemanticEventsProperty, value);
        }

        public static readonly PropertyData SemanticEventsProperty = RegisterProperty("SemanticEvents", typeof(List<ISemanticEvent>), () => new List<ISemanticEvent>());

        /// <summary>The <see cref="ISemanticEvent" />'s parent <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public CLPPage ParentPage
        {
            get => GetValue<CLPPage>(ParentPageProperty);
            set => SetValue(ParentPageProperty, value);
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

        #endregion // Backing Properties

        #region Calculated Properties

        public IHistoryAction FirstHistoryAction
        {
            get
            {
                if (HistoryActions.Any())
                {
                    return HistoryActions.First();
                }

                var firstSemanticEvent = SemanticEvents.FirstOrDefault();
                return firstSemanticEvent?.FirstHistoryAction;
            }
        }

        public IHistoryAction LastHistoryAction
        {
            get
            {
                if (HistoryActions.Any())
                {
                    return HistoryActions.Last();
                }

                var lastSemanticEvent = SemanticEvents.LastOrDefault();
                return lastSemanticEvent?.LastHistoryAction;
            }
        }

        /// <summary>List of the HistoryActions that make up this SemanticEvent.</summary>
        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.CompleteOrderedHistoryActions.Where(x => HistoryActionIDs.Contains(x.ID)).OrderBy(x => x.HistoryActionIndex).ToList(); }
        }

        /// <summary> Takes the following form: CODED_OBJECT eventType [ID id_increment, SUB_ID sub_id_increment] eventInfo </summary>
        public string CodedValue
        {
            get
            {
                var idIncrement = string.IsNullOrWhiteSpace(CodedObjectIDIncrement) ? string.Empty : " " + CodedObjectIDIncrement;
                var subID = string.IsNullOrWhiteSpace(CodedObjectSubID) ? string.Empty : ", " + CodedObjectSubID;
                var subIDIncrement = string.IsNullOrWhiteSpace(CodedObjectSubIDIncrement) ? string.Empty : " " + CodedObjectSubIDIncrement;
                var compositeCodedObjectID = $"{CodedObjectID}{idIncrement}{subID}{subIDIncrement}";
                if (!string.IsNullOrWhiteSpace(compositeCodedObjectID))
                {
                    compositeCodedObjectID = $" [{compositeCodedObjectID}]";
                }

                var eventInfo = string.IsNullOrWhiteSpace(EventInformation) ? string.Empty : " " + EventInformation;

                var codedEvent = $"{CodedObject} {EventType}{compositeCodedObjectID}{eventInfo}";
                if (!codedEvent.Equals(CachedCodedValue))
                {
                    CachedCodedValue = codedEvent;
                }

                return codedEvent;
            }
        }

        #endregion //Calculated Properties

        #endregion //Properties

        #region Methods

        public bool ContainsHistoryActionID(string historyActionID)
        {
            if (!HistoryActionIDs.Any() &&
                !SemanticEvents.Any())
            {
                return false;
            }

            if (HistoryActionIDs.Any())
            {
                return HistoryActionIDs.Contains(historyActionID);
            }

            return SemanticEvents.Any(e => e.ContainsHistoryActionID(historyActionID));
        }

        #endregion // Methods

        #region Static Methods

        public static SemanticEvent GetErrorSemanticEvent(CLPPage page, IHistoryAction historyAction, string errorType, string errorMessage)
        {
            return GetErrorSemanticEvent(page,
                                         new List<IHistoryAction>
                                         {
                                             historyAction
                                         },
                                         errorType,
                                         errorMessage);
        }

        public static SemanticEvent GetErrorSemanticEvent(CLPPage page, List<IHistoryAction> historyActions, string errorType, string errorMessage)
        {
            var codedObject = Codings.OBJECT_ERROR;
            var codedID = "Action";
            var incrementID = string.Join(", ", historyActions.Select(h => h.HistoryActionIndex));

            var semanticEvent = new SemanticEvent(page, historyActions)
                                {
                                    CodedObject = codedObject,
                                    EventType = errorType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = errorMessage
                                };

            return semanticEvent;
        }

        public static SemanticEvent GetErrorSemanticEvent(CLPPage page, ISemanticEvent semanticEvent, string errorType, string errorMessage)
        {
            return GetErrorSemanticEvent(page,
                                         new List<ISemanticEvent>
                                         {
                                             semanticEvent
                                         },
                                         errorType,
                                         errorMessage);
        }

        public static SemanticEvent GetErrorSemanticEvent(CLPPage page, List<ISemanticEvent> semanticEvents, string errorType, string errorMessage)
        {
            var codedObject = Codings.OBJECT_ERROR;
            var codedID = "Event";
            var incrementID = string.Join(", ", semanticEvents.Select(h => $"{h.SemanticPassNumber}.{h.SemanticEventIndex}"));

            var semanticEvent = new SemanticEvent(page, semanticEvents)
                                {
                                    CodedObject = codedObject,
                                    EventType = errorType,
                                    CodedObjectID = codedID,
                                    CodedObjectIDIncrement = incrementID,
                                    EventInformation = errorMessage
                                };

            return semanticEvent;
        }

        #endregion // Static Methods
    }
}