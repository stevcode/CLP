using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel;
using Catel.Data;

namespace CLP.Models
{
    public enum CLPPageOrientation
    {
        Portrait,
        Landscape,
        Custom
    }

    public enum GroupSubmitType
    {
        Deny,
        Allow,
        Force
    }

    public enum PageTypeEnum
    {
        CLPPage,
        CLPProofPage
    }

    /// <summary>
    /// CLPPage Data object class which fully supports serialization, property changed notifications,
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
    KnownType(typeof(CLPSnapTileContainer)),
    KnownType(typeof(CLPStamp)),
    KnownType(typeof(CLPStrokePathContainer)),
    KnownType(typeof(CLPTextBox)),
    KnownType(typeof(CLPDataTable)),
    KnownType(typeof(CLPGroupingRegion)),
    KnownType(typeof(CorrectnessTagType)),
    KnownType(typeof(StarredTagType)),
    KnownType(typeof(PageTopicTagType)),
    KnownType(typeof(DomainInterpretationTagType)),
    KnownType(typeof(CLPHandwritingRegion)),
    KnownType(typeof(CLPInkShapeRegion)),
    KnownType(typeof(StrokeDTO)),
    KnownType(typeof(StylusPointDTO)),
    KnownType(typeof(DrawingAttributesDTO)),
    KnownType(typeof(CLPShadingRegion))]
    [AllowNonSerializableMembers]
    public class CLPPage : ModelBase
    {
        #region Variables

        public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");

        public const double LANDSCAPE_HEIGHT = 816;
        public const double LANDSCAPE_WIDTH = 1056;
        public const double PORTRAIT_HEIGHT = 1056;
        public const double PORTRAIT_WIDTH = 816;

        #endregion

        #region Constructor & destructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPPage()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            NumberOfSubmissions = 0;
            PageAspectRatio = PageWidth / PageHeight;

            //Initialize page tags to contain correctness and starred tags with values of unknown and unstarred
            Tag correctnessTag  = new Tag(Tag.Origins.Teacher, CorrectnessTagType.Instance);
            Tag starredTag  = new Tag(Tag.Origins.Teacher, StarredTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Unknown",""));
            starredTag.AddTagOptionValue(new TagOptionValue("Unstarred","..\\Images\\Unstarred.png"));
            PageTags.Add(correctnessTag);
            PageTags.Add(starredTag);
        }
     

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPPage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public Boolean CutEnabled
        {
            get { return GetValue<bool>(CutEnabledProperty); }
            set { SetValue(CutEnabledProperty, value); }
        }
        public static readonly PropertyData CutEnabledProperty = RegisterProperty("CutEnabled", typeof(bool), false);

        public PageTypeEnum PageType
        {
            get { return GetValue<PageTypeEnum>(PageTypeProperty); }
            set { SetValue(PageTypeProperty, value); }
        }

        public static readonly PropertyData PageTypeProperty = RegisterProperty("PageType", typeof(PageTypeEnum), PageTypeEnum.CLPPage);



        /// <summary>
        /// Pool of Images used on a page, so that duplications don't occur
        /// UniqueID, ByteSource
        /// </summary>
        public Dictionary<string,List<byte>> ImagePool
        {
            get { return GetValue<Dictionary<string,List<byte>>>(ImagePoolProperty); }
            set { SetValue(ImagePoolProperty, value); }
        }

        public static readonly PropertyData ImagePoolProperty = RegisterProperty("ImagePool", typeof(Dictionary<string,List<byte>>), () => new Dictionary<string, List<byte>>());

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

        public double ProofProgressCurrent
        {
            get { return GetValue<double>(ProofProgressCurrentProperty); }
            set { SetValue(ProofProgressCurrentProperty, value); }
        }

        public static volatile PropertyData ProofProgressCurrentProperty = RegisterProperty("ProofProgressCurrent", typeof(double),0);
        
        ////////////////////////////////
        public string ProofProgressVisible
        {
            get { return GetValue<string>(ProofProgressVisibleProperty); }
            set { SetValue(ProofProgressVisibleProperty, value); }
        }

        public static volatile PropertyData ProofProgressVisibleProperty = RegisterProperty("ProofProgressVisible", typeof(string), "Hidden");
        
        public string ProofPresent
        {
            get { return GetValue<string>(ProofPresentProperty); }
            set { SetValue(ProofPresentProperty, value); }
        }

        public static volatile PropertyData ProofPresentProperty = RegisterProperty("ProofPresent", typeof(string), "Hidden");

        /////////////////////////////////

        /// <summary>
        /// Aspect Ratio of page = PageWidth / PageHeight.
        /// </summary>
        public double PageAspectRatio
        {
            get { return GetValue<double>(PageAspectRatioProperty); }
            set { SetValue(PageAspectRatioProperty, value); }
        }

        public static readonly PropertyData PageAspectRatioProperty = RegisterProperty("PageAspectRatio", typeof(double), LANDSCAPE_WIDTH / LANDSCAPE_HEIGHT);

        /// <summary>
        /// Number of Student Submissions associated with this page.
        /// </summary>
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); }
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int), 0);

        /// <summary>
        /// Number of Group Submissions associated with this page.
        /// </summary>
        public int NumberOfGroupSubmissions
        {
            get { return GetValue<int>(NumberOfGroupSubmissionsProperty); }
            set { SetValue(NumberOfGroupSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfGroupSubmissionsProperty = RegisterProperty("NumberOfGroupSubmissions", typeof(int), 0);

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
        /// Gets a list of the serialized strokes for a page.
        /// </summary>
        public ObservableCollection<List<byte>> ByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(ByteStrokesProperty); }
            set { SetValue(ByteStrokesProperty, value); }
        }

        public static readonly PropertyData ByteStrokesProperty = RegisterProperty("ByteStrokes", typeof(ObservableCollection<List<byte>>), () => new ObservableCollection<List<byte>>());

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
        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection), () => new StrokeCollection(), includeInSerialization:false);
        
        /// <summary>
        /// Set to True to ignore InkStrokeCollectionChange event.
        /// </summary>
        public bool IsInkAutoAdding
        {
            get { return GetValue<bool>(IsInkAutoAddingProperty); }
            set { SetValue(IsInkAutoAddingProperty, value); }
        }

        public static readonly PropertyData IsInkAutoAddingProperty = RegisterProperty("IsInkAutoAdding", typeof(bool), false);

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
        /// Gets the CLPPage history.
        /// </summary>
        public virtual ICLPHistory PageHistory
        {
            get { return GetValue<ICLPHistory>(PageHistoryProperty); }
            set { SetValue(PageHistoryProperty, value); }
        }

        public static readonly PropertyData PageHistoryProperty = RegisterProperty("PageHistory", typeof(ICLPHistory), () => new CLPHistory());

        /// <summary>
        /// Whether or not the page is a submissions from a student.
        /// </summary>
        public bool IsSubmission
        {
            get { return GetValue<bool>(IsSubmissionProperty); }
            set { SetValue(IsSubmissionProperty, value); }
        }

        public static readonly PropertyData IsSubmissionProperty = RegisterProperty("IsSubmission", typeof(bool), false);

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
        /// Index of page in notebook.
        /// </summary>
        public int PageIndex
        {
            get { return GetValue<int>(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }

        public static readonly PropertyData PageIndexProperty = RegisterProperty("PageIndex", typeof(int), -1);

        /// <summary>
        /// Author created pageTags associated with the page.
        /// </summary>
        public ObservableCollection<Tag> PageTags
        {
            get { return GetValue<ObservableCollection<Tag>>(PageTagsProperty); }
            set { SetValue(PageTagsProperty, value); }
        }

        public static readonly PropertyData PageTagsProperty = RegisterProperty("PageTags", typeof(ObservableCollection<Tag>), () => new ObservableCollection<Tag>());

        /// <summary>
        /// Author created pageTopics associated with the page.
        /// </summary>
        public ObservableCollection<string> PageTopics
        {
            get { return GetValue<ObservableCollection<string>>(PageTopicsProperty); }
            set { SetValue(PageTopicsProperty, value); }
        }

        public static readonly PropertyData PageTopicsProperty = RegisterProperty("PageTopics", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

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
        /// Unique submission ID for submitted pages.
        /// </summary>
        public string SubmissionID
        {
            get { return GetValue<string>(SubmissionIDProperty); }
            set { SetValue(SubmissionIDProperty, value); }
        }

        public static readonly PropertyData SubmissionIDProperty = RegisterProperty("SubmissionID", typeof(string), Guid.NewGuid().ToString());


        /// <summary>
        /// Name of the submitter on a submitted page.
        /// </summary>
        public string SubmitterName
        {
            get { return GetValue<string>(SubmitterNameProperty); }
            set { SetValue(SubmitterNameProperty, value); }
        }

        public static readonly PropertyData SubmitterNameProperty = RegisterProperty("SubmitterName", typeof(string));

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
        /// Availability of Group Submit option for a page.
        /// </summary>
        public GroupSubmitType GroupSubmitType
        {
            get { return GetValue<GroupSubmitType>(GroupSubmitTypeProperty); }
            set { SetValue(GroupSubmitTypeProperty, value); }
        }

        public static readonly PropertyData GroupSubmitTypeProperty = RegisterProperty("GroupSubmitType", typeof(GroupSubmitType), GroupSubmitType.Deny);

        /// <summary>
        /// Flag a page as a page submitted via GroupSubmit.
        /// </summary>
        public bool IsGroupSubmission
        {
            get { return GetValue<bool>(IsGroupSubmissionProperty); }
            set { SetValue(IsGroupSubmissionProperty, value); }
        }

        public static readonly PropertyData IsGroupSubmissionProperty = RegisterProperty("IsGroupSubmission", typeof(bool), false);

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

        public string GroupName 
        { 
            get 
            {
                return GroupSubmitter != null ? GroupSubmitter.GroupName : "No Group";
            }
        }

        #endregion

        #region Methods
        


        public CLPPage DuplicatePage()
        {
            var newPage = new CLPPage
                {
                    PageTopics = PageTopics,
                    PageHeight = PageHeight,
                    PageWidth = PageWidth,
                    PageAspectRatio = PageAspectRatio,
                    ImagePool = ImagePool,
                    ParentNotebookID = ParentNotebookID,
                    PageTags = PageTags
                };

            foreach(Stroke stroke in InkStrokes)
            {
                Stroke s = stroke.Clone();
                s.RemovePropertyData(StrokeIDKey);

                string newUniqueID = Guid.NewGuid().ToString();
                s.AddPropertyData(StrokeIDKey, newUniqueID);

                newPage.InkStrokes.Add(s);
            }

            foreach(ICLPPageObject clonedPageObject in PageObjects.Select(pageObject => pageObject.Duplicate()))
            {
                clonedPageObject.ParentPage = newPage;
                clonedPageObject.ParentPageID = newPage.UniqueID;
                newPage.PageObjects.Add(clonedPageObject);
                clonedPageObject.RefreshStrokeParentIDs();
            }

            return newPage;
        }

        public static ObservableCollection<StrokeDTO> SaveInkStrokes(StrokeCollection strokes)
        {
            var serializedStrokes = new ObservableCollection<StrokeDTO>();
            foreach(var stroke in strokes)
            {
                serializedStrokes.Add(new StrokeDTO(stroke));
            }

            return serializedStrokes;
        }

        public static StrokeCollection LoadInkStrokes(ObservableCollection<StrokeDTO> serializedStrokes)
        {
            var strokes = new StrokeCollection();
            foreach(var strokeDTO in serializedStrokes.Where(strokeDTO => strokeDTO.StrokePoints.Any())) {
                strokes.Add(strokeDTO.ToStroke());
            }

            return strokes;
        }

        public static Stroke ByteToStroke(List<byte> byteStroke)
        {
            var mStream = new MemoryStream(byteStroke.ToArray());
            var sc = new StrokeCollection(mStream);

            mStream.Dispose();

            return sc[0];
        }

        public static List<byte> StrokeToByte(Stroke stroke)
        {
            if (stroke == null)
            {
                Logger.Instance.WriteToLog("***INDIVIDUAL STROKE IS NULL***");
                MessageBox.Show("One Stroke Null");
            }

            var sc = new StrokeCollection {stroke};

            var mStream = new MemoryStream();
            sc.Save(mStream, true);
            var byteStroke = new List<byte>(mStream.ToArray());

            mStream.Dispose();

            return byteStroke;
        }

        /**
         * Helper method that converts a ObservableCollection of List<byte> to a StrokeCollection
         */
        public static StrokeCollection BytesToStrokes(ObservableCollection<List<byte>> byteStrokes)
        {
            var strokes = new StrokeCollection();
            foreach(var s in byteStrokes)
            {
                strokes.Add(ByteToStroke(s));
            }

            return strokes;
        }

        /**
         * Helper method that converts a StrokeCollection to an ObservableCollection of List<byte>
         */
        public static ObservableCollection<List<byte>> StrokesToBytes(StrokeCollection strokes)
        {
            if (strokes == null)
            {
                Logger.Instance.WriteToLog("***STROKES is NULL***");
                MessageBox.Show("STROKES Null");
            }
            var byteStrokes = new ObservableCollection<List<byte>>();
            foreach(var stroke in strokes)
            {
                byteStrokes.Add(StrokeToByte(stroke));
            }

            return byteStrokes;
        }

        protected override void OnDeserialized()
        {
            base.OnDeserialized();
            //if(!SerializedStrokes.Any() && ByteStrokes.Any())
            //{
            //    InkStrokes = BytesToStrokes(ByteStrokes);
            //}
        }

        [OnSerializing]
        void OnSerializing(StreamingContext sc)
        {
            if (InkStrokes != null && InkStrokes.Any())
            {
                SerializedStrokes = SaveInkStrokes(InkStrokes);
            }
        }

        public void TrimPage()
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
            if (newHeight < PageHeight)
            {
                PageHeight = newHeight;
            }        
        }

        public List<ObservableCollection<ICLPPageObject>> CutObjects(double leftX, double rightX, double topY, double botY)
        {
            var lr = new List<ObservableCollection<ICLPPageObject>>();
            var c1 = new ObservableCollection<ICLPPageObject>();
            var c2 = new ObservableCollection<ICLPPageObject>();

            //TODO: Tim - This is fine for now, but you could have an instance where a really wide, but short rectangle is made
            // and a stroke could be made that was only a few pixels high, and quite wide, that would try to make a horizontal
            // cut instead of the vertical cut that was intended.
            if(Math.Abs(leftX - rightX) < Math.Abs(topY - botY))
            {
                double Ave = (rightX + leftX) / 2;
                foreach(ICLPPageObject pageObject in PageObjects)
                {
                    double otopYVal = pageObject.YPosition;
                    double obotYVal = otopYVal + pageObject.Height;
                    double oLeftXVal = pageObject.XPosition;
                    double oRightXVal = pageObject.XPosition + pageObject.Width;
                    if(pageObject.PageObjectType.Equals("CLPArray"))
                    {
                        if((oLeftXVal + CLPArray.LargeLabelLength <= leftX && oRightXVal - 2*CLPArray.SmallLabelLength >= rightX) &&
                           (topY - otopYVal - CLPArray.LargeLabelLength < 15 && obotYVal - 2*CLPArray.SmallLabelLength - botY < 15))
                        {
                            ObservableCollection<ICLPPageObject> c = pageObject.SplitAtX(Ave);
                            if(c.Count == 2){
                                foreach(ICLPPageObject no in c)
                                {
                                    c1.Add(no);
                                }
                                c2.Add(pageObject);
                            }
                        }
                    }
                    else if(pageObject.PageObjectType.Equals(CLPShape.Type))
                    {
                        CLPShape oc = (CLPShape)pageObject;
                        if(oc.ShapeType.Equals(CLPShape.CLPShapeType.Rectangle)){
                            if(((otopYVal >= topY) && (obotYVal <= botY)) &&
                             ((oLeftXVal <= leftX) && (oRightXVal >= rightX)))
                            {
                                ObservableCollection<ICLPPageObject> c = pageObject.SplitAtX(Ave);
                                foreach(ICLPPageObject no in c)
                                {
                                    c1.Add(no);
                                }
                                c2.Add(pageObject);
                            }
                        }
                    }
                }
            }
            else{ 
                double Ave = (topY + botY) / 2;
                foreach(ICLPPageObject o in this.PageObjects)
                {
                    double otopYVal = o.YPosition;
                    double obotYVal = otopYVal + o.Height;
                    double oLeftXVal = o.XPosition;
                    double oRightXVal = o.XPosition + o.Width;
                    if(o.PageObjectType.Equals("CLPArray"))
                    {
                        if((otopYVal + CLPArray.LargeLabelLength <= topY && obotYVal - 2*CLPArray.SmallLabelLength >= botY) &&
                           (leftX - oLeftXVal - CLPArray.LargeLabelLength < 15 && oRightXVal - 2*CLPArray.SmallLabelLength - rightX < 15))
                        {
                            ObservableCollection<ICLPPageObject> c = o.SplitAtY(Ave);
                            if(c.Count == 2){
                                foreach(ICLPPageObject no in c)
                                {
                                    c1.Add(no);
                                }
                                c2.Add(o);
                            }
                        }
                    }
                    else if(o.PageObjectType.Equals(CLPShape.Type))
                    {
                        CLPShape oc = (CLPShape)o;
                        if(oc.ShapeType.Equals(CLPShape.CLPShapeType.Rectangle)){
                            if(((otopYVal <= topY) && (obotYVal >= botY)) &&
                               ((oLeftXVal >= leftX) && (oRightXVal <= rightX))){
                                ObservableCollection<ICLPPageObject> c = o.SplitAtY(Ave);
                                foreach(ICLPPageObject no in c){
                                    c1.Add(no);
                                }
                                c2.Add(o);
                            }
                        }
                    }
               }  
            }
            lr.Add(c1);
            lr.Add(c2);
            return lr;
        }

        public void updateProgress()
        {
            try
            {
                //CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                CLPProofHistory proofPageHistory1 = (CLPProofHistory)PageHistory;
                double FutureItemsNumber = proofPageHistory1.Future.Count;
                double pastItemsNumber = proofPageHistory1.MetaPast.Count;
                double totalItemsNumber = FutureItemsNumber + pastItemsNumber;

                if(totalItemsNumber == 0)
                {
                    
                    ProofPresent = "Hidden";
                    ProofProgressCurrent = 0;
                    return;
                }
                else
                {
                    ProofPresent = "Visible";
                    ProofProgressCurrent =

                        (pastItemsNumber * PageWidth * 0.7175) /
                        totalItemsNumber;

                    
                }

                if(proofPageHistory1.ProofPageAction.Equals(CLPProofHistory.CLPProofPageAction.Record))
                {
                    ProofProgressVisible = "Hidden";
                }
                else {
                    ProofProgressVisible = "Visible";
                    
                }


            }catch(Exception e){
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}

