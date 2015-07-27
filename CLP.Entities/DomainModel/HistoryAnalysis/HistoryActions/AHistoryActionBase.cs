using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class AHistoryActionBase : AEntityBase, IHistoryAction
    {
        #region Constructors

        /// <summary>Initializes <see cref="AHistoryActionBase" /> from scratch.</summary>
        public AHistoryActionBase() { ID = Guid.NewGuid().ToCompactID(); }

        /// <summary>Initializes <see cref="AHistoryActionBase" /> using <see cref="CLPPage" />.</summary>
        public AHistoryActionBase(CLPPage parentPage)
            : this() { ParentPage = parentPage; }

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

        /// <summary>Cached value of CodedValue with correct page state.</summary>
        public string CachedCodedValue
        {
            get { return GetValue<string>(CachedCodedValueProperty); }
            set { SetValue(CachedCodedValueProperty, value); }
        }

        public static readonly PropertyData CachedCodedValueProperty = RegisterProperty("CachedCodedValue", typeof(string), string.Empty);

        /// <summary>Cached value of the FormattedValue with correct page state.</summary>
        public string CachedFormattedValue
        {
            get { return GetValue<string>(CachedFormattedValueProperty); }
            set { SetValue(CachedFormattedValueProperty, value); }
        }

        public static readonly PropertyData CachedFormattedValueProperty = RegisterProperty("CachedFormattedValue", typeof(string), string.Empty);

        public abstract string CodedValue { get; }

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

        #region Methods

        public abstract void GenerateValues();

        #endregion //Methods
    }
}