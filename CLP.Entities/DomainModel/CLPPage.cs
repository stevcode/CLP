using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public enum PageTypes
    {
        Default,
        Animation
    }

    public enum SubmissionTypes
    {
        Unsubmitted,
        Single,
        Group
    }

    public class CLPPage : AEntityBase
    {
        #region Fields

        public const double LANDSCAPE_HEIGHT = 816;
        public const double LANDSCAPE_WIDTH = 1056;
        public const double PORTRAIT_HEIGHT = 1056;
        public const double PORTRAIT_WIDTH = 816;

        #endregion //Fields

        #region Constructors

        /// <summary>
        /// Initializes <see cref="CLPPage" /> from scratch.
        /// </summary>
        public CLPPage()
            : this(LANDSCAPE_HEIGHT, LANDSCAPE_WIDTH) { }

        /// <summary>
        /// Initializes <see cref="CLPPage" /> from page dimensions.
        /// </summary>
        /// <param name="height">Height of the <see cref="CLPPage" />.</param>
        /// <param name="width">Width of the <see cref="CLPPage" />.</param>
        public CLPPage(double height, double width)
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToString();
            Height = height;
            Width = width;
            InitialAspectRatio = Width / Height;
        }

        /// <summary>
        /// Initializes <see cref="CLPPage" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPPage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="CLPPage" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// The type of page.
        /// </summary>
        public PageTypes PageType
        {
            get { return GetValue<PageTypes>(PageTypeProperty); }
            set { SetValue(PageTypeProperty, value); }
        }

        public static readonly PropertyData PageTypeProperty = RegisterProperty("PageType", typeof(PageTypes), PageTypes.Default);

        /// <summary>
        /// Date and Time the <see cref="CLPPage" /> was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// Height of the <see cref="CLPPage" />.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), LANDSCAPE_HEIGHT);

        /// <summary>
        /// Width of the <see cref="CLPPage" />.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), LANDSCAPE_WIDTH);

        /// <summary>
        /// Initial Aspect Ratio of the <see cref="CLPPage" />, where Aspect Ratio = Width / Height.
        /// </summary>
        public double InitialAspectRatio
        {
            get { return GetValue<double>(InitialAspectRatioProperty); }
            set { SetValue(InitialAspectRatioProperty, value); }
        }

        public static readonly PropertyData InitialAspectRatioProperty = RegisterProperty("InitialAspectRatio", typeof(double));

        /// <summary>
        /// Unique Identifier of the <see cref="CLPPage" />'s parent <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key
        /// </remarks>
        public string NotebookID
        {
            get { return GetValue<string>(NotebookIDProperty); }
            set { SetValue(NotebookIDProperty, value); }
        }

        public static readonly PropertyData NotebookIDProperty = RegisterProperty("NotebookID", typeof(string));

        #region Submission Data

        /// <summary>
        /// Unique Identifier for the the <see cref="CLPPage" /> when it is a Submission.
        /// </summary>
        public string SubmissionID
        {
            get { return GetValue<string>(SubmissionIDProperty); }
            set { SetValue(SubmissionIDProperty, value); }
        }

        public static readonly PropertyData SubmissionIDProperty = RegisterProperty("SubmissionID", typeof(string), string.Empty);
 
        /// <summary>
        /// Type of Submission for the <see cref="CLPPage" />.
        /// </summary>
        public SubmissionTypes SubmissionType
        {
            get { return GetValue<SubmissionTypes>(SubmissionTypeProperty); }
            set { SetValue(SubmissionTypeProperty, value); }
        }

        public static readonly PropertyData SubmissionTypeProperty = RegisterProperty("SubmissionType", typeof(SubmissionTypes), SubmissionTypes.Unsubmitted);

        /// <summary>
        /// Date and Time the <see cref="CLPPage" /> was submitted.
        /// </summary>
        public DateTime? SubmissionTime
        {
            get { return GetValue<DateTime?>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof(DateTime?));

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> who submitted the <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string SubmitterID
        {
            get { return GetValue<string>(SubmitterIDProperty); }
            set { SetValue(SubmitterIDProperty, value); }
        }

        public static readonly PropertyData SubmitterIDProperty = RegisterProperty("SubmitterID", typeof(string), string.Empty);

        /// <summary>
        /// <see cref="Person" /> who submitted the <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual Person Submitter
        {
            get { return GetValue<Person>(SubmitterProperty); }
            set { SetValue(SubmitterProperty, value); }
        }

        public static readonly PropertyData SubmitterProperty = RegisterProperty("Submitter", typeof(Person));

        #endregion //Submission Data

        #region MetaData

        /// <summary>
        /// Title of the chapter the <see cref="CLPPage" /> is part of within the <see cref="Notebook" />.
        /// </summary>
        public string ChapterTitle
        {
            get { return GetValue<string>(ChapterTitleProperty); }
            set { SetValue(ChapterTitleProperty, value); }
        }

        public static readonly PropertyData ChapterTitleProperty = RegisterProperty("ChapterTitle", typeof(string), string.Empty);

        /// <summary>
        /// Title of the section the <see cref="CLPPage" /> is part of within the <see cref="Notebook" />.
        /// </summary>
        public string SectionTitle
        {
            get { return GetValue<string>(SectionTitleProperty); }
            set { SetValue(SectionTitleProperty, value); }
        }

        public static readonly PropertyData SectionTitleProperty = RegisterProperty("SectionTitle", typeof(string), string.Empty);

        /// <summary>
        /// Page Number of the <see cref="CLPPage" /> within the <see cref="Notebook" />.
        /// </summary>
        public int PageNumber
        {
            get { return GetValue<int>(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(int), 1);

        /// <summary>
        /// Page Number of the <see cref="CLPPage" />'s corresponding page in the Student Workbook.
        /// </summary>
        public string StudentWorkbookPageNumber
        {
            get { return GetValue<string>(StudentWorkbookPageNumberProperty); }
            set { SetValue(StudentWorkbookPageNumberProperty, value); }
        }

        public static readonly PropertyData StudentWorkbookPageNumberProperty = RegisterProperty("StudentWorkbookPageNumber", typeof(string), string.Empty);

        /// <summary>
        /// Page Number of the <see cref="CLPPage" />'s corresponding page in the Teacher Workbook.
        /// </summary>
        public string TeacherWorkbookPageNumber
        {
            get { return GetValue<string>(TeacherWorkbookPageNumberProperty); }
            set { SetValue(TeacherWorkbookPageNumberProperty, value); }
        }

        public static readonly PropertyData TeacherWorkbookPageNumberProperty = RegisterProperty("TeacherWorkbookPageNumber", typeof(string), string.Empty);

        /// <summary>
        /// Curriculum the <see cref="CLPPage" /> employs.
        /// </summary>
        public string Curriculum
        {
            get { return GetValue<string>(CurriculumProperty); }
            set { SetValue(CurriculumProperty, value); }
        }

        public static readonly PropertyData CurriculumProperty = RegisterProperty("Curriculum", typeof(string), string.Empty);

        #endregion //MetaData

        /// <summary>
        /// Authored <see cref="IPageObject" />s for the <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual ObservableCollection<IPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<IPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<IPageObject>), () => new ObservableCollection<IPageObject>());

        /// <summary>
        /// <see cref="ATagBase" />s for the <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual ObservableCollection<ATagBase> Tags
        {
            get { return GetValue<ObservableCollection<ATagBase>>(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly PropertyData TagsProperty = RegisterProperty("Tags", typeof(ObservableCollection<ATagBase>), () => new ObservableCollection<ATagBase>());

        #endregion //Properties

        #region Overrides of ObservableObject

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            IsCached = false;
        }

        #region Overrides of ModelBase

        protected override void OnPropertyObjectCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyObjectCollectionChanged(sender, e);
            IsCached = false;
        }

        protected override void OnPropertyObjectCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyObjectCollectionItemPropertyChanged(sender, e);
            IsCached = false;
        }

        #endregion

        #endregion

        #region Methods

         public CLPPage DuplicatePage()
        {
             // TODO: Entities
            var newPage = new CLPPage
                          {
                              NotebookID = NotebookID,
                             // PageTags = PageTags,
                              //GroupSubmitType = GroupSubmitType,
                              Height = Height,
                              Width = Width,
                              InitialAspectRatio = InitialAspectRatio
                           //   ImagePool = ImagePool
                          };

            //foreach(var s in InkStrokes.Select(stroke => stroke.Clone())) 
            //{
            //    s.RemovePropertyData(StrokeIDKey);

            //    var newUniqueID = Guid.NewGuid().ToString();
            //    s.AddPropertyData(StrokeIDKey, newUniqueID);

            //    newPage.InkStrokes.Add(s);
            //}
            //newPage.SerializedStrokes = StrokeDTO.SaveInkStrokes(newPage.InkStrokes);

            foreach(var clonedPageObject in PageObjects.Select(pageObject => pageObject.Duplicate()))
            {
                clonedPageObject.ParentPage = newPage;
                clonedPageObject.ParentPageID = newPage.ID;
                newPage.PageObjects.Add(clonedPageObject);
               // clonedPageObject.RefreshStrokeParentIDs();
            }

            return newPage;
        }

        public void TrimPage()
        {
            double lowestY = PageObjects.Select(pageObject => pageObject.YPosition + pageObject.Height).Concat(new double[] { 0 }).Max();
            // TODO: Entities
            //foreach(var bounds in InkStrokes.Select(s => s.GetBounds()))
            //{
            //    if(bounds.Bottom >= Height)
            //    {
            //        lowestY = Math.Max(lowestY, Height);
            //        break;
            //    }
            //    lowestY = Math.Max(lowestY, bounds.Bottom);
            //}

            //double defaultHeight = Math.Abs(Width - LANDSCAPE_WIDTH) < .000001 ? LANDSCAPE_HEIGHT : PORTRAIT_HEIGHT;

            //double newHeight = Math.Max(defaultHeight, lowestY);
            //if(newHeight < Height)
            //{
            //    Height = newHeight;
            //}   
        }

        public void AddTag(ATagBase newTag)
        {
            if(newTag.IsSingleValueTag)
            {
                var toRemove = Tags.Where(t => t.GetType() == newTag.GetType()).ToList();
                foreach(var tag in toRemove)
                {
                    Tags.Remove(tag);
                }
            }

            newTag.ParentPageID = ID;
            Tags.Add(newTag);
        }

        #endregion //Methods

        #region Cache

        public bool IsCached { get; set; }

        public void ToXML(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
            IsCached = true;
        }

        #endregion //Cache
    }
}