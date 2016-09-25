﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public class ClassRoster : AEntityBase
    {
        #region Constructor

        /// <summary>Initializes <see cref="ClassRoster" /> from scratch.</summary>
        public ClassRoster()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        #endregion // Constructor

        #region Properties

        /// <summary>Unique ID for the class.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>Name of the subject being taught.</summary>
        public string SubjectName
        {
            get { return GetValue<string>(SubjectNameProperty); }
            set { SetValue(SubjectNameProperty, value); }
        }

        public static readonly PropertyData SubjectNameProperty = RegisterProperty("SubjectName", typeof(string), string.Empty);

        /// <summary>List of all the Teachers in the class.</summary>
        public ObservableCollection<Person> ListOfTeachers
        {
            get { return GetValue<ObservableCollection<Person>>(ListOfTeachersProperty); }
            set { SetValue(ListOfTeachersProperty, value); }
        }

        public static readonly PropertyData ListOfTeachersProperty = RegisterProperty("ListOfTeachers", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        /// <summary>List of all the Students in the class.</summary>
        public ObservableCollection<Person> ListOfStudents
        {
            get { return GetValue<ObservableCollection<Person>>(ListOfStudentsProperty); }
            set { SetValue(ListOfStudentsProperty, value); }
        }

        public static readonly PropertyData ListOfStudentsProperty = RegisterProperty("ListOfStudents", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        /// <summary>Grade Level of the subject being taught.</summary>
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof(string), string.Empty);

        /// <summary>Start date of the class subject.</summary>
        public DateTime? StartDate
        {
            get { return GetValue<DateTime?>(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly PropertyData StartDateProperty = RegisterProperty("StartDate", typeof(DateTime?));

        /// <summary>End date of the class subject.</summary>
        public DateTime? EndDate
        {
            get { return GetValue<DateTime?>(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly PropertyData EndDateProperty = RegisterProperty("EndDate", typeof(DateTime?));

        /// <summary>Name of the school.</summary>
        public string SchoolName
        {
            get { return GetValue<string>(SchoolNameProperty); }
            set { SetValue(SchoolNameProperty, value); }
        }

        public static readonly PropertyData SchoolNameProperty = RegisterProperty("SchoolName", typeof(string), string.Empty);

        /// <summary>Name of the school district.</summary>
        public string SchoolDistrict
        {
            get { return GetValue<string>(SchoolDistrictProperty); }
            set { SetValue(SchoolDistrictProperty, value); }
        }

        public static readonly PropertyData SchoolDistrictProperty = RegisterProperty("SchoolDistrict", typeof(string), string.Empty);

        /// <summary>Name of the city in which the school exists.</summary>
        public string City
        {
            get { return GetValue<string>(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly PropertyData CityProperty = RegisterProperty("City", typeof(string), string.Empty);

        /// <summary>Name of the state in which the school exists.</summary>
        public string State
        {
            get { return GetValue<string>(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly PropertyData StateProperty = RegisterProperty("State", typeof(string), string.Empty);

        #endregion // Properties

        #region Storage

        /// <summary>Associated FileInfo of the classroster's container.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public string ContainerFilePath { get; set; }

        public string DefaultContainerFileName
        {
            get
            {
                var teacherName = ListOfTeachers.Any() ? ListOfTeachers.First().FullName : Person.Author.FullName;
                return $"{teacherName} - {SubjectName}";
            }
        }

        #endregion // Storage
    }
}