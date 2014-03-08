using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Xml;
using System.Xml.Serialization;
using Catel.Data;
using Path = Catel.IO.Path;

namespace CLP.Models
{
    /// <summary>
    /// Abstract base ICLPPage object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// 
    /// KnownTypes allow ICLPPageObjects to be (de)serialized via DataContracts
    /// for transmission over network calls.
    /// </summary>
    [Serializable,
    KnownType(typeof(CLPAggregationDataTable)),
    KnownType(typeof(CLPArray)),
    KnownType(typeof(CLPAudio)),
    KnownType(typeof(CLPImage)),
    KnownType(typeof(CLPShape)),
    KnownType(typeof(CLPStamp)),
    KnownType(typeof(CLPStampCopy)),
    KnownType(typeof(CLPTextBox)),
    KnownType(typeof(CLPDataTable)),
    KnownType(typeof(CLPGroupingRegion)),
    KnownType(typeof(CLPHandwritingRegion)),
    KnownType(typeof(CLPInkShapeRegion)),
    KnownType(typeof(CLPShadingRegion)),
    KnownType(typeof(StrokeDTO)),
    KnownType(typeof(StylusPointDTO)),
    KnownType(typeof(DrawingAttributesDTO))]
    [AllowNonSerializableMembers]
    abstract public class ACLPPageBase : ModelBase, ICLPPage
    {
        #region Variables

        public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");

        public const double LANDSCAPE_HEIGHT = 816;
        public const double LANDSCAPE_WIDTH = 1056;
        public const double PORTRAIT_HEIGHT = 1056;
        public const double PORTRAIT_WIDTH = 816;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        protected ACLPPageBase()
            : this(LANDSCAPE_HEIGHT, LANDSCAPE_WIDTH) {}

        protected ACLPPageBase(double pageHeight, double pageWidth)
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            PageHeight = pageHeight;
            PageWidth = pageWidth;
            InitialPageAspectRatio = PageWidth / PageHeight;
            AddInkWithoutHistory = false;
            PageHistory = new CLPHistory();

