using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Path = Catel.IO.Path;

namespace CLP.Entities
{
    public class NotebookNameComposite
    {
        public string FullNotebookDirectoryPath { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string OwnerName { get; set; }
        public string OwnerID { get; set; }
        public string OwnerTypeTag { get; set; }
        public bool IsLocal { get; set; }
    }

    [Serializable]
    public class Notebook : AEntityBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="Notebook" /> from scratch.
        /// </summary>
        public Notebook()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>
        /// Initializes <see cref="Notebook" /> with name and owner.
        /// </summary>
        /// <param name="notebookName">The name of the notebook.</param>
        /// <param name="owner">The <see cref="Person" /> who owns the notebook.</param>
        public Notebook(string notebookName, Person owner)
            : this()
        {
            Name = notebookName;
            Owner = owner;
        }

        /// <summary>
        /// Initializes <see cref="Notebook" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Notebook(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Unique Identifier for the <see cref="Person" /> who owns the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// Foreign Key.
        /// </remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), String.Empty);

        /// <summary>
        /// The <see cref="Person" /> who owns the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual Person Owner
        {
            get { return GetValue<Person>(OwnerProperty); }
            set
            {
                SetValue(OwnerProperty, value);
                if(value == null)
                {
                    return;
                }
                OwnerID = value.ID;
            }
        }

        public static readonly PropertyData OwnerProperty = RegisterProperty("Owner", typeof(Person));

        /// <summary>
        /// Date and Time the <see cref="Notebook" /> was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// Date and Time the <see cref="Notebook" /> was last saved.
        /// </summary>
        /// <remarks>
        /// Type set to DateTime? (i.e. nullable DateTime) to allow NULL in database if LastSavedDate hasn't been set yet.
        /// </remarks>
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof(DateTime?));

