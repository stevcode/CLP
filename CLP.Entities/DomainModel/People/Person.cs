using System;
using System.Linq;
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

    public enum Genders
    {
        Male,
        Female,
        Unknown
    }

    [Serializable]
    public class Person : ASerializableBase, IConnectedPerson
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
            get => GetValue<string>(IDProperty);
            set => SetValue(IDProperty, value);
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), string.Empty);

        /// <summary>Person's First Name</summary>
        public string FirstName
        {
            get => GetValue<string>(FirstNameProperty);
            set => SetValue(FirstNameProperty, value);
        }

        public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string), string.Empty);

        /// <summary>Person's nickname or Title, overrides first name in displays.</summary>
        public string Nickname
        {
            get => GetValue<string>(NicknameProperty);
            set => SetValue(NicknameProperty, value);
        }

        public static readonly PropertyData NicknameProperty = RegisterProperty("Nickname", typeof(string), string.Empty);

        /// <summary>Person's Middle Name</summary>
        public string MiddleName
        {
            get => GetValue<string>(MiddleNameProperty);
            set => SetValue(MiddleNameProperty, value);
        }

        public static readonly PropertyData MiddleNameProperty = RegisterProperty("MiddleName", typeof(string), string.Empty);

        /// <summary>Person's Last name</summary>
        public string LastName
        {
            get => GetValue<string>(LastNameProperty);
            set => SetValue(LastNameProperty, value);
        }

        public static readonly PropertyData LastNameProperty = RegisterProperty("LastName", typeof(string), string.Empty);

        /// <summary>Complete override of DisplayName</summary>
        public string Alias
        {
            get => GetValue<string>(AliasProperty);
            set => SetValue(AliasProperty, value);
        }

        public static readonly PropertyData AliasProperty = RegisterProperty("Alias", typeof(string), string.Empty);

        /// <summary>Signifies the <see cref="Person" /> is a student.</summary>
        public bool IsStudent
        {
            get => GetValue<bool>(IsStudentProperty);
            set => SetValue(IsStudentProperty, value);
        }

        public static readonly PropertyData IsStudentProperty = RegisterProperty("IsStudent", typeof(bool), true);

        public string CurrentDifferentiationGroup
        {
            get => GetValue<string>(CurrentDifferentiationGroupProperty);
            set => SetValue(CurrentDifferentiationGroupProperty, value);
        }

        public static readonly PropertyData CurrentDifferentiationGroupProperty = RegisterProperty("CurrentDifferentiationGroup", typeof(string), string.Empty);

        public string TemporaryDifferentiationGroup
        {
            get => GetValue<string>(TemporaryDifferentiationGroupProperty);
            set => SetValue(TemporaryDifferentiationGroupProperty, value);
        }

        public static readonly PropertyData TemporaryDifferentiationGroupProperty = RegisterProperty("TemporaryDifferentiationGroup", typeof(string), string.Empty);

        /// <summary>Left or Right Handed.</summary>
        public Handedness Handedness
        {
            get => GetValue<Handedness>(HandednessProperty);
            set => SetValue(HandednessProperty, value);
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        /// <summary>Gender of the person.</summary>
        public Genders Gender
        {
            get => GetValue<Genders>(GenderProperty);
            set => SetValue(GenderProperty, value);
        }

        public static readonly PropertyData GenderProperty = RegisterProperty(nameof(Gender), typeof(Genders), Genders.Unknown);

        #region Calculated Properties

        /// <summary>Formatted full name of the person.</summary>
        public string FullName => CreateFullName(FirstName, LastName, ID, MiddleName, Nickname, Alias);

        /// <summary>Formatted display name of the person.</summary>
        public string DisplayName => CreateDisplayName(FirstName, LastName, ID, Nickname, Alias);

        #endregion // Calculated Properties

        #endregion //Properties

        #region IConnectedPerson Properties

        /// <summary>Friendly Name of the computer the <see cref="Person" /> is currently using.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public string CurrentMachineName
        {
            get => GetValue<string>(CurrentMachineNameProperty);
            set => SetValue(CurrentMachineNameProperty, value);
        }

        public static readonly PropertyData CurrentMachineNameProperty = RegisterProperty("CurrentMachineName", typeof(string), string.Empty);

        /// <summary>TCP address of the computer the <see cref="Person" /> is currently using.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public string CurrentMachineAddress
        {
            get => GetValue<string>(CurrentMachineAddressProperty);
            set => SetValue(CurrentMachineAddressProperty, value);
        }

        public static readonly PropertyData CurrentMachineAddressProperty = RegisterProperty("CurrentMachineAddress", typeof(string), string.Empty);

        /// <summary>Whether or not this <see cref="Person" /> currently has an established connection with CurrentUser.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public bool IsConnected
        {
            get => GetValue<bool>(IsConnectedProperty);
            set => SetValue(IsConnectedProperty, value);
        }

        public static readonly PropertyData IsConnectedProperty = RegisterProperty("IsConnected", typeof(bool), false);

        #endregion // IConnectedPerson Properties

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

        #region Storage

        public string OwnerType => ID == AUTHOR_ID ? "A" : IsStudent ? "S" : "T";

        public string NotebookOwnerDirectoryName => $"{OwnerType};{FullName};{ID}";

        #endregion // Storage

        #region Static Methods

        public static Person ParseFromFullName(string fullName, bool isStudent = true)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            var nameParts = fullName.Split(' ').ToList();
            if (!nameParts.Any())
            {
                return null;
            }

            var firstName = nameParts.First();
            nameParts.RemoveAt(0);
            if (!nameParts.Any())
            {
                var person = new Person
                             {
                                 IsStudent = isStudent,
                                 FirstName = firstName
                             };

                return person;
            }

            var lastName = nameParts.Last();
            nameParts.RemoveAt(nameParts.Count - 1);
            if (!nameParts.Any())
            {
                var person = new Person
                             {
                                 IsStudent = isStudent,
                                 FirstName = firstName,
                                 LastName = lastName
                             };

                return person;
            }

            var middleName = string.Join(" ", nameParts);
            var personfull = new Person
                             {
                                 IsStudent = isStudent,
                                 FirstName = firstName,
                                 MiddleName = middleName,
                                 LastName = lastName
                             };

            return personfull;
        }

        public static string CreateFullName(string firstName, string lastName, string personID, string middleName = "", string nickname = "", string alias = "")
        {
            if (personID == AUTHOR_ID)
            {
                return "AUTHOR";
            }

            if (!string.IsNullOrWhiteSpace(alias))
            {
                return alias;
            }

            var first = string.IsNullOrWhiteSpace(nickname) ? firstName : nickname;
            var middleInitial = string.IsNullOrWhiteSpace(middleName) ? string.Empty : $" {middleName[0]}.";
            return $"{first}{middleInitial} {lastName}";
        }

        public static string CreateDisplayName(string firstName, string lastName, string personID, string nickname = "", string alias = "")
        {
            if (personID == AUTHOR_ID)
            {
                return "AUTHOR";
            }

            if (!string.IsNullOrWhiteSpace(alias))
            {
                return alias;
            }

            var first = string.IsNullOrWhiteSpace(nickname) ? firstName : nickname;
            var lastInitial = string.IsNullOrWhiteSpace(lastName) ? string.Empty : $" {lastName[0]}.";
            var firstAndLastInitial = first + lastInitial;
            return firstAndLastInitial;
        }

        #endregion // Static Methods

        #region Static Persons

        public static Person Author { get; } = new Person
                                               {
                                                   ID = AUTHOR_ID,
                                                   FirstName = "AUTHOR",
                                                   Alias = "AUTHOR",
                                                   IsStudent = false
                                               };

        public static int GuestCount { get; set; }

        public static Person Guest => new Person
                                      {
                                          ID = Guid.NewGuid().ToCompactID(),
                                          FirstName = "GUEST",
                                          Alias = "GUEST " + ++GuestCount,
                                          IsStudent = true
                                      };

        #endregion //Static Persons
    }
}