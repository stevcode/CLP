using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public enum Origin
    {
        Author,
        Teacher,
        TeacherPageGenerated,
        TeacherPageObjectGenerated,
        Student,
        StudentPageGenerated,
        StudentPageObjectGenerated
    }

    [Serializable]
    public abstract class ATagBase : AEntityBase, ITag
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ATagBase" /> from scratch.
        /// </summary>
        public ATagBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes <see cref="ATagBase" /> using <see cref="CLPPage" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ATagBase" /> belongs to.</param>
        protected ATagBase(CLPPage parentPage)
            : this() { ParentPage = parentPage; }

        /// <summary>
        /// Initializes <see cref="ATagBase" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ATagBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="ATagBase" />.
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
        /// Unique Identifier for the <see cref="Person" /> who owns the <see cref="ATagBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>
        /// Version Index of the <see cref="ATagBase" />.
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

        /// <summary>
        /// Date and Time the <see cref="ATagBase" /> was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// <see cref="Origin" /> from which the <see cref="ATagBase" /> originates.
        /// </summary>
        public Origin Origin
        {
            get { return GetValue<Origin>(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }

        public static readonly PropertyData OriginProperty = RegisterProperty("Origin", typeof(Origin), Origin.Author);
 
        /// <summary>
        /// Determines if the <see cref="ATagBase" /> can have more than one value.
        /// </summary>
        public bool IsSingleValueTag
        {
            get { return GetValue<bool>(IsSingleValueTagProperty); }
            set { SetValue(IsSingleValueTagProperty, value); }
        }

        public static readonly PropertyData IsSingleValueTagProperty = RegisterProperty("IsSingleValueTag", typeof(bool), false);

        /// <summary>
        /// Value of the <see cref="ATagBase" />.
        /// </summary>
        public string Value
        {
            get { return GetValue<string>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(string), string.Empty);

         #region Navigation Properties

        /// <summary>
        /// Unique Identifier for the <see cref="IPageObject" />'s parent <see cref="CLPPage" />.
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
        /// Unique Identifier of the <see cref="Person" /> who owns the parent <see cref="CLPPage" /> of the <see cref="ATagBase" />.
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
        /// The <see cref="ATagBase" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
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

        #endregion //Properties
    }
}