using System;
using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ClassSubjectCreationViewModel : ViewModelBase
    {
        public ClassSubjectCreationViewModel(ClassInformation classInformation)
        {
            ClassInformation = classInformation;
            ClassInformation.Teacher = new Person
                                   {
                                       IsStudent = false
                                   };

            GroupCreationViewModel = new GroupCreationViewModel();
            TempGroupCreationViewModel = new GroupCreationViewModel("Temp");

            AddStudentCommand = new Command(OnAddStudentCommandExecute);
        }

        public override string Title { get { return "Class Subject Creation Window."; } }

        #region Model

        /// <summary>
        /// <see cref="ClassInformation" /> being created.
        /// </summary>
        [Model]
        public ClassInformation ClassInformation
        {
            get { return GetValue<ClassInformation>(ClassSubjectProperty); }
            set { SetValue(ClassSubjectProperty, value); }
        }

        public static readonly PropertyData ClassSubjectProperty = RegisterProperty("ClassInformation", typeof(ClassInformation));

        /// <summary>
        /// Name of the <see cref="ClassInformation" />.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), string.Empty);

        /// <summary>
        /// Grade Level to which the <see cref="ClassInformation" /> is taught.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof(string), "unknown");

        /// <summary>
        /// Start date of the <see cref="ClassInformation" />.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public DateTime? StartDate
        {
            get { return GetValue<DateTime?>(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly PropertyData StartDateProperty = RegisterProperty("StartDate", typeof(DateTime?));

        /// <summary>
        /// End date of the <see cref="ClassInformation" />.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public DateTime? EndDate
        {
            get { return GetValue<DateTime?>(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly PropertyData EndDateProperty = RegisterProperty("EndDate", typeof(DateTime?));

        /// <summary>
        /// Name of the school.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public string SchoolName
        {
            get { return GetValue<string>(SchoolNameProperty); }
            set { SetValue(SchoolNameProperty, value); }
        }

        public static readonly PropertyData SchoolNameProperty = RegisterProperty("SchoolName", typeof(string), string.Empty);

        /// <summary>
        /// Name of the school district.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public string SchoolDistrict
        {
            get { return GetValue<string>(SchoolDistrictProperty); }
            set { SetValue(SchoolDistrictProperty, value); }
        }

        public static readonly PropertyData SchoolDistrictProperty = RegisterProperty("SchoolDistrict", typeof(string), string.Empty);

        /// <summary>
        /// Name of the city in which the school exists.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public string City
        {
            get { return GetValue<string>(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly PropertyData CityProperty = RegisterProperty("City", typeof(string), string.Empty);

        /// <summary>
        /// Name of the state in which the school exists.
        /// </summary>
        [ViewModelToModel("ClassInformation")]
        public string State
        {
            get { return GetValue<string>(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly PropertyData StateProperty = RegisterProperty("State", typeof(string), string.Empty);

        /// <summary>
        /// List of all the Students in the <see cref="ClassInformation" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [ViewModelToModel("ClassInformation")]
        public virtual ObservableCollection<Person> StudentList
        {
            get { return GetValue<ObservableCollection<Person>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof(ObservableCollection<Person>));

        #endregion //Model

        public GroupCreationViewModel GroupCreationViewModel
        {
            get { return GetValue<GroupCreationViewModel>(GroupCreationViewModelProperty); }
            set { SetValue(GroupCreationViewModelProperty, value); }
        }

        public static readonly PropertyData GroupCreationViewModelProperty = RegisterProperty("GroupCreationViewModel", typeof(GroupCreationViewModel));

        public GroupCreationViewModel TempGroupCreationViewModel
        {
            get { return GetValue<GroupCreationViewModel>(TempGroupCreationViewModelProperty); }
            set { SetValue(TempGroupCreationViewModelProperty, value); }
        }

        public static readonly PropertyData TempGroupCreationViewModelProperty = RegisterProperty("TempGroupCreationViewModel", typeof(GroupCreationViewModel));

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
            GroupCreationViewModel.StudentsNotInGroup.Add(person);
            TempGroupCreationViewModel.StudentsNotInGroup.Add(person);
        }
    }
}
