using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum Handedness
    {
        Right,
        Left
    }

    public class Person : AEntityBase
    {
        private const string AUTHOR_ID = "00000000-0000-0000-1111-000000000001";

        #region Constructors

        /// <summary>
        /// Initializes <see cref="Person" /> from scratch.
        /// </summary>
        public Person() { ID = Guid.NewGuid().ToString(); }

        /// <summary>
        /// Initializes <see cref="Person" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Person(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="Person" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Full Name of the <see cref="Person" />, delimited by spaces.
        /// </summary>
        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), string.Empty);

        /// <summary>
        /// Alternate name for the <see cref="Person" />, delimited by spaces.
        /// </summary>
        public string Alias
        {
            get { return GetValue<string>(AliasProperty); }
            set { SetValue(AliasProperty, value); }
        }

        public static readonly PropertyData AliasProperty = RegisterProperty("Alias", typeof(string), string.Empty);

        /// <summary>
        /// Left or Right Handed.
        /// </summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        /// <summary>
        /// Signifies the <see cref="Person" /> is a student.
        /// </summary>
        public bool IsStudent
        {
            get { return GetValue<bool>(IsStudentProperty); }
            set { SetValue(IsStudentProperty, value); }
        }

        public static readonly PropertyData IsStudentProperty = RegisterProperty("IsStudent", typeof(bool), true);

        #endregion //Properties

        public static Person Author
        {
            get
            {
                var author = new Person
                             {
                                 ID = AUTHOR_ID,
                                 FullName = "AUTHOR",
                                 IsStudent = false
                             };
                return author;
            }
        }

        //TODO: Remove once database established
        private const string EMILY_ID = "00000000-0000-0000-2222-000000000002";
        public static Person Emily
        {
            get
            {
                var teacher = new Person
                              {
                                  ID = EMILY_ID,
                                  FullName = "Emily Sparks",
                                  IsStudent = false
                              };

                return teacher;
            }
        }

        private const string EMILY_PROJECTOR_ID = "00000000-0000-0000-2222-000000000003";
        public static Person EmilyProjector
        {
            get
            {
                var teacher = new Person
                              {
                                  ID = EMILY_PROJECTOR_ID,
                                  FullName = "Projector",
                                  Alias = "Emily Sparks",
                                  IsStudent = false
                              };

                return teacher;
            }
        }
    }
}