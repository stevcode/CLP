using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class AHistoryActionBase : AEntityBase
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

        /// <summary>Unique Identifier for the <see cref="AHistoryItemBase" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

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

        #region Calculated Properties



        #endregion //Calculated Properties

        #endregion //Properties


    }
}