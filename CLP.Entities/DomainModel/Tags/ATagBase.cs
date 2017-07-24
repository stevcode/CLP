using System;
using Catel.Data;

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

    public enum Category
    {
        Array,
        DivisionTemplate,
        Stamp,
        Strategy,
        NumberLine,
        Definition,
        CurriculumInformation,
        OtherPageInformation,
        Representation,
        Answer,
        MetaData
    }

    [Serializable]
    public abstract class ATagBase : ASerializableBase, ITag
    {
        #region Constructors

        /// <summary>Initializes <see cref="ATagBase" /> from scratch.</summary>
        protected ATagBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="ATagBase" /> using <see cref="CLPPage" />.</summary>
        protected ATagBase(CLPPage parentPage, Origin origin)
            : this()
        {
            ParentPage = parentPage;
            Origin = origin;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ATagBase" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), string.Empty);

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="ATagBase" />.</summary>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary>Date and Time the <see cref="ATagBase" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>Designates the <see cref="ITag" /> as invisible in the PageInfoPanel.</summary>
        public bool IsHiddenTag
        {
            get { return GetValue<bool>(IsHiddenTagProperty); }
            set { SetValue(IsHiddenTagProperty, value); }
        }

        public static readonly PropertyData IsHiddenTagProperty = RegisterProperty("IsHiddenTag", typeof(bool), false);

        /// <summary> From where the <see cref="ATagBase" /> originates. </summary>
        public Origin Origin
        {
            get { return GetValue<Origin>(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }

        public static readonly PropertyData OriginProperty = RegisterProperty("Origin", typeof(Origin), Origin.Author);

        /// <summary>The <see cref="ATagBase" />'s parent <see cref="CLPPage" />.</summary>
        public CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage));

        #region Calculated Properties

        /// <summary>Determines if the <see cref="ATagBase" /> can have more than one value.</summary>
        public virtual bool IsSingleValueTag => false;

        /// <summary>Category the Tag belongs to for sorting purposes.</summary>
        public abstract Category Category { get; }

        /// <summary>Produces a human-readable string that describes the name of the tag.</summary>
        public virtual string FormattedName => GetType().Name;

        /// <summary>Produces a human-readable string that describes the value of the tag.</summary>
        public abstract string FormattedValue { get; }

        #endregion // Calculated Properties

        #endregion //Properties
    }
}