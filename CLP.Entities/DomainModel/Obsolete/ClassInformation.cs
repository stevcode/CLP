using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public class ClassInformationNameComposite
    {
        public const string QUALIFIER_TEXT = "classInfo";
        public string TeacherName { get; set; }
        public string ID { get; set; }

        public string ToFileName() { return string.Format("{0};{1};{2}", QUALIFIER_TEXT, TeacherName, ID); }

        public static ClassInformationNameComposite ParseClassInformation(ClassInformation classInformation)
        {
            var nameComposite = new ClassInformationNameComposite
                                {
                                    ID = classInformation.ID,
                                    TeacherName = classInformation.Teacher.FullName
                                };

            return nameComposite;
        }

        public static ClassInformationNameComposite ParseFilePath(string classInformationFilePath)
        {
            var classInformationFileName = Path.GetFileNameWithoutExtension(classInformationFilePath);
            var classInformationFileNameParts = classInformationFileName.Split(';');
            if (classInformationFileNameParts.Length != 3)
            {
                return null;
            }

            var nameComposite = new ClassInformationNameComposite
                                {
                                    TeacherName = classInformationFileNameParts[1],
                                    ID = classInformationFileNameParts[2]
                                };

            return nameComposite;
        }
    }

    [Serializable]
    public class ClassInformation : AEntityBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ClassInformation" /> from scratch.</summary>
        public ClassInformation()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="ClassInformation" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ClassInformation(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ClassInformation" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Name of the <see cref="ClassInformation" />.</summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof (string), string.Empty);

        /// <summary>Grade Level to which the <see cref="ClassInformation" /> is taught.</summary>
        public string GradeLevel
        {
            get { return GetValue<string>(GradeLevelProperty); }
            set { SetValue(GradeLevelProperty, value); }
        }

        public static readonly PropertyData GradeLevelProperty = RegisterProperty("GradeLevel", typeof (string), string.Empty);

        /// <summary>Start date of the <see cref="ClassInformation" />.</summary>
        public DateTime? StartDate
        {
            get { return GetValue<DateTime?>(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly PropertyData StartDateProperty = RegisterProperty("StartDate", typeof (DateTime?));

        /// <summary>End date of the <see cref="ClassInformation" />.</summary>
        public DateTime? EndDate
        {
            get { return GetValue<DateTime?>(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly PropertyData EndDateProperty = RegisterProperty("EndDate", typeof (DateTime?));

        /// <summary>Name of the school.</summary>
        public string SchoolName
        {
            get { return GetValue<string>(SchoolNameProperty); }
            set { SetValue(SchoolNameProperty, value); }
        }

        public static readonly PropertyData SchoolNameProperty = RegisterProperty("SchoolName", typeof (string), string.Empty);

        /// <summary>Name of the school district.</summary>
        public string SchoolDistrict
        {
            get { return GetValue<string>(SchoolDistrictProperty); }
            set { SetValue(SchoolDistrictProperty, value); }
        }

        public static readonly PropertyData SchoolDistrictProperty = RegisterProperty("SchoolDistrict", typeof (string), string.Empty);

        /// <summary>Name of the city in which the school exists.</summary>
        public string City
        {
            get { return GetValue<string>(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly PropertyData CityProperty = RegisterProperty("City", typeof (string), string.Empty);

        /// <summary>Name of the state in which the school exists.</summary>
        public string State
        {
            get { return GetValue<string>(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly PropertyData StateProperty = RegisterProperty("State", typeof (string), string.Empty);

        #region Navigation Properties

        /// <summary>Unique Identifier of the <see cref="Person" /> teaching the <see cref="ClassInformation" />.</summary>
        /// <remarks>Foreign Key.</remarks>
        public string TeacherID
        {
            get { return GetValue<string>(TeacherIDProperty); }
            set { SetValue(TeacherIDProperty, value); }
        }

        public static readonly PropertyData TeacherIDProperty = RegisterProperty("TeacherID", typeof (string), string.Empty);

        /// <summary>The <see cref="Person" /> teaching the <see cref="ClassInformation" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual Person Teacher
        {
            get { return GetValue<Person>(TeacherProperty); }
            set
            {
                SetValue(TeacherProperty, value);
                if (value == null)
                {
                    return;
                }
                TeacherID = value.ID;
            }
        }

        public static readonly PropertyData TeacherProperty = RegisterProperty("Teacher", typeof (Person));

        /// <summary>Unique Identifier of the <see cref="Person" /> projector in the <see cref="ClassInformation" />.</summary>
        /// <remarks>Foreign Key.</remarks>
        public string ProjectorID
        {
            get { return GetValue<string>(ProjectorIDProperty); }
            set { SetValue(ProjectorIDProperty, value); }
        }

        public static readonly PropertyData ProjectorIDProperty = RegisterProperty("ProjectorID", typeof (string), string.Empty);

        /// <summary>The <see cref="Person" /> projector in the <see cref="ClassInformation" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual Person Projector
        {
            get { return GetValue<Person>(ProjectorProperty); }
            set
            {
                SetValue(ProjectorProperty, value);
                if (value == null)
                {
                    return;
                }
                ProjectorID = value.ID;
            }
        }

        public static readonly PropertyData ProjectorProperty = RegisterProperty("Projector", typeof (Person));

        /// <summary>List of all the Students in the <see cref="ClassInformation" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual ObservableCollection<Person> StudentList
        {
            get { return GetValue<ObservableCollection<Person>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof (ObservableCollection<Person>), () => new ObservableCollection<Person>());

        #endregion //Navigation Properties

        #endregion //Properties

        #region Cache

        public void ToXML(string classInformationFilePath)
        {
            var fileInfo = new FileInfo(classInformationFilePath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            // TODO: Use ModelBaseExtensions.serialization.cs extention method SaveAsXml(this ModelBase model, string filePath)
            using (Stream stream = new FileStream(classInformationFilePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public void SaveToXML(string folderPath)
        {
            var nameComposite = ClassInformationNameComposite.ParseClassInformation(this);
            var possiblePreExistingFiles =
                Directory.EnumerateFiles(folderPath, string.Format("{0};*;{1}.xml", ClassInformationNameComposite.QUALIFIER_TEXT, nameComposite.ID)).ToList();
            foreach (var oldFilePath in possiblePreExistingFiles)
            {
                File.Delete(oldFilePath);
            }

            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");
            ToXML(filePath);
        }

        public static ClassInformation LoadFromXML(string classInformationFilePath)
        {
            try
            {
                var nameComposite = ClassInformationNameComposite.ParseFilePath(classInformationFilePath);
                if (nameComposite == null)
                {
                    return null;
                }

                var classInformation = Load<ClassInformation>(classInformationFilePath, SerializationMode.Xml);
                if (classInformation == null)
                {
                    return null;
                }

                classInformation.ID = nameComposite.ID;

                return classInformation;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion //Cache
    }
}