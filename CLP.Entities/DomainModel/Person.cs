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

        #endregion //Properties

        public static Person Author
        {
            get
            {
                var author = new Person
                             {
                                 ID = AUTHOR_ID,
                                 FullName = "AUTHOR"
                             };
                return author;
            }
        }
    }
}