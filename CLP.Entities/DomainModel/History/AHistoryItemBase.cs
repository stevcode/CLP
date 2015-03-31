using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class AHistoryItemBase : AEntityBase, IHistoryItem
    {
        public virtual int AnimationDelay
        {
            get { return 50; }
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="AHistoryItemBase" /> from scratch.
        /// </summary>
        public AHistoryItemBase() { ID = Guid.NewGuid().ToCompactID(); }

        /// <summary>
        /// Initializes <see cref="APageObjectBase" /> using <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="IHistoryItem" /> belongs to.</param>
        /// <param name="owner">The <see cref="Person" /> who created the <see cref="IHistoryItem" />.</param>
        public AHistoryItemBase(CLPPage parentPage, Person owner)
            : this()
        {
            ParentPage = parentPage;
            OwnerID = owner.ID;
        }

        /// <summary>
        /// Initializes <see cref="AHistoryItemBase" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public AHistoryItemBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Location of the <see cref="IHistoryItem" /> in the entirety of history, including UndoItems and RedoItems.
        /// </summary>
        public int HistoryIndex
        {
            get { return GetValue<int>(HistoryIndexProperty); }
            set { SetValue(HistoryIndexProperty, value); }
        }

        public static readonly PropertyData HistoryIndexProperty = RegisterProperty("HistoryIndex", typeof (int), -1);

        /// <summary>
        /// Unique Identifier for the <see cref="AHistoryItemBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Unique Identifier for the <see cref="Person" /> who owns the <see cref="AHistoryItemBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// Also Foregin Key for <see cref="Person" /> who owns the <see cref="AHistoryItemBase" />.
        /// </remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>
        /// Version Index of the <see cref="AHistoryItemBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof(uint), 0);

        /// <summary>
        /// Version Index of the latest submission.
        /// </summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof(uint?));

        public string DifferentiationGroup
        {
            get { return GetValue<string>(DifferentiationGroupProperty); }
            set { SetValue(DifferentiationGroupProperty, value); }
        }

        public static readonly PropertyData DifferentiationGroupProperty = RegisterProperty("DifferentiationGroup", typeof(string));

        #region Navigation Properties

        /// <summary>
        /// Unique Identifier for the <see cref="AHistoryItemBase" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key.
        /// </remarks>
        public string ParentPageID
        {
            get { return GetValue<string>(ParentPageIDProperty); }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string));

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> who owns the parent <see cref="CLPPage" /> of the <see cref="AHistoryItemBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key.
        /// </remarks>
        public string ParentPageOwnerID
        {
            get { return GetValue<string>(ParentPageOwnerIDProperty); }
            set { SetValue(ParentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData ParentPageOwnerIDProperty = RegisterProperty("ParentPageOwnerID", typeof(string));

        /// <summary>
        /// The parent <see cref="CLPPage" />'s Version Index.
        /// </summary>
        public uint ParentPageVersionIndex
        {
            get { return GetValue<uint>(ParentPageVersionIndexProperty); }
            set { SetValue(ParentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData ParentPageVersionIndexProperty = RegisterProperty("ParentPageVersionIndex", typeof(uint), 0);

        /// <summary>
        /// The <see cref="AHistoryItemBase" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set
            {
                SetValue(ParentPageProperty, value);
                if(value == null)
                {
                    return;
                }
                ParentPageID = value.ID;
                ParentPageOwnerID = value.OwnerID;
                ParentPageVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

        #endregion //Navigation Properties

        public virtual string FormattedValue
        {
            get { return "Not yet Implemented."; }
        }

        #endregion //Properties

        #region Methods

        public void Undo(bool isAnimationUndo = false)
        {
            if(ParentPage == null)
            {
                return;
            }
            UndoAction(isAnimationUndo);
        }

        protected abstract void UndoAction(bool isAnimationUndo);

        public void Redo(bool isAnimationRedo = false)
        {
            if(ParentPage == null)
            {
                return;
            }
            RedoAction(isAnimationRedo);
        }

        protected abstract void RedoAction(bool isAnimationRedo);

        public abstract IHistoryItem CreatePackagedHistoryItem();

        public abstract void UnpackHistoryItem();

        public virtual bool IsUsingTrashedPageObject(string id, bool isUndoItem) { return false; }

        public virtual bool IsUsingTrashedInkStroke(string id, bool isUndoItem) { return false; }

        #endregion //Methods
    }
}