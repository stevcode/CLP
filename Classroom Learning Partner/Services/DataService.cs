using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel;
using Catel.Collections;
using Catel.Reflection;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{
    public class DataService : IDataService
    {
        #region Nested Classes

        public class ZipEntrySaver
        {
            public ZipEntrySaver(AInternalZipEntryFile entryFile, string parentNotebookName = "")
            {
                EntryFile = entryFile;
                InternalFilePath = entryFile.GetZipEntryFullPath(parentNotebookName);
                JsonString = entryFile.ToJsonString();
            }

            public AInternalZipEntryFile EntryFile { get; set; }
            private string InternalFilePath { get; set; }
            private string JsonString { get; set; }

            public void UpdateEntry(ZipFile zip)
            {
                zip.UpdateEntry(InternalFilePath, JsonString);
            }
        }

        public class PageZipEntryLoader
        {
            public PageZipEntryLoader(string jsonString, int pageNumber)
            {
                JsonString = jsonString;
                PageNumber = pageNumber;
            }

            public string JsonString { get; set; }
            public int PageNumber { get; set; }
        }

        #endregion // Nested Classes

        #region Constants

        private const string DEFAULT_CLP_DATA_FOLDER_NAME = "CLPData";
        private const string DEFAULT_CACHE_FOLDER_NAME = "Cache";
        private const string DEFAULT_TEMP_CACHE_FOLDER_NAME = "TempCache";
        private const string DEFAULT_CONFIG_FOLDER_NAME = "Config"; // Config Service?
        private const string DEFAULT_ARCHIVE_FOLDER_NAME = "Archive";
        private const string DEFAULT_LOGS_FOLDER_NAME = "Logs";

        #endregion // Constants

        #region Constructors

        public DataService()
        {
            CurrentCLPDataFolderPath = DefaultCLPDataFolderPath;
        }

        #endregion // Constructors

        #region IDataService Implementation

        #region Properties

        #region Current Folder Paths

        public string CurrentCLPDataFolderPath { get; set; }

        public string CurrentCacheFolderPath
        {
            get
            {
                var folderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_CACHE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        public string CurrentTempCacheFolderPath
        {
            get
            {
                var folderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_TEMP_CACHE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        public string CurrentArchiveFolderPath
        {
            get
            {
                var folderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_ARCHIVE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        #endregion // Current Folder Paths

        #region Cache Properties

        public List<FileInfo> AvailableZipContainerFileInfos
        {
            get
            {
                // ReSharper disable once ConvertPropertyToExpressionBody
                return GetCLPContainersInFolder(CurrentCacheFolderPath);
            }
        }

        public bool IsAutoSaveOn { get; set; } = true;

        #endregion // Cache Properties

        #region Program Properties

        /// <summary>ImagePool for the current CLP instance, populated by all open notebooks.</summary>
        public Dictionary<string, BitmapImage> ImagePool { get; } = new Dictionary<string, BitmapImage>();

        #endregion // Program Properties

        #region Notebook Properties

        //AvailableNotebookSets   
        //LoadedNotebookSets      //Differentiate between a .clp file and the individual notebooks within?
        
        public List<Notebook> LoadedNotebooks { get; } = new List<Notebook>();

        public ClassRoster CurrentClassRoster { get; private set; }
        public NotebookSet CurrentNotebookSet { get; private set; }
        public Notebook CurrentNotebook { get; private set; }
        public IDisplay CurrentDisplay { get; private set; }
        public CLPPage CurrentPage { get; private set; }

        #endregion // Notebook Properties

        #region Page Properties

        //CurrentNotebookPage
        //CurrentSubmissionPage
        //CurrentSelectedPage

        #endregion // Page Properties

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> CurrentClassRosterChanged;
        public event EventHandler<EventArgs> CurrentNotebookChanged;
        public event EventHandler<EventArgs> CurrentDisplayChanged;
        public event EventHandler<EventArgs> CurrentPageChanged;

        #endregion // Events

        #region Methods

        #region Cache Methods

        public void SaveLocal()
        {
            var zipContainerFilePath = CurrentNotebook.ContainerZipFilePath;
            var parentNotebookName = CurrentNotebook.InternalZipFileDirectoryName;
            var entryList = new List<ZipEntrySaver>
                            {
                                new ZipEntrySaver(CurrentClassRoster, parentNotebookName),
                                new ZipEntrySaver(CurrentNotebook, parentNotebookName)
                            };

            foreach (var page in CurrentNotebook.Pages)
            {
                entryList.Add(new ZipEntrySaver(page, parentNotebookName));
            }

            if (File.Exists(zipContainerFilePath))
            {
                //var readOptions = new ReadOptions
                //                  {
                //                      ReadProgress = Zip_ReadProgress
                //                  };

                //var zip = ZipFile.Read(fullFilePath, readOptions)

                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    // TODO: Test if needed. Won't work unless zip has been saved.
                    // Implied that entries are not added to zip.Entries until saved. Need to verify. Code definitely says added to internal _entries before save, so test this
                    //zip.SelectEntries("*.json");
                    //zip.SelectEntries("p;*.json", "blah/blah/pages/"); test this.

                    //zip.UpdateFile only applies to adding a file from the disc to the zip archive, N/A for clp unless we need it for images?
                    //          for images, probably zip.AddEntry(entryPath, memoryStream); also have byte[] byteArray for content

                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    //zip.UseZip64WhenSaving = Zip64Option.Always;  Only one that seems persistent, but need to test
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save();
                }
            }
            else
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save(zipContainerFilePath);
                }
            }

            foreach (var zipEntrySaver in entryList)
            {
                zipEntrySaver.EntryFile.IsSavedLocally = true;
            }
        }

        private void Zip_ReadProgress(object sender, ReadProgressEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion // Cache Methods

        #region Image Methods

        private readonly object _zipLock = new object();

        public BitmapImage GetImage(string imageHashID, IPageObject pageObject)
        {
            if (ImagePool.ContainsKey(imageHashID))
            {
                return ImagePool[imageHashID];
            }

            var parentPage = pageObject.ParentPage;
            if (parentPage == null)
            {
                return null;
            }

            lock (_zipLock)
            {
                var containerZipFilePath = parentPage.ContainerZipFilePath;

                using (var zip = ZipFile.Read(containerZipFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    var entry = GetImageEntryFromImageHashID(zip, imageHashID);
                    if (entry == null)
                    {
                        return null;
                    }

                    using (var ms = new MemoryStream())
                    {
                        entry.Extract(ms);

                        var genBmpImage = new BitmapImage();

                        genBmpImage.BeginInit();
                        genBmpImage.CacheOption = BitmapCacheOption.OnLoad;
                        genBmpImage.StreamSource = ms;
                        genBmpImage.EndInit();

                        genBmpImage.Freeze();

                        ImagePool.Add(imageHashID, genBmpImage);

                        return genBmpImage;
                    }
                }
            }
        }

        public string SaveImageToImagePool(string imageFilePath, CLPPage page)
        {
            try
            {
                var bytes = File.ReadAllBytes(imageFilePath);

                var md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(bytes);
                var imageHashID = Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Replace("=", "");
                if (ImagePool.ContainsKey(imageHashID))
                {
                    return imageHashID;
                }

                var bitmapImage = CLPImage.GetImageFromPath(imageFilePath);
                if (bitmapImage == null)
                {
                    MessageBox.Show("Failed to load image..");
                    return null;
                }

                var newFileName = $"{imageHashID};{Path.GetFileNameWithoutExtension(imageFilePath)}{Path.GetExtension(imageFilePath)}";
                var internalFilePath = ZipExtensions.CombineEntryDirectoryAndName(AInternalZipEntryFile.ZIP_IMAGES_FOLDER_NAME, newFileName);
                var containerZipFilePath = page.ContainerZipFilePath;

                using (var zip = ZipFile.Read(containerZipFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    if (!zip.ContainsEntry(internalFilePath))
                    {
                        var entry = zip.AddFile(imageFilePath);
                        entry.FileName = internalFilePath;
                        zip.Save();
                    }
                }

                ImagePool.Add(imageHashID, bitmapImage);
                return imageHashID;
            }
            catch (Exception)
            {
                MessageBox.Show("Error opening image file. Please try again.");
            }

            return null;
        }

        #endregion // Image Methods

        #region ClassRoster Methods

        public void SetCurrentClassRoster(ClassRoster classRoster)
        {
            CurrentClassRoster = classRoster;
            CurrentClassRosterChanged.SafeInvoke(this);
        }

        #endregion // ClassRoster Methods

        #region Notebook Methods

        public void SetCurrentNotebook(Notebook notebook)
        {
            if (CurrentNotebook != null)
            {
                SavePage(CurrentNotebook, CurrentNotebook.CurrentPage);
            }

            CurrentNotebook = notebook;
            CurrentNotebookChanged.SafeInvoke(this);

            if (CurrentNotebook.CurrentPage == null)
            {
                return;
            }

            SetCurrentPage(CurrentNotebook.CurrentPage);
        }

        public void CreateAuthorNotebook(string notebookName, string zipContainerFilePath)
        {
            var notebookSet = new NotebookSet(notebookName);
            var notebook = new Notebook(notebookSet, Person.Author)
            {
                ContainerZipFilePath = zipContainerFilePath
            };

            if (!File.Exists(zipContainerFilePath))
            {
                CreateEmptyZipContainer(zipContainerFilePath);
                CurrentClassRoster = new ClassRoster();
            }
            else
            {
                CurrentClassRoster = LoadClassRosterFromCLPContainer(zipContainerFilePath);
            }

            CurrentClassRoster.ContainerZipFilePath = zipContainerFilePath;
            CurrentClassRoster.ListOfNotebookSets.Add(notebookSet);
            SaveClassRoster(CurrentClassRoster);

            SetCurrentNotebook(notebook);
            AddPage(notebook, new CLPPage(Person.Author));
        }

        public void LoadNotebook(Notebook notebook, List<int> pageNumbers, bool isLoadingStudentNotebooks = true)
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            var classRoster = LoadClassRosterFromCLPContainer(zipContainerFilePath);
            SetCurrentClassRoster(classRoster);

            LoadPagesIntoNotebook(notebook, pageNumbers);
            LoadedNotebooks.Add(notebook);

            if (!owner.IsStudent &&
                owner.ID != Person.AUTHOR_ID &&
                isLoadingStudentNotebooks)
            {
                var otherNotebooks = LoadAllNotebooksFromCLPContainer(zipContainerFilePath);
                foreach (var studentNotebook in otherNotebooks.Where(n => n.Owner.IsStudent && classRoster.ListOfStudents.Any(p => n.Owner.DisplayName == p.DisplayName)))
                {
                    LoadPagesIntoNotebook(studentNotebook, pageNumbers);
                    LoadedNotebooks.Add(studentNotebook);
                }
            }

            SetCurrentNotebook(notebook);
        }

        #endregion // Notebook Methods

        #region Display Methods

        public void SetCurrentDisplay(IDisplay display)
        {
            CurrentDisplay = display;

            CurrentDisplayChanged.SafeInvoke(this);
        }

        public void AddDisplay(Notebook notebook, IDisplay display)
        {
            display.NotebookID = notebook.ID;
            display.DisplayNumber = notebook.Displays.Any(d => d.GetType() == display.GetType()) ? notebook.Displays.Last().DisplayNumber + 1 : 1;
            notebook.Displays.Add(display);
        }

        public void AddPageToCurrentDisplay(CLPPage page, bool isSavingOldPage = true)
        {
            if (CurrentNotebook == null)
            {
                return;
            }


            
        }

        #endregion // Display Methods

        #region Page Methods

        public void SetCurrentPage(CLPPage page, bool isSavingOldPage = true)
        {
            if (CurrentNotebook == null)
            {
                return;
            }

            var oldPage = CurrentNotebook.CurrentPage;
            if (oldPage != null &&
                isSavingOldPage)
            {
                AutoSavePage(CurrentNotebook, oldPage);
            }

            CurrentNotebook.CurrentPage = page;
            CurrentPage = page;

            // TODO: Handle multiDisplays
            //var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            //if (notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //if (notebookWorkspaceViewModel.CurrentDisplay == null)
            //{
            //    //Take thumbnail of page before navigating away from it.
            //    ACLPPageBaseViewModel.TakePageThumbnail(CurrentPage);
            //    ACLPPageBaseViewModel.ClearAdorners(CurrentPage);
            //    CurrentPage = page;
            //    return;
            //}

            //notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);

            CurrentPageChanged.SafeInvoke(this);
        }

        public void AddPage(Notebook notebook, CLPPage page)
        {
            page.ContainerZipFilePath = notebook.ContainerZipFilePath;
            page.PageNumber = notebook.Pages.Any() ? notebook.Pages.Last().PageNumber + 1 : 1;
            notebook.Pages.Add(page);
            SetCurrentPage(page);
            SavePage(notebook, page);
        }

        public void InsertPageAt(Notebook notebook, CLPPage page, int index)
        {
            ChangePageNumbersAfterGivenPage(notebook, index, true);
            page.ContainerZipFilePath = notebook.ContainerZipFilePath;
            page.PageNumber = index + 1;
            notebook.Pages.Insert(index, page);
            SetCurrentPage(page);
            SavePage(notebook, page);
        }

        public void DeletePage(Notebook notebook, CLPPage page)
        {
            var pageIndex = notebook.Pages.IndexOf(page);
            DeletePageAt(notebook, pageIndex);
        }

        public void DeletePageAt(Notebook notebook, int index)
        {
            //TODO: Delete page from notebook
            //delete page's json
            //renumber existing pages
            //function with full cache

            if (notebook.Pages.Count <= index ||
                index < 0)
            {
                return;
            }

            var pageToDelete = notebook.Pages[index];
            notebook.Pages.RemoveAt(index);

            var zipContainerFilePath = notebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.RemoveEntry(pageToDelete.GetZipEntryFullPath(notebook.Owner.ParentNotebookFolderName));
                zip.Save();
            }

            ChangePageNumbersAfterGivenPage(notebook, index, false);

            if (!notebook.Pages.Any())
            {
                var newPage = new CLPPage(Person.Author)
                {
                    PageNumber = notebook.Pages.Any() ? notebook.Pages.First().PageNumber : 1
                };

                notebook.Pages.Add(newPage);
                SavePage(notebook, newPage);
            }

            if (index >= notebook.Pages.Count)
            {
                index = notebook.Pages.Count - 1;
            }

            var nextPage = notebook.Pages.ElementAt(index);
            SetCurrentPage(nextPage, false);
        }

        public void AutoSavePage(Notebook notebook, CLPPage page)
        {
            if (!IsAutoSaveOn)
            {
                return;
            }

            SavePage(notebook, page);
            //TODO: take screenshot of page if not already cached
            //set LastAutoSaveTime of notebook
            //save page locally, and to export folder
            //save page async to teacher machine, and partial cache folder
        }

        #endregion // Page Methods

        #endregion // Methods

        #endregion // IDataService Implementation

        #region Static Properties

        #region Special Folder Paths

        public static string WindowsDriveFolderPath => Path.GetPathRoot(Environment.SystemDirectory);

        public static string DesktopFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string CLPProgramFolderPath => typeof(DataService).Assembly.GetDirectory();

        #endregion // Special Folder Paths

        #region Default Folder Paths

        public static string DefaultCLPDataFolderPath
        {
            get
            {
                var folderPath = Path.Combine(WindowsDriveFolderPath, DEFAULT_CLP_DATA_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        #endregion // Default Folder Paths

        #endregion // Static Properties

        #region Static Methods

        #region Files

        public static string ValidateFileNameString(string name)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            return invalidFileNameCharacters.Aggregate(name, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static List<FileInfo> GetCLPContainersInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return new List<FileInfo>();
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            return directoryInfo.GetFiles("*.clp").ToList();
        }

        public static void CreateEmptyZipContainer(string zipContainerFilePath)
        {
            if (File.Exists(zipContainerFilePath))
            {
                MessageBox.Show(".clp file with that name already exists.");
                return;
            }

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                zip.Save(zipContainerFilePath);
            }
        }

        #endregion // Files

        #region Entry

        public static List<PageZipEntryLoader> GetPageZipEntryLoadersFromEntries(List<ZipEntry> entries)
        {
            var pageZipEntryLoaders = new List<PageZipEntryLoader>();
            foreach (var entry in entries)
            {
                var jsonString = entry.ExtractJsonString();
                var nameCompositeString = entry.GetEntryNameWithoutExtension();
                var pageNameComposite = CLPPage.NameComposite.ParseFromString(nameCompositeString);
                var pageNumber = pageNameComposite.PageNumber;
                var pageZipEntryLoader = new PageZipEntryLoader(jsonString, pageNumber);
                pageZipEntryLoaders.Add(pageZipEntryLoader);
            }

            return pageZipEntryLoaders;
        }

        public static T LoadJsonEntry<T>(string zipContainerFilePath, string entryPath) where T : AInternalZipEntryFile
        {
            try
            {
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    var entry = zip.GetEntry(entryPath);
                    var zipEntryFile = entry.ExtractJsonEntity<T>();
                    zipEntryFile.ContainerZipFilePath = zipContainerFilePath;

                    return zipEntryFile;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void SaveAInternalZipEntryFile(AInternalZipEntryFile entryFile, string parentNotebookName = "")
        {
            if (!File.Exists(entryFile.ContainerZipFilePath))
            {
                return;
            }

            SaveZipEntry(entryFile.ContainerZipFilePath, new ZipEntrySaver(entryFile, parentNotebookName));
        }

        public static void SaveAInternalZipEntryFiles(List<AInternalZipEntryFile> entryFiles, string parentNotebookName = "")
        {
            var groupedEntryFiles = entryFiles.GroupBy(e => e.ContainerZipFilePath);

            foreach (var entryFileGroup in groupedEntryFiles)
            {
                var zipContainerFilePath = entryFileGroup.Key;
                if (!File.Exists(zipContainerFilePath))
                {
                    continue;
                }

                var zipEntrySavers = entryFileGroup.Select(entryFile => new ZipEntrySaver(entryFile, parentNotebookName)).ToList();
                SaveZipEntries(zipContainerFilePath, zipEntrySavers);
            }
        }

        public static void SaveZipEntry(string zipContainerFilePath, ZipEntrySaver zipEntrySaver)
        {
            SaveZipEntries(zipContainerFilePath, new List<ZipEntrySaver> { zipEntrySaver });
        }

        public static void SaveZipEntries(string zipContainerFilePath, List<ZipEntrySaver> zipEntrySavers)
        {
            try
            {
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in zipEntrySavers)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save(zipContainerFilePath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion // Entry

        #region Images

        public static ZipEntry GetImageEntryFromImageHashID(ZipFile zip, string imageHashID)
        {
            try
            {
                var internalImagesDirectory = $"{AInternalZipEntryFile.ZIP_IMAGES_FOLDER_NAME}/";
                var allImageEntries = zip.GetEntriesInDirectory(internalImagesDirectory).ToList();
                var imageEntry = (from entry in allImageEntries
                                  let hashID = entry.GetEntryNameWithoutExtension().Split(';')[0]
                                  where hashID == imageHashID
                                  select entry).FirstOrDefault();

                return imageEntry;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion // Images

        #region Class Roster

        public static ClassRoster LoadClassRosterFromCLPContainer(string fullFilePath)
        {
            var fileInfo = new FileInfo(fullFilePath);
            return LoadClassRosterFromCLPContainer(fileInfo);
        }

        public static ClassRoster LoadClassRosterFromCLPContainer(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return null;
            }

            if (!fileInfo.Exists ||
                fileInfo.Extension != ".clp")
            {
                return null;
            }

            var entryPath = $"{ClassRoster.DEFAULT_INTERNAL_FILE_NAME}.json";

            return LoadJsonEntry<ClassRoster>(fileInfo.FullName, entryPath);
        }

        public static void SaveClassRoster(ClassRoster classRoster)
        {
            SaveAInternalZipEntryFile(classRoster);
        }

        #endregion // Class Roster

        #region Session

        public static List<Session> LoadAllSessionsFromCLPContainer(string zipContainerFilePath)
        {
            var sessions = new List<Session>();
            try
            {
                var sessionStrings = new List<string>();
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    var internalSessionsDirectory = $"{AInternalZipEntryFile.ZIP_SESSIONS_FOLDER_NAME}/";
                    var sessionEntries = zip.GetEntriesInDirectory(internalSessionsDirectory);
                    sessionStrings.AddRange(sessionEntries.Select(sessionEntry => sessionEntry.ExtractJsonString()));
                }

                foreach (var sessionString in sessionStrings)
                {
                    var session = AEntityBase.FromJsonString<Session>(sessionString);
                    session.ContainerZipFilePath = zipContainerFilePath;
                    sessions.Add(session);
                }

                return sessions.OrderByDescending(s => s.StartTime).ToList();
            }
            catch (Exception)
            {
                return sessions;
            }
        }

        public static void SaveSession(Session session)
        {
            SaveAInternalZipEntryFile(session);
        }

        #endregion // Session

        #region Notebook

        public static void GenerateClassNotebooks(Notebook authorNotebook, ClassRoster classRoster)
        {
            var zipContainerFilePath = authorNotebook.ContainerZipFilePath;

            var entryList = new List<ZipEntrySaver>();

            // TODO: Parallel?
            foreach (var teacher in classRoster.ListOfTeachers)
            {
                var notebook = CopyNotebookForNewOwner(authorNotebook, teacher);
                var parentNotebookName = notebook.InternalZipFileDirectoryName;
                entryList.Add(new ZipEntrySaver(notebook, parentNotebookName));
                foreach (var authorPage in authorNotebook.Pages)
                {
                    var page = CopyPageForNewOwner(authorPage, teacher);
                    entryList.Add(new ZipEntrySaver(page, parentNotebookName));
                }
            }

            // TODO: Parallel?
            foreach (var student in classRoster.ListOfStudents)
            {
                var notebook = CopyNotebookForNewOwner(authorNotebook, student);
                var parentNotebookName = notebook.InternalZipFileDirectoryName;
                entryList.Add(new ZipEntrySaver(notebook, parentNotebookName));
                foreach (var authorPage in authorNotebook.Pages)
                {
                    var page = CopyPageForNewOwner(authorPage, student);
                    entryList.Add(new ZipEntrySaver(page, parentNotebookName));
                }
            }

            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }
        }

        public static Notebook CopyFullNotebookForNewOwner(Notebook originalNotebook, Person newOwner)
        {
            var newNotebook = CopyNotebookForNewOwner(originalNotebook, newOwner);
            foreach (var originalPage in originalNotebook.Pages)
            {
                var newPage = CopyPageForNewOwner(originalPage, newOwner);
                newNotebook.Pages.Add(newPage);
            }

            return newNotebook;
        }

        public static Notebook CopyNotebookForNewOwner(Notebook notebook, Person newOwner)
        {
            var newNotebook = notebook.DeepCopy();
            newNotebook.Owner = newOwner;
            newNotebook.GenerationDate = DateTime.Now;
            newNotebook.LastSavedDate = DateTime.Now;

            return newNotebook;
        }

        public static List<Notebook> LoadAllNotebooksFromCLPContainer(string zipContainerFilePath)
        {
            var notebooks = new List<Notebook>();
            try
            {
                var notebookStrings = new List<string>();
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    var notebookEntries = zip.SelectEntries($"*{Notebook.DEFAULT_INTERNAL_FILE_NAME}.json");
                    notebookStrings.AddRange(notebookEntries.Select(notebookEntry => notebookEntry.ExtractJsonString()));
                }

                foreach (var notebookString in notebookStrings)
                {
                    var notebook = AEntityBase.FromJsonString<Notebook>(notebookString);
                    notebook.ContainerZipFilePath = zipContainerFilePath;
                    notebooks.Add(notebook);
                }

                return notebooks;
            }
            catch (Exception)
            {
                return notebooks;
            }
        }

        public static void LoadPagesIntoNotebook(Notebook notebook, List<int> pageNumbers)
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;

            var pageZipEntryLoaders = new List<PageZipEntryLoader>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                List<ZipEntry> pageEntries;
                if (pageNumbers.Any())
                {
                    var pageIDs = GetPageIDsFromPageNumbers(zip, owner, pageNumbers);
                    pageEntries = GetPageEntriesFromPageIDs(zip, owner, pageIDs);
                }
                else
                {
                    pageEntries = GetAllPageEntriesInNotebook(zip, owner);
                }

                pageZipEntryLoaders = GetPageZipEntryLoadersFromEntries(pageEntries);
            }

            var pages = GetPagesFromPageZipEntryLoaders(pageZipEntryLoaders, zipContainerFilePath).OrderBy(p => p.PageNumber).ToList();

            if (owner.IsStudent)
            {
                var submissions = GetSubmissionsForPages(notebook, pages);
                foreach (var submission in submissions)
                {
                    var page = pages.FirstOrDefault(p => p.ID == submission.ID && p.DifferentiationLevel == submission.DifferentiationLevel && p.SubPageNumber == submission.SubPageNumber);
                    if (page != null)
                    {
                        page.Submissions.Add(submission);
                    }
                }
            }

            notebook.Pages.AddRange(pages);
            notebook.CurrentPage = pages.FirstOrDefault(p => p.ID == notebook.CurrentPageID) ?? pages.FirstOrDefault();
        }

        public static void SaveNotebook(Notebook notebook)
        {
            SaveAInternalZipEntryFile(notebook);
        }

        #endregion // Notebook

        #region Page

        public static CLPPage CopyPageForNewOwner(CLPPage page, Person newOwner)
        {
            var newPage = page.DeepCopy();
            newPage.Owner = newOwner;

            return newPage;
        }

        public static List<string> GetAllPageIDsInNotebook(ZipFile zip, Person notebookOwner)
        {
            var internalPagesDirectory = notebookOwner.NotebookPagesFolderPath;
            var pageEntryNames = zip.GetEntriesInDirectory(internalPagesDirectory).Select(e => e.GetEntryNameWithoutExtension()).ToList();
            var pageNameComposites = pageEntryNames.Select(CLPPage.NameComposite.ParseFromString).ToList();
            var pageIDs = pageNameComposites.Select(nc => nc.ID).Distinct().ToList();

            return pageIDs;
        }

        public static List<string> GetPageIDsFromPageNumbers(ZipFile zip, Person notebookOwner, List<int> pageNumbers)
        {
            var internalPagesDirectory = notebookOwner.NotebookPagesFolderPath;
            var pageEntryNames = zip.GetEntriesInDirectory(internalPagesDirectory).Select(e => e.GetEntryNameWithoutExtension()).ToList();
            var pageNameComposites = pageEntryNames.Select(CLPPage.NameComposite.ParseFromString).ToList();
            var pageIDs = pageNameComposites.Where(nc => pageNumbers.Contains(nc.PageNumber)).Select(nc => nc.ID).Distinct().ToList();

            return pageIDs;
        }

        // TODO: GetAllPageNumbersInNotebook?

        public static Dictionary<string, int> GetPageNumbersFromPageIDs(ZipFile zip, List<string> pageIDs)
        {
            var internalAuthorPagesDirectory = Person.Author.NotebookPagesFolderPath;
            var pageEntryNames = zip.GetEntriesInDirectory(internalAuthorPagesDirectory).Select(e => e.GetEntryNameWithoutExtension()).ToList();
            var pageNameComposites = pageEntryNames.Select(CLPPage.NameComposite.ParseFromString).ToList();
            var mappedIDs = new Dictionary<string, int>();
            foreach (var pageNameComposite in pageNameComposites)
            {
                if (!pageIDs.Contains(pageNameComposite.ID))
                {
                    continue;
                }

                if (!mappedIDs.ContainsKey(pageNameComposite.ID))
                {
                    mappedIDs.Add(pageNameComposite.ID, pageNameComposite.PageNumber);
                }
            }

            return mappedIDs;
        }

        public static List<ZipEntry> GetAllPageEntriesInNotebook(ZipFile zip, Person notebookOwner)
        {
            var internalPagesDirectory = notebookOwner.NotebookPagesFolderPath;
            var pageEntries = zip.GetEntriesInDirectory(internalPagesDirectory).ToList();

            return pageEntries;
        }

        public static List<ZipEntry> GetPageEntriesFromPageIDs(ZipFile zip, Person notebookOwner, List<string> pageIDs, bool isSubmissions = false)
        {
            var internalPagesDirectory = isSubmissions ? notebookOwner.NotebookSubmissionsFolderPath : notebookOwner.NotebookPagesFolderPath;
            var allPageEntries = zip.GetEntriesInDirectory(internalPagesDirectory).ToList();
            var pageEntries = (from pageEntry in allPageEntries
                               let nameComposite = CLPPage.NameComposite.ParseFromString(pageEntry.GetEntryNameWithoutExtension())
                               where pageIDs.Contains(nameComposite.ID)
                               select pageEntry).ToList();

            return pageEntries;
        }

        public static List<ZipEntry> GetPageEntriesFromPageNumbers(ZipFile zip, Person notebookOwner, List<int> pageNumbers)
        {
            var pageIDs = GetPageIDsFromPageNumbers(zip, notebookOwner, pageNumbers);
            var pageEntries = GetPageEntriesFromPageIDs(zip, notebookOwner, pageIDs);

            return pageEntries;
        }

        public static List<CLPPage> GetPagesFromPageZipEntryLoaders(List<PageZipEntryLoader> pageZipEntryLoaders, string zipContainerFilePath)
        {
            var pages = new List<CLPPage>();
            foreach (var pageZipEntryLoader in pageZipEntryLoaders)
            {
                var page = AEntityBase.FromJsonString<CLPPage>(pageZipEntryLoader.JsonString);
                page.ContainerZipFilePath = zipContainerFilePath;
                page.PageNumber = pageZipEntryLoader.PageNumber;
                pages.Add(page);
            }

            return pages;
        }

        public static List<CLPPage> GetSubmissionsForPages(Notebook notebook, List<CLPPage> pages)
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;

            var pageIDs = pages.Select(p => p.ID).Distinct().ToList();

            var submissionZipEntryLoaders = new List<PageZipEntryLoader>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var submissionEntries = GetPageEntriesFromPageIDs(zip, owner, pageIDs, true);
                submissionZipEntryLoaders = GetPageZipEntryLoadersFromEntries(submissionEntries);
            }

            var submissions = GetPagesFromPageZipEntryLoaders(submissionZipEntryLoaders, zipContainerFilePath);

            return submissions;
        }

        public static void ChangePageNumbersAfterGivenPage(Notebook notebook, int pageNumber, bool isIncreasing)
        {
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                foreach (var page in notebook.Pages)
                {
                    if (page.PageNumber <= pageNumber)
                    {
                        continue;
                    }

                    if (isIncreasing)
                    {
                        var newPageNumber = page.PageNumber + 1;
                        ChangePageNumber(zip, notebook, page, newPageNumber, false);
                    }
                    else
                    {
                        var newPageNumber = page.PageNumber - 1;
                        ChangePageNumber(zip, notebook, page, newPageNumber, false);
                    }
                }

                zip.Save();
            }
        }

        public static void ChangePageNumber(Notebook notebook, CLPPage page, int newPageNumber, bool isSavingImmediately = true)
        {
            var zipContainerFilePath = page.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                ChangePageNumber(zip, notebook, page, newPageNumber, isSavingImmediately);
            }
        }

        public static void ChangePageNumber(ZipFile zip, Notebook notebook, CLPPage page, int newPageNumber, bool isSavingImmediately = true)
        {
            var notebookName = notebook.InternalZipFileDirectoryName;
            var oldInternalFilePath = page.GetZipEntryFullPath(notebookName);
            page.PageNumber = newPageNumber;
            var newInternalFilePath = page.GetZipEntryFullPath(notebookName);

            zip.RenameEntry(oldInternalFilePath, newInternalFilePath);
            if (isSavingImmediately)
            {
                zip.Save();
            }
        }

        public static void SavePage(Notebook notebook, CLPPage page)
        {
            if (!File.Exists(page.ContainerZipFilePath))
            {
                return;
            }

            // HACK: get rid of this after history-rewrite.
            page.History.ClearHistory();

            var parentNotebookName = notebook.InternalZipFileDirectoryName;
            var entires = new List<ZipEntrySaver>
                          {
                              new ZipEntrySaver(page, parentNotebookName),
                              new ZipEntrySaver(notebook, parentNotebookName)
                          };

            SaveZipEntries(page.ContainerZipFilePath, entires);
        }

        public static void SavePagesInSameNotebook(Notebook notebook, List<CLPPage> pages)
        {
            if (!File.Exists(notebook.ContainerZipFilePath))
            {
                return;
            }

            var parentNotebookName = notebook.InternalZipFileDirectoryName;
            var entries = pages.Select(page => new ZipEntrySaver(page, parentNotebookName)).ToList();
            entries.Add(new ZipEntrySaver(notebook, parentNotebookName));

            SaveZipEntries(notebook.ContainerZipFilePath, entries);
        }

        #endregion // Page

        #endregion // Static Methods

        #region OBSOLETE

        public List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel)
        {
            var submissions = new List<CLPPage>();
            //foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.NameComposite.ID == notebookID && n.Notebook.Owner.IsStudent))
            //{
            //    var pageSubmissions = notebookInfo.Notebook.Pages.Where(p => p.ID == pageID && p.DifferentiationLevel == differentiationLevel && p.VersionIndex == 0).Select(p => p.Submissions).ToList();
            //    foreach (var pageSubmission in pageSubmissions)
            //    {
            //        submissions.AddRange(pageSubmission);
            //    }
            //}

            return submissions;
        }

        #endregion // OBSOLETE

        #region Tests

        private void ConvertEmilyCache()
        {
            var dirInfo = new DirectoryInfo(ConversionService.NotebooksFolder);
            var notebooks = new List<Notebook>();
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var notebookFolder = directory.FullName;
                Console.WriteLine($"Notebook Folder: {notebookFolder}");
                var notebook = ConversionService.ConvertCacheNotebook(notebookFolder);
                notebooks.Add(notebook);
            }

            ConversionService.SaveNotebooksToZip(ConversionService.ZipFilePath, notebooks);
        }

        private void AddSessions()
        {
            var dirInfo = new DirectoryInfo(ConversionService.ClassesFolder);
            var sessions = dirInfo.EnumerateFiles("period;*.xml").Select(file => file.FullName).Select(ConversionService.ConvertCacheClassPeriod).OrderBy(s => s.StartTime).ToList();
            var i = 1;
            foreach (var session in sessions)
            {
                session.SessionTitle = $"Class {i}";
                i++;
            }

            ConversionService.SaveSessionsToZip(ConversionService.ZipFilePath, sessions);
        }

        #endregion // Tests
    }
}