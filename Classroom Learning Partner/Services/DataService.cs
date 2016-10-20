using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.Reflection;
using Catel.Runtime.Serialization.Json;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Classroom_Learning_Partner.Services
{
    #region Info Classes

    public class CacheInfo
    {
        public CacheInfo(string cacheFolderPath)
        {
            CacheFolderPath = cacheFolderPath;
        }

        public string CacheFolderPath { get; private set; }

        public string CacheName
        {
            get
            {
                var directoryInfo = new DirectoryInfo(CacheFolderPath);
                return string.Join(" ", directoryInfo.Name.Split('.').Where(s => !s.ToLower().Equals("cache")));
            }
        }

        public string ClassesFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Classes"); }
        }

        public string ImagesFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Images"); }
        }

        public string NotebooksFolderPath
        {
            get { return Path.Combine(CacheFolderPath, "Notebooks"); }
        }

        public void Initialize()
        {
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }

            if (!Directory.Exists(ClassesFolderPath))
            {
                Directory.CreateDirectory(ClassesFolderPath);
            }

            if (!Directory.Exists(ImagesFolderPath))
            {
                Directory.CreateDirectory(ImagesFolderPath);
            }

            if (!Directory.Exists(NotebooksFolderPath))
            {
                Directory.CreateDirectory(NotebooksFolderPath);
            }
        }
    }

    public class NotebookInfo
    {
        public NotebookInfo(CacheInfo cache, string notebookFolderPath)
        {
            Cache = cache;
            NotebookFolderPath = notebookFolderPath;
        }

        public CacheInfo Cache { get; set; }

        public string NotebookFolderPath { get; private set; }

        public Notebook Notebook { get; set; }

        public List<CLPPage> Pages { get; set; } //TODO: Rename to something like NetworkLoadedPages to avoid confusion.

        public string DisplaysFolderPath
        {
            get { return Path.Combine(NotebookFolderPath, "Displays"); }
        }

        public string PagesFolderPath
        {
            get { return Path.Combine(NotebookFolderPath, "Pages"); }
        }

        public string NotebookFilePath
        {
            get { return Path.Combine(NotebookFolderPath, "notebook.xml"); }
        }

        public string LastSavedTime
        {
            get { return !File.Exists(NotebookFilePath) ? string.Empty : File.GetLastWriteTime(NotebookFilePath).ToString("MM/dd/yy HH:mm:ss"); }
        }

        public void Initialize()
        {
            if (!Directory.Exists(NotebookFolderPath))
            {
                Directory.CreateDirectory(NotebookFolderPath);
            }

            if (!Directory.Exists(DisplaysFolderPath))
            {
                Directory.CreateDirectory(DisplaysFolderPath);
            }

            if (!Directory.Exists(PagesFolderPath))
            {
                Directory.CreateDirectory(PagesFolderPath);
            }
        }
    }

    #endregion //Info Classes

    public class DataService : IDataService
    {
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

        #region Static Properties

        #region Special Folder Paths

        public static string WindowsDriveFolderPath => Path.GetPathRoot(Environment.SystemDirectory);

        public static string DesktopFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string CLPProgramFolderPath => typeof(MainWindowView).Assembly.GetDirectory();

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

        #region Program Properties

        /// <summary>ImagePool for the current CLP instance, populated by all open notebooks.</summary>
        public Dictionary<string, BitmapImage> ImagePool { get; } = new Dictionary<string, BitmapImage>();

        #endregion // Program Properties

        #region Cache Properties

        public List<FileInfo> AvailableZipContainerFileInfos
        {
            get
            {
                // ReSharper disable once ConvertPropertyToExpressionBody
                return GetCLPContainersInFolder(CurrentCacheFolderPath);
            }
        }

        #endregion // Cache Properties

        #region Notebook Properties

        //FilePathPair
        //AvailableNotebookSets   
        //LoadedNotebookSets      //Differentiate between a .clp file and the individual notebooks within?
        //CurrentNotebookSet

        public NotebookSet CurrentNotebookSet { get; private set; }
        public ClassRoster CurrentClassRoster { get; private set; }
        public Notebook CurrentNotebook { get; private set; }
        public CLPPage CurrentPage { get; private set; }

        //CurrentMultiDisplay

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
        public event EventHandler<EventArgs> CurrentPageChanged;

        #endregion // Events

        #region Methods

        #region ClassRoster Methods

        public void SetCurrentClassRoster(ClassRoster classRoster)
        {
            CurrentClassRoster = classRoster;
            CurrentClassRosterChanged.SafeInvoke(this);
        }

        #endregion // ClassRoster Methods

        #region Notebook Methods

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
            SaveClassRoaster(CurrentClassRoster);

            SetCurrentNotebook(notebook);
            AddPage(notebook, new CLPPage(Person.Author));
        }

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

        #endregion // Notebook Methods

        #region Page Methods

        public void SetCurrentPage(CLPPage page)
        {
            if (CurrentNotebook == null)
            {
                return;
            }

            var oldPage = CurrentNotebook.CurrentPage;
            if (oldPage != null)
            {
                ACLPPageBaseViewModel.ClearAdorners(oldPage);
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
            page.ContainerZipFilePath = notebook.ContainerZipFilePath;
            page.PageNumber = index + 1;
            notebook.Pages.Insert(index, page);
            AlterPageNumbersAfterPageNumber(notebook, index, true);
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

            // TODO: Refactor to include ZipFile as parameter and put above
            // Refactor all these methods to static.
            AlterPageNumbersAfterPageNumber(notebook, index, false);

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
            SetCurrentPage(nextPage);
        }

        public void AutoSavePage(Notebook notebook, CLPPage page)
        {
            SavePage(notebook, page);
            //TODO: take screenshot of page if not already cached
            //set LastAutoSaveTime of notebook
            //save page locally, and to export folder
            //save page async to teacher machine, and partial cache folder
        }

        public void SavePage(Notebook notebook, CLPPage page)
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

        public void SavePagesInSameNotebook(Notebook notebook, List<CLPPage> pages)
        {
            if (!File.Exists(notebook.ContainerZipFilePath))
            {
                return;
            }

            var parentNotebookName = notebook.InternalZipFileDirectoryName;
            var entries = pages.Select(page => new ZipEntrySaver(page, parentNotebookName)).ToList();

            SaveZipEntries(notebook.ContainerZipFilePath, entries);
        }

        public void SaveClassRoaster(ClassRoster classRoster)
        {
            if (!File.Exists(classRoster.ContainerZipFilePath))
            {
                return;
            }

            var zipEntrySaver = new ZipEntrySaver(classRoster);

            SaveZipEntry(classRoster.ContainerZipFilePath, zipEntrySaver);
        }

        public void SaveNotebook(Notebook notebook)
        {
            if (!File.Exists(notebook.ContainerZipFilePath))
            {
                return;
            }

            var zipEntrySaver = new ZipEntrySaver(notebook, notebook.InternalZipFileDirectoryName);

            SaveZipEntry(notebook.ContainerZipFilePath, zipEntrySaver);
        }

        private void SaveZipEntry(string zipContainerFilePath, ZipEntrySaver zipEntrySaver)
        {
            try
            {
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.CaseSensitiveRetrieval = true;

                    zipEntrySaver.UpdateEntry(zip);

                    zip.Save(zipContainerFilePath);
                }
            }
            catch (Exception)
            {
            }
        }

        private void SaveZipEntries(string zipContainerFilePath, List<ZipEntrySaver> zipEntrySavers)
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
            }
        }

        #endregion // Page Methods

        #region Display Methods

        public void AddDisplay(Notebook notebook, IDisplay display)
        {
            display.NotebookID = notebook.ID;
            display.DisplayNumber = notebook.Displays.Any(d => d.GetType() == display.GetType()) ? notebook.Displays.Last().DisplayNumber + 1 : 1;
            notebook.Displays.Add(display);
        }

        #endregion // Display Methods

        #region Session Methods

        public void SaveSession(Session session)
        {
            if (!File.Exists(session.ContainerZipFilePath))
            {
                return;
            }

            var entires = new List<ZipEntrySaver>
                          {
                              new ZipEntrySaver(session)
                          };

            SaveZipEntries(session.ContainerZipFilePath, entires);
        }

        #endregion // Session Methods

        #region Save Methods

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

        public void CreateEmptyZipContainer(string zipContainerFilePath)
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

        #endregion // Save Methods

        #region Load Methods

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

            return null;

            // TODO: Needs a lock?
            //var containerZipFilePath = parentPage.ContainerZipFilePath;

            //using (var zip = ZipFile.Read(containerZipFilePath))
            //{
            //    zip.CompressionMethod = CompressionMethod.None;
            //    zip.CompressionLevel = CompressionLevel.None;
            //    zip.UseZip64WhenSaving = Zip64Option.Always;
            //    zip.CaseSensitiveRetrieval = true;

            //    var entry = GetImageEntryFromImageHashID(zip, imageHashID);
            //    if (entry == null)
            //    {
            //        return null;
            //    }

            //    using (var ms = new MemoryStream())
            //    {
            //        entry.Extract(ms);

            //        var genBmpImage = new BitmapImage();

            //        genBmpImage.BeginInit();
            //        genBmpImage.CacheOption = BitmapCacheOption.OnDemand;
            //        //genBmpImage.DecodePixelHeight = Convert.ToInt32(this.Height);
            //        genBmpImage.StreamSource = ms;
            //        genBmpImage.EndInit();
            //        genBmpImage.Freeze();

            //        ImagePool.Add(imageHashID, genBmpImage);

            //        return genBmpImage;
            //    }
            //}
        }

        public void LoadAllNotebookPages(Notebook notebook, bool isLoadingSubmissions = true)
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            var classRoster = LoadClassRosterFromCLPContainer(zipContainerFilePath);
            SetCurrentClassRoster(classRoster);

            var pageJsonStrings = new List<string>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var pageEntries = GetAllPageEntriesInNotebook(zip, owner);
                pageJsonStrings = GetJsonStringsFromEntries(pageEntries);
            }

            var pages = GetPagesFromJsonStrings(pageJsonStrings, zipContainerFilePath).OrderBy(p => p.PageNumber).ToList();

            if (isLoadingSubmissions)
            {
                var submissions = GetSubmissions(notebook, pages);
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
            SetCurrentNotebook(notebook);
        }

        public void LoadRangeOfNotebookPages(Notebook notebook, List<int> pageNumbers, bool isLoadingSubmissions = true)
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            var classRoster = LoadClassRosterFromCLPContainer(zipContainerFilePath);
            SetCurrentClassRoster(classRoster);

            var pageJsonStrings = new List<string>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var pageIDs = GetPageIDsFromPageNumbers(zip, owner, pageNumbers);
                var pageEntries = GetPageEntriesFromPageIDs(zip, owner, pageIDs);
                pageJsonStrings = GetJsonStringsFromEntries(pageEntries);
            }

            var pages = GetPagesFromJsonStrings(pageJsonStrings, zipContainerFilePath).OrderBy(p => p.PageNumber).ToList();
            
            if (isLoadingSubmissions)
            {
                var submissions = GetSubmissions(notebook, pages);
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
            SetCurrentNotebook(notebook);
        }

        public static List<CLPPage> GetSubmissions(Notebook notebook, List<CLPPage> pages)
        {
            var owner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;

            var pageIDs = pages.Select(p => p.ID).Distinct().ToList();

            var submissionJsonStrings = new List<string>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var submissionEntries = GetPageEntriesFromPageIDs(zip, owner, pageIDs, true);
                submissionJsonStrings = GetJsonStringsFromEntries(submissionEntries);
            }

            var submissions = GetPagesFromJsonStrings(submissionJsonStrings, zipContainerFilePath);

            return submissions;
        }

        #endregion // Load Methods

        #endregion // Methods

        #endregion // IDataService Implementation

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

        #endregion // Files

        #region Json

        public static List<string> GetJsonStringsFromEntries(List<ZipEntry> entries)
        {
            var jsonStrings = entries.Select(entry => entry.ExtractJsonString()).ToList();
            return jsonStrings;
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

        #endregion // Json

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

        public static List<CLPPage> GetPagesFromJsonStrings(List<string> pageJsonStrings, string zipContainerFilePath)
        {
            // TODO: Need to include updated pageNumber from nameComposite on load. Use static dictionary while loading?
            var pages = new List<CLPPage>();
            foreach (var jsonString in pageJsonStrings)
            {
                var page = AEntityBase.FromJsonString<CLPPage>(jsonString);
                page.ContainerZipFilePath = zipContainerFilePath;
                pages.Add(page);
            }

            return pages;
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

        public static void AlterPageNumbersAfterPageNumber(Notebook notebook, decimal pageNumber, bool isIncreasing)
        {
            var notebookOwner = notebook.Owner;
            var zipContainerFilePath = notebook.ContainerZipFilePath;
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                var pageEntries = GetAllPageEntriesInNotebook(zip, notebookOwner);
                foreach (var pageEntry in pageEntries)
                {
                    var oldNameCompositeString = pageEntry.GetEntryNameWithoutExtension();
                    var pageNameComposite = CLPPage.NameComposite.ParseFromString(oldNameCompositeString);
                    if (pageNameComposite.PageNumber <= pageNumber)
                    {
                        continue;
                    }

                    if (isIncreasing)
                    {
                        pageNameComposite.PageNumber++;
                    }
                    else
                    {
                        pageNameComposite.PageNumber--;
                    }

                    var newNameCompositeString = pageNameComposite.ToNameCompositeString();
                    var newEntryName = $"{newNameCompositeString}.json";
                    pageEntry.RenameEntry(newEntryName);
                }

                zip.Save();
            }
        }

        #endregion // Page

        #endregion // Static Methods

        #region OBSOLETE

        #region Properties

        #region Cache Properties

        public CacheInfo CurrentCacheInfo { get; set; }

        #endregion //Cache Properties

        #region Notebook Properties

        public List<NotebookInfo> LoadedNotebooksInfo { get; } = new List<NotebookInfo>();

        #endregion //Notebook Properties

        #endregion //Properties

        #region Methods

        #region Static Methods

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

        public static List<CLPPage> LoadOwnSubmissionsForLoadedPages(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

            return LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePaths);
        }

        public static List<CLPPage> LoadGivenSubmissionsForLoadedPages(NotebookInfo notebookInfo, List<string> pageFilePathsToCheck)
        {
            var submissions = new ConcurrentBag<CLPPage>();

            //Parallel.ForEach(pageFilePathsToCheck,
            //                 pageFilePath =>
            //                 {
            //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //                     if (pageNameComposite == null ||
            //                         pageNameComposite.VersionIndex == "0")
            //                     {
            //                         return;
            //                     }

            //                     var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
            //                     if (!isPageToBeLoaded)
            //                     {
            //                         return;
            //                     }

            //                     var page = CLPPage.LoadFromXML(pageFilePath);
            //                     if (page == null)
            //                     {
            //                         return;
            //                     }

            //                     submissions.Add(page);
            //                 });

            return submissions.ToList();
        }

        public static List<CLPPage> GetPagesToSave(NotebookInfo notebookInfo, bool isForcedFullSave)
        {
            var pagesToSave = new List<CLPPage>();
            //foreach (var page in notebookInfo.Notebook.Pages)
            //{
            //    if (isForcedFullSave || !page.IsCached)
            //    {
            //        pagesToSave.Add(page);
            //    }
            //    pagesToSave.AddRange(page.Submissions.Where(s => isForcedFullSave || !s.IsCached));
            //}

            return pagesToSave;
        }

        #endregion //Static Methods

        #region Load Methods

        public void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isSetToNotebookCurrentNotebook = true)
        {
            //// Is Notebook already loaded in memory?
            //var loadedNotebooks = LoadedNotebooksInfo.ToList();
            //var existingNotebookInfo = loadedNotebooks.FirstOrDefault(n => n.NameComposite.ToFolderName() == notebookInfo.NameComposite.ToFolderName());
            //if (existingNotebookInfo != null)
            //{
            //    if (isForcedOpen)
            //    {
            //        LoadedNotebooksInfo.Remove(existingNotebookInfo);
            //    }
            //    else
            //    {
            //        if (CurrentNotebookInfo == existingNotebookInfo)
            //        {
            //            App.MainWindowViewModel.IsBackStageVisible = false;
            //        }
            //        else if (isSetToNotebookCurrentNotebook)
            //        {
            //            SetCurrentNotebook(existingNotebookInfo);
            //        }
            //        return;
            //    }
            //}

            //// Guarantee folder structure.
            //notebookInfo.Cache.Initialize();
            //notebookInfo.Initialize();

            //// Is Notebook included in notebookInfo (e.g. send across the network instead of being loaded from the disk).
            //if (notebookInfo.Notebook != null)
            //{
            //    LoadedNotebooksInfo.Add(notebookInfo);
            //    if (isSetToNotebookCurrentNotebook)
            //    {
            //        SetCurrentNotebook(notebookInfo);
            //    }
            //    return;
            //}

            //// Load Notebook from disk.
            //var notebook = Notebook.LoadFromXML(notebookInfo.NotebookFolderPath);
            //if (notebook == null)
            //{
            //    MessageBox.Show("Notebook couldn't be loaded.");
            //    return;
            //}

            //notebookInfo.Notebook = notebook;

            //LoadedNotebooksInfo.Add(notebookInfo);
            //if (isSetToNotebookCurrentNotebook)
            //{
            //    SetCurrentNotebook(notebookInfo);
            //}
        }

        public void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced)
        {
            //if (notebookInfo.Notebook == null)
            //{
            //    return;
            //}

            //if (isExistingPagesReplaced)
            //{
            //    notebookInfo.Notebook.Pages.Clear();
            //    notebookInfo.Notebook.CurrentPage = null;
            //}

            //var pagesToLoad = new List<CLPPage>();

            //if (notebookInfo.Pages != null &&
            //    notebookInfo.Pages.Any()) // Load pages included in notebookInfo (e.g. ones sent across the network).
            //{
            //    pagesToLoad = notebookInfo.Pages;
            //}
            //else // Load local pages.
            //{
            //    var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

            //    foreach (var pageFilePath in pageFilePaths)
            //    {
            //        var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //        if (pageNameComposite == null ||
            //            pageNameComposite.VersionIndex != "0")
            //        {
            //            continue;
            //        }

            //        var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
            //        if (!isPageToBeLoaded)
            //        {
            //            continue;
            //        }

            //        var page = CLPPage.LoadFromXML(pageFilePath);
            //        if (page == null)
            //        {
            //            continue;
            //        }

            //        pagesToLoad.Add(page);
            //    }

            //    // BUG: Parellel calls invoke threading errors.
            //    //Parallel.ForEach(pageFilePaths,
            //    //                 pageFilePath =>
            //    //                 {
            //    //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //    //                     if (pageNameComposite == null ||
            //    //                         pageNameComposite.VersionIndex != "0")
            //    //                     {
            //    //                         return;
            //    //                     }

            //    //                     var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
            //    //                     if (!isPageToBeLoaded)
            //    //                     {
            //    //                         return;
            //    //                     }

            //    //                     var page = CLPPage.LoadFromXML(pageFilePath);
            //    //                     if (page == null)
            //    //                     {
            //    //                         return;
            //    //                     }

            //    //                     pagesToLoad.Add(page);
            //    //                 });
            //}

            //foreach (var page in pagesToLoad)
            //{
            //    var index = notebookInfo.Notebook.Pages.ToList().BinarySearch(page, new PageNumberComparer());
            //    if (index < 0)
            //    {
            //        index = ~index;
            //    }
            //    notebookInfo.Notebook.Pages.Insert(index, page);

            //    if (notebookInfo.Notebook.CurrentPageID == page.ID &&
            //        notebookInfo.Notebook.CurrentPageOwnerID == page.OwnerID &&
            //        notebookInfo.Notebook.CurrentPageVersionIndex == page.VersionIndex)
            //    {
            //        notebookInfo.Notebook.CurrentPage = page;
            //    }
            //}

            //if (notebookInfo.Notebook.CurrentPage == null)
            //{
            //    notebookInfo.Notebook.CurrentPage = notebookInfo.Notebook.Pages.FirstOrDefault();
            //}

            //// Load submissions from disk.
            ////if ((App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher && App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector) ||
            ////    notebookInfo.Notebook.Owner.ID == Person.Author.ID ||
            ////    notebookInfo.Notebook.Owner.IsStudent) // Load student's own submission history.
            ////{
            ////    var submissions = LoadOwnSubmissionsForLoadedPages(notebookInfo);

            ////    foreach (var page in notebookInfo.Notebook.Pages)
            ////    {
            ////        page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(p => p.ID == page.ID).OrderBy(p => p.VersionIndex).ToList());
            ////    }
            ////}
            ////else // Load all student submissions for Teacher Notebook.
            ////{
            ////    var notebookInfos = GetNotebooksInCache(notebookInfo.Cache).Where(n => n.NameComposite.ID == notebookInfo.Notebook.ID && n.NameComposite.OwnerTypeTag == "S");
            ////    var pageFilePathsToCheck = new List<string>();

            ////    foreach (var info in notebookInfos)
            ////    {
            ////        pageFilePathsToCheck.AddRange(Directory.EnumerateFiles(info.PagesFolderPath, "*.xml").ToList());
            ////    }

            ////    var submissions = LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePathsToCheck);

            ////    foreach (var page in notebookInfo.Notebook.Pages)
            ////    {
            ////        page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            ////    }
            ////}
        }

        public void LoadLocalSubmissions(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced)
        {
            if (notebookInfo.Notebook == null)
            {
                return;
            }

            if (isExistingPagesReplaced)
            {
                foreach (var page in notebookInfo.Notebook.Pages)
                {
                    page.Submissions.Clear();
                }
            }

            var submissions = new List<CLPPage>();

            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            foreach (var pageFilePath in pageFilePaths)
            {
                //var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                //if (pageNameComposite == null ||
                //    pageNameComposite.VersionIndex == "0")
                //{
                //    continue;
                //}

                //var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
                //if (!isPageToBeLoaded)
                //{
                //    continue;
                //}

                //var page = CLPPage.LoadFromXML(pageFilePath);
                //if (page == null)
                //{
                //    continue;
                //}

                //submissions.Add(page);
            }

            foreach (var page in notebookInfo.Notebook.Pages)
            {
                page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            }

            // BUG: Parellel calls invoke threading errors. see concurrentBag, or PLINQ AsParallel()
            //Parallel.ForEach(pageFilePaths,
            //                 pageFilePath =>
            //                 {
            //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
            //                     if (pageNameComposite == null ||
            //                         pageNameComposite.VersionIndex == "0")
            //                     {
            //                         return;
            //                     }

            //                     var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
            //                     if (!isPageToBeLoaded)
            //                     {
            //                         return;
            //                     }

            //                     var page = CLPPage.LoadFromXML(pageFilePath);
            //                     if (page == null)
            //                     {
            //                         return;
            //                     }

            //                     submissions.Add(page);
            //                 });

            //Parallel.ForEach(notebookInfo.Notebook.Pages,
            //                 page =>
            //                 {
            //                     page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            //                 });
        }

        #endregion //Load Methods

        #region Old ClassPeriod Methods

        //public void StartLocalClassPeriod(ClassPeriodNameComposite classPeriodNameComposite, string localCacheFolderPath)
        //{
        //    var filePath = classPeriodNameComposite.FullClassPeriodFilePath;
        //    if (!File.Exists(filePath))
        //    {
        //        MessageBox.Show("Class Period doesn't exist!");
        //        return;
        //    }

        //    var classPeriod = ClassPeriod.LoadLocalClassPeriod(filePath);
        //    if (classPeriod == null)
        //    {
        //        MessageBox.Show("Class Period could not be opened. Check error log.");
        //        return;
        //    }

        //    CurrentLocalCacheDirectory = localCacheFolderPath;
        //    CurrentClassPeriod = classPeriod;
        //    var authoredNotebook = LoadClassPeriodNotebookForPerson(classPeriod, Person.Author.ID);
        //    if (authoredNotebook == null)
        //    {
        //        return;
        //    }
        //    //var authoredNotebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == Person.Author.ID);
        //    //var authoredPagesFolderPath = Path.Combine(authoredNotebookNameComposite.NotebookFolderPath, "Pages");
        //    //var pageIDs = GetPageIDsFromStartIDAndForwardRange(authoredPagesFolderPath, classPeriod.StartPageID, classPeriod.NumberOfPages);
        //    //if (!pageIDs.Contains(classPeriod.TitlePageID))
        //    //{
        //    //    pageIDs.Insert(0, classPeriod.TitlePageID);
        //    //}

        //    //var authoredPages = LoadOrCopyPagesForNotebook(authoredNotebook, null, pageIDs, false);
        //    //authoredNotebook.Pages = new ObservableCollection<CLPPage>(authoredPages);
        //    //authoredNotebook.CurrentPage = authoredNotebook.Pages.FirstOrDefault(); //HACK

        //    //var teacherNotebook = LoadClassPeriodNotebookForPerson(classPeriod, classPeriod.ClassInformation.TeacherID) ??
        //    //                      CopyNotebookForNewOwner(authoredNotebook, classPeriod.ClassInformation.Teacher);

        //    //var teacherPages = LoadOrCopyPagesForNotebook(teacherNotebook, authoredNotebook, pageIDs, true);
        //    //teacherNotebook.Pages = new ObservableCollection<CLPPage>(teacherPages);
        //    //teacherNotebook.CurrentPage = teacherNotebook.Pages.FirstOrDefault(); //HACK

        //    ////Generates pages in cache
        //    //foreach (var student in classPeriod.ClassInformation.StudentList)
        //    //{
        //    //    var studentNotebook = LoadClassPeriodNotebookForPerson(classPeriod, student.ID) ??
        //    //                      CopyNotebookForNewOwner(authoredNotebook, student);

        //    //    var studentPages = LoadOrCopyPagesForNotebook(studentNotebook, authoredNotebook, pageIDs, false);
        //    //    studentNotebook.Pages = new ObservableCollection<CLPPage>(studentPages);
        //    //    studentNotebook.CurrentPage = studentNotebook.Pages.FirstOrDefault(); //HACK
        //    //    SaveNotebookLocally(studentNotebook);
        //    //}

        //    //OpenNotebooks.Clear();
        //    //OpenNotebooks.Add(authoredNotebook);
        //    //OpenNotebooks.Add(teacherNotebook);
        //    //SetNotebookAsCurrentNotebook(teacherNotebook);
        //}

        //public Notebook LoadClassPeriodNotebookForPerson(ClassPeriod classPeriod, string ownerID)
        //{
        //    var notebookNameComposite = AvailableLocalNotebookNameComposites.FirstOrDefault(x => x.ID == classPeriod.NotebookID && x.OwnerID == ownerID);
        //    if (notebookNameComposite == null)
        //    {
        //       // MessageBox.Show("Notebook for Class Period not found for " + ownerID + ".");
        //        return null;
        //    }

        //    Notebook notebook = null; // = Notebook.LoadLocalNotebook(notebookNameComposite.NotebookFolderPath);
        //    if (notebook == null)
        //    {
        //        //MessageBox.Show("Notebook for Class Period could not be loaded " + ownerID + ".");
        //        return null;
        //    }

        //    return notebook;
        //}

        //public Notebook CopyNotebookForNewOwner(Notebook originalNotebook, Person newOwner)
        //{
        //    var newNotebook = originalNotebook.Clone() as Notebook;
        //    if (newNotebook == null)
        //    {
        //        return null;
        //    }
        //    newNotebook.Owner = newOwner;
        //    newNotebook.CreationDate = DateTime.Now;
        //    newNotebook.LastSavedDate = null;

        //    return newNotebook;
        //}

        //public CLPPage CopyPageForNewOwner(CLPPage originalPage, Person newOwner)
        //{
        //    var newPage = originalPage.Clone() as CLPPage;
        //    if (newPage == null)
        //    {
        //        return null;
        //    }
        //    newPage.Owner = newOwner;
        //    newPage.CreationDate = DateTime.Now;

        //    foreach (var pageObject in newPage.PageObjects)
        //    {
        //        pageObject.ParentPage = newPage;
        //        if (pageObject.IsBackgroundInteractable)
        //        {
        //            pageObject.OwnerID = newOwner.ID;
        //        }
        //    }

        //    foreach (var tag in newPage.Tags)
        //    {
        //        tag.ParentPage = newPage;
        //    }

        //    newPage.AfterDeserialization();

        //    return newPage;
        //}

        //public List<CLPPage> LoadOrCopyPagesForNotebook(Notebook notebook, Notebook authoredNotebook, List<string> pageIDs, bool includeSubmissions)
        //{
        //    var pages = new List<CLPPage>();

        //    //var notebookNameComposite =
        //    //    GetAvailableNotebookNameCompositesInCache(CurrentLocalCacheDirectory).FirstOrDefault(x => x.ID == notebook.ID && x.OwnerID == notebook.Owner.ID);

        //    //var pageNameComposites = new List<PageNameComposite>();
        //    //if (notebookNameComposite != null)
        //    //{
        //    //    var pagesFolderPath = Path.Combine(notebookNameComposite.NotebookFolderPath, "Pages");
        //    //    pageNameComposites = Directory.EnumerateFiles(pagesFolderPath, "*.xml").Select(PageNameComposite.ParseFilePath).Where(x => pageIDs.Contains(x.ID)).ToList();
        //    //}

        //    //foreach (var pageID in pageIDs)
        //    //{
        //    //    var pageNameComposite = pageNameComposites.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == "0");
        //    //    if (pageNameComposite == null)
        //    //    {
        //    //        if (authoredNotebook == null)
        //    //        {
        //    //            continue;
        //    //        }
        //    //        var authoredPage = authoredNotebook.Pages.FirstOrDefault(x => x.ID == pageID && x.VersionIndex == 0);
        //    //        if (authoredPage == null)
        //    //        {
        //    //            continue;
        //    //        }

        //    //        var newPage = CopyPageForNewOwner(authoredPage, notebook.Owner);
        //    //        if (newPage == null)
        //    //        {
        //    //            continue;
        //    //        }

        //    //        pages.Add(newPage);
        //    //        continue;
        //    //    }

        //    //    var page = CLPPage.LoadLocalPage(pageNameComposite.FullPageFilePath);
        //    //    if (page == null)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    if (includeSubmissions)
        //    //    {
        //    //        var id = pageID;
        //    //        foreach (var submissionComposite in pageNameComposites.Where(x => x.ID == id && x.VersionIndex != "0"))
        //    //        {
        //    //            var submission = CLPPage.LoadLocalPage(submissionComposite.FullPageFilePath);
        //    //            if (submission == null)
        //    //            {
        //    //                continue;
        //    //            }
        //    //            page.Submissions.Add(submission);
        //    //        }
        //    //    }

        //    //    pages.Add(page);
        //    //}

        //    return pages;
        //} 

        #endregion //Old ClassPeriod Methods

        #endregion //Methods

        #endregion // OBSOLETE
    }
}