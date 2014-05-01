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
    [Serializable]
    public class ClassPeriod : AEntityBase
    {
        #region Constructors

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
        /// List of the <see cref="CLPPage" /> IDs for the <see cref="ClassPeriod" />.
        /// </summary>
        public List<string> PageIDs
        {
            get { return GetValue<List<string>>(PageIDsProperty); }
            set { SetValue(PageIDsProperty, value); }
        }

        public static readonly PropertyData PageIDsProperty = RegisterProperty("PageIDs", typeof(List<string>), () => new List<string>());

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

        public void ToXML(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if(!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using(Stream stream = new FileStream(fileName, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public void SaveClassPeriod(string folderPath)
        {
            var fileName = Path.Combine(folderPath, "period;" + StartTime.ToString("yyyy.M.dd.HH.mm") + ";" + ID + ".xml");
            ToXML(fileName);
        }

        public static ClassPeriod OpenClassPeriod(string filePath)
        {
            try
            {
                var classPeriod = Load<ClassPeriod>(filePath, SerializationMode.Xml);
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