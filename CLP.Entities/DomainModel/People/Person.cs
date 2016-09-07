using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

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
        #region Constants

        public const string AUTHOR_ID = "AUTHOR0000000000000000";

        #endregion // Constants

        #region Constructors

        /// <summary>Initializes <see cref="Person" /> from scratch.</summary>
        public Person()
        {
            ID = Guid.NewGuid().ToCompactID(); 
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="Person" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Person's First Name</summary>
        public string FirstName
        {
            get { return GetValue<string>(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }

        public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string), string.Empty);

        /// <summary>Person's nickname or Title, overrides first name in displays.</summary>
        public string Nickname
        {
            get { return GetValue<string>(NicknameProperty); }
            set { SetValue(NicknameProperty, value); }
        }

        public static readonly PropertyData NicknameProperty = RegisterProperty("Nickname", typeof(string), string.Empty);

        /// <summary>Person's Middle Name</summary>
        public string MiddleName
        {
            get { return GetValue<string>(MiddleNameProperty); }
            set { SetValue(MiddleNameProperty, value); }
        }

        public static readonly PropertyData MiddleNameProperty = RegisterProperty("MiddleName", typeof(string), string.Empty);
        
        /// <summary>Person's Last name</summary>
        public string LastName
        {
            get { return GetValue<string>(LastNameProperty); }
            set { SetValue(LastNameProperty, value); }
        }

        public static readonly PropertyData LastNameProperty = RegisterProperty("LastName", typeof(string), string.Empty);

        /// <summary>Complete override of DisplayName</summary>
        public string Alias
        {
            get { return GetValue<string>(AliasProperty); }
            set { SetValue(AliasProperty, value); }
        }

        public static readonly PropertyData AliasProperty = RegisterProperty("Alias", typeof(string), string.Empty);

        /// <summary>Signifies the <see cref="Person" /> is a student.</summary>
        public bool IsStudent
        {
            get { return GetValue<bool>(IsStudentProperty); }
            set { SetValue(IsStudentProperty, value); }
        }

        public static readonly PropertyData IsStudentProperty = RegisterProperty("IsStudent", typeof (bool), true);

        public string CurrentDifferentiationGroup
        {
            get { return GetValue<string>(CurrentDifferentiationGroupProperty); }
            set { SetValue(CurrentDifferentiationGroupProperty, value); }
        }

        public static readonly PropertyData CurrentDifferentiationGroupProperty = RegisterProperty("CurrentDifferentiationGroup", typeof (string), string.Empty);

        public string TemporaryDifferentiationGroup
        {
            get { return GetValue<string>(TemporaryDifferentiationGroupProperty); }
            set { SetValue(TemporaryDifferentiationGroupProperty, value); }
        }

        public static readonly PropertyData TemporaryDifferentiationGroupProperty = RegisterProperty("TemporaryDifferentiationGroup", typeof (string), string.Empty);

        /// <summary>Left or Right Handed.</summary>
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        #region Calculated Properties

        /// <summary>Formatted full name of the person.</summary>
        [Obsolete("Only use getter.")]
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public string FullName
        {
            get
            {
                if (ID == AUTHOR_ID)
                {
                    return "AUTHOR";
                }

                if (!string.IsNullOrWhiteSpace(Alias))
                {
                    return Alias;
                }

                var first = string.IsNullOrWhiteSpace(Nickname) ? FirstName : Nickname;
                var middleInitial = string.IsNullOrWhiteSpace(MiddleName) ? string.Empty : $" {MiddleName[0]}.";
                return $"{first}{middleInitial} {LastName}";
            }
            set { SetValue(FullNameProperty, value); }
        }

        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), string.Empty);

        /// <summary>Formatted display name of the person.</summary>
        public string DisplayName
        {
            get
            {
                if (ID == AUTHOR_ID)
                {
                    return "AUTHOR";
                }

                if (!string.IsNullOrWhiteSpace(Alias))
                {
                    return Alias;
                }

                var first = string.IsNullOrWhiteSpace(Nickname) ? FirstName : Nickname;
                var lastInitial = string.IsNullOrWhiteSpace(LastName) ? string.Empty : $" {LastName[0]}.";
                var firstAndLastInitial = first + lastInitial;
                return firstAndLastInitial;
            }
        }

        #endregion // Calculated Properties

        #endregion //Properties

        #region IConnectedPerson Properties

        /// <summary>Friendly Name of the computer the <see cref="Person" /> is currently using.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public string CurrentMachineName
        {
            get { return GetValue<string>(CurrentMachineNameProperty); }
            set { SetValue(CurrentMachineNameProperty, value); }
        }

        public static readonly PropertyData CurrentMachineNameProperty = RegisterProperty("CurrentMachineName", typeof(string), string.Empty);

        /// <summary>TCP address of the computer the <see cref="Person" /> is currently using.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public string CurrentMachineAddress
        {
            get { return GetValue<string>(CurrentMachineAddressProperty); }
            set { SetValue(CurrentMachineAddressProperty, value); }
        }

        public static readonly PropertyData CurrentMachineAddressProperty = RegisterProperty("CurrentMachineAddress", typeof(string), string.Empty);

        /// <summary>Whether or not this <see cref="Person" /> currently has an established connection with CurrentUser.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public bool IsConnected
        {
            get { return GetValue<bool>(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public static readonly PropertyData IsConnectedProperty = RegisterProperty("IsConnected", typeof(bool), false);

        #endregion // IConnectedPerson Properties

        #region Methods

        #region Overrides of ObservableObject

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FirstName" ||
                e.PropertyName == "Nickname" ||
                e.PropertyName == "MiddleName" ||
                e.PropertyName == "LastName" ||
                e.PropertyName == "Alias")
            {
                RaisePropertyChanged(nameof(FullName));
                RaisePropertyChanged(nameof(DisplayName));
            }

            base.OnPropertyChanged(e);
        }

        #endregion

        #endregion // Methods

        #region Static Persons

        public static Person Author { get; } = new Person
                                               {
                                                   ID = AUTHOR_ID,
                                                   FirstName = "AUTHOR",
                                                   Alias = "AUTHOR",
                                                   IsStudent = false
                                               };

        // TODO: Test each call to this Get produces a new person with new ID.
        public static Person Guest { get; } = new Person
                                              {
                                                  ID = Guid.NewGuid().ToCompactID(),
                                                  FirstName = "GUEST",
                                                  Alias = "GUEST",
                                                  IsStudent = true
                                              };

        #endregion //Static Persons
    }
}