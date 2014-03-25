﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class ClassPeriod : AEntityBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ClassPeriod" /> from scratch.
        /// </summary>
        public ClassPeriod() { ID = Guid.NewGuid().ToString(); }

        /// <summary>
        /// Initializes <see cref="ClassPeriod" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ClassPeriod(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="ClassPeriod" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string)); 

        /// <summary>
        /// Unique Identifier of the <see cref="ClassSubject" /> of the <see cref="ClassPeriod" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string ClassSubjectID
        {
            get { return GetValue<string>(ClassSubjectIDProperty); }
            set { SetValue(ClassSubjectIDProperty, value); }
        }

        public static readonly PropertyData ClassSubjectIDProperty = RegisterProperty("ClassSubjectID", typeof(string), string.Empty);

        /// <summary>
        /// <see cref="ClassSubject" /> of the <see cref="ClassPeriod" />
        /// </summary>
        public virtual ClassSubject ClassSubject
        {
            get { return GetValue<ClassSubject>(ClassSubjectProperty); }
            set { SetValue(ClassSubjectProperty, value); }
        }

        public static readonly PropertyData ClassSubjectProperty = RegisterProperty("ClassSubject", typeof(ClassSubject));

        /// <summary>
        /// Start Time and Date of the <see cref="ClassPeriod" />.
        /// </summary>
        public DateTime StartTime
        {
            get { return GetValue<DateTime>(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly PropertyData StartTimeProperty = RegisterProperty("StartTime", typeof(DateTime));

        //expected pages and actual pages??

        #endregion //Properties

        #region Methods

        #endregion //Methods
    }
}