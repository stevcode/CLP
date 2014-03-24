using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum PageTypes
    {
        Default,
        Animation
    }


    public class CLPPage : EntityBase
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
        /// <param name="pageHeight">Height of the <see cref="CLPPage" />.</param>
        /// <param name="pageWidth">Width of the <see cref="CLPPage" />.</param>
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

        #region Navigation Parameters

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

        #endregion //Navigation Parameters

        #endregion //Properties

        #region Methods

        #endregion //Methods
    }
}