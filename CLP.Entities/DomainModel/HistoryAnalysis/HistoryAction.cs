﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class HistoryAction : AEntityBase, IHistoryAction
    {
        #region Constructors

        /// <summary>Initializes <see cref="HistoryAction" /> from scratch.</summary>
        public HistoryAction() { ID = Guid.NewGuid().ToCompactID(); }

        public HistoryAction(CLPPage parentPage, IHistoryItem historyItem)
            : this(parentPage, new List<IHistoryItem> { historyItem }, new List<IHistoryAction>())
        { }

        public HistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            : this(parentPage, historyItems, new List<IHistoryAction>()) { }

        public HistoryAction(CLPPage parentPage, IHistoryAction historyAction)
            : this(parentPage, new List<IHistoryItem>(), new List<IHistoryAction> { historyAction })
        { }

        public HistoryAction(CLPPage parentPage, List<IHistoryAction> historyActions)
            : this(parentPage, new List<IHistoryItem>(), historyActions)
        { }

        public HistoryAction(CLPPage parentPage, List<IHistoryItem> historyItems, List<IHistoryAction> historyActions)
            : this()
        {
            ParentPage = parentPage;
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            HistoryActions = historyActions;
        }

        #endregion //Constructors

        #region Properties

        #region Navigation Properties

        /// <summary>Location of the <see cref="IHistoryAction" /> in the list of <see cref="IHistoryAction" />s.</summary>
        public int HistoryActionIndex
        {
            get { return GetValue<int>(HistoryActionIndexProperty); }
            set { SetValue(HistoryActionIndexProperty, value); }
        }

        public static readonly PropertyData HistoryActionIndexProperty = RegisterProperty("HistoryActionIndex", typeof(int), -1);

        /// <summary>Unique Identifier for the <see cref="AHistoryItemBase" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Unique Identifier for the <see cref="AHistoryItemBase" />'s parent <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Foreign Key.</remarks>
        public string ParentPageID
        {
            get { return GetValue<string>(ParentPageIDProperty); }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof (string));

        /// <summary>Unique Identifier of the <see cref="Person" /> who owns the parent <see cref="CLPPage" /> of the <see cref="AHistoryItemBase" />.</summary>
        /// <remarks>Composite Foreign Key.</remarks>
        public string ParentPageOwnerID
        {
            get { return GetValue<string>(ParentPageOwnerIDProperty); }
            set { SetValue(ParentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData ParentPageOwnerIDProperty = RegisterProperty("ParentPageOwnerID", typeof (string));

        /// <summary>The parent <see cref="CLPPage" />'s Version Index.</summary>
        public uint ParentPageVersionIndex
        {
            get { return GetValue<uint>(ParentPageVersionIndexProperty); }
            set { SetValue(ParentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData ParentPageVersionIndexProperty = RegisterProperty("ParentPageVersionIndex", typeof (uint), 0);

        /// <summary>The <see cref="AHistoryItemBase" />'s parent <see cref="CLPPage" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set
            {
                SetValue(ParentPageProperty, value);
                if (value == null)
                {
                    return;
                }
                ParentPageID = value.ID;
                ParentPageOwnerID = value.OwnerID;
                ParentPageVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof (CLPPage));

        #endregion //Navigation Properties

        #region Coded Portions

        /// <summary>CodedObject portion of the CodedHistoryAction report.</summary>
        public string CodedObject
        {
            get { return GetValue<string>(CodedObjectProperty); }
            set { SetValue(CodedObjectProperty, value); }
        }

        public static readonly PropertyData CodedObjectProperty = RegisterProperty("CodedObject", typeof(string), string.Empty);

        /// <summary>SubType portion of the CodedHistoryAction report.</summary>
        public string CodedObjectSubType
        {
            get { return GetValue<string>(CodedObjectSubTypeProperty); }
            set { SetValue(CodedObjectSubTypeProperty, value); }
        }

        public static readonly PropertyData CodedObjectSubTypeProperty = RegisterProperty("CodedObjectSubType", typeof(string), string.Empty);

        /// <summary>Determines if SubType portion of the CodedHistoryAction is visibly reported.</summary>
        public bool IsSubTypeVisisble
        {
            get { return GetValue<bool>(IsSubTypeVisisbleProperty); }
            set { SetValue(IsSubTypeVisisbleProperty, value); }
        }

        public static readonly PropertyData IsSubTypeVisisbleProperty = RegisterProperty("IsSubTypeVisisble", typeof(bool), true);

        /// <summary>Forces SubType portion of the CodedHistoryAction to be visibly reported.</summary>
        [XmlIgnore]
        public bool IsSubTypeForcedVisible
        {
            get { return GetValue<bool>(IsSubTypeForcedVisibleProperty); }
            set { SetValue(IsSubTypeForcedVisibleProperty, value); }
        }

        public static readonly PropertyData IsSubTypeForcedVisibleProperty = RegisterProperty("IsSubTypeForcedVisible", typeof(bool), false);

        /// <summary>ObjectAction portion of the CodedHistoryAction report.</summary>
        public string CodedObjectAction
        {
            get { return GetValue<string>(CodedObjectActionProperty); }
            set { SetValue(CodedObjectActionProperty, value); }
        }

        public static readonly PropertyData CodedObjectActionProperty = RegisterProperty("CodedObjectAction", typeof(string), string.Empty);

        /// <summary>Determines if ObjectAction portion of the CodedHistoryAction is visibly reported.</summary>
        public bool IsObjectActionVisible
        {
            get { return GetValue<bool>(IsObjectActionVisibleProperty); }
            set { SetValue(IsObjectActionVisibleProperty, value); }
        }

        public static readonly PropertyData IsObjectActionVisibleProperty = RegisterProperty("IsObjectActionVisible", typeof(bool), true);

        /// <summary>Forces ObjectAction portion of the CodedHistoryAction to be visibly reported.</summary>
        [XmlIgnore]
        public bool IsObjectActionForcedVisible
        {
            get { return GetValue<bool>(IsObjectActionForcedVisibleProperty); }
            set { SetValue(IsObjectActionForcedVisibleProperty, value); }
        }

        public static readonly PropertyData IsObjectActionForcedVisibleProperty = RegisterProperty("IsObjectActionForcedVisible", typeof(bool), false);

        /// <summary>ObjectID portion of the CodedHistoryAction report.</summary>
        public string CodedObjectID
        {
            get { return GetValue<string>(CodedObjectIDProperty); }
            set { SetValue(CodedObjectIDProperty, value); }
        }

        public static readonly PropertyData CodedObjectIDProperty = RegisterProperty("CodedObjectID", typeof(string), string.Empty);

        /// <summary>ObjectID Increment portion of the CodedHistoryaction report.</summary>
        public string CodedObjectIDIncrement
        {
            get { return GetValue<string>(CodedObjectIDIncrementProperty); }
            set { SetValue(CodedObjectIDIncrementProperty, value); }
        }

        public static readonly PropertyData CodedObjectIDIncrementProperty = RegisterProperty("CodedObjectIDIncrement", typeof(string), string.Empty);

        /// <summary>ObjectSubID portion of the CodedHistoryAction report.</summary>
        public string CodedObjectSubID
        {
            get { return GetValue<string>(CodedObjectSubIDProperty); }
            set { SetValue(CodedObjectSubIDProperty, value); }
        }

        public static readonly PropertyData CodedObjectSubIDProperty = RegisterProperty("CodedObjectSubID", typeof(string), string.Empty);

        /// <summary>ObjectSubID Increment portion of the CodedHistoryaction report.</summary>
        public string CodedObjectSubIDIncrement
        {
            get { return GetValue<string>(CodedObjectSubIDIncrementProperty); }
            set { SetValue(CodedObjectSubIDIncrementProperty, value); }
        }

        public static readonly PropertyData CodedObjectSubIDIncrementProperty = RegisterProperty("CodedObjectSubIDIncrement", typeof(string), string.Empty);

        /// <summary>ObjectActionID portion of the CodedHistoryAction report.</summary>
        public string CodedObjectActionID
        {
            get { return GetValue<string>(CodedObjectActionIDProperty); }
            set { SetValue(CodedObjectActionIDProperty, value); }
        }

        public static readonly PropertyData CodedObjectActionIDProperty = RegisterProperty("CodedObjectActionID", typeof(string), string.Empty);

        #endregion // Coded Portions

        #region Backing Properties

        /// <summary>List of the IDs of the HistoryItems that make up this HistoryAction.</summary>
        public List<string> HistoryItemIDs
        {
            get { return GetValue<List<string>>(HistoryItemIDsProperty); }
            set { SetValue(HistoryItemIDsProperty, value); }
        }

        public static readonly PropertyData HistoryItemIDsProperty = RegisterProperty("HistoryItemIDs", typeof(List<string>), () => new List<string>());

        /// <summary>List of any HistoryActions that make up this HistoryAction.</summary>
        public List<IHistoryAction> HistoryActions
        {
            get { return GetValue<List<IHistoryAction>>(HistoryActionsProperty); }
            set { SetValue(HistoryActionsProperty, value); }
        }

        public static readonly PropertyData HistoryActionsProperty = RegisterProperty("HistoryActions", typeof(List<IHistoryAction>), () => new List<IHistoryAction>());

        /// <summary>Cached value of CodedValue with correct page state.</summary>
        public string CachedCodedValue
        {
            get { return GetValue<string>(CachedCodedValueProperty); }
            set { SetValue(CachedCodedValueProperty, value); }
        }

        public static readonly PropertyData CachedCodedValueProperty = RegisterProperty("CachedCodedValue", typeof(string), string.Empty);

        #endregion // Backing Properties

        #region Calculated Properties

        /// <summary>List of the HistoryItems that make up this HistoryAction.</summary>
        public List<IHistoryItem> HistoryItems
        {
            get { return ParentPage.History.CompleteOrderedHistoryItems.Where(x => HistoryItemIDs.Contains(x.ID)).OrderBy(x => x.HistoryIndex).ToList(); }
        }

        /// <summary>
        /// Take the following form: OBJECT SUB_TYPE action [ID id_increment, SUB_ID sub_id_increment: action_id]
        /// Additional actions signified by +action.
        /// QUESTION: Would like SUB_TYPE to be in parenthesis. Would make analysis easier.
        /// </summary>
        public string CodedValue
        {
            get
            {
                var subType = !string.IsNullOrWhiteSpace(CodedObjectSubType) && (IsSubTypeForcedVisible || IsSubTypeVisisble) ? " " + CodedObjectSubType : string.Empty;
                var objectAction = !string.IsNullOrWhiteSpace(CodedObjectAction) && (IsObjectActionForcedVisible || IsObjectActionVisible) ? " " + CodedObjectAction : string.Empty;
                var idIncrement = string.IsNullOrWhiteSpace(CodedObjectIDIncrement) ? string.Empty : " " + CodedObjectIDIncrement;
                var subID = string.IsNullOrWhiteSpace(CodedObjectSubID) ? string.Empty : ", " + CodedObjectSubID;
                var subIDIncrement = string.IsNullOrWhiteSpace(CodedObjectSubIDIncrement) ? string.Empty : " " + CodedObjectSubIDIncrement;
                var objectActionID = string.IsNullOrWhiteSpace(CodedObjectActionID) ? string.Empty : ": " + CodedObjectActionID;
                var bracketsContent = string.IsNullOrWhiteSpace(CodedObjectID) ? string.Empty : string.Format(" [{0}{1}{2}{3}{4}]", CodedObjectID, idIncrement, subID, subIDIncrement, objectActionID);
                return string.Format("{0}{1}{2}{3}", CodedObject, subType, objectAction, bracketsContent);
            }
        }

        #endregion //Calculated Properties

        #region Meta Data Properties

        /// <summary>Storage dictionary  for all meta data.</summary>
        public Dictionary<string, string> MetaData
        {
            get { return GetValue<Dictionary<string, string>>(MetaDataProperty); }
            set { SetValue(MetaDataProperty, value); }
        }

        public static readonly PropertyData MetaDataProperty = RegisterProperty("MetaData", typeof(Dictionary<string, string>), () => new Dictionary<string, string>());

        public string ReferencePageObjectID
        {
            get { return MetaData.ContainsKey(Codings.META_REFERENCE_PAGE_OBJECT_ID) ? MetaData[Codings.META_REFERENCE_PAGE_OBJECT_ID] : null; }
            set
            {
                if (!MetaData.ContainsKey(Codings.META_REFERENCE_PAGE_OBJECT_ID))
                {
                    MetaData.Add(Codings.META_REFERENCE_PAGE_OBJECT_ID, value);
                }
                else
                {
                    MetaData[Codings.META_REFERENCE_PAGE_OBJECT_ID] = value;
                }
            }
        }

        #endregion // Meta Data Properties

        #endregion //Properties
    }
}