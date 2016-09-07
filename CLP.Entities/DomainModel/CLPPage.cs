using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;
using Catel.IoC;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Json;

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

    public enum PageLineTypes
    {
        None,
        Lined,
        Grid
    }

    public class PageNameComposite
    {
        public const string QUALIFIER_TEXT = "p";
        public string PageNumber { get; set; }
        public string ID { get; set; }
        public string DifferentiationGroupName { get; set; }
        public string VersionIndex { get; set; }

        public string ToFileName() { return string.Format("{0};{1};{2};{3};{4}", QUALIFIER_TEXT, PageNumber, ID, DifferentiationGroupName, VersionIndex); }

        public static PageNameComposite ParsePage(CLPPage page)
        {
            var nameComposite = new PageNameComposite
                                {
                                    PageNumber = page.PageNumber.ToString(),
                                    ID = page.ID,
                                    DifferentiationGroupName = page.DifferentiationLevel,
                                    VersionIndex = page.VersionIndex.ToString()
                                };

            return nameComposite;
        }

        public static PageNameComposite ParseFilePath(string pageFilePath)
        {
            var fileInfo = new FileInfo(pageFilePath);
            var pageFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var pageFileParts = pageFileName.Split(';');
            if (pageFileParts.Length != 5)
            {
                return null;
            }

            var nameComposite = new PageNameComposite
                                {
                                    PageNumber = pageFileParts[1],
                                    ID = pageFileParts[2],
                                    DifferentiationGroupName = pageFileParts[3],
                                    VersionIndex = pageFileParts[4]
                                };

            return nameComposite;
        }
    }

    public class PageNumberComparer : IComparer<CLPPage>
    {
        public int Compare(CLPPage x, CLPPage y)
        {
            return x.PageNumber.CompareTo(y.PageNumber);
        }
    }

    [Serializable]
    public class CLPPage : AEntityBase
    {
        #region Fields

        public const double LANDSCAPE_HEIGHT = 816;
        public const double LANDSCAPE_WIDTH = 1056;
        public const double PORTRAIT_HEIGHT = 1056;
        public const double PORTRAIT_WIDTH = 816;

        #endregion //Fields

        #region Constructors

        /// <summary>Initializes <see cref="CLPPage" /> from scratch.</summary>
        public CLPPage()
        {
            Height = LANDSCAPE_HEIGHT;
            Width = LANDSCAPE_WIDTH;
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
            InitialAspectRatio = Width / Height;
            History = new PageHistory();
        }

        /// <summary>Initializes <see cref="CLPPage" /> from page dimensions.</summary>
        /// <param name="height">Height of the <see cref="CLPPage" />.</param>
        /// <param name="width">Width of the <see cref="CLPPage" />.</param>
        public CLPPage(Person owner, double height, double width)
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
            Owner = owner;
            Height = height;
            Width = width;
            InitialAspectRatio = Width / Height;
            History = new PageHistory();
        }

        /// <summary>Initializes <see cref="CLPPage" /> from page dimensions.</summary>
        /// <param name="owner">The owner of the <see cref="CLPPage" />.</param>
        public CLPPage(Person owner)
            : this()
        {
            Owner = owner;
        }

        /// <summary>Initializes <see cref="CLPPage" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPPage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>The thumbnail for the <see cref="CLPPage" />
        /// </summary>
        [XmlIgnore]
        public ImageSource PageThumbnail
        {
            get { return GetValue<ImageSource>(PageThumbnailProperty); }
            set { SetValue(PageThumbnailProperty, value); }
        }

        public static readonly PropertyData PageThumbnailProperty = RegisterProperty("PageThumbnail", typeof (ImageSource));

        /// <summary>Unique Identifier for the <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Primary Key. Also Foregin Key for <see cref="Person" /> who owns the <see cref="CLPPage" />.</remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof (string), string.Empty);

        /// <summary>Version Index of the <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof (uint), 0);

        /// <summary>Version Index of the latest submission.</summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof (uint?));

        /// <summary>Differentiation Level of the <see cref="CLPPage" />.</summary>
        public string DifferentiationLevel
        {
            get { return GetValue<string>(DifferentiationGroupProperty); }
            set { SetValue(DifferentiationGroupProperty, value); }
        }

        public static readonly PropertyData DifferentiationGroupProperty = RegisterProperty("DifferentiationGroup", typeof (string), "0");

        /// <summary>The type of page.</summary>
        public PageTypes PageType
        {
            get { return GetValue<PageTypes>(PageTypeProperty); }
            set { SetValue(PageTypeProperty, value); }
        }

        public static readonly PropertyData PageTypeProperty = RegisterProperty("PageType", typeof (PageTypes), PageTypes.Default);

        /// <summary>Date and Time the <see cref="CLPPage" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof (DateTime));

        /// <summary>Height of the <see cref="CLPPage" />.</summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof (double), LANDSCAPE_HEIGHT);

        /// <summary>Width of the <see cref="CLPPage" />.</summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof (double), LANDSCAPE_WIDTH);

        /// <summary>Initial Aspect Ratio of the <see cref="CLPPage" />, where Aspect Ratio = Width / Height.</summary>
        public double InitialAspectRatio
        {
            get { return GetValue<double>(InitialAspectRatioProperty); }
            set { SetValue(InitialAspectRatioProperty, value); }
        }

        public static readonly PropertyData InitialAspectRatioProperty = RegisterProperty("InitialAspectRatio", typeof (double));

        /// <summary>Type of lines on the background of the <see cref="CLPPage" />.</summary>
        public PageLineTypes PageLineType
        {
            get { return GetValue<PageLineTypes>(PageLineTypeProperty); }
            set { SetValue(PageLineTypeProperty, value); }
        }

        public static readonly PropertyData PageLineTypeProperty = RegisterProperty("PageLineType", typeof (PageLineTypes), PageLineTypes.None);

        /// <summary>Amount of space between PageLines on the <see cref="CLPPage" />.</summary>
        public double PageLineLength
        {
            get { return GetValue<double>(PageLineLengthProperty); }
            set { SetValue(PageLineLengthProperty, value); }
        }

        public static readonly PropertyData PageLineLengthProperty = RegisterProperty("PageLineLength", typeof (double), 20.0);

        /// <summary>Type of Submission for the <see cref="CLPPage" />.</summary>
        public SubmissionTypes SubmissionType
        {
            get { return GetValue<SubmissionTypes>(SubmissionTypeProperty); }
            set { SetValue(SubmissionTypeProperty, value); }
        }

        public static readonly PropertyData SubmissionTypeProperty = RegisterProperty("SubmissionType", typeof (SubmissionTypes), SubmissionTypes.Unsubmitted);

        /// <summary>Date and Time the <see cref="CLPPage" /> was submitted.</summary>
        public DateTime? SubmissionTime
        {
            get { return GetValue<DateTime?>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof (DateTime?));

        #region MetaData

        /// <summary>Title of the chapter the <see cref="CLPPage" /> is part of within the <see cref="Notebook" />.</summary>
        public string ChapterTitle
        {
            get { return GetValue<string>(ChapterTitleProperty); }
            set { SetValue(ChapterTitleProperty, value); }
        }

        public static readonly PropertyData ChapterTitleProperty = RegisterProperty("ChapterTitle", typeof (string), string.Empty);

        /// <summary>Title of the section the <see cref="CLPPage" /> is part of within the <see cref="Notebook" />.</summary>
        public string SectionTitle
        {
            get { return GetValue<string>(SectionTitleProperty); }
            set { SetValue(SectionTitleProperty, value); }
        }

        public static readonly PropertyData SectionTitleProperty = RegisterProperty("SectionTitle", typeof (string), string.Empty);

        /// <summary>Page Number of the <see cref="CLPPage" /> within the <see cref="Notebook" />.</summary>
        public decimal PageNumber
        {
            get { return GetValue<decimal>(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof (decimal), 1);

        /// <summary>Page Number of the <see cref="CLPPage" />'s corresponding page in the Student Workbook.</summary>
        public string StudentWorkbookPageNumber
        {
            get { return GetValue<string>(StudentWorkbookPageNumberProperty); }
            set { SetValue(StudentWorkbookPageNumberProperty, value); }
        }

        public static readonly PropertyData StudentWorkbookPageNumberProperty = RegisterProperty("StudentWorkbookPageNumber", typeof (string), string.Empty);

        /// <summary>Page Number of the <see cref="CLPPage" />'s corresponding page in the Teacher Workbook.</summary>
        public string TeacherWorkbookPageNumber
        {
            get { return GetValue<string>(TeacherWorkbookPageNumberProperty); }
            set { SetValue(TeacherWorkbookPageNumberProperty, value); }
        }

        public static readonly PropertyData TeacherWorkbookPageNumberProperty = RegisterProperty("TeacherWorkbookPageNumber", typeof (string), string.Empty);

        /// <summary>Curriculum the <see cref="CLPPage" /> employs.</summary>
        public string Curriculum
        {
            get { return GetValue<string>(CurriculumProperty); }
            set { SetValue(CurriculumProperty, value); }
        }

        public static readonly PropertyData CurriculumProperty = RegisterProperty("Curriculum", typeof (string), string.Empty);

        #endregion //MetaData

        #region Navigation Properties

        /// <summary>
        ///     <see cref="Person" /> who submitted the <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual Person Owner
        {
            get { return GetValue<Person>(OwnerProperty); }
            set
            {
                SetValue(OwnerProperty, value);
                if (value == null)
                {
                    return;
                }
                OwnerID = value.ID;
                History.OwnerID = value.ID;
            }
        }

        public static readonly PropertyData OwnerProperty = RegisterProperty("Owner", typeof (Person));

        /// <summary>Submissions associated with this <see cref="CLPPage" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ObservableCollection<CLPPage> Submissions
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionsProperty); }
            set { SetValue(SubmissionsProperty, value); }
        }

        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof (ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>Authored <see cref="IPageObject" />s for the <see cref="CLPPage" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual ObservableCollection<IPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<IPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects",
                                                                                   typeof (ObservableCollection<IPageObject>),
                                                                                   () => new ObservableCollection<IPageObject>());

        /// <summary>
        ///     <see cref="ATagBase" />s for the <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        public virtual ObservableCollection<ITag> Tags
        {
            get { return GetValue<ObservableCollection<ITag>>(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly PropertyData TagsProperty = RegisterProperty("Tags", typeof (ObservableCollection<ITag>), () => new ObservableCollection<ITag>());

        /// <summary>Unserialized <see cref="Stroke" />s of the <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            set { SetValue(InkStrokesProperty, value); }
        }

        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof (StrokeCollection), () => new StrokeCollection());

        /// <summary>Serialized <see cref="Stroke" />s in the form of <see cref="StrokeDTO" />.</summary>
        public List<StrokeDTO> SerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof (List<StrokeDTO>), () => new List<StrokeDTO>());

        #endregion //Navigation Properties

        /// <summary>Interaction history of the page.</summary>
        public PageHistory History
        {
            get { return GetValue<PageHistory>(HistoryProperty); }
            set { SetValue(HistoryProperty, value); }
        }

        public static readonly PropertyData HistoryProperty = RegisterProperty("History", typeof (PageHistory));

        #region Calculated Sort Properties

        public virtual string IsStarred
        {
            get
            {
                var starredTag = Tags.FirstOrDefault(x => x is StarredTag) as StarredTag;
                if (starredTag != null &&
                    starredTag.Value == StarredTag.AcceptedValues.Starred)
                {
                    return "Starred";
                }
                return "Unstarred";
            }
        }

        public virtual string HadHelp
        {
            get
            {
                var hadHelpTag = Tags.FirstOrDefault(x => x is DottedTag) as DottedTag;
                if (hadHelpTag != null &&
                    hadHelpTag.Value == DottedTag.AcceptedValues.Dotted)
                {
                    return "Had Help";
                }
                return "No Help";
            }
        }

        public virtual string TroubleWithFactorPairs
        {
            get
            {
                var tags = Tags.OfType<DivisionTemplateFactorPairErrorsTag>().Where(x => x.HadTrouble);
                return tags.Any() ? "Trouble With Factor Pairs" : "No Trouble With Factor Pairs";
            }
        }

        public virtual string TroubleWithRemainders
        {
            get
            {
                var tags = Tags.OfType<DivisionTemplateRemainderErrorsTag>().Where(x => x.HadTrouble);
                return tags.Any() ? "Trouble With Remainders" : "No Trouble With Remainders";
            }
        }

        public virtual string TroubleWithDivision
        {
            get
            {
                var tag = Tags.FirstOrDefault(x => x is TroubleWithDivisionTag) as TroubleWithDivisionTag;
                if (tag != null)
                {
                    return "Trouble With Division";
                }
                return "No Trouble With Division";
            }
        }

        public virtual Correctness Correctness
        {
            get
            {
                var correctnessTag = Tags.FirstOrDefault(x => x is CorrectnessTag) as CorrectnessTag;
                return correctnessTag != null ? correctnessTag.Correctness : Correctness.Unknown;
            }
        }

        public virtual string RepresentationType
        {
            get
            {
                var objectTypes = new List<string>();
                foreach (var pageObject in PageObjects.Where(pageObject => pageObject.OwnerID == OwnerID))
                {
                    if (pageObject is DivisionTemplate)
                    {
                        objectTypes.Add("Division Templates");
                        continue;
                    }

                    if (pageObject is CLPArray)
                    {
                        objectTypes.Add("Arrays");
                        continue;
                    }

                    if (pageObject is NumberLine)
                    {
                        objectTypes.Add("Number Lines");
                        continue;
                    }

                    if (pageObject is Stamp ||
                        pageObject is StampedObject)
                    {
                        objectTypes.Add("Stamps");
                        continue;
                    }

                    if (pageObject is Shape)
                    {
                        objectTypes.Add("Shapes");
                        continue;
                    }

                    if (pageObject is Bin)
                    {
                        objectTypes.Add("Bins");
                        continue;
                    }
                }

                var usedRepresentationTypes = objectTypes.Distinct().ToList();

                if (usedRepresentationTypes.Count == 1)
                {
                    return usedRepresentationTypes.First();
                }

                if (usedRepresentationTypes.Count > 1)
                {
                    return "Multiple Types";
                }

                return InkStrokes.Count > 2 ? "Ink" : "None";
            }
        }

        public virtual string RepresentationCorrectness
        {
            get
            {
                try
                {
                    var representationCorrectnessTag = Tags.FirstOrDefault(x => x is RepresentationCorrectnessTag) as RepresentationCorrectnessTag;
                    if (representationCorrectnessTag == null)
                    {
                        return "None";
                    }

                    var codes = representationCorrectnessTag.AnalysisCodes;
                    if (codes.All(c => c.Contains("COR")))
                    {
                        return "Correct";
                    }
                    if (codes.All(c => c.Contains("PAR")))
                    {
                        return "Partially Correct";
                    }
                    if (codes.All(c => c.Contains("INC")))
                    {
                        return "Incorrect";
                    }

                    return "Mixed";
                }
                catch (Exception)
                {
                    return "None";
                }
            }
        }

        public virtual string AnswerCorrectness
        {
            get
            {
                try
                {
                    var answerCorrectnessTag = Tags.FirstOrDefault(x => x is AnswerCorrectnessTag) as AnswerCorrectnessTag;
                    if (answerCorrectnessTag == null)
                    {
                        return "None";
                    }

                    var code = answerCorrectnessTag.AnalysisCode;
                    if (code.Contains("COR"))
                    {
                        return "Correct";
                    }
                    if (code.Contains("INC"))
                    {
                        return "Incorrect";
                    }

                    return "Partially Correct";
                }
                catch (Exception)
                {
                    return "None";
                }
            }
        }

        public virtual string ABR
        {
            get
            {
                try
                {
                    var abrTag = Tags.FirstOrDefault(x => x is AnswerBeforeRepresentationTag) as AnswerBeforeRepresentationTag;
                    if (abrTag == null)
                    {
                        return "None";
                    }

                    var code = abrTag.AnalysisCode;
                    if (code.Contains("ABR-I"))
                    {
                        return "ABR-I";
                    }

                    return "ABR-C";
                }
                catch (Exception)
                {
                    return "None";
                }
            }
        }

        public virtual string ARIC
        {
            get
            {
                try
                {
                    var aricTag = Tags.FirstOrDefault(x => x is AnswerChangedAfterRepresentationTag) as AnswerChangedAfterRepresentationTag;
                    if (aricTag == null)
                    {
                        return "None";
                    }

                    return aricTag.AnalysisCode.Substring(0, 4);
                }
                catch (Exception)
                {
                    return "None";
                }
            }
        }

        #endregion //Calculated Sort Properties

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
            var newPage = new CLPPage
                          {
                              Owner = Owner,
                              Height = Height,
                              Width = Width,
                              InitialAspectRatio = InitialAspectRatio,
                              PageType = PageType
                          };

            foreach (var s in InkStrokes.Select(stroke => stroke.Clone()))
            {
                // TODO: Make sure all accepted strokes change appropriate strokeIDs lists
                s.SetStrokeID(Guid.NewGuid().ToCompactID());

                newPage.InkStrokes.Add(s);
            }
            newPage.SerializedStrokes = StrokeDTO.SaveInkStrokes(newPage.InkStrokes);

            foreach (var clonedPageObject in PageObjects.Select(pageObject => pageObject.Duplicate()))
            {
                clonedPageObject.ParentPage = newPage;
                newPage.PageObjects.Add(clonedPageObject);
            }

            newPage.History.ClearHistory();

            return newPage;
        }

        public CLPPage NextVersionCopy()
        {
            if (LastVersionIndex == null)
            {
                LastVersionIndex = 1;
            }
            else
            {
                LastVersionIndex++;
            }
            SubmissionTime = DateTime.Now;
            SerializedStrokes = StrokeDTO.SaveInkStrokes(InkStrokes.Where(x => x != null));
            History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(History.TrashedInkStrokes.Where(x => x!= null));
            var copy = this.DeepCopy();
            if (copy == null)
            {
                return null;
            }
            copy.SubmissionType = SubmissionTypes.Single;
            copy.VersionIndex = LastVersionIndex.GetValueOrDefault(1);
            copy.History.VersionIndex = LastVersionIndex.GetValueOrDefault(1);
            copy.History.LastVersionIndex = LastVersionIndex;
            foreach (var pageObject in copy.PageObjects.Where(x => x != null))
            {
                pageObject.VersionIndex = LastVersionIndex.GetValueOrDefault(1);
                pageObject.LastVersionIndex = LastVersionIndex;
                pageObject.ParentPage = copy;
            }

            foreach (var pageObject in copy.History.TrashedPageObjects.Where(x => x != null))
            {
                pageObject.VersionIndex = LastVersionIndex.GetValueOrDefault(1);
                pageObject.LastVersionIndex = LastVersionIndex;
                pageObject.ParentPage = copy;
            }

            foreach (var tag in copy.Tags.Where(x => x != null))
            {
                tag.VersionIndex = LastVersionIndex.GetValueOrDefault(1);
                tag.LastVersionIndex = LastVersionIndex;
                tag.ParentPage = copy;
            }

            foreach (var serializedStroke in copy.SerializedStrokes.Where(x => x != null))
            {
                //TODO: Stroke Version Index should be uint
                serializedStroke.VersionIndex = (int)LastVersionIndex.GetValueOrDefault(1);
            }

            foreach (var serializedStroke in copy.History.SerializedTrashedInkStrokes.Where(x => x != null))
            {
                serializedStroke.VersionIndex = (int)LastVersionIndex.GetValueOrDefault(1);
            }

            return copy;
        }

        public void TrimPage()
        {
            var lowestY = PageObjects.Select(pageObject => pageObject.YPosition + pageObject.Height).Concat(new double[] { 0 }).Max();
            foreach (var bounds in InkStrokes.Select(s => s.GetBounds()))
            {
                if (bounds.Bottom >= Height)
                {
                    lowestY = Math.Max(lowestY, Height);
                    break;
                }
                lowestY = Math.Max(lowestY, bounds.Bottom);
            }

            var defaultHeight = Math.Abs(Width - LANDSCAPE_WIDTH) < .000001 ? LANDSCAPE_HEIGHT : PORTRAIT_HEIGHT;

            var newHeight = Math.Max(defaultHeight, lowestY);
            if (newHeight < Height)
            {
                Height = newHeight;
            }
        }

        public bool IsTagAddPrevented = false;

        public void AddTag(ITag newTag)
        {
            if (IsTagAddPrevented)
            {
                return;
            }

            if (newTag.IsSingleValueTag)
            {
                var toRemove = Tags.Where(t => t.GetType() == newTag.GetType()).ToList();
                foreach (var tag in toRemove)
                {
                    Tags.Remove(tag);
                }
            }

            Tags.Add(newTag);
        }

        public void RemoveTag(ITag tag)
        {
            if (IsTagAddPrevented)
            {
                return;
            }

            Tags.Remove(tag);
        }

        public void AddBoundary(Rect rect, string regionText = "") { AddBoundary(rect.X, rect.Y, rect.Height, rect.Width, regionText); }

        public void AddBoundary(double xPos, double yPos, double height, double width, string  regionText = "")
        {
            var boundary = new TemporaryBoundary(this, xPos, yPos, height, width)
                           {
                               RegionText = regionText
                           };
            PageObjects.Add(boundary);
        }

        public void ClearBoundaries()
        {
            var boundariesToRemove = PageObjects.OfType<TemporaryBoundary>().ToList();
            foreach (var temporaryBoundary in boundariesToRemove)
            {
                PageObjects.Remove(temporaryBoundary);
            }
        }

        public CLPPage CopyForNewOwner(Person owner)
        {
            var newPage = this.DeepCopy();
            if (newPage == null)
            {
                return null;
            }
            newPage.Owner = owner;

            foreach (var pageObject in newPage.PageObjects)
            {
                pageObject.ParentPage = newPage;
                if (pageObject.IsBackgroundInteractable)
                {
                    pageObject.OwnerID = owner.ID;
                }
            }

            foreach (var tag in newPage.Tags)
            {
                tag.ParentPage = newPage;
            }

            return newPage;
        }

        public CLPPage CreateOriginalVersion()
        {
            SerializedStrokes = StrokeDTO.SaveInkStrokes(InkStrokes.Where(x => x != null));
            History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(History.TrashedInkStrokes.Where(x => x != null));
            var copy = this.DeepCopy();
            if (copy == null)
            {
                return null;
            }
            copy.VersionIndex = 0;
            copy.History.VersionIndex = 0;

            foreach (var pageObject in copy.PageObjects.Where(x => x != null))
            {
                pageObject.VersionIndex = 0;
                pageObject.ParentPage = copy;
            }

            foreach (var pageObject in copy.History.TrashedPageObjects.Where(x => x != null))
            {
                pageObject.VersionIndex = 0;
                pageObject.ParentPage = copy;
            }

            foreach (var tag in copy.Tags.Where(x => x != null))
            {
                tag.VersionIndex = 0;
                tag.ParentPage = copy;
            }

            foreach (var serializedStroke in copy.SerializedStrokes.Where(x => x != null))
            {
                //TODO: Stroke Version Index should be uint
                serializedStroke.VersionIndex = 0;
            }

            foreach (var serializedStroke in copy.History.SerializedTrashedInkStrokes.Where(x => x != null))
            {
                serializedStroke.VersionIndex = 0;
            }

            return copy;
        }

        public Stroke GetVerifiedStrokeOnPageByID(string id)
        {
            var stroke = GetStrokeByID(id);

            if (stroke != null)
            {
                return stroke;
            }

            stroke = History.GetStrokeByID(id);
            if (stroke == null)
            {
                return null;
            }

            Console.WriteLine("Stroke incorrectly existed in TrashedInkStrokes.");
            History.TrashedInkStrokes.Remove(stroke);
            InkStrokes.Add(stroke);

            return stroke;
        }

        public Stroke GetVerifiedStrokeInHistoryByID(string id)
        {
            var stroke = History.GetStrokeByID(id);

            if (stroke != null)
            {
                return stroke;
            }

            stroke = GetStrokeByID(id);
            if (stroke == null)
            {
                return null;
            }

            Console.WriteLine("Stroke incorrectly existed on Page.");
            History.TrashedInkStrokes.Add(stroke);
            InkStrokes.Remove(stroke);

            return stroke;
        }

        public Stroke GetStrokeByIDOnPageOrInHistory(string id)
        {
            var stroke = History.GetStrokeByID(id);

            if (stroke != null)
            {
                return stroke;
            }

            stroke = GetStrokeByID(id);
            return stroke;
        }

        public Stroke GetStrokeByID(string id) { return !InkStrokes.Any() ? null : InkStrokes.FirstOrDefault(stroke => stroke.GetStrokeID() == id); }

        public IPageObject GetVerifiedPageObjectOnPageByID(string id)
        {
            var pageObject = GetPageObjectByID(id);

            if (pageObject != null)
            {
                return pageObject;
            }

            pageObject = History.GetPageObjectByID(id);
            if (pageObject == null)
            {
                return null;
            }

            Console.WriteLine("PageObject incorrectly existed in TrashedPageObjects.");
            History.TrashedPageObjects.Remove(pageObject);
            PageObjects.Add(pageObject);

            return pageObject;
        }

        public IPageObject GetVerifiedPageObjectInTrashByID(string id)
        {
            var pageObject = History.GetPageObjectByID(id);

            if (pageObject != null)
            {
                return pageObject;
            }

            pageObject = GetPageObjectByID(id);
            if (pageObject == null)
            {
                return null;
            }

            Console.WriteLine("PageObject incorrectly existed on Page.");
            History.TrashedPageObjects.Add(pageObject);
            PageObjects.Remove(pageObject);

            return pageObject;
        }

        public IPageObject GetPageObjectByIDOnPageOrInHistory(string id)
        {
            var pageObject = History.GetPageObjectByID(id);

            if (pageObject != null)
            {
                return pageObject;
            }

            pageObject = GetPageObjectByID(id);
            return pageObject;
        }

        public IPageObject GetPageObjectByID(string id) { return !PageObjects.Any() ? null : PageObjects.FirstOrDefault(pageObject => pageObject.ID == id); }

        public void UpdateAllReporters()
        {
            foreach (var reporter in PageObjects.OfType<IReporter>())
            {
                reporter.UpdateReport();
            }
        }

        public void ValidateStrokeIDs()
        {
            foreach (var stroke in InkStrokes)
            {
                if (stroke.GetStrokeID() == "noStrokeID")
                {
                    Console.WriteLine("Stroke without ID on page {0}, {1}", PageNumber, Owner.FullName);
                }
            }

            foreach (var stroke in History.TrashedInkStrokes)
            {
                if (stroke.GetStrokeID() == "noStrokeID")
                {
                    Console.WriteLine("Trashed Stroke without ID on page {0}, {1}", PageNumber, Owner.FullName);
                }
            }
        }

        #endregion //Methods

        #region Cache

        public bool IsCached { get; set; }

        public void ToJSON(string pageFilePath, bool serializeInkStrokes = true)
        {
            if (serializeInkStrokes)
            {
                SerializedStrokes = StrokeDTO.SaveInkStrokes(InkStrokes);
                History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(History.TrashedInkStrokes);
            }

            var fileInfo = new FileInfo(pageFilePath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(pageFilePath, FileMode.Create))
            {
                var jsonSerializer = ServiceLocator.Default.ResolveType<IJsonSerializer>();
                jsonSerializer.WriteTypeInfo = true;
                jsonSerializer.PreserveReferences = true;
                jsonSerializer.Serialize(this, stream);
                
                ClearIsDirtyOnAllChilds();
            }
            IsCached = true;
        }

        public void ToXML(string pageFilePath, bool serializeInkStrokes = true)
        {
            if (serializeInkStrokes)
            {
                SerializedStrokes = StrokeDTO.SaveInkStrokes(InkStrokes);
                History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(History.TrashedInkStrokes);
            }

            var fileInfo = new FileInfo(pageFilePath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(pageFilePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
            IsCached = true;
        }

        public void SaveToXML(string folderPath, bool serializeInkStrokes = true)
        {
            var nameComposite = PageNameComposite.ParsePage(this);
            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");
            ToXML(filePath, serializeInkStrokes);

            //if(page.PageThumbnail == null)
            //{
            //    return;
            //}

            //var thumbnailsFolderPath = Path.Combine(folderPath, "Thumbnails");
            //if(!Directory.Exists(thumbnailsFolderPath))
            //{
            //    Directory.CreateDirectory(thumbnailsFolderPath);
            //}
            //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, nameComposite.ToFileName() + ".png");

            //var pngEncoder = new PngBitmapEncoder();
            //pngEncoder.Frames.Add(BitmapFrame.Create(page.PageThumbnail as BitmapSource));
            //using(var outputStream = new MemoryStream())
            //{
            //    pngEncoder.Save(outputStream);
            //    File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
            //}
        }

        public void SaveToJSON(string folderPath, bool serializeInkStrokes = true)
        {
            var nameComposite = PageNameComposite.ParsePage(this);
            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".json");
            ToJSON(filePath, serializeInkStrokes);

            //if(page.PageThumbnail == null)
            //{
            //    return;
            //}

            //var thumbnailsFolderPath = Path.Combine(folderPath, "Thumbnails");
            //if(!Directory.Exists(thumbnailsFolderPath))
            //{
            //    Directory.CreateDirectory(thumbnailsFolderPath);
            //}
            //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, nameComposite.ToFileName() + ".png");

            //var pngEncoder = new PngBitmapEncoder();
            //pngEncoder.Frames.Add(BitmapFrame.Create(page.PageThumbnail as BitmapSource));
            //using(var outputStream = new MemoryStream())
            //{
            //    pngEncoder.Save(outputStream);
            //    File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
            //}
        }

        public static CLPPage LoadFromJSON(string pageFilePath)
        {
            try
            {
                var nameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                if (nameComposite == null)
                {
                    return null;
                }

                CLPPage page;
                using (var stream = new FileStream(pageFilePath, FileMode.Open))
                {
                    var jsonSerializer = ServiceLocator.Default.ResolveType<IJsonSerializer>();
                    jsonSerializer.WriteTypeInfo = true;
                    jsonSerializer.PreserveReferences = true;
                    //page = (CLPPage)jsonSerializer.Deserialize(typeof(CLPPage), stream);
                    var uncastPage = jsonSerializer.Deserialize(typeof(CLPPage), stream);
                    page = (CLPPage)uncastPage;
                }

                if (page == null)
                {
                    return null;
                }

                page.PageNumber = decimal.Parse(nameComposite.PageNumber);
                page.ID = nameComposite.ID;
                page.DifferentiationLevel = nameComposite.DifferentiationGroupName;
                page.VersionIndex = uint.Parse(nameComposite.VersionIndex);
                page.AfterDeserialization();

                // BUG: loaded thumbnails don't let go of their disk reference.
                //var fileInfo = new FileInfo(pageFilePath);
                //var thumbnailsFolderPath = Path.Combine(fileInfo.DirectoryName, "Thumbnails");
                //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, nameComposite.ToFileName() + ".png");
                //page.PageThumbnail = CLPImage.GetImageFromPath(thumbnailFilePath);

                return page;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static CLPPage LoadFromXML(string pageFilePath)
        {
            try
            {
                var nameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                if (nameComposite == null)
                {
                    return null;
                }

                var page = Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                if (page == null)
                {
                    return null;
                }

                page.PageNumber = decimal.Parse(nameComposite.PageNumber);
                page.ID = nameComposite.ID;
                page.DifferentiationLevel = nameComposite.DifferentiationGroupName;
                page.VersionIndex = uint.Parse(nameComposite.VersionIndex);
                page.AfterDeserialization();

                // BUG: loaded thumbnails don't let go of their disk reference.
                //var fileInfo = new FileInfo(pageFilePath);
                //var thumbnailsFolderPath = Path.Combine(fileInfo.DirectoryName, "Thumbnails");
                //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, nameComposite.ToFileName() + ".png");
                //page.PageThumbnail = CLPImage.GetImageFromPath(thumbnailFilePath);

                return page;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void AfterDeserialization()
        {
            InkStrokes = StrokeDTO.LoadInkStrokes(SerializedStrokes);
            History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(History.SerializedTrashedInkStrokes);
            foreach (var pageObject in PageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.LoadAcceptedStrokes();
            }
            foreach (var pageObject in History.TrashedPageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.LoadAcceptedStrokes();
            }
            foreach (var pageObject in PageObjects.OfType<IPageObjectAccepter>())
            {
                pageObject.LoadAcceptedPageObjects();
            }
            foreach (var pageObject in History.TrashedPageObjects.OfType<IPageObjectAccepter>())
            {
                pageObject.LoadAcceptedPageObjects();
            }
            IsCached = true;
        }

        #region Overrides of ModelBase

        protected override void OnDeserialized()
        {
            base.OnDeserialized();
            AfterDeserialization();
        }

        #endregion

        #endregion //Cache
    }
}