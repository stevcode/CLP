using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.ViewModels
{
    public class PersonCreationViewModel : ViewModelBase
    {
        public PersonCreationViewModel(Person person)
        {
            Person = person;
        }
        
        public override string Title { get { return "Person Creation Window."; } }

        /// <summary>
        /// SUMMARY
        /// </summary>
        [Model]
        public Person Person
        {
            get { return GetValue<Person>(PersonProperty); }
            set { SetValue(PersonProperty, value); }
        }

        public static readonly PropertyData PersonProperty = RegisterProperty("Person", typeof(Person));

        /// <summary>
        /// Unique Identifier for the <see cref="CLP.Entities.Ann.Person" />.
        /// </summary>
        [ViewModelToModel("Person")]
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Full Name of the <see cref="CLP.Entities.Ann.Person" />, delimited by spaces.
        /// </summary>
        [ViewModelToModel("Person")]
        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string), string.Empty);

        /// <summary>
        /// Alternate name for the <see cref="CLP.Entities.Ann.Person" />, delimited by spaces.
        /// </summary>
        [ViewModelToModel("Person")]
        public string Alias
        {
            get { return GetValue<string>(AliasProperty); }
            set { SetValue(AliasProperty, value); }
        }

        public static readonly PropertyData AliasProperty = RegisterProperty("Alias", typeof(string), string.Empty);

        /// <summary>
        /// Left or Right Handed.
        /// </summary>
        [ViewModelToModel("Person")]
        public Handedness Handedness
        {
            get { return GetValue<Handedness>(HandednessProperty); }
            set { SetValue(HandednessProperty, value); }
        }

        public static readonly PropertyData HandednessProperty = RegisterProperty("Handedness", typeof(Handedness), Handedness.Right);

        /// <summary>
        /// Signifies the <see cref="CLP.Entities.Ann.Person" /> is a student.
        /// </summary>
        [ViewModelToModel("Person")]
        public bool IsStudent
        {
            get { return GetValue<bool>(IsStudentProperty); }
            set { SetValue(IsStudentProperty, value); }
        }

        public static readonly PropertyData IsStudentProperty = RegisterProperty("IsStudent", typeof(bool), true);

    }
}