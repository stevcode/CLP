using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public class NotebookNameComposite
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string OwnerName { get; set; }
        public string OwnerID { get; set; }
        public string OwnerTypeTag { get; set; }

        public string ToFolderName() { return string.Format("{0};{1};{2};{3}{4}", Name, ID, OwnerName, OwnerID, OwnerTypeTag == "U" ? string.Empty : ";" + OwnerTypeTag); }

        public static NotebookNameComposite ParseNotebook(Notebook notebook)
        {
            var nameComposite = new NotebookNameComposite
                                {
                                    Name = notebook.Name,
                                    ID = notebook.ID,
                                    OwnerName = notebook.Owner.FullName,
                                    OwnerID = notebook.Owner.ID,
                                    OwnerTypeTag = notebook.Owner == null ? "U" : notebook.Owner.ID == Person.Author.ID ? "A" : notebook.Owner.IsStudent ? "S" : "T"
                                };

            return nameComposite;
        }

        public static NotebookNameComposite ParseFolderPath(string notebookFolderPath)
        {
            var directoryInfo = new DirectoryInfo(notebookFolderPath);
            var notebookDirectoryName = directoryInfo.Name;
            var notebookDirectoryParts = notebookDirectoryName.Split(';');
            if (notebookDirectoryParts.Length != 5 &&
                notebookDirectoryParts.Length != 4)
            {
                return null;
            }

            var nameComposite = new NotebookNameComposite
                                {
                                    Name = notebookDirectoryParts[0],
                                    ID = notebookDirectoryParts[1],
                                    OwnerName = notebookDirectoryParts[2],
                                    OwnerID = notebookDirectoryParts[3],
                                    OwnerTypeTag = notebookDirectoryParts.Length == 5 ? notebookDirectoryParts[4] : "U"
                                };

            return nameComposite;
        }
    }

    [Serializable]
    public class Notebook : AEntityBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="Notebook" /> from scratch.</summary>
        public Notebook()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="Notebook" /> with name and owner.</summary>
        /// <param name="notebookName">The name of the notebook.</param>
        /// <param name="owner">The <see cref="Person" /> who owns the notebook.</param>
        public Notebook(string notebookName, Person owner)
            : this()
        {
            Name = notebookName;
            Owner = owner;
        }

        /// <summary>Initializes <see cref="Notebook" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Notebook(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="Notebook" />.</summary>
        /// <remarks>Composite Primary Key.</remarks>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof (string));

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="Notebook" />.</summary>
        /// <remarks>Composite Primary Key. Foreign Key.</remarks>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof (string), String.Empty);

        /// <summary>The <see cref="Person" /> who owns the <see cref="Notebook" />.</summary>
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
            }
        }

        public static readonly PropertyData OwnerProperty = RegisterProperty("Owner", typeof (Person));

        /// <summary>Date and Time the <see cref="Notebook" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof (DateTime));

        /// <summary>Date and Time the <see cref="Notebook" /> was last saved.</summary>
        /// <remarks>Type set to DateTime? (i.e. nullable DateTime) to allow NULL in database if LastSavedDate hasn't been set yet.</remarks>
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof (DateTime?));

        /// <summary>Name of the <see cref="Notebook" />.</summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof (string), String.Empty);

        /// <summary>Overall Curriculum the <see cref="Notebook" /> employs. Curriculum of individual pages may vary.</summary>
        public string Curriculum
        {
            get { return GetValue<string>(CurriculumProperty); }
            set { SetValue(CurriculumProperty, value); }
        }

        public static readonly PropertyData CurriculumProperty = RegisterProperty("Curriculum", typeof (string), String.Empty);

        /// <summary>List of all the HashIDs for each <see cref="CLPImage" /> that is in the notebook.</summary>
        public List<string> ImagePoolHashIDs
        {
            get { return Pages.SelectMany(page => page.PageObjects).OfType<CLPImage>().Select(image => image.ImageHashID).ToList().Distinct().ToList(); }
        }

        #region Navigation Properties

        /// <summary>Unique Identifier of the currently selected <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Foreign Key for CurrentPage.</remarks>
        public string CurrentPageID
        {
            get { return GetValue<string>(CurrentPageIDProperty); }
            set { SetValue(CurrentPageIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageIDProperty = RegisterProperty("CurrentPageID", typeof (string));

        /// <summary>Unique Identifier of the <see cref="Person" /> who owns the currently selected <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Foreign Key for CurrentPage.</remarks>
        public string CurrentPageOwnerID
        {
            get { return GetValue<string>(CurrentPageOwnerIDProperty); }
            set { SetValue(CurrentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageOwnerIDProperty = RegisterProperty("CurrentPageOwnerID", typeof (string));

        /// <summary>Version Index of the currently selected <see cref="CLPPage" />.</summary>
        /// <remarks>Composite Foreign Key for CurrentPage.</remarks>
        public uint CurrentPageVersionIndex
        {
            get { return GetValue<uint>(CurrentPageVersionIndexProperty); }
            set { SetValue(CurrentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData CurrentPageVersionIndexProperty = RegisterProperty("CurrentPageVersionIndex", typeof (uint));

        /// <summary>Currently selected <see cref="CLPPage" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        [XmlIgnore]
        //  [ExcludeFromSerialization]
        public virtual CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set
            {
                SetValue(CurrentPageProperty, value);
                if (value == null)
                {
                    return;
                }
                CurrentPageID = value.ID;
                CurrentPageOwnerID = value.OwnerID;
                CurrentPageVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof (CLPPage));

        /// <summary>Collection of all the <see cref="CLPPage" />s in the <see cref="Notebook" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        [XmlIgnore]
        //   [ExcludeFromSerialization]
        public virtual ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof (ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>List of the <see cref="IDisplay" />s in the <see cref="Notebook" />.</summary>
        /// <remarks>Virtual to facilitate lazy loading of navigation property by Entity Framework.</remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof (ObservableCollection<IDisplay>), () => new ObservableCollection<IDisplay>());

        #endregion //Navigation Properties

        #endregion //Properties

        #region Methods

        public void AddPage(CLPPage page)
        {
            page.PageNumber = Pages.Any() ? Pages.Last().PageNumber + 1 : 1;
            Pages.Add(page);
            CurrentPage = page;
        }

        public void AddDisplay(IDisplay display)
        {
            display.NotebookID = ID;
            display.DisplayNumber = Displays.Any(d => d.GetType() == display.GetType()) ? Displays.Last().DisplayNumber + 1 : 1;
            Displays.Add(display);
        }

        public void InsertPageAt(int index, CLPPage page)
        {
            Pages.Insert(index, page);
            GeneratePageNumbers();
            CurrentPage = page;
        }

        private List<CLPPage> _trashedPages = new List<CLPPage>();

        public void RemovePageAt(int index)
        {
            if (Pages.Count <= index ||
                index < 0)
            {
                return;
            }

            if (Pages.Count == 1)
            {
                var newPage = new CLPPage(Person.Author)
                              {
                                  PageNumber = Pages.Any() ? Pages.First().PageNumber : 1
                              };

                Pages.Add(newPage);
            }

            int newIndex;
            if (index + 1 < Pages.Count)
            {
                newIndex = index + 1;
            }
            else
            {
                newIndex = index - 1;
            }

            var nextPage = Pages.ElementAt(newIndex);
            CurrentPage = nextPage;
            if (index == 0)
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
            foreach (var page in Pages)
            {
                if (lastPage == null ||
                    page.ID != lastPage.ID)
                {
                    initialPageNumber++;
                }
                if (page.PageNumber != 999) // TODO: less stupid special case for exit tickets?
                {
                    page.PageNumber = initialPageNumber;
                }
                lastPage = page;
            }
        }

        public Notebook CopyForNewOwner(Person owner)
        {
            var newNotebook = this.DeepCopy();
            if (newNotebook == null)
            {
                return null;
            }
            newNotebook.Owner = owner;
            newNotebook.CurrentPage = CurrentPage == null ? null : CurrentPage.CopyForNewOwner(owner);
            foreach (var newPage in Pages.Select(page => page.CopyForNewOwner(owner)))
            {
                if (!owner.IsStudent)
                {
                    newNotebook.Pages.Add(newPage);
                    continue;
                }

                if (newPage.DifferentiationLevel == String.Empty ||
                    newPage.DifferentiationLevel == "0" ||
                    newPage.DifferentiationLevel == owner.CurrentDifferentiationGroup)
                {
                    newNotebook.Pages.Add(newPage);
                    continue;
                }

                if (owner.CurrentDifferentiationGroup == String.Empty &&
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
            var notebookPage =
                Pages.FirstOrDefault(x => x.ID == pageID && x.OwnerID == pageOwnerID && x.DifferentiationLevel == differentiationLevel && x.VersionIndex == versionIndex);
            if (notebookPage != null)
            {
                return notebookPage;
            }

            notebookPage = Pages.FirstOrDefault(x => x.ID == pageID && x.DifferentiationLevel == differentiationLevel);
            return notebookPage == null ? null : notebookPage.Submissions.FirstOrDefault(x => x.OwnerID == pageOwnerID && x.VersionIndex == versionIndex);
        }

        #endregion //Methods

        #region Cache

        public void ToXML(string notebookFilePath)
        {
            LastSavedDate = DateTime.Now;
            var fileInfo = new FileInfo(notebookFilePath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(notebookFilePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public void SaveToXML(string folderPath)
        {
            var filePath = Path.Combine(folderPath, "notebook.xml");
            ToXML(filePath);
        }

        public static Notebook LoadFromXML(string notebookFolderPath)
        {
            try
            {
                var nameComposite = NotebookNameComposite.ParseFolderPath(notebookFolderPath);
                var notebookFilePath = Path.Combine(notebookFolderPath, "notebook.xml");
                if (nameComposite == null ||
                    !File.Exists(notebookFilePath))
                {
                    return null;
                }

                var notebook = Load<Notebook>(notebookFilePath, SerializationMode.Xml);
                if (notebook == null)
                {
                    return null;
                }

                notebook.Name = nameComposite.Name;

                return notebook;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion //Cache 

        public void SavePartialNotebook(string folderPath, bool serializeInkStrokes = true)
        {
            var fileName = Path.Combine(folderPath, "notebook.xml");
            ToXML(fileName);

            var pagesFolderPath = Path.Combine(folderPath, "Pages");
            if (!Directory.Exists(pagesFolderPath))
            {
                Directory.CreateDirectory(pagesFolderPath);
            }

            foreach (var page in Pages)
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
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var page in Pages)
            {
                foreach (var submission in page.Submissions)
                {
                    var pageFilePath = Path.Combine(folderPath,
                                                    "p;" + submission.PageNumber + ";" + submission.ID + ";" + submission.DifferentiationLevel + ";" + submission.VersionIndex +
                                                    ".xml");
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
            foreach (var page in Pages)
            {
                foreach (var submission in page.Submissions)
                {
                    var notebookFolderPaths = Directory.EnumerateDirectories(folderPath);
                    foreach (var notebookFolderPath in notebookFolderPaths)
                    {
                        if (notebookFolderPath.Contains(submission.OwnerID))
                        {
                            var pagesPath = Path.Combine(notebookFolderPath, "Pages");
                            var pageFilePath = Path.Combine(pagesPath,
                                                            "p;" + submission.PageNumber + ";" + submission.ID + ";" + submission.DifferentiationLevel + ";" +
                                                            submission.VersionIndex + ".xml");
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
    }
}