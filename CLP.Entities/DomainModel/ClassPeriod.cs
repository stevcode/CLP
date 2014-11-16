using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Path = Catel.IO.Path;

namespace CLP.Entities
{
    public class ClassPeriodNameComposite
    {
        public string FullClassPeriodFilePath { get; set; }
        public string ID { get; set; }
        public string StartTime { get; set; }
        public string StartPageID { get; set; }
        public string NumberOfPages { get; set; }
        public string AllowedBlankPages { get; set; }
        public bool IsLocal { get; set; }

        public string ToFileName()
        {
            return string.Format("period;{0};{1};{2};{3};{4}",
            ID, StartTime, StartPageID, NumberOfPages, AllowedBlankPages);
        }

        public static ClassPeriodNameComposite ParseClassPeriodToNameComposite(ClassPeriod classPeriod)
        {
            var nameComposite = new ClassPeriodNameComposite
                                {
                                    ID = classPeriod.ID,
                                    StartTime = classPeriod.StartTime.ToString("yyyy.M.dd.HH.mm"),
                                    StartPageID = classPeriod.StartPageID,
                                    NumberOfPages = classPeriod.NumberOfPages.ToString(),
                                    AllowedBlankPages = classPeriod.NumberOfAllowedBlankPages.ToString(),
                                    IsLocal = true
                                };

            return nameComposite;
        }

        public static ClassPeriodNameComposite ParseFilePathToNameComposite(string filePath)
        {
            var directoryInfo = new DirectoryInfo(filePath);
            var classPeriodDirectoryName = directoryInfo.Name;
            var classPeriodDirectoryParts = classPeriodDirectoryName.Split(';');
            if (classPeriodDirectoryParts.Length != 6)
            {
                return null;
            }

            var nameComposite = new ClassPeriodNameComposite
            {
                FullClassPeriodFilePath = filePath,
                ID = classPeriodDirectoryParts[1],
                StartTime = classPeriodDirectoryParts[2],
                StartPageID = classPeriodDirectoryParts[3],
                NumberOfPages = classPeriodDirectoryParts[4],
                AllowedBlankPages = classPeriodDirectoryParts[5],
                IsLocal = true
            };

            return nameComposite;
        }
    }

    [Serializable]
    public class ClassPeriod : AEntityBase
    {
        #region Constructors

        public ClassPeriod(ClassSubject classSubject, string notebookID, DateTime startTime, string titlePageID, string startPageID, uint numberOfPages, uint numberOfAllowedBlankPages)
            :this()
        {
            ClassSubject = classSubject;
            NotebookID = notebookID;
            StartTime = startTime;
            TitlePageID = titlePageID;
            StartPageID = startPageID;
            NumberOfPages = numberOfPages;
            NumberOfAllowedBlankPages = numberOfAllowedBlankPages;
        }

        /// <summary>
        /// Initializes <see cref="ClassPeriod" /> from scratch.
        /// </summary>
        public ClassPeriod() { ID = Guid.NewGuid().ToCompactID(); }

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
        /// ID for the notebook's Title Page to insure it is always loaded.
        /// </summary>
        public string TitlePageID
        {
            get { return GetValue<string>(TitlePageIDProperty); }
            set { SetValue(TitlePageIDProperty, value); }
        }

        public static readonly PropertyData TitlePageIDProperty = RegisterProperty("TitlePageID", typeof (string));

        /// <summary>
        /// Start Time and Date of the <see cref="ClassPeriod" />.
        /// </summary>
        public DateTime StartTime
        {
            get { return GetValue<DateTime>(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly PropertyData StartTimeProperty = RegisterProperty("StartTime", typeof(DateTime));

        /// <summary>
        /// Unique Identifier of the <see cref="Notebook" /> used during this <see cref="ClassPeriod" />.
        /// </summary>
        public string NotebookID
        {
            get { return GetValue<string>(NotebookIDProperty); }
            set { SetValue(NotebookIDProperty, value); }
        }

        public static readonly PropertyData NotebookIDProperty = RegisterProperty("NotebookID", typeof(string));

        /// <summary>
        /// ID of the page to start with.
        /// </summary>
        public string StartPageID
        {
            get { return GetValue<string>(StartPageIDProperty); }
            set { SetValue(StartPageIDProperty, value); }
        }

        public static readonly PropertyData StartPageIDProperty = RegisterProperty("StartPageID", typeof (string));

        /// <summary>
        /// Number of pages, including Start Page to include in the ClassPeriod.
        /// </summary>
        public uint NumberOfPages
        {
            get { return GetValue<uint>(NumberOfPagesProperty); }
            set { SetValue(NumberOfPagesProperty, value); }
        }

        public static readonly PropertyData NumberOfPagesProperty = RegisterProperty("NumberOfPages", typeof (uint));

        /// <summary>
        /// Number of Blank Pages a student can self generate.
        /// </summary>
        public uint NumberOfAllowedBlankPages
        {
            get { return GetValue<uint>(NumberOfAllowedBlankPagesProperty); }
            set { SetValue(NumberOfAllowedBlankPagesProperty, value); }
        }

        public static readonly PropertyData NumberOfAllowedBlankPagesProperty = RegisterProperty("NumberOfAllowedBlankPages", typeof (uint));

        #region Navigation Properties

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

        public static readonly PropertyData ClassSubjectIDProperty = RegisterProperty("ClassSubjectID", typeof(string));

        /// <summary>
        /// <see cref="ClassSubject" /> of the <see cref="ClassPeriod" />
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ClassSubject ClassSubject
        {
            get { return GetValue<ClassSubject>(ClassSubjectProperty); }
            set
            {
                SetValue(ClassSubjectProperty, value);
                if(value == null)
                {
                    return;
                }
                ClassSubjectID = value.ID;
            }
        }

        public static readonly PropertyData ClassSubjectProperty = RegisterProperty("ClassSubject", typeof(ClassSubject));

        #endregion //Navigation Properties

        #endregion //Properties

        #region Cache

        public void ToXML(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if(!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using(Stream stream = new FileStream(filePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public void SaveClassPeriodLocally(string folderPath)
        {
            var nameComposite = ClassPeriodNameComposite.ParseClassPeriodToNameComposite(this);
            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");
            ToXML(filePath);
        }

        public static ClassPeriod OpenClassPeriod(string filePath)
        {
            try
            {
                var classPeriod = Load<ClassPeriod>(filePath, SerializationMode.Xml);
                var nameComposite = ClassPeriodNameComposite.ParseFilePathToNameComposite(filePath);
                if (nameComposite == null)
                {
                    return null;
                }
                classPeriod.ID = nameComposite.ID;
                classPeriod.StartTime = DateTime.Parse(nameComposite.StartTime);
                classPeriod.StartPageID = nameComposite.StartPageID;
                classPeriod.NumberOfPages = UInt32.Parse(nameComposite.NumberOfPages);
                classPeriod.NumberOfAllowedBlankPages = UInt32.Parse(nameComposite.AllowedBlankPages);

                var classPeriodFolderPath = Path.GetDirectoryName(filePath);
                var classSubjectFilePath = Path.Combine(classPeriodFolderPath, "subject" + ";" + classPeriod.ClassSubjectID + ".xml");
                classPeriod.ClassSubject = ClassSubject.OpenClassSubject(classSubjectFilePath);

                return classPeriod;
            }
            catch(Exception)
            {
                return null;
            }
        }

        #endregion //Cache
    }
}