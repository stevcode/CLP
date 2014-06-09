using System;
using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ClassSubjectCreationViewModel : ViewModelBase
    {
        public ClassSubjectCreationViewModel(ClassSubject classSubject)
        {
            ClassSubject = classSubject;
            ClassSubject.Teacher = new Person();

            AddStudentCommand = new Command(OnAddStudentCommandExecute);
        }

        #region Model

        /// <summary>
        /// <see cref="ClassSubject" /> being created.
        /// </summary>
        [Model]
        public ClassSubject ClassSubject
        {
            get { return GetValue<ClassSubject>(ClassSubjectProperty); }
            set { SetValue(ClassSubjectProperty, value); }
        }

        public static readonly PropertyData ClassSubjectProperty = RegisterProperty("ClassSubject", typeof(ClassSubject));

        /// <summary>
        /// Name of the <see cref="ClassSubject" />.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), string.Empty);

        /// <summary>
        /// Grade Level to which the <see cref="ClassSubject" /> is taught.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof(string), "unknown");

        /// <summary>
        /// Start date of the <see cref="ClassSubject" />.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public DateTime? StartDate
        {
            get { return GetValue<DateTime?>(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly PropertyData StartDateProperty = RegisterProperty("StartDate", typeof(DateTime?));

        /// <summary>
        /// End date of the <see cref="ClassSubject" />.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public DateTime? EndDate
        {
            get { return GetValue<DateTime?>(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly PropertyData EndDateProperty = RegisterProperty("EndDate", typeof(DateTime?));

        /// <summary>
        /// Name of the school.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public string SchoolName
        {
            get { return GetValue<string>(SchoolNameProperty); }
            set { SetValue(SchoolNameProperty, value); }
        }

        public static readonly PropertyData SchoolNameProperty = RegisterProperty("SchoolName", typeof(string), string.Empty);

        /// <summary>
        /// Name of the school district.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public string SchoolDistrict
        {
            get { return GetValue<string>(SchoolDistrictProperty); }
            set { SetValue(SchoolDistrictProperty, value); }
        }

        public static readonly PropertyData SchoolDistrictProperty = RegisterProperty("SchoolDistrict", typeof(string), string.Empty);

        /// <summary>
        /// Name of the city in which the school exists.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public string City
        {
            get { return GetValue<string>(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly PropertyData CityProperty = RegisterProperty("City", typeof(string), string.Empty);

        /// <summary>
        /// Name of the state in which the school exists.
        /// </summary>
        [ViewModelToModel("ClassSubject")]
        public string State
        {
            get { return GetValue<string>(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly PropertyData StateProperty = RegisterProperty("State", typeof(string), string.Empty);

        /// <summary>
        /// List of all the Students in the <see cref="CLP.Entities.ClassSubject" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [ViewModelToModel("ClassSubject")]
        public virtual ObservableCollection<Person> StudentList
        {
            get { return GetValue<ObservableCollection<Person>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof(ObservableCollection<Person>));

        #endregion //Model

        /// <summary>
        /// SUMMARY
        /// </summary>
        public Command AddStudentCommand { get; private set; }

        private void OnAddStudentCommandExecute()
        {
            var person = new Person
                         {
                             IsStudent = true
                         };
            var personCreationView = new PersonCreationView(new PersonCreationViewModel(person));
            personCreationView.ShowDialog();

            if(personCreationView.DialogResult == null ||
               personCreationView.DialogResult != true)
            {
                return;
            }

            StudentList.Add(person);
        }
    }
}
