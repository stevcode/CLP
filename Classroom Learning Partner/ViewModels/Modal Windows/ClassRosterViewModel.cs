using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ClassRosterViewModel : ViewModelBase
    {
        #region Constructor

        public ClassRosterViewModel(ClassRoster classRoster)
        {
            ClassRoster = classRoster;

            InitializeCommands();
        }

        #endregion // Constructor

        #region Models

        #region ClassRoster

        /// <summary>Model of this ViewModel.</summary>
        [Model]
        public ClassRoster ClassRoster
        {
            get { return GetValue<ClassRoster>(ClassRosterProperty); }
            set { SetValue(ClassRosterProperty, value); }
        }

        public static readonly PropertyData ClassRosterProperty = RegisterProperty("ClassRoster", typeof(ClassRoster));

        /// <summary>Auto-Mapped property of the Roster Model.</summary>
        [ViewModelToModel("ClassRoster")]
        public ObservableCollection<Person> ListOfStudents
        {
            get { return GetValue<ObservableCollection<Person>>(ListOfStudentsProperty); }
            set { SetValue(ListOfStudentsProperty, value); }
        }

        public static readonly PropertyData ListOfStudentsProperty = RegisterProperty("ListOfStudents", typeof(ObservableCollection<Person>));

        /// <summary>Auto-Mapped property of the Roster Model.</summary>
        [ViewModelToModel("ClassRoster")]
        public ObservableCollection<Person> ListOfTeachers
        {
            get { return GetValue<ObservableCollection<Person>>(ListOfTeachersProperty); }
            set { SetValue(ListOfTeachersProperty, value); }
        }

        public static readonly PropertyData ListOfTeachersProperty = RegisterProperty("ListOfTeachers", typeof(ObservableCollection<Person>));

        /// <summary>Name of the subject being taught.</summary>
        [ViewModelToModel("ClassRoster")]
        public string SubjectName
        {
            get { return GetValue<string>(SubjectNameProperty); }
            set { SetValue(SubjectNameProperty, value); }
        }

        public static readonly PropertyData SubjectNameProperty = RegisterProperty("SubjectName", typeof(string));

        /// <summary>Grade Level of the subject being taught.</summary>
        [ViewModelToModel("ClassRoster")]
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof(string));

        /// <summary>Start date of the class subject.</summary>
        [ViewModelToModel("ClassRoster")]
        public DateTime? StartDate
        {
            get { return GetValue<DateTime?>(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly PropertyData StartDateProperty = RegisterProperty("StartDate", typeof(DateTime?));

        /// <summary>End date of the class subject.</summary>
        [ViewModelToModel("ClassRoster")]
        public DateTime? EndDate
        {
            get { return GetValue<DateTime?>(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly PropertyData EndDateProperty = RegisterProperty("EndDate", typeof(DateTime?));

        /// <summary>Name of the school.</summary>
        [ViewModelToModel("ClassRoster")]
        public string SchoolName
        {
            get { return GetValue<string>(SchoolNameProperty); }
            set { SetValue(SchoolNameProperty, value); }
        }

        public static readonly PropertyData SchoolNameProperty = RegisterProperty("SchoolName", typeof(string));

        /// <summary>Name of the school district.</summary>
        [ViewModelToModel("ClassRoster")]
        public string SchoolDistrict
        {
            get { return GetValue<string>(SchoolDistrictProperty); }
            set { SetValue(SchoolDistrictProperty, value); }
        }

        public static readonly PropertyData SchoolDistrictProperty = RegisterProperty("SchoolDistrict", typeof(string));

        /// <summary>Name of the city in which the school exists.</summary>
        [ViewModelToModel("ClassRoster")]
        public string City
        {
            get { return GetValue<string>(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly PropertyData CityProperty = RegisterProperty("City", typeof(string));

        /// <summary>Name of the state in which the school exists.</summary>
        [ViewModelToModel("ClassRoster")]
        public string State
        {
            get { return GetValue<string>(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly PropertyData StateProperty = RegisterProperty("State", typeof(string));

        #endregion // ClassRoster

        #endregion // Models

        #region Commands

        private void InitializeCommands()
        {
            ImportStudentNamesCommand = new Command(OnImportStudentNamesCommandExecute);
            EditStudentGroupsCommand = new Command(OnEditStudentGroupsCommandExecute);
            AddPersonCommand = new Command<bool>(OnAddPersonCommandExecute);
            EditPersonCommand = new Command<Person>(OnEditPersonCommandExecute);
            DeleteTeacherCommand = new Command<Person>(OnDeleteTeacherCommandExecute);
            DeleteStudentCommand = new Command<Person>(OnDeleteStudentCommandExecute);
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Builds list of students from an imported StudentNames.txt file</summary>
        public Command ImportStudentNamesCommand { get; private set; }

        private void OnImportStudentNamesCommandExecute()
        {
            var dependencyResolver = this.GetDependencyResolver();
            var openFileService = dependencyResolver.Resolve<IOpenFileService>();
            openFileService.Filter = "Text files (*.txt)|*.txt";
            openFileService.IsMultiSelect = false;
            if (!openFileService.DetermineFile())
            {
                return;
            }

            var filePath = openFileService.FileName;
            try
            {
                var listOfStudents = new List<Person>();
                foreach (var line in File.ReadLines(filePath))
                {
                    var person = Person.ParseFromFullName(line);
                    if (person == null)
                    {
                        continue;
                    }

                    listOfStudents.Add(person);
                }

                ListOfStudents.AddRange(listOfStudents.OrderBy(p => p.FullName).ToList());
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, unable to open or read this text file.");
            }
        }

        /// <summary>Edits the differentiation groups of the students.</summary>
        public Command EditStudentGroupsCommand { get; private set; }

        private void OnEditStudentGroupsCommandExecute()
        {
            var viewModel = new StudentDifferentiationViewModel(ClassRoster);
            viewModel.ShowWindowAsDialog();
        }

        /// <summary>Adds a new person to the roster.</summary>
        public Command<bool> AddPersonCommand { get; private set; }

        private void OnAddPersonCommandExecute(bool isTeacher)
        {
            var person = new Person
            {
                IsStudent = !isTeacher
            };

            var viewModel = new PersonViewModel(person)
            {
                WindowTitle = isTeacher ? "New Teacher" : "New Student"
            };

            var result = viewModel.ShowWindowAsDialog();

            if (result != true)
            {
                return;
            }

            if (isTeacher)
            {
                ListOfTeachers.Add(person);
                ListOfTeachers = ListOfTeachers.OrderBy(p => p.FullName).ToObservableCollection();
            }
            else
            {
                ListOfStudents.Add(person);
                ListOfStudents = ListOfStudents.OrderBy(p => p.FullName).ToObservableCollection();
            }
        }

        /// <summary>Edits the details of a Student or Teacher.</summary>
        public Command<Person> EditPersonCommand { get; private set; }

        private void OnEditPersonCommandExecute(Person person)
        {
            var viewModel = new PersonViewModel(person)
            {
                WindowTitle = person.IsStudent ? "Edit Student" : "Edit Teacher"
            };
            viewModel.ShowWindowAsDialog();
        }

        /// <summary>Removes student from the List of Students</summary>
        public Command<Person> DeleteTeacherCommand { get; private set; }

        private void OnDeleteTeacherCommandExecute(Person teacher)
        {
            if (teacher == null ||
                !ListOfTeachers.Contains(teacher))
            {
                return;
            }

            ListOfTeachers.Remove(teacher);
        }

        /// <summary>Removes student from the List of Students</summary>
        public Command<Person> DeleteStudentCommand { get; private set; }

        private void OnDeleteStudentCommandExecute(Person student)
        {
            if (student == null ||
                !ListOfStudents.Contains(student))
            {
                return;
            }

            ListOfStudents.Remove(student);
        }

        /// <summary>Validates and confirms changes to the ClassRoster.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            await SaveViewModelAsync();
            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the ClassRoster.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CancelViewModelAsync();
            await CloseViewModelAsync(false);
        }

        #endregion //Commands
    }
}
