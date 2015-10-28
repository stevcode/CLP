using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public class ClassPeriodNameComposite
    {
        public const string QUALIFIER_TEXT = "period";
        public string StartTime { get; set; }
        public string ID { get; set; }
        public string PageNumbers { get; set; }
        public string AllowedBlankPages { get; set; }

        public string ToFileName() { return string.Format("{0};{1};{2};{3};{4}", QUALIFIER_TEXT, StartTime, ID, PageNumbers, AllowedBlankPages); }

        public static ClassPeriodNameComposite ParseClassPeriod(ClassPeriod classPeriod)
        {
            var nameComposite = new ClassPeriodNameComposite
                                {
                                    StartTime = classPeriod.StartTime.ToString("yyyy.M.dd.HH.mm"),
                                    ID = classPeriod.ID,
                                    PageNumbers = classPeriod.PageNumbers,
                                    AllowedBlankPages = classPeriod.NumberOfAllowedBlankPages.ToString(),
                                };

            return nameComposite;
        }

        public static ClassPeriodNameComposite ParseFilePath(string filePath)
        {
            var classPeriodFileName = Path.GetFileNameWithoutExtension(filePath);
            var classPeriodFileNameParts = classPeriodFileName.Split(';');
            if (classPeriodFileNameParts.Length != 5)
            {
                return null;
            }

            var nameComposite = new ClassPeriodNameComposite
                                {
                                    StartTime = classPeriodFileNameParts[1],
                                    ID = classPeriodFileNameParts[2],
                                    PageNumbers = classPeriodFileNameParts[3],
                                    AllowedBlankPages = classPeriodFileNameParts[4]
                                };

            return nameComposite;
        }
    }

    [Serializable]
    public class ClassPeriod : AEntityBase
    {
        #region Constructors

        public ClassPeriod(ClassInformation classInformation, string notebookID, DateTime startTime, string pageNumbers, uint numberOfAllowedBlankPages)
            : this()
        {
            ClassInformation = classInformation;
            NotebookID = notebookID;
            StartTime = startTime;
            PageNumbers = pageNumbers;
            NumberOfAllowedBlankPages = numberOfAllowedBlankPages;
        }

        /// <summary>Initializes <see cref="ClassPeriod" /> from scratch.</summary>
        public ClassPeriod()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="ClassPeriod" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ClassPeriod(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="ClassPeriod" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Unique Identifier of the <see cref="Notebook" /> used during this <see cref="ClassPeriod" />.</summary>
        public string NotebookID
        {
            get { return GetValue<string>(NotebookIDProperty); }
            set { SetValue(NotebookIDProperty, value); }
        }

        public static readonly PropertyData NotebookIDProperty = RegisterProperty("NotebookID", typeof (string));

        /// <summary>Start Time and Date of the <see cref="ClassPeriod" />.</summary>
        public DateTime StartTime
        {
            get { return GetValue<DateTime>(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly PropertyData StartTimeProperty = RegisterProperty("StartTime", typeof (DateTime));

        /// <summary>Range of page numbers to include in the class period.</summary>
        public string PageNumbers
        {
            get { return GetValue<string>(PageNumbersProperty); }
            set { SetValue(PageNumbersProperty, value); }
        }

        public static readonly PropertyData PageNumbersProperty = RegisterProperty("PageNumbers", typeof (string), string.Empty);

        /// <summary>Number of Blank Pages a student can self generate.</summary>
        public uint NumberOfAllowedBlankPages
        {
            get { return GetValue<uint>(NumberOfAllowedBlankPagesProperty); }
            set { SetValue(NumberOfAllowedBlankPagesProperty, value); }
        }

        public static readonly PropertyData NumberOfAllowedBlankPagesProperty = RegisterProperty("NumberOfAllowedBlankPages", typeof (uint));

        #region Navigation Properties

        /// <summary>Unique Identifier of the <see cref="ClassInformation" /> of the <see cref="ClassPeriod" />.</summary>
        /// <remarks>Foreign Key.</remarks>
        public string ClassInformationID
        {
            get { return GetValue<string>(ClassInformationIDProperty); }
            set { SetValue(ClassInformationIDProperty, value); }
        }

        public static readonly PropertyData ClassInformationIDProperty = RegisterProperty("ClassInformationID", typeof (string));

        /// <summary>
        ///     <see cref="ClassInformation" /> of the <see cref="ClassPeriod" />
        /// </summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ClassInformation ClassInformation
        {
            get { return GetValue<ClassInformation>(ClassInformationProperty); }
            set
            {
                SetValue(ClassInformationProperty, value);
                if (value == null)
                {
                    return;
                }
                ClassInformationID = value.ID;
            }
        }

        public static readonly PropertyData ClassInformationProperty = RegisterProperty("ClassInformation", typeof (ClassInformation));

        #endregion //Navigation Properties

        #endregion //Properties

        #region Cache

        public void ToXML(string classPeriodFilePath)
        {
            var fileInfo = new FileInfo(classPeriodFilePath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(classPeriodFilePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public void SaveToXML(string folderPath)
        {
            var nameComposite = ClassPeriodNameComposite.ParseClassPeriod(this);
            var possiblePreExistingFiles =
                Directory.EnumerateFiles(folderPath, string.Format("{0};*;{1};*.xml", ClassPeriodNameComposite.QUALIFIER_TEXT, nameComposite.ID)).ToList();
            foreach (var oldFilePath in possiblePreExistingFiles)
            {
                File.Delete(oldFilePath);
            }

            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");
            ToXML(filePath);
        }

        public static ClassPeriod LoadFromXML(string classPeriodFilePath)
        {
            try
            {
                var nameComposite = ClassPeriodNameComposite.ParseFilePath(classPeriodFilePath);
                if (nameComposite == null)
                {
                    return null;
                }

                var classPeriod = Load<ClassPeriod>(classPeriodFilePath, SerializationMode.Xml);
                if (classPeriod == null)
                {
                    return null;
                }

                var time = nameComposite.StartTime;
                var timeParts = time.Split('.');
                var year = Int32.Parse(timeParts[0]);
                var month = Int32.Parse(timeParts[1]);
                var day = Int32.Parse(timeParts[2]);
                var hour = Int32.Parse(timeParts[3]);
                var minute = Int32.Parse(timeParts[4]);
                var dateTime = new DateTime(year, month, day, hour, minute, 0);

                classPeriod.StartTime = dateTime;
                classPeriod.ID = nameComposite.ID;
                classPeriod.PageNumbers = nameComposite.PageNumbers;
                classPeriod.NumberOfAllowedBlankPages = UInt32.Parse(nameComposite.AllowedBlankPages);

                var classPeriodFolderPath = Path.GetDirectoryName(classPeriodFilePath);
                var classInformationFilePath =
                    Directory.EnumerateFiles(classPeriodFolderPath, string.Format("{0};*;{1}.xml", ClassInformationNameComposite.QUALIFIER_TEXT, classPeriod.ClassInformationID))
                             .FirstOrDefault();
                if (string.IsNullOrEmpty(classInformationFilePath))
                {
                    return null;
                }
                var classInformation = ClassInformation.LoadFromXML(classInformationFilePath);
                if (classInformation == null)
                {
                    return null;
                }
                classPeriod.ClassInformation = classInformation;

                return classPeriod;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion //Cache
    }
}