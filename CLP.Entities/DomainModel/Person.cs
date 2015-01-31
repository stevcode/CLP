using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public enum Handedness
    {
        Right,
        Left
    }

    [Serializable]
    public class Person : AEntityBase, IConnectedPerson
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="Person" /> from scratch.
        /// </summary>
        public Person() { ID = Guid.NewGuid().ToCompactID(); }

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

        public string DisplayName
        {
            get
            {
                var nameParts = FullName.Split(' ');
                var first = nameParts.FirstOrDefault() ?? "First";
                var last = nameParts.LastOrDefault();
                var lastInitial = last == null ? string.Empty : " " + last[0];
                var firstAndLastInitial = first + lastInitial + ".";
                return ID == Author.ID ? "AUTHOR" : !string.IsNullOrEmpty(Alias) ? Alias : firstAndLastInitial;
            }
        }

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

        public string CurrentDifferentiationGroup
        {
            get { return GetValue<string>(CurrentDifferentiationGroupProperty); }
            set { SetValue(CurrentDifferentiationGroupProperty, value); }
        }

        public static readonly PropertyData CurrentDifferentiationGroupProperty = RegisterProperty("CurrentDifferentiationGroup", typeof(string), string.Empty);

        public string TempDifferentiationGroup
        {
            get { return GetValue<string>(TempDifferentiationGroupProperty); }
            set { SetValue(TempDifferentiationGroupProperty, value); }
        }

        public static readonly PropertyData TempDifferentiationGroupProperty = RegisterProperty("TempDifferentiationGroup", typeof(string), string.Empty);


        #endregion //Properties

        #region Static Persons

        public static Person Author
        {
            get { return AuthorPerson; }
        }

        private const string AUTHOR_ID = "AUTHOR0000000000000000";

        private static readonly Person AuthorPerson = new Person
                                                      {
                                                          ID = AUTHOR_ID,
                                                          FullName = "AUTHOR",
                                                          IsStudent = false
                                                      };

        public static Person TestSubmitter
        {
            get { return TestSubmitterPerson; }
        }

        private const string TEST_SUBMITTER_ID = "TEST000000000000000000";

        private static readonly Person TestSubmitterPerson = new Person
                                                             {
                                                                 ID = TEST_SUBMITTER_ID,
                                                                 FullName = "TestSubmitter",
                                                                 IsStudent = true
                                                             };

        public static Person Guest
        {
            get { return GuestPerson; }
        }

        private static readonly Person GuestPerson = new Person
                                                     {
                                                         ID = Guid.NewGuid().ToCompactID(),
                                                         FullName = "GUEST",
                                                         IsStudent = true
                                                     };

        #endregion //Static Persons

        #region IConnectedPerson Members

        /// <summary>
        /// Friendly Name of the computer the <see cref="Person" /> is currently using.
        /// </summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public string CurrentMachineName
        {
            get { return GetValue<string>(CurrentMachineNameProperty); }
            set { SetValue(CurrentMachineNameProperty, value); }
        }

        public static readonly PropertyData CurrentMachineNameProperty = RegisterProperty("CurrentMachineName", typeof(string), string.Empty);

        /// <summary>
        /// TCP address of the computer the <see cref="Person" /> is currently using.
        /// </summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public string CurrentMachineAddress
        {
            get { return GetValue<string>(CurrentMachineAddressProperty); }
            set { SetValue(CurrentMachineAddressProperty, value); }
        }

        public static readonly PropertyData CurrentMachineAddressProperty = RegisterProperty("CurrentMachineAddress", typeof(string), string.Empty);

        /// <summary>
        /// Whether or not this <see cref="Person" /> currently has an established connection with CurrentUser.
        /// </summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public bool IsConnected
        {
            get { return GetValue<bool>(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public static readonly PropertyData IsConnectedProperty = RegisterProperty("IsConnected", typeof(bool), false);

        #endregion
    }
}