        /// <summary>
        /// Name of the <see cref="Notebook" />.
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), String.Empty);

        /// <summary>
        /// Overall Curriculum the <see cref="Notebook" /> employs. Curriculum of individual pages may vary.
        /// </summary>
        public string Curriculum
        {
            get { return GetValue<string>(CurriculumProperty); }
            set { SetValue(CurriculumProperty, value); }
        }

        public static readonly PropertyData CurriculumProperty = RegisterProperty("Curriculum", typeof(string), String.Empty);

        /// <summary>
        /// List of all the HashIDs for each <see cref="CLPImage" /> that is in the notebook.
        /// </summary>
        public List<string> ImagePoolHashIDs
        {
            get
            {
                return Pages.SelectMany(page => page.PageObjects).OfType<CLPImage>().Select(image => image.ImageHashID).ToList().Distinct().ToList();
            }
        }

        #region Navigation Properties

        /// <summary>
        /// Unique Identifier of the currently selected <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key for CurrentPage.
        /// </remarks>
        public string CurrentPageID
        {
            get { return GetValue<string>(CurrentPageIDProperty); }
            set { SetValue(CurrentPageIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageIDProperty = RegisterProperty("CurrentPageID", typeof(string));

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> who owns the currently selected <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key for CurrentPage.
        /// </remarks>
        public string CurrentPageOwnerID
        {
            get { return GetValue<string>(CurrentPageOwnerIDProperty); }
            set { SetValue(CurrentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageOwnerIDProperty = RegisterProperty("CurrentPageOwnerID", typeof(string));

        /// <summary>
        /// Version Index of the currently selected <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key for CurrentPage.
        /// </remarks>
        public uint CurrentPageVersionIndex
        {
            get { return GetValue<uint>(CurrentPageVersionIndexProperty); }
            set { SetValue(CurrentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData CurrentPageVersionIndexProperty = RegisterProperty("CurrentPageVersionIndex", typeof(uint));

        /// <summary>
        /// Currently selected <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
      //  [ExcludeFromSerialization]
        public virtual CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set
            {
                SetValue(CurrentPageProperty, value);
                if(value == null)
                {
                    return;
                }
                CurrentPageID = value.ID;
                CurrentPageOwnerID = value.OwnerID;
                CurrentPageVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        /// <summary>
        /// Collection of all the <see cref="CLPPage" />s in the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
     //   [ExcludeFromSerialization]
        public virtual ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// List of the <see cref="IDisplay" />s in the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>), () => new ObservableCollection<IDisplay>());

        #endregion //Navigation Properties

        #endregion //Properties

        #region Methods

        public void AddCLPPageToNotebook(CLPPage page)
        {
            page.PageNumber = Pages.Any() ? Pages.Last().PageNumber + 1 : 1;
            page.Curriculum = Curriculum;
            Pages.Add(page);
            CurrentPage = page;
        }

        public void AddDisplayToNotebook(IDisplay display)
        {
            display.NotebookID = ID;
            display.DisplayNumber = Displays.Any() ? Displays.Last().DisplayNumber + 1 : 1;
            Displays.Add(display);
        }

        public void InsertPageAt(int index, CLPPage page)
        {
            Pages.Insert(index, page);
            page.Curriculum = Curriculum;
            GeneratePageNumbers();
            CurrentPage = page;
        }

        private List<CLPPage> _trashedPages = new List<CLPPage>();

        public void RemovePageAt(int index)
        {
            if(Pages.Count <= index ||
               index < 0)
            {
                return;
            }

            if(Pages.Count == 1)
            {
                var newPage = new CLPPage(Person.Author)
                              {
                                  PageNumber = Pages.Any() ? Pages.First().PageNumber : 1
                              };

                Pages.Add(newPage);
            }

            int newIndex;
            if(index + 1 < Pages.Count)
            {
                newIndex = index + 1;
            }
            else
            {
                newIndex = index - 1;
            }

            var nextPage = Pages.ElementAt(newIndex);
            CurrentPage = nextPage;
            if(index == 0)
            {
                CurrentPage.PageNumber = Pages.First().PageNumber;
            }

            _trashedPages.Add(Pages[index]);
            Pages.RemoveAt(index);
            GeneratePageNumbers();
        }

        public void GeneratePageNumbers()
        {
            var initialPageNumber = Pages.Any() ? Pages.First().PageNumber - 1 : 0;
            CLPPage lastPage = null;
            foreach(var page in Pages)
            {
                if(lastPage == null || page.ID != lastPage.ID)
                {
                    initialPageNumber++;
                }
                if(page.PageNumber != 999) // TODO: less stupid special case for exit tickets?
                {
                    page.PageNumber = initialPageNumber;
                }
                lastPage = page;
            }
        }

        public Notebook CopyForNewOwner(Person owner)
        {
            var newNotebook = Clone() as Notebook;
            if(newNotebook == null)
            {
                return null;
            }
            newNotebook.Owner = owner;
            newNotebook.CurrentPage = CurrentPage == null ? null : CurrentPage.CopyForNewOwner(owner);
            foreach(var newPage in Pages.Select(page => page.CopyForNewOwner(owner))) 
            {
                if(!owner.IsStudent)
                {
                    newNotebook.Pages.Add(newPage);
                    continue;
                }

                if(newPage.DifferentiationLevel == String.Empty ||
                   newPage.DifferentiationLevel == "0" ||
                   newPage.DifferentiationLevel == owner.CurrentDifferentiationGroup)
                {
                    newNotebook.Pages.Add(newPage);
                    continue;
                }

                if(owner.CurrentDifferentiationGroup == String.Empty &&
                   newPage.DifferentiationLevel == "A")
                {
                    newNotebook.Pages.Add(newPage);
                }
            }

            return newNotebook;
        }

        public CLPPage GetPageByCompositeKeys(string pageID, string pageOwnerID, string differentiationLevel, uint versionIndex, bool searchDatabaseAndCache = false)
        {
            // TODO: Database, search through cache and database if not found in memory.
            var notebookPage = Pages.FirstOrDefault(x => x.ID == pageID && x.OwnerID == pageOwnerID && x.DifferentiationLevel == differentiationLevel && x.VersionIndex == versionIndex);
            if(notebookPage != null)
            {
                return notebookPage;
            }

            notebookPage = Pages.FirstOrDefault(x => x.ID == pageID && x.DifferentiationLevel == differentiationLevel);
            return notebookPage == null ? null : notebookPage.Submissions.FirstOrDefault(x => x.OwnerID == pageOwnerID && x.VersionIndex == versionIndex);
        }

        #endregion //Methods

        #region Cache

        public string NotebookToNotebookFolderName()
        {
            var ownerTypeTag = OwnerID == Person.Author.ID ? "A" : Owner.IsStudent ? "S" : "T";
            return Name + ";" + ID + ";" + Owner.FullName + ";" + OwnerID + ";" + ownerTypeTag;
        }

        public static NotebookNameComposite NotebookDirectoryToNotebookNameComposite(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var notebookDirectoryName = directoryInfo.Name;
            var notebookDirectoryParts = notebookDirectoryName.Split(';');
            if (notebookDirectoryParts.Length != 5 &&
                notebookDirectoryParts.Length != 4)
            {
                return null;
            }

            var nameComposite = new NotebookNameComposite
            {
                FullNotebookDirectoryPath = path,
                Name = notebookDirectoryParts[0],
                ID = notebookDirectoryParts[1],
                OwnerName = notebookDirectoryParts[2],
                OwnerID = notebookDirectoryParts[3],
                IsLocal = true,
                OwnerTypeTag = notebookDirectoryParts.Length == 5 ? notebookDirectoryParts[4] : "U"
            };

            return nameComposite;
        }

        #region Loading

        public static Notebook OpenFullNotebook(string notebookFolderPath, bool includeSubmissions = true)
        {
            var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
            var pageNameComposites = Directory.EnumerateFiles(pagesFolderPath, "*.xml").Select(CLPPage.PageFilePathToPageNameComposite);
            var allPageIDs = pageNameComposites.Select(x => x.ID).Distinct().ToList();

            return OpenPartialNotebook(notebookFolderPath, allPageIDs, includeSubmissions);
        }

        public static Notebook OpenPartialNotebook(string notebookFolderPath, List<string> pageIDs, bool includeSubmissions = true)
        {
            var notebook = LoadNotebook(notebookFolderPath);
            if (notebook == null)
            {
                return null;
            }

            var loadedPages = LoadNotebookPages(notebookFolderPath, pageIDs, includeSubmissions);
            var notebookPages = loadedPages.Where(x => x.VersionIndex == 0).OrderBy(x => x.PageNumber).ToList();
            if (includeSubmissions)
            {
                foreach (var notebookPage in notebookPages)
                {
                    var page = notebookPage;
                    var submissions = loadedPages.Where(x => x.ID == page.ID && x.OwnerID == page.OwnerID && x.VersionIndex != 0).OrderBy(x => x.VersionIndex).ToList();
                    notebookPage.Submissions = new ObservableCollection<CLPPage>(submissions);
                }
            }

            notebook.Pages = new ObservableCollection<CLPPage>(notebookPages);

            return notebook;
        }

        private static Notebook LoadNotebook(string notebookFolderPath)
        {
            try
            {
                var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                var notebook = Load<Notebook>(filePath, SerializationMode.Xml);
                return notebook;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<CLPPage> LoadNotebookPages(string notebookFolderPath, List<string> pageIDs, bool includeSubmissions = true)
        {
            var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
            var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
            var loadedPages = new List<CLPPage>();
            foreach (var pageFilePath in pageFilePaths)
            {
                var pageNameComposite = CLPPage.PageFilePathToPageNameComposite(pageFilePath);
                if (pageNameComposite == null)
                {
                    continue;
                }

                if (!includeSubmissions &&
                    pageNameComposite.VersionIndex != "0")
                {
                    continue;
                }

                var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
                if (!isPageToBeLoaded)
                {
                    continue;
                }

                try
                {
                    var page = Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    page.PageNumber = Decimal.Parse(pageNameComposite.PageNumber); //TODO: Make PageNumber a string.
                    //TODO: Deal with what happens if these values change.
                    //page.ID = pageNameComposite.ID;
                    //page.DifferentiationLevel = pageNameComposite.DifferentiationGroupName;
                    //page.VersionIndex = UInt32.Parse(pageNameComposite.VersionIndex);
                    loadedPages.Add(page);
                }
                catch (Exception) { }
            }

            return loadedPages;
        }

        #endregion //Loading

        #region Saving

         

        #endregion //Saving

        public void ToXML(string fileName)
        {
            LastSavedDate = DateTime.Now;
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

        public void SaveNotebook(string folderPath, bool isFullSaveForced = false)
        {
            var fileName = Path.Combine(folderPath, "notebook.xml");
            ToXML(fileName);

            var pagesFolderPath = Path.Combine(folderPath, "Pages");
            if(!Directory.Exists(pagesFolderPath))
            {
                Directory.CreateDirectory(pagesFolderPath);
            }

            //var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml").ToList();
            //foreach(var pageFilePath in pageFilePaths)
            //{
            //    File.Delete(pageFilePath);
            //}

            foreach(var page in Pages)
            {
                if(page.IsCached &&
                   !isFullSaveForced)
                {
                    continue;
                }
                var pageFilePath = Path.Combine(pagesFolderPath, "p;" + page.PageNumber + ";" + page.ID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex + ".xml");
                page.ToXML(pageFilePath);
                //if(page.PageThumbnail == null)
                //{
                //    continue;
                //}

                //var thumbnailsFolderPath = Path.Combine(pagesFolderPath, "Thumbnails");
                //if(!Directory.Exists(thumbnailsFolderPath))
                //{
                //    Directory.CreateDirectory(thumbnailsFolderPath);
                //}
                //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, "p;" + page.PageNumber + ";" + page.ID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex + ".png");

                //var pngEncoder = new PngBitmapEncoder();
                //pngEncoder.Frames.Add(BitmapFrame.Create(page.PageThumbnail as BitmapSource));
                //using(var outputStream = new MemoryStream())
                //{
                //    pngEncoder.Save(outputStream);
                //    File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
                //}
            }

            //var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml").ToList();
            //foreach(var pageFilePath in from trashedPage in _trashedPages
            //                            from pageFilePath in pageFilePaths
            //                            where pageFilePath.Contains(trashedPage.ID)
            //                            select pageFilePath)
            //{
            //    File.Delete(pageFilePath);
            //}
            _trashedPages.Clear();

            //foreach(var page in Pages)
            //{
            //    foreach(var pageFilePath in pageFilePaths)
            //    {
            //        if(pageFilePath.Contains(page.ID))
            //        {
            //            var pageNumberOfFile = Convert.ToInt32(Path.GetFileName(pageFilePath).Split(' ')[1]);
            //            if(page.PageNumber != pageNumberOfFile)
            //            {
            //                File.Delete(pageFilePath);
            //            }
            //        }
            //    }
            //}

            //foreach(var pageFilePath in from pageFilePath in pageFilePaths
            //                            let pageNumberOfFile = Convert.ToInt32(Path.GetFileName(pageFilePath).Split(' ')[1])
            //                            from page in Pages
            //                            where pageFilePath.Contains(page.ID) && page.PageNumber != pageNumberOfFile
            //                            select pageFilePath) 
            //{
            //    File.Delete(pageFilePath);
            //}

            var displaysFolderPath = Path.Combine(folderPath, "Displays");
            if(!Directory.Exists(displaysFolderPath))
            {
                Directory.CreateDirectory(displaysFolderPath);
            }

            foreach(var display in Displays)
            {
                display.Save(displaysFolderPath);
            }
        }

        public void SavePartialNotebook(string folderPath, bool serializeInkStrokes = true)
        {
            var fileName = Path.Combine(folderPath, "notebook.xml");
            ToXML(fileName);

            var pagesFolderPath = Path.Combine(folderPath, "Pages");
            if(!Directory.Exists(pagesFolderPath))
            {
                Directory.CreateDirectory(pagesFolderPath);
            }

            foreach(var page in Pages)
            {
                var pageFilePath = Path.Combine(pagesFolderPath, "p;" + page.PageNumber + ";" + page.ID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex + ".xml");
                page.ToXML(pageFilePath, serializeInkStrokes);
                //if(page.PageThumbnail == null)
                //{
                //    continue;
                //}

                //var thumbnailsFolderPath = Path.Combine(pagesFolderPath, "Thumbnails");
                //if(!Directory.Exists(thumbnailsFolderPath))
                //{
                //    Directory.CreateDirectory(thumbnailsFolderPath);
                //}
                //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, "p;" + page.PageNumber + ";" + page.ID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex + ".png");

                //var pngEncoder = new PngBitmapEncoder();
                //pngEncoder.Frames.Add(BitmapFrame.Create(page.PageThumbnail as BitmapSource));
                //using(var outputStream = new MemoryStream())
                //{
                //    pngEncoder.Save(outputStream);
                //    File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
                //}
            }
        }

        public void SaveSubmissions(string folderPath)
        {
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach(var page in Pages)
            {
                foreach(var submission in page.Submissions)
                {
                    var pageFilePath = Path.Combine(folderPath, "p;" + submission.PageNumber + ";" + submission.ID + ";" + submission.DifferentiationLevel + ";" + submission.VersionIndex + ".xml");
                    submission.ToXML(pageFilePath);
                    //if(submission.PageThumbnail == null)
                    //{
                    //    continue;
                    //}

                    //var thumbnailsFolderPath = Path.Combine(folderPath, "Thumbnails");
                    //if(!Directory.Exists(thumbnailsFolderPath))
                    //{
                    //    Directory.CreateDirectory(thumbnailsFolderPath);
                    //}
                    //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, "p;" + submission.PageNumber + ";" + submission.ID + ";" + submission.DifferentiationLevel + ";" + submission.VersionIndex + ".png");

                    //var pngEncoder = new PngBitmapEncoder();
                    //pngEncoder.Frames.Add(BitmapFrame.Create(submission.PageThumbnail as BitmapSource));
                    //using(var outputStream = new MemoryStream())
                    //{
                    //    pngEncoder.Save(outputStream);
                    //    File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
                    //}
                }
            }
        }

        public void SaveOthersSubmissions(string folderPath)
        {
            foreach(var page in Pages)
            {
                foreach(var submission in page.Submissions)
                {
                    var notebookFolderPaths = Directory.EnumerateDirectories(folderPath);
                    foreach(var notebookFolderPath in notebookFolderPaths)
                    {
                        if(notebookFolderPath.Contains(submission.OwnerID))
                        {
                            var pagesPath = Path.Combine(notebookFolderPath, "Pages");
                            var pageFilePath = Path.Combine(pagesPath, "p;" + submission.PageNumber + ";" + submission.ID + ";" + submission.DifferentiationLevel + ";" + submission.VersionIndex + ".xml");
                            submission.ToXML(pageFilePath);
                            //if(submission.PageThumbnail == null)
                            //{
                            //    continue;
                            //}

                            //var thumbnailsFolderPath = Path.Combine(pagesPath, "Thumbnails");
                            //if(!Directory.Exists(thumbnailsFolderPath))
                            //{
                            //    Directory.CreateDirectory(thumbnailsFolderPath);
                            //}
                            //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, "p;" + submission.PageNumber + ";" + submission.ID + ";" + submission.DifferentiationLevel + ";" + submission.VersionIndex + ".png");

                            //var pngEncoder = new PngBitmapEncoder();
                            //pngEncoder.Frames.Add(BitmapFrame.Create(submission.PageThumbnail as BitmapSource));
                            //using(var outputStream = new MemoryStream())
                            //{
                            //    pngEncoder.Save(outputStream);
                            //    File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
                            //}
                        }
                    }
                }
            }
        }

        public static Notebook OpenNotebook(string folderPath, bool includeSubmissions = true)
        {
            try
            {
                var filePath = Path.Combine(folderPath, "notebook.xml");
                var notebook = Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(folderPath, "Pages");
                var thumbnailsFolderPath = Path.Combine(pagesFolderPath, "Thumbnails");
                var pageAndHistoryFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                var pages = new List<CLPPage>();
                foreach(var pageAndHistoryFilePath in pageAndHistoryFilePaths)
                {
                    var pageAndHistoryFileName = System.IO.Path.GetFileNameWithoutExtension(pageAndHistoryFilePath);
                    if(pageAndHistoryFileName != null)
                    {
                        var pageAndHistoryInfo = pageAndHistoryFileName.Split(';');
                        if(pageAndHistoryInfo.Length != 5 ||
                           pageAndHistoryInfo[0] != "p")
                        {
                            continue;
                        }
                        if(!includeSubmissions &&
                           pageAndHistoryInfo[4] != "0")
                        {
                            continue;
                        }
                    }

                    var page = Load<CLPPage>(pageAndHistoryFilePath, SerializationMode.Xml);

                    if(page.ID == notebook.CurrentPageID)
                    {
                        notebook.CurrentPage = page;
                    }

                    // BUG: loaded thumbnails don't let go of their disc reference.
                    //var thumbnailFilePath = Path.Combine(thumbnailsFolderPath, pageAndHistoryFileName + ".png");
                    //page.PageThumbnail = CLPImage.GetImageFromPath(thumbnailFilePath);

                    pages.Add(page);
                }

                var notebookPages = new List<CLPPage>();

                foreach(var notebookPage in pages)
                {
                    if(notebookPage.VersionIndex != 0)
                    {
                        continue;
                    }
                    notebookPages.Add(notebookPage);
                    if(!includeSubmissions)
                    {
                        continue;
                    }
                    foreach(var submission in pages)
                    {
                        if(submission.ID == notebookPage.ID &&
                           submission.OwnerID == notebookPage.OwnerID &&
                           submission.VersionIndex != 0)
                        {
                            notebookPage.Submissions.Add(submission);
                        }
                    }
                }

                notebook.Pages = new ObservableCollection<CLPPage>(notebookPages.OrderBy(x => x.PageNumber));

                var displaysFolderPath = Path.Combine(folderPath, "Displays");
                if(!Directory.Exists(displaysFolderPath))
                {
                    return notebook;
                }
                var displayFilePaths = Directory.EnumerateFiles(displaysFolderPath, "*.xml");
                var displays = new List<IDisplay>();
                foreach(var displayFilePath in displayFilePaths)
                {
                    var displayFileName = System.IO.Path.GetFileNameWithoutExtension(displayFilePath);
                    if(displayFileName == null)
                    {
                        continue;
                    }
                    var displayInfo = displayFileName.Split(';');
                    if(displayInfo.Length != 3)
                    {
                        continue;
                    }

                    var displayType = displayInfo[0];
                    switch(displayType)
                    {
                        case "grid":
                            var gridDisplay = GridDisplay.Load(displayFilePath, notebook);
                            displays.Add(gridDisplay);
                            break;
                        default:
                            continue;
                    }
                }

                notebook.Displays = new ObservableCollection<IDisplay>(displays.OrderBy(x => x.DisplayNumber));

                return notebook;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static Notebook OpenPartialNotebook(string folderPath, IEnumerable<string> pageIDs, IEnumerable<string> displayIDs, bool includeSubmissions = true)
        {
            try
            {
                var filePath = Path.Combine(folderPath, "notebook.xml");
                var notebook = Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(folderPath, "Pages");
                var pageAndHistoryFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                var pages = new List<CLPPage>();
                var pageIds = pageIDs as IList<string> ?? pageIDs.ToList();
                foreach(var pageAndHistoryFilePath in pageAndHistoryFilePaths)
                {
                    var pageAndHistoryFileName = System.IO.Path.GetFileNameWithoutExtension(pageAndHistoryFilePath);
                    var pageAndHistoryInfo = pageAndHistoryFileName.Split(';');
                    if(pageAndHistoryInfo.Length != 5 ||
                       pageAndHistoryInfo[0] != "p")
                    {
                        continue;
                    }
                    if(!includeSubmissions &&
                       pageAndHistoryInfo[4] != "0")
                    {
                        continue;
                    }
                    var isFilePartOfPartialPages = pageIds.Any(pageID => pageID == pageAndHistoryInfo[2]);

                    if(!isFilePartOfPartialPages)
                    {
                        continue;
                    }

                    var page = Load<CLPPage>(pageAndHistoryFilePath, SerializationMode.Xml);

                    if(page.ID == notebook.CurrentPageID)
                    {
                        notebook.CurrentPage = page;
                    }
                    pages.Add(page);
                }

                var notebookPages = new List<CLPPage>();

                foreach(var notebookPage in pages)
                {
                    if(notebookPage.VersionIndex != 0)
                    {
                        continue;
                    }
                    notebookPages.Add(notebookPage);
                    if(!includeSubmissions)
                    {
                        continue;
                    }
                    foreach(var submission in pages)
                    {
                        if(submission.ID == notebookPage.ID &&
                           submission.OwnerID == notebookPage.OwnerID &&
                           submission.DifferentiationLevel == notebookPage.DifferentiationLevel &&
                           submission.VersionIndex != 0)
                        {
                            notebookPage.Submissions.Add(submission);
                        }
                    }
                }

                notebook.Pages = new ObservableCollection<CLPPage>(notebookPages.OrderBy(x => x.PageNumber));

                var displaysFolderPath = Path.Combine(folderPath, "Displays");
                if(!Directory.Exists(displaysFolderPath))
                {
                    return notebook;
                }

                var displayFilePaths = Directory.EnumerateFiles(displaysFolderPath, "*.xml");
                var displays = new List<IDisplay>();
                foreach(var displayFilePath in displayFilePaths)
                {
                    var displayFileName = System.IO.Path.GetFileNameWithoutExtension(displayFilePath);
                    if(displayFileName == null)
                    {
                        continue;
                    }
                    var displayInfo = displayFileName.Split(';');
                    if(displayInfo.Length != 3)
                    {
                        continue;
                    }

                    var displayType = displayInfo[0];
                    switch(displayType)
                    {
                        case "grid":
                            var gridDisplay = GridDisplay.Load(displayFilePath, notebook);
                            if(gridDisplay == null)
                            {
                                continue;
                            }
                            displays.Add(gridDisplay);
                            break;
                        default:
                            continue;
                    }
                }

                notebook.Displays = new ObservableCollection<IDisplay>(displays.OrderBy(x => x.DisplayNumber));

                return notebook;
            }
            catch(Exception)
            {
                return null;
            }
        }

        #endregion //Cache

        
    }
}