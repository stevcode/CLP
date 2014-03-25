﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class ClassSubject : AEntityBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ClassSubject" /> from scratch.
        /// </summary>
        public ClassSubject() { ID = Guid.NewGuid().ToString(); }

        /// <summary>
        /// Initializes <see cref="ClassSubject" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ClassSubject(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="ClassSubject" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Name of the <see cref="ClassSubject" />.
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), string.Empty);

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> teaching the <see cref="ClassSubject" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string TeacherID
        {
            get { return GetValue<string>(TeacherIDProperty); }
            set { SetValue(TeacherIDProperty, value); }
        }

        public static readonly PropertyData TeacherIDProperty = RegisterProperty("TeacherID", typeof(string), string.Empty);

        /// <summary>
        /// The <see cref="Person" /> teaching the <see cref="ClassSubject" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual Person Teacher
        {
            get { return GetValue<Person>(TeacherProperty); }
            set { SetValue(TeacherProperty, value); }
        }

        public static readonly PropertyData TeacherProperty = RegisterProperty("Teacher", typeof(Person));

        /// <summary>
        /// Grade Level to which the <see cref="ClassSubject" /> is taught.
        /// </summary>
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof(string), "unknown");

        /// <summary>
        /// Start date of the <see cref="ClassSubject" />.
        /// </summary>
        public DateTime? StartDate
        {
            get { return GetValue<DateTime?>(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly PropertyData StartDateProperty = RegisterProperty("StartDate", typeof(DateTime?));

        /// <summary>
        /// End date of the <see cref="ClassSubject" />.
        /// </summary>
        public DateTime? EndDate
        {
            get { return GetValue<DateTime?>(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly PropertyData EndDateProperty = RegisterProperty("EndDate", typeof(DateTime?));

        /// <summary>
        /// Name of the school.
        /// </summary>
        public string SchoolName
        {
            get { return GetValue<string>(SchoolNameProperty); }
            set { SetValue(SchoolNameProperty, value); }
        }

        public static readonly PropertyData SchoolNameProperty = RegisterProperty("SchoolName", typeof(string), string.Empty);

        /// <summary>
        /// Name of the school district.
        /// </summary>
        public string SchoolDistrict
        {
            get { return GetValue<string>(SchoolDistrictProperty); }
            set { SetValue(SchoolDistrictProperty, value); }
        }

        public static readonly PropertyData SchoolDistrictProperty = RegisterProperty("SchoolDistrict", typeof(string), string.Empty);

        /// <summary>
        /// Name of the city in which the school exists.
        /// </summary>
        public string City
        {
            get { return GetValue<string>(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly PropertyData CityProperty = RegisterProperty("City", typeof(string), string.Empty);

        /// <summary>
        /// Name of the state in which the school exists.
        /// </summary>
        public string State
        {
            get { return GetValue<string>(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly PropertyData StateProperty = RegisterProperty("State", typeof(string), string.Empty);

        #endregion //Properties

        #region Methods

        #endregion //Methods
    }
}