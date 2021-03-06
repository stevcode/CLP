﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    #region Enums

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

    #endregion // Enums

    [Serializable]
    public class CLPPage : AInternalZipEntryFile
    {
        #region Constants

        public const double LANDSCAPE_HEIGHT = 816.0;
        public const double LANDSCAPE_WIDTH = 1056.0;
        public const double PORTRAIT_HEIGHT = 1056.0;
        public const double PORTRAIT_WIDTH = 816.0;

        #endregion // Constants

        #region Constructors

        /// <summary>Initializes <see cref="CLPPage" /> from scratch.</summary>
        public CLPPage()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
            Width = LANDSCAPE_WIDTH;
            Height = LANDSCAPE_HEIGHT;
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

        #endregion // Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="CLPPage" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), string.Empty);

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="CLPPage" />.</summary>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary><see cref="Person" /> who submitted the <see cref="CLPPage" />.</summary>
        public Person Owner
        {
            get { return GetValue<Person>(OwnerProperty); }
            set { SetValue(OwnerProperty, value); }
        }

        public static readonly PropertyData OwnerProperty = RegisterProperty("Owner", typeof(Person), propertyChangedEventHandler: OnOwnerChanged);

        private static void OnOwnerChanged(object sender, AdvancedPropertyChangedEventArgs e)
        {
            if (!e.IsNewValueMeaningful ||
                e.NewValue == null)
            {
                return;
            }

            var page = sender as CLPPage;
            var newOwner = e.NewValue as Person;
            if (page == null ||
                newOwner == null)
            {
                return;
            }

            page.OwnerID = newOwner.ID;
        }

        /// <summary>Page Number of the <see cref="CLPPage" /> within the <see cref="Notebook" />.</summary>
        public int PageNumber
        {
            get { return GetValue<int>(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(int), 1);

        /// <summary>Identifier for the subpage of a page.</summary>
        public int SubPageNumber
        {
            get { return GetValue<int>(SubPageNumberProperty); }
            set { SetValue(SubPageNumberProperty, value); }
        }

        public static readonly PropertyData SubPageNumberProperty = RegisterProperty("SubPageNumber", typeof(int), 0);

        /// <summary>Differentiation Level of the <see cref="CLPPage" />.</summary>
        public string DifferentiationLevel
        {
            get { return GetValue<string>(DifferentiationLevelProperty); }
            set { SetValue(DifferentiationLevelProperty, value); }
        }

        public static readonly PropertyData DifferentiationLevelProperty = RegisterProperty("DifferentiationLevel", typeof(string), "0");

        /// <summary>Version Index of the <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public uint VersionIndex
        {
            get { return GetValue<uint>(VersionIndexProperty); }
            set { SetValue(VersionIndexProperty, value); }
        }

        public static readonly PropertyData VersionIndexProperty = RegisterProperty("VersionIndex", typeof(uint), 0);

        /// <summary>Version Index of the latest submission.</summary>
        public uint? LastVersionIndex
        {
            get { return GetValue<uint?>(LastVersionIndexProperty); }
            set { SetValue(LastVersionIndexProperty, value); }
        }

        public static readonly PropertyData LastVersionIndexProperty = RegisterProperty("LastVersionIndex", typeof(uint?));

        /// <summary>Date and Time the <see cref="CLPPage" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>The type of page.</summary>
        public PageTypes PageType
        {
            get { return GetValue<PageTypes>(PageTypeProperty); }
            set { SetValue(PageTypeProperty, value); }
        }

        public static readonly PropertyData PageTypeProperty = RegisterProperty("PageType", typeof(PageTypes), PageTypes.Default);

        /// <summary>Type of Submission for the <see cref="CLPPage" />.</summary>
        public SubmissionTypes SubmissionType
        {
            get { return GetValue<SubmissionTypes>(SubmissionTypeProperty); }
            set { SetValue(SubmissionTypeProperty, value); }
        }

        public static readonly PropertyData SubmissionTypeProperty = RegisterProperty("SubmissionType", typeof(SubmissionTypes), SubmissionTypes.Unsubmitted);

        /// <summary>Date and Time the <see cref="CLPPage" /> was submitted.</summary>
        public DateTime? SubmissionTime
        {
            get { return GetValue<DateTime?>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof(DateTime?));

        /// <summary>Height of the <see cref="CLPPage" />.</summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), LANDSCAPE_HEIGHT);

        /// <summary>Width of the <see cref="CLPPage" />.</summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), LANDSCAPE_WIDTH);

        /// <summary>Initial Aspect Ratio of the <see cref="CLPPage" />, where Aspect Ratio = Width / Height.</summary>
        public double InitialAspectRatio
        {
            get { return GetValue<double>(InitialAspectRatioProperty); }
            set { SetValue(InitialAspectRatioProperty, value); }
        }

        public static readonly PropertyData InitialAspectRatioProperty = RegisterProperty("InitialAspectRatio", typeof(double), 0.0);

        /// <summary>Type of lines on the background of the <see cref="CLPPage" />.</summary>
        public PageLineTypes PageLineType
        {
            get { return GetValue<PageLineTypes>(PageLineTypeProperty); }
            set { SetValue(PageLineTypeProperty, value); }
        }

        public static readonly PropertyData PageLineTypeProperty = RegisterProperty("PageLineType", typeof(PageLineTypes), PageLineTypes.None);

        /// <summary>Amount of space between PageLines on the <see cref="CLPPage" />.</summary>
        public double PageLineLength
        {
            get { return GetValue<double>(PageLineLengthProperty); }
            set { SetValue(PageLineLengthProperty, value); }
        }

        public static readonly PropertyData PageLineLengthProperty = RegisterProperty("PageLineLength", typeof(double), 20.0);

        /// <summary><see cref="ATagBase" />s for the <see cref="CLPPage" />.</summary>
        public ObservableCollection<ITag> Tags
        {
            get { return GetValue<ObservableCollection<ITag>>(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly PropertyData TagsProperty = RegisterProperty("Tags", typeof(ObservableCollection<ITag>), () => new ObservableCollection<ITag>());

        /// <summary><see cref="IPageObject" />s for the <see cref="CLPPage" />.</summary>
        public ObservableCollection<IPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<IPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<IPageObject>), () => new ObservableCollection<IPageObject>());

        /// <summary>Serialized <see cref="Stroke" />s in the form of <see cref="StrokeDTO" />.</summary>
        public List<StrokeDTO> SerializedStrokes
        {
            get { return GetValue<List<StrokeDTO>>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(List<StrokeDTO>), () => new List<StrokeDTO>());

        /// <summary>Interaction history of the page.</summary>
        public PageHistory History
        {
            get { return GetValue<PageHistory>(HistoryProperty); }
            set { SetValue(HistoryProperty, value); }
        }

        public static readonly PropertyData HistoryProperty = RegisterProperty("History", typeof(PageHistory));

        #region Non-Serialized

        /// <summary>The thumbnail for the <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public ImageSource PageThumbnail
        {
            get { return GetValue<ImageSource>(PageThumbnailProperty); }
            set { SetValue(PageThumbnailProperty, value); }
        }

        public static readonly PropertyData PageThumbnailProperty = RegisterProperty("PageThumbnail", typeof(ImageSource));

        /// <summary>Submissions associated with this <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public ObservableCollection<CLPPage> Submissions
        {
            get { return GetValue<ObservableCollection<CLPPage>>(SubmissionsProperty); }
            set { SetValue(SubmissionsProperty, value); }
        }

        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>Unserialized <see cref="Stroke" />s of the <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            set { SetValue(InkStrokesProperty, value); }
        }

        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection), () => new StrokeCollection());

        #endregion // Non-Serialized

        #region Calculated Sort Properties

        public string IsStarred
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

        public string HadHelp
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

        public string TroubleWithFactorPairs
        {
            get
            {
                var tags = Tags.OfType<DivisionTemplateFactorPairErrorsTag>().Where(x => x.HadTrouble);
                return tags.Any() ? "Trouble With Factor Pairs" : "No Trouble With Factor Pairs";
            }
        }

        public string TroubleWithRemainders
        {
            get
            {
                var tags = Tags.OfType<DivisionTemplateRemainderErrorsTag>().Where(x => x.HadTrouble);
                return tags.Any() ? "Trouble With Remainders" : "No Trouble With Remainders";
            }
        }

        public string TroubleWithDivision
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

        public string Correctness
        {
            get
            {
                var correctnessTag = Tags.FirstOrDefault(x => x is CorrectnessSummaryTag) as CorrectnessSummaryTag;
                if (correctnessTag == null)
                {
                    return "Unknown";
                }

                switch (correctnessTag.OverallCorrectness)
                {
                    case Entities.Correctness.Correct:
                        return "COR";
                    case Entities.Correctness.PartiallyCorrect:
                        return "PAR";
                    case Entities.Correctness.Incorrect:
                        return "INC";
                    case Entities.Correctness.Unknown:
                        return "Unknown";
                }

                return "Unknown";
            }
        }

        public string RepresentationType
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

        public string RepresentationCorrectness
        {
            get
            {
                try
                {
                    var representationCorrectnessTag = Tags.FirstOrDefault(x => x is FinalRepresentationCorrectnessTag) as FinalRepresentationCorrectnessTag;
                    if (representationCorrectnessTag == null)
                    {
                        return "None";
                    }

                    var codes = representationCorrectnessTag.SpreadSheetCodes;
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

        // TODO: Rename to FinalAnswerCorrectness
        public string AnswerCorrectness
        {
            get
            {
                try
                {
                    var answerCorrectnessTag = Tags.FirstOrDefault(x => x is FinalAnswerCorrectnessTag) as FinalAnswerCorrectnessTag;
                    if (answerCorrectnessTag == null)
                    {
                        return "None";
                    }

                    var codedCorrectness = Codings.CorrectnessToCodedCorrectness(answerCorrectnessTag.FinalAnswerCorrectness);
                    if (codedCorrectness.Contains("COR"))
                    {
                        return "Correct";
                    }
                    if (codedCorrectness.Contains("INC"))
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

        public string ABR
        {
            get
            {
                try
                {
                    var abrTag = Tags.FirstOrDefault(x => x is AnswerRepresentationSequenceTag) as AnswerRepresentationSequenceTag;
                    if (abrTag == null)
                    {
                        return "None";
                    }

                    var code = abrTag.SpreadSheetCodes;
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

        #endregion //Calculated Sort Properties

        #endregion //Properties

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
            History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(History.TrashedInkStrokes.Where(x => x != null));
            CLPPage copy = this.DeepCopy();
            if (copy == null)
            {
                return null;
            }
            copy.SubmissionType = SubmissionTypes.Single;
            copy.VersionIndex = LastVersionIndex.GetValueOrDefault(1);
            foreach (var pageObject in copy.PageObjects.Where(x => x != null))
            {
                pageObject.ParentPage = copy;
            }

            foreach (var pageObject in copy.History.TrashedPageObjects.Where(x => x != null))
            {
                pageObject.ParentPage = copy;
            }

            foreach (var tag in copy.Tags.Where(x => x != null))
            {
                tag.ParentPage = copy;
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

        public void UpdateAllReporters()
        {
            foreach (var reporter in PageObjects.OfType<IReporter>())
            {
                reporter.UpdateReport();
            }
        }

        public bool IsEqualByCompositeIDIgnoringVersion(CLPPage otherPage)
        {
            return ID == otherPage.ID && PageNumber == otherPage.PageNumber && SubPageNumber == otherPage.SubPageNumber && DifferentiationLevel == otherPage.DifferentiationLevel;
        }

        public bool IsEqualByCompositeID(CLPPage otherPage)
        {
            return IsEqualByCompositeIDIgnoringVersion(otherPage) && VersionIndex == otherPage.VersionIndex;
        }

        #endregion //Methods

        #region Overrides of ModelBase

        protected override void OnSerializing()
        {
            base.OnSerializing();

            SerializedStrokes = StrokeDTO.SaveInkStrokes(InkStrokes);
            History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(History.TrashedInkStrokes);

            // TODO: Save thumbnail after/during serialization
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

        protected override void OnDeserialized()
        {
            InkStrokes = StrokeDTO.LoadInkStrokes(SerializedStrokes);
            History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(History.SerializedTrashedInkStrokes);

            foreach (var pageObject in PageObjects)
            {
                pageObject.ParentPage = this;
            }
            foreach (var pageObject in History.TrashedPageObjects)
            {
                pageObject.ParentPage = this;
            }
            foreach (var historyAction in History.UndoActions)
            {
                historyAction.ParentPage = this;
            }
            foreach (var historyAction in History.RedoActions)
            {
                historyAction.ParentPage = this;
            }
            foreach (var semanticEvent in History.SemanticEvents)
            {
                semanticEvent.ParentPage = this;
            }
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

            // TODO: Load thumbnail after deserialized
            // BUG: loaded thumbnails don't let go of their disk reference.
            //var fileInfo = new FileInfo(pageFilePath);
            //var thumbnailsFolderPath = Path.Combine(fileInfo.DirectoryName, "Thumbnails");
            //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, nameComposite.ToFileName() + ".png");
            //page.PageThumbnail = CLPImage.GetImageFromPath(thumbnailFilePath);

            base.OnDeserialized();
        }

        #endregion

        #region Overrides of AInternalZipEntryFile

        public class NameComposite
        {
            public int PageNumber { get; set; }
            public int SubPageNumber { get; set; }
            public string DifferentiationLevel { get; set; }
            public uint VersionIndex { get; set; }
            public string ID { get; set; }

            public string ToNameCompositeString()
            {
                return $"p;{PageNumber};{SubPageNumber};{DifferentiationLevel};{VersionIndex};{ID}";
            }

            public static NameComposite ParseFromString(string nameCompositeString)
            {
                var parts = nameCompositeString.Split(';');
                if (parts.Length != 6)
                {
                    return null;
                }

                try
                {
                    var nameComposite = new NameComposite
                                        {
                                            PageNumber = Convert.ToInt32(parts[1]),
                                            SubPageNumber = Convert.ToInt32(parts[2]),
                                            DifferentiationLevel = parts[3],
                                            VersionIndex = Convert.ToUInt32(parts[4]),
                                            ID = parts[5]
                                        };

                    return nameComposite;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override string DefaultZipEntryName => $"p;{PageNumber};{SubPageNumber};{DifferentiationLevel};{VersionIndex};{ID}";

        public override string GetZipEntryFullPath(Notebook parentNotebook)
        {
            var notebookOwnerDirectoryPath = $"{ZIP_NOTEBOOKS_FOLDER_NAME}/{parentNotebook.NotebookSetDirectoryName}/{parentNotebook.NotebookOwnerDirectoryName}";

            return VersionIndex == 0
                       ? $"{notebookOwnerDirectoryPath}/{ZIP_NOTEBOOK_PAGES_FOLDER_NAME}/{DefaultZipEntryName}.xml"
                       : $"{notebookOwnerDirectoryPath}/{ZIP_NOTEBOOK_SUBMISSIONS_FOLDER_NAME}/{DefaultZipEntryName}.xml";
        }

        #endregion
    }
}