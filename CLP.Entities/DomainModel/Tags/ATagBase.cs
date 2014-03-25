using System;
using System.Runtime.Serialization;
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

    public abstract class ATagBase : AEntityBase
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
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

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

        /// <summary>
        /// Unique Identifier for the <see cref="ATagBase" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string ParentPageID
        {
            get { return GetValue<string>(ParentPageIDProperty); }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string), string.Empty);

        #endregion //Properties

        #region Methods

        #endregion //Methods
    }
}