            //Initialize page tags to contain correctness and starred tags with values of unknown and unstarred
            Tag correctnessTag = new Tag(Tag.Origins.Teacher, CorrectnessTagType.Instance);
            Tag starredTag = new Tag(Tag.Origins.Teacher, StarredTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Unknown", ""));
            starredTag.AddTagOptionValue(new TagOptionValue("Unstarred", "..\\Images\\Unstarred.png"));
            PageTags.Add(correctnessTag);
            PageTags.Add(starredTag);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ACLPPageBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        protected ACLPPageBase(string pageXMLFilePath)
        {
            var reader = new XmlTextReader(pageXMLFilePath)
                         {
                             WhitespaceHandling = WhitespaceHandling.None
                         };

            while(reader.Read())
            {
                if(reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch(reader.Name)
                {
                    case "CreationDate":
                        CreationDate = Convert.ToDateTime(reader.ReadString());
                        break;
                    case "UniqueID":
                        UniqueID = reader.ReadString();
                        break;
                    case "ParentNotebookID":
                        ParentNotebookID = reader.ReadString();
                        break;
                    case "SubmissionType":
                        var submissionType = reader.ReadString();
                        switch(submissionType)
                        {
                            case "None":
                                SubmissionType = SubmissionType.None;
                                break;
                            case "Single":
                                SubmissionType = SubmissionType.Single;
                                break;
                            case "Group":
                                SubmissionType = SubmissionType.Group;
                                break;
                        }
                        break;
                    case "SubmissionTime":
                        if(!reader.IsEmptyElement)
                        {
                            SubmissionTime = Convert.ToDateTime(reader.ReadString());
                        }
                        break;
                    case "SubmissionID":
                        if(!reader.IsEmptyElement)
                        {
                            SubmissionID = reader.ReadString();
                        }
                        break;
                    case "Submitter":
                        if(!reader.IsEmptyElement)
                        {
                            var submitter = new Person();
                            reader.Read();
                            reader.MoveToContent();
                            submitter.UniqueID = reader.ReadElementContentAsString();

                            reader.MoveToContent();
                            submitter.FullName = reader.ReadElementContentAsString();

                            reader.MoveToContent();
                            submitter.GroupName = reader.ReadElementContentAsString();

                            reader.MoveToContent();
                            submitter.CurrentMachineName = reader.ReadElementContentAsString();

                            reader.MoveToContent();
                            submitter.CurrentMachineAddress = reader.ReadElementContentAsString();
                            Submitter = submitter;
                        }
                        break;
                    case "GroupSubmitter":
                        if(!reader.IsEmptyElement)
                        {
                            var group = new Group();
                            reader.Read();
                            reader.MoveToContent();
                            group.GroupName = reader.ReadElementContentAsString();

                            reader.MoveToContent();
                            group.GroupID = reader.ReadElementContentAsString();
                            GroupSubmitter = group;
                        }
                        break;
                    case "PageIndex":
                        PageIndex = reader.ReadElementContentAsInt();        //Convert.ToInt32(reader.ReadString());
                        break;
                    case "NumberOfSubmissions":
                        NumberOfSubmissions = Convert.ToInt32(reader.ReadString());
                        break;
                    case "NumberOfGroupSubmissions":
                        NumberOfGroupSubmissions = Convert.ToInt32(reader.ReadString());
                        break;
                    //case "Tag":
                    //    Tag tag = null;
                    //    TagType tagTypeInstance = null;
                    //    var tagType = reader.GetAttribute("TagType");
                    //    switch(tagType)
                    //    {
                    //        case "Correctness":
                    //            tagTypeInstance = CorrectnessTagType.Instance;
                    //            break;
                    //        case "Starred":
                    //            tagTypeInstance = StarredTagType.Instance;
                    //            break;
                    //        case "PageTopic":
                    //            tagTypeInstance = PageTopicTagType.Instance;
                    //            break;
                    //    }
                    //    var originType = reader.GetAttribute("Origin");
                    //    switch(originType)
                    //    {
                    //        case "Author":
                    //            tag = new Tag(Tag.Origins.Author, tagTypeInstance);
                    //            break;
                    //        case "Generated":
                    //            tag = new Tag(Tag.Origins.Generated, tagTypeInstance);
                    //            break;
                    //        case "Teacher":
                    //            tag = new Tag(Tag.Origins.Teacher, tagTypeInstance);
                    //            break;
                    //    }
                    //    break;
                    case "GroupSubmitType":
                        var groupSubmitType = reader.ReadString();
                        switch(groupSubmitType)
                        {
                            case "Deny":
                                GroupSubmitType = GroupSubmitType.Deny;
                                break;
                            case "Allow":
                                GroupSubmitType = GroupSubmitType.Allow;
                                break;
                            case "Force":
                                GroupSubmitType = GroupSubmitType.Force;
                                break;
                        }
                        break;
                    case "PageHeight":
                        PageHeight = Convert.ToDouble(reader.ReadString());
                        break;
                    case "PageWidth":
                        PageWidth = Convert.ToDouble(reader.ReadString());
                        break;
                    case "InitialAspectRatio":
                        InitialPageAspectRatio = Convert.ToDouble(reader.ReadString());
                        break;
                    case "ImagePool":
                        //TODO ***********************
                        break;
                    case "Stroke":
                        var stroke = ImportFromXML.ParseStroke(reader);
                        if(stroke != null)
                        {
                            SerializedStrokes.Add(stroke);
                        }
                        break;  
                    case "PageObject":
                        var pageObject = ImportFromXML.ParsePageObject(reader, this);
                        if(pageObject != null)
                        {
                            PageObjects.Add(pageObject);
                        }
                        break;
                }
            }
            reader.Close();

            var pageHistoryXMLDirectory = Path.GetDirectoryName(pageXMLFilePath);
            var pageHistoryXMLFileName = System.IO.Path.GetFileNameWithoutExtension(pageXMLFilePath) + " History.xml";
            var pageHistoryXMLFilePath = Path.Combine(pageHistoryXMLDirectory, pageHistoryXMLFileName);

            PageHistory = File.Exists(pageHistoryXMLFilePath) ? new CLPHistory(pageHistoryXMLFilePath, this) : new CLPHistory();
            InkStrokes = StrokeDTO.LoadInkStrokes(SerializedStrokes);
        }

        #endregion //Constructors

        #region ICLPPage Properties

        /// <summary>
        /// JPG Thumbnail of the page for use in submissions panel.
        /// </summary>
        public byte[] PageThumbnail
        {
            get { return GetValue<byte[]>(PageThumbnailProperty); }
            set { SetValue(PageThumbnailProperty, value); }
        }

        public static readonly PropertyData PageThumbnailProperty = RegisterProperty("PageThumbnail", typeof(byte[]));

        /// <summary>
        /// Exact time and date the page was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// UniqueID of the page.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// UniqueID of the Notebook this page is part of.
        /// </summary>
        public string ParentNotebookID
        {
            get { return GetValue<string>(ParentNotebookIDProperty); }
            set { SetValue(ParentNotebookIDProperty, value); }
        }

        public static readonly PropertyData ParentNotebookIDProperty = RegisterProperty("ParentNotebookID", typeof(string));

        /// <summary>
        /// The manner in which the page was submitted. None for an unsubmitted page, Single if
        /// an individual submitted the page, or Group if the page was submitted by a group.
        /// </summary>
        public SubmissionType SubmissionType
        {
            get { return GetValue<SubmissionType>(SubmissionTypeProperty); }
            set { SetValue(SubmissionTypeProperty, value); }
        }

        public static readonly PropertyData SubmissionTypeProperty = RegisterProperty("SubmissionType", typeof(SubmissionType), SubmissionType.None);

        /// <summary>
        /// Time the page was submitted.
        /// </summary>
        public DateTime SubmissionTime
        {
            get { return GetValue<DateTime>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof(DateTime));

        /// <summary>
        /// Unique submission ID for submitted pages.
        /// </summary>
        public string SubmissionID
        {
            get { return GetValue<string>(SubmissionIDProperty); }
            set { SetValue(SubmissionIDProperty, value); }
        }

        public static readonly PropertyData SubmissionIDProperty = RegisterProperty("SubmissionID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// The Person that submitted the page.
        /// </summary>
        public Person Submitter
        {
            get { return GetValue<Person>(SubmitterProperty); }
            set { SetValue(SubmitterProperty, value); }
        }

        public static readonly PropertyData SubmitterProperty = RegisterProperty("Submitter", typeof(Person));

        /// <summary>
        /// The Group that submitted the page.
        /// </summary>
        public Group GroupSubmitter
        {
            get { return GetValue<Group>(GroupSubmitterProperty); }
            set { SetValue(GroupSubmitterProperty, value); }

        }

        public static readonly PropertyData GroupSubmitterProperty = RegisterProperty("GroupSubmitter", typeof(Group));

        /// <summary>
        /// Index of the Page within the Notebook.
        /// </summary>
        public int PageIndex
        {
            get { return GetValue<int>(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }

        public static readonly PropertyData PageIndexProperty = RegisterProperty("PageIndex", typeof(int), 0);

        /// <summary>
        /// Number of unique individual submissions this page has associated with it.
        /// </summary>
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); }
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int), 0);

        /// <summary>
        /// Number of group submissions this page has associated with it.
        /// </summary>
        public int NumberOfGroupSubmissions
        {
            get { return GetValue<int>(NumberOfGroupSubmissionsProperty); }
            set { SetValue(NumberOfGroupSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfGroupSubmissionsProperty = RegisterProperty("NumberOfGroupSubmissions", typeof(int), 0);

        /// <summary>
        /// Tags associated with the page. Either Author generated,
        /// or automatically generated.
        /// </summary>
        public ObservableCollection<Tag> PageTags
        {
            get { return GetValue<ObservableCollection<Tag>>(PageTagsProperty); }
            set { SetValue(PageTagsProperty, value); }
        }

        public static readonly PropertyData PageTagsProperty = RegisterProperty("PageTags", typeof(ObservableCollection<Tag>), () => new ObservableCollection<Tag>());

        /// <summary>
        /// Availability of Group Submit option for a page.
        /// </summary>
        public GroupSubmitType GroupSubmitType
        {
            get { return GetValue<GroupSubmitType>(GroupSubmitTypeProperty); }
            set { SetValue(GroupSubmitTypeProperty, value); }
        }

        public static readonly PropertyData GroupSubmitTypeProperty = RegisterProperty("GroupSubmitType", typeof(GroupSubmitType), GroupSubmitType.Deny);

        /// <summary>
        /// Height of the page in pixels.
        /// </summary>
        public double PageHeight
        {
            get { return GetValue<double>(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        public static readonly PropertyData PageHeightProperty = RegisterProperty("PageHeight", typeof(double), LANDSCAPE_HEIGHT);

        /// <summary>
        /// Width of the page in pixels.
        /// </summary>
        public double PageWidth
        {
            get { return GetValue<double>(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        public static readonly PropertyData PageWidthProperty = RegisterProperty("PageWidth", typeof(double), LANDSCAPE_WIDTH);

        /// <summary>
        /// Aspect Ratio of the page = PageWidth / PageHeight.
        /// </summary>
        public double InitialPageAspectRatio
        {
            get { return GetValue<double>(InitialPageAspectRatioProperty); }
            set { SetValue(InitialPageAspectRatioProperty, value); }
        }

        public static readonly PropertyData InitialPageAspectRatioProperty = RegisterProperty("InitialPageAspectRatio", typeof(double));

        /// <summary>
        /// Pool of Images used on a page, so that duplications don't occur
        /// UniqueID, ByteSource
        /// </summary>
        public Dictionary<string, List<byte>> ImagePool
        {
            get { return GetValue<Dictionary<string, List<byte>>>(ImagePoolProperty); }
            set { SetValue(ImagePoolProperty, value); }
        }

        public static readonly PropertyData ImagePoolProperty = RegisterProperty("ImagePool", typeof(Dictionary<string, List<byte>>), () => new Dictionary<string, List<byte>>());

        /// <summary>
        /// Ink Strokes serialized via Data Transfer Object, StrokeDTO.
        /// </summary>
        public ObservableCollection<StrokeDTO> SerializedStrokes
        {
            get { return GetValue<ObservableCollection<StrokeDTO>>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(ObservableCollection<StrokeDTO>), () => new ObservableCollection<StrokeDTO>());

        /// <summary>
        /// Deserialized Ink Strokes.
        /// </summary>
        [XmlIgnore]
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            set { SetValue(InkStrokesProperty, value); }
        }

        [NonSerialized]
        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection), () => new StrokeCollection(), includeInSerialization: false);

        public bool AddInkWithoutHistory { get; set; } //TODO: Do Not Serialize

        /// <summary>
        /// Gets a list of pageObjects on the page.
        /// </summary>
        public ObservableCollection<ICLPPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<ICLPPageObject>), () => new ObservableCollection<ICLPPageObject>());

        /// <summary>
        /// The stored interaction history for the page.
        /// </summary>
        public CLPHistory PageHistory
        {
            get { return GetValue<CLPHistory>(PageHistoryProperty); }
            set { SetValue(PageHistoryProperty, value); }
        }

        public static readonly PropertyData PageHistoryProperty = RegisterProperty("PageHistory", typeof(CLPHistory));

        #endregion //ICLPPage Properties

        #region ICLPPage Methods

        public abstract ICLPPage DuplicatePage();

        public virtual ICLPPageObject GetPageObjectByUniqueID(string uniqueID)
        {
            return PageObjects.FirstOrDefault(pageObject => pageObject.UniqueID.Equals(uniqueID));
        }

        public virtual Stroke GetStrokeByStrokeID(string strokeID)
        {
            return InkStrokes.FirstOrDefault(stroke => stroke.GetStrokeUniqueID() == strokeID);
        }

        public virtual void TrimPage()
        {
            double lowestY = PageObjects.Select(pageObject => pageObject.YPosition + pageObject.Height).Concat(new double[] { 0 }).Max();
            foreach(Rect bounds in InkStrokes.Select(s => s.GetBounds()))
            {
                if(bounds.Bottom >= PageHeight)
                {
                    lowestY = Math.Max(lowestY, PageHeight);
                    break;
                }
                lowestY = Math.Max(lowestY, bounds.Bottom);
            }

            double defaultHeight = Math.Abs(PageWidth - LANDSCAPE_WIDTH) < .000001 ? LANDSCAPE_HEIGHT : PORTRAIT_HEIGHT;

            double newHeight = Math.Max(defaultHeight, lowestY);
            if(newHeight < PageHeight)
            {
                PageHeight = newHeight;
            }   
        }

        #endregion //ICLPPage Methods

        #region Methods

        [OnSerializing]
        void OnSerializing(StreamingContext sc)
        {
            if(InkStrokes != null)
            {
                SerializedStrokes = StrokeDTO.SaveInkStrokes(InkStrokes);
            }
        }

        public static void Deserialize(ICLPPage page)
        {
            page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);

            foreach(var pageObject in page.PageObjects)
            {
                pageObject.ParentPage = page;
            }
            foreach(var clpHistoryItem in page.PageHistory.UndoItems)
            {
                clpHistoryItem.ParentPage = page;
            }
            foreach(var clpHistoryItem in page.PageHistory.RedoItems)
            {
                clpHistoryItem.ParentPage = page;
            }
        }

        #endregion //Methods
    }
}
