﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class AHistoryActionBase : AEntityBase, IHistoryAction
    {
        public static Dictionary<string,int> IncrementIDs = new Dictionary<string, int>(); 

        #region Constructors

        /// <summary>Initializes <see cref="AHistoryActionBase" /> from scratch.</summary>
        public AHistoryActionBase() { ID = Guid.NewGuid().ToCompactID(); }

        public AHistoryActionBase(CLPPage parentPage, List<IHistoryItem> historyItems)
            : this(parentPage, historyItems, new List<IHistoryAction>()) { }

        public AHistoryActionBase(CLPPage parentPage, List<IHistoryAction> historyActions)
            : this(parentPage, new List<IHistoryItem>(), historyActions)
        { }

        public AHistoryActionBase(CLPPage parentPage, List<IHistoryItem> historyItems, List<IHistoryAction> historyActions)
            : this()
        {
            ParentPage = parentPage;
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
        }

        /// <summary>Initializes <see cref="AHistoryActionBase" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public AHistoryActionBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

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

        /// <summary>List of the IDs of the HistoryItems that make up this HistoryAction.</summary>
        public List<string> HistoryItemIDs
        {
            get { return GetValue<List<string>>(HistoryItemIDsProperty); }
            set { SetValue(HistoryItemIDsProperty, value); }
        }

        public static readonly PropertyData HistoryItemIDsProperty = RegisterProperty("HistoryItemIDs", typeof (List<string>), () => new List<string>());

        /// <summary>List of the IDs of any HistoryActions that make up this HistoryAction.</summary>
        public List<string> HistoryActionIDs
        {
            get { return GetValue<List<string>>(HistoryActionIDsProperty); }
            set { SetValue(HistoryActionIDsProperty, value); }
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof (List<string>), () => new List<string>());

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

        /// <summary>Cached value of CodedValue with correct page state.</summary>
        public string CachedCodedValue
        {
            get { return GetValue<string>(CachedCodedValueProperty); }
            set { SetValue(CachedCodedValueProperty, value); }
        }

        public static readonly PropertyData CachedCodedValueProperty = RegisterProperty("CachedCodedValue", typeof(string), string.Empty);

        /// <summary>
        /// Take the following form: OBJECT SUB_TYPE action [ID id_increment, SUB_ID sub_id_increment: action_id]
        /// Additional actions signified by +action.
        /// QUESTION: Would like SUB_TYPE to be in parenthesis. Would make analysis easier.
        /// </summary>
        public string CodedValue
        {
            get
            {
                var subType = IsSubTypeForcedVisible || IsSubTypeVisisble ? " " + CodedObjectSubType : string.Empty;
                var objectAction = IsObjectActionForcedVisible || IsObjectActionVisible ? " " + CodedObjectAction : string.Empty;
                var idIncrement = string.IsNullOrWhiteSpace(CodedObjectIDIncrement) ? string.Empty : " " + CodedObjectIDIncrement;
                var subID = string.IsNullOrWhiteSpace(CodedObjectSubID) ? string.Empty : ", " + CodedObjectSubID;
                var subIDIncrement = string.IsNullOrWhiteSpace(CodedObjectSubIDIncrement) ? string.Empty : " " + CodedObjectSubIDIncrement;
                var objectActionID = string.IsNullOrWhiteSpace(CodedObjectActionID) ? string.Empty : ": " + CodedObjectActionID;
                return string.Format("{0}{1}{2} [{3}{4}{5}{6}{7}]", CodedObject, subType, objectAction, CodedObjectID, idIncrement, subID, subIDIncrement, objectActionID);
            }
        }

        #region Calculated Properties

        /// <summary>List of the HistoryItems that make up this HistoryAction.</summary>
        public List<IHistoryItem> HistoryItems
        {
            get { return ParentPage.History.CompleteOrderedHistoryItems.Where(x => HistoryItemIDs.Contains(x.ID)).OrderBy(x => x.HistoryIndex).ToList(); }
        }

        /// <summary>List of the HistoryActions that make up this HistoryAction.</summary>
        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.HistoryActions.Where(x => HistoryActionIDs.Contains(x.ID)).OrderBy(x => x.HistoryActionIndex).ToList(); }
        }

        #endregion //Calculated Properties

        #endregion //Properties

        #region Static Methods

        public static string GetIncrementID(string codedObject, string codedID)
        {
            var codedKey = string.Format("{0} {1}", codedObject, codedID);
            if (!IncrementIDs.ContainsKey(codedKey))
            {
                IncrementIDs.Add(codedKey, 0);
            }

            return IncrementIDs[codedKey].ToLetter();
        }

        public static string IncrementAndGetIncrementID(string codedObject, string codedID)
        {
            var codedKey = string.Format("{0} {1}", codedObject, codedID);
            if (!IncrementIDs.ContainsKey(codedKey))
            {
                IncrementIDs.Add(codedKey, 0);
            }
            else
            {
                IncrementIDs[codedKey]++;
            }

            return IncrementIDs[codedKey].ToLetter();
        }

        #endregion // Static Methods
    }
}