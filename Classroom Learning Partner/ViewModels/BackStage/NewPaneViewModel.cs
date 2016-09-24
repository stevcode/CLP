using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NewPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public NewPaneViewModel()
        {
            ClassRoster = new ClassRoster();

            InitializeCommands();
        }

        #endregion //Constructor

        #region Models

        #region NotebookSet

        /// <summary>Model of this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public NotebookSet NotebookSet
        {
            get { return GetValue<NotebookSet>(NotebookSetProperty); }
            set { SetValue(NotebookSetProperty, value); }
        }

        public static readonly PropertyData NotebookSetProperty = RegisterProperty("NotebookSet", typeof(NotebookSet));

        /// <summary>Auto-Mapped property of the NotebookSet Model.</summary>
        [ViewModelToModel("NotebookSet")]
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string));

        #endregion // NotebookSet

        #region ClassRoster

        /// <summary>Model of this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
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

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText => "New Notebook";

        #endregion //Bindings

        #region Commands

        private void InitializeCommands()
        {
            CreateNotebookCommand = new Command(OnCreateNotebookCommandExecute, OnCreateNotebookCanExecute);
            ImportStudentNamesCommand = new Command(OnImportStudentNamesCommandExecute);
            AddPersonCommand = new Command<bool>(OnAddPersonCommandExecute);
            EditPersonCommand = new Command<Person>(OnEditPersonCommandExecute);
            DeleteTeacherCommand = new Command<Person>(OnDeleteTeacherCommandExecute);
            DeleteStudentCommand = new Command<Person>(OnDeleteStudentCommandExecute);
        }

        /// <summary>Creates a new notebook.</summary>
        public Command CreateNotebookCommand { get; private set; }

        private void OnCreateNotebookCommandExecute()
        {
            var dependencyResolver = this.GetDependencyResolver();
            var dataService = dependencyResolver.Resolve<IDataService>();
            dataService.SetCurrentClassRoster(ClassRoster);
            dataService.CreateAuthorNotebook(NotebookName);
        }

        private bool OnCreateNotebookCanExecute()
        {
            return !string.IsNullOrWhiteSpace(NotebookName);
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
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var nameParts = line.Split(' ').ToList();
                    if (!nameParts.Any())
                    {
                        continue;
                    }

                    var firstName = nameParts.First();
                    nameParts.RemoveAt(0);
                    if (!nameParts.Any())
                    {
                        var person = new Person
                                     {
                                         IsStudent = true,
                                         FirstName = firstName
                                     };
                        listOfStudents.Add(person);
                        continue;
                    }

                    var lastName = nameParts.Last();
                    nameParts.RemoveAt(nameParts.Count - 1);
                    if (!nameParts.Any())
                    {
                        var person = new Person
                                     {
                                         IsStudent = true,
                                         FirstName = firstName,
                                         LastName = lastName
                                     };
                        listOfStudents.Add(person);
                        continue;
                    }

                    var middleName = string.Join(" ", nameParts);
                    var personfull = new Person
                                     {
                                         IsStudent = true,
                                         FirstName = firstName,
                                         MiddleName = middleName,
                                         LastName = lastName
                                     };
                    listOfStudents.Add(personfull);
                }

                ListOfStudents.AddRange(listOfStudents.OrderBy(p => p.FirstName).ToList());
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, unable to open or read this file.");
            }
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
            }
            else
            {
                ListOfStudents.Add(person);
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

        #endregion //Commands
    }
}