using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.IoC;
using Catel.Reflection;
using Catel.Runtime.Serialization;
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
        public CacheInfo(string cacheFolderPath) { CacheFolderPath = cacheFolderPath; }

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

    public class ClassPeriodInfo
    {
        public ClassPeriodInfo(CacheInfo cache, string classPeriodFilePath)
        {
            Cache = cache;
            ClassPeriodFilePath = classPeriodFilePath;
        }

        public CacheInfo Cache { get; set; }

        public string ClassPeriodFilePath { get; set; }

        public ClassPeriod ClassPeriod { get; set; }

        public ClassPeriodNameComposite NameComposite
        {
            get { return ClassPeriodNameComposite.ParseFilePath(ClassPeriodFilePath); }
        }

        public string StartTime
        {
            get { return NameComposite.StartTime; }
        }

        public string PageNumbers
        {
            get { return NameComposite.PageNumbers; }
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

        public NotebookNameComposite NameComposite
        {
            get { return NotebookNameComposite.ParseFolderPath(NotebookFolderPath); }
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
        private const string DEFAULT_CLP_DATA_FOLDER_NAME = "CLP";
        private const string DEFAULT_CACHE_FOLDER_NAME = "Caches";

        

        public DataService()
        {
            //Warm up Serializer to make deserializing Notebooks, ClassPeriods, ClassSubjects, and Pages faster.
            var typesToWarmup = new[] { typeof (Notebook), typeof (ClassPeriod), typeof (ClassInformation), typeof (CLPPage) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);

            CurrentCLPDataFolderPath = Path.Combine(DesktopFolderPath, DEFAULT_CLP_DATA_FOLDER_NAME);
            if (!Directory.Exists(CurrentCLPDataFolderPath))
            {
                Directory.CreateDirectory(CurrentCLPDataFolderPath);
            }

            CurrentCachesFolderPath = Path.Combine(CurrentCLPDataFolderPath, DEFAULT_CACHE_FOLDER_NAME);
            if (!Directory.Exists(CurrentCachesFolderPath))
            {
                Directory.CreateDirectory(CurrentCachesFolderPath);
            }

            CreateTestNotebookSet();
        }

        #region Properties

        #region Cache Properties

        public string CurrentCachesFolderPath { get; set; }

        public List<CacheInfo> AvailableCaches
        {
            get { return GetCachesInFolder(CurrentCachesFolderPath); }
        }

        public CacheInfo CurrentCacheInfo { get; set; }

        #endregion //Cache Properties

        #region Notebook Properties

        public List<NotebookInfo> NotebooksInCurrentCache
        {
            get { return GetNotebooksInCache(CurrentCacheInfo); }
        }

        private readonly List<NotebookInfo> _loadedNotebooksInfo = new List<NotebookInfo>();

        public List<NotebookInfo> LoadedNotebooksInfo
        {
            get { return _loadedNotebooksInfo; }
        }

        public NotebookInfo CurrentNotebookInfo { get; set; }

        public Notebook CurrentNotebook
        {
            get { return CurrentNotebookInfo == null ? null : CurrentNotebookInfo.Notebook; }
        }

        public CLPPage CurrentPage
        {
            get { return CurrentNotebook == null ? null : CurrentNotebook.CurrentPage; }
        }

        #endregion //Notebook Properties

        #endregion //Properties

        #region Methods

        #region Static Methods

        public static List<CacheInfo> GetCachesInFolder(string cachesFolderPath)
        {
            if (!Directory.Exists(cachesFolderPath))
            {
                return new List<CacheInfo>();
            }

            var directoryInfo = new DirectoryInfo(cachesFolderPath);
            return directoryInfo.GetDirectories().Where(directory => directory.Name.StartsWith("Cache")).Select(directory => new CacheInfo(directory.FullName)).OrderBy(c => c.CacheName).ToList();
        }

        public static List<NotebookInfo> GetNotebooksInCache(CacheInfo cache)
        {
            if (cache == null ||
                !Directory.Exists(cache.NotebooksFolderPath))
            {
                return new List<NotebookInfo>();
            }

            var directoryInfo = new DirectoryInfo(cache.NotebooksFolderPath);
            return
                directoryInfo.GetDirectories()
                             .Select(directory => new NotebookInfo(cache, directory.FullName))
                             .Where(nc => nc != null)
                             .OrderBy(nc => nc.NameComposite.OwnerTypeTag != "T")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "A")
                             .ThenBy(nc => nc.NameComposite.OwnerTypeTag != "S")
                             .ThenBy(nc => nc.NameComposite.OwnerName)
                             .ToList();
        }

        public static List<string> GetAllPageIDsInNotebook(NotebookInfo notebookInfo)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new ConcurrentBag<string>();

            Parallel.ForEach(pageFilePaths,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex != "0")
                                 {
                                     return;
                                 }

                                 pageIDs.Add(pageNameComposite.ID);
                             });

            return pageIDs.Distinct().ToList();
        }

        public static List<string> GetPageIDsFromPageNumbers(NotebookInfo notebookInfo, List<int> pageNumbers)
        {
            var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();
            var pageIDs = new ConcurrentBag<string>();

            Parallel.ForEach(pageFilePaths,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex != "0")
                                 {
                                     return;
                                 }

                                 int pageNumber;
                                 var isPageNumber = int.TryParse(pageNameComposite.PageNumber, out pageNumber);
                                 if (!isPageNumber)
                                 {
                                     return;
                                 }

                                 var isPageToBeLoaded = pageNumbers.Contains(pageNumber);
                                 if (!isPageToBeLoaded)
                                 {
                                     return;
                                 }

                                 pageIDs.Add(pageNameComposite.ID);
                             });

            return pageIDs.Distinct().ToList();
        }

        public List<CLPPage> GetLoadedSubmissionsForTeacherPage(string notebookID, string pageID, string differentiationLevel)
        {
            var submissions = new List<CLPPage>();
            foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.NameComposite.ID == notebookID && n.Notebook.Owner.IsStudent))
            {
                var pageSubmissions = notebookInfo.Notebook.Pages.Where(p => p.ID == pageID && p.DifferentiationLevel == differentiationLevel && p.VersionIndex == 0).Select(p => p.Submissions).ToList();
                foreach (var pageSubmission in pageSubmissions)
                {
                    submissions.AddRange(pageSubmission);
                }
            }

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

            Parallel.ForEach(pageFilePathsToCheck,
                             pageFilePath =>
                             {
                                 var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                                 if (pageNameComposite == null ||
                                     pageNameComposite.VersionIndex == "0")
                                 {
                                     return;
                                 }

                                 var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
                                 if (!isPageToBeLoaded)
                                 {
                                     return;
                                 }

                                 var page = CLPPage.LoadFromXML(pageFilePath);
                                 if (page == null)
                                 {
                                     return;
                                 }

                                 submissions.Add(page);
                             });

            return submissions.ToList();
        }

        public static List<CLPPage> GetPagesToSave(NotebookInfo notebookInfo, bool isForcedFullSave)
        {
            var pagesToSave = new List<CLPPage>();
            foreach (var page in notebookInfo.Notebook.Pages)
            {
                if (isForcedFullSave || !page.IsCached)
                {
                    pagesToSave.Add(page);
                }
                pagesToSave.AddRange(page.Submissions.Where(s => isForcedFullSave || !s.IsCached));
            }

            return pagesToSave;
        }

        #endregion //Static Methods

        #region Create Methods

        public CacheInfo CreateNewCache(string cacheName, bool isCacheCurrent = true) { return CreateNewCache(cacheName, CurrentCachesFolderPath, isCacheCurrent); }

        public CacheInfo CreateNewCache(string cacheName, string cachesFolderPath, bool isCacheCurrent = true)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            cacheName = invalidFileNameCharacters.Aggregate(cacheName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var cacheFileName = "Cache." + cacheName;
            var cacheFolderPath = Path.Combine(cachesFolderPath, cacheFileName);
            var availableCaches = GetCachesInFolder(cachesFolderPath);

            var existingCache = availableCaches.FirstOrDefault(c => c.CacheFolderPath == cacheFolderPath);
            if (existingCache != null)
            {
                if (isCacheCurrent)
                {
                    CurrentCacheInfo = existingCache;
                }

                return existingCache;
            }

            var newCache = new CacheInfo(cacheFolderPath);
            newCache.Initialize();
            if (isCacheCurrent)
            {
                CurrentCacheInfo = newCache;
            }

            return newCache;
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum, bool isNotebookCurrent = true)
        {
            return CreateNewNotebook(notebookName, curriculum, CurrentCacheInfo, isNotebookCurrent);
        }

        public NotebookInfo CreateNewNotebook(string notebookName, string curriculum, CacheInfo cache, bool isNotebookCurrent = true)
        {
            var invalidFileNameCharacters = new string(Path.GetInvalidFileNameChars());
            notebookName = invalidFileNameCharacters.Aggregate(notebookName, (current, c) => current.Replace(c.ToString(), string.Empty));

            var newNotebook = new Notebook(notebookName, Person.Author)
                              {
                                  Curriculum = curriculum
                              };

            var newPage = new CLPPage(Person.Author);
            newNotebook.AddPage(newPage);

            var notebookFolderName = NotebookNameComposite.ParseNotebook(newNotebook).ToFolderName();
            var notebookFolderPath = Path.Combine(cache.NotebooksFolderPath, notebookFolderName);
            if (Directory.Exists(notebookFolderPath))
            {
                return null;
            }

            var notebookInfo = new NotebookInfo(cache, notebookFolderPath)
                               {
                                   Notebook = newNotebook
                               };
            notebookInfo.Initialize();
            notebookInfo.Notebook.SaveToXML(notebookInfo.NotebookFolderPath);
            newPage.SaveToXML(notebookInfo.PagesFolderPath);

            LoadedNotebooksInfo.Add(notebookInfo);
            if (isNotebookCurrent)
            {
                SetCurrentNotebook(notebookInfo);
            }

            return notebookInfo;
        }

        #endregion //Create Methods 

        #region AutoSave Methods

        public void AutoSavePage(CLPPage page)
        {
            //TODO: take screenshot of page if not already cached
            //set LastAutoSaveTime of notebook
            //save page locally, and to export folder
            //save page async to teacher machine, and partial cache folder
        }

        public void SetCurrentPage(CLPPage page)
        {
            if (CurrentNotebook == null)
            {
                return;
            }

            var oldPage = CurrentNotebook.CurrentPage;
            ACLPPageBaseViewModel.ClearAdorners(oldPage);
            AutoSavePage(oldPage);
            CurrentNotebook.CurrentPage = page;
        }

        public void DeletePage(CLPPage page)
        {
            //TODO: Delete page from notebook
            //delete page's xml
            //renumber existing pages
            //function with full cache
        }

        #endregion //AutoSave Methods

        #region Save Methods

        public void SaveNotebookLocally(NotebookInfo notebookInfo, bool isForcedFullSave = false)
        {
            notebookInfo.Cache.Initialize();
            if (isForcedFullSave &&
                Directory.Exists(notebookInfo.NotebookFolderPath))
            {
                Directory.Delete(notebookInfo.NotebookFolderPath, true);
            }
            notebookInfo.Initialize();
            notebookInfo.Notebook.SaveToXML(notebookInfo.NotebookFolderPath);

            var pagesToSave = GetPagesToSave(notebookInfo, isForcedFullSave);
            //Parallel.ForEach(pagesToSave, page => { page.SaveToXML(notebookInfo.PagesFolderPath); });
            foreach (var page in pagesToSave)
            {
                page.SaveToXML(notebookInfo.PagesFolderPath);
            }
        }

        #endregion //Save Methods

        public void PackageAndSendNotebook(NotebookInfo notebookInfo, bool isNotebookSaved = true)
        {
            if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Student ||
                App.Network.InstructorProxy == null)
            {
                return;
            }

            if (!isNotebookSaved) { }

            var sNotebook = ObjectSerializer.ToString(notebookInfo.Notebook);
            var zippedNotebook = sNotebook.CompressWithGZip();
            App.Network.InstructorProxy.CollectStudentNotebook(zippedNotebook, App.MainWindowViewModel.CurrentUser.FullName);
        }

        #region Load Methods

        public void OpenNotebook(NotebookInfo notebookInfo, bool isForcedOpen = false, bool isSetToNotebookCurrentNotebook = true)
        {
            // Is Notebook already loaded in memory?
            var loadedNotebooks = LoadedNotebooksInfo.ToList();
            var existingNotebookInfo = loadedNotebooks.FirstOrDefault(n => n.NameComposite.ToFolderName() == notebookInfo.NameComposite.ToFolderName());
            if (existingNotebookInfo != null)
            {
                if (isForcedOpen)
                {
                    LoadedNotebooksInfo.Remove(existingNotebookInfo);
                }
                else
                {
                    if (CurrentNotebookInfo == existingNotebookInfo)
                    {
                        App.MainWindowViewModel.IsBackStageVisible = false;
                    }
                    else if (isSetToNotebookCurrentNotebook)
                    {
                        SetCurrentNotebook(existingNotebookInfo);
                    }
                    return;
                }
            }

            // Guarantee folder structure.
            notebookInfo.Cache.Initialize();
            notebookInfo.Initialize();

            // Is Notebook included in notebookInfo (e.g. send across the network instead of being loaded from the disk).
            if (notebookInfo.Notebook != null)
            {
                LoadedNotebooksInfo.Add(notebookInfo);
                if (isSetToNotebookCurrentNotebook)
                {
                    SetCurrentNotebook(notebookInfo);
                }
                return;
            }

            // Load Notebook from disk.
            var notebook = Notebook.LoadFromXML(notebookInfo.NotebookFolderPath);
            if (notebook == null)
            {
                MessageBox.Show("Notebook couldn't be loaded.");
                return;
            }

            notebookInfo.Notebook = notebook;

            LoadedNotebooksInfo.Add(notebookInfo);
            if (isSetToNotebookCurrentNotebook)
            {
                SetCurrentNotebook(notebookInfo);
            }
        }

        public void LoadPages(NotebookInfo notebookInfo, List<string> pageIDs, bool isExistingPagesReplaced)
        {
            if (notebookInfo.Notebook == null)
            {
                return;
            }

            if (isExistingPagesReplaced)
            {
                notebookInfo.Notebook.Pages.Clear();
                notebookInfo.Notebook.CurrentPage = null;
            }

            var pagesToLoad = new List<CLPPage>();

            if (notebookInfo.Pages != null &&
                notebookInfo.Pages.Any()) // Load pages included in notebookInfo (e.g. ones sent across the network).
            {
                pagesToLoad = notebookInfo.Pages;
            }
            else // Load local pages.
            {
                var pageFilePaths = Directory.EnumerateFiles(notebookInfo.PagesFolderPath, "*.xml").ToList();

                foreach (var pageFilePath in pageFilePaths)
                {
                    var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                    if (pageNameComposite == null ||
                        pageNameComposite.VersionIndex != "0")
                    {
                        continue;
                    }

                    var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
                    if (!isPageToBeLoaded)
                    {
                        continue;
                    }

                    var page = CLPPage.LoadFromXML(pageFilePath);
                    if (page == null)
                    {
                        continue;
                    }

                    pagesToLoad.Add(page);
                }

                // BUG: Parellel calls invoke threading errors.
                //Parallel.ForEach(pageFilePaths,
                //                 pageFilePath =>
                //                 {
                //                     var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                //                     if (pageNameComposite == null ||
                //                         pageNameComposite.VersionIndex != "0")
                //                     {
                //                         return;
                //                     }

                //                     var isPageToBeLoaded = pageIDs.Any(pageID => pageID == pageNameComposite.ID);
                //                     if (!isPageToBeLoaded)
                //                     {
                //                         return;
                //                     }

                //                     var page = CLPPage.LoadFromXML(pageFilePath);
                //                     if (page == null)
                //                     {
                //                         return;
                //                     }

                //                     pagesToLoad.Add(page);
                //                 });
            }

            foreach (var page in pagesToLoad)
            {
                var index = notebookInfo.Notebook.Pages.ToList().BinarySearch(page, new PageNumberComparer());
                if (index < 0)
                {
                    index = ~index;
                }
                notebookInfo.Notebook.Pages.Insert(index, page);

                if (notebookInfo.Notebook.CurrentPageID == page.ID &&
                    notebookInfo.Notebook.CurrentPageOwnerID == page.OwnerID &&
                    notebookInfo.Notebook.CurrentPageVersionIndex == page.VersionIndex)
                {
                    notebookInfo.Notebook.CurrentPage = page;
                }
            }

            if (notebookInfo.Notebook.CurrentPage == null)
            {
                notebookInfo.Notebook.CurrentPage = notebookInfo.Notebook.Pages.FirstOrDefault();
            }

            // Load submissions from disk.
            //if ((App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher && App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector) ||
            //    notebookInfo.Notebook.Owner.ID == Person.Author.ID ||
            //    notebookInfo.Notebook.Owner.IsStudent) // Load student's own submission history.
            //{
            //    var submissions = LoadOwnSubmissionsForLoadedPages(notebookInfo);

            //    foreach (var page in notebookInfo.Notebook.Pages)
            //    {
            //        page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(p => p.ID == page.ID).OrderBy(p => p.VersionIndex).ToList());
            //    }
            //}
            //else // Load all student submissions for Teacher Notebook.
            //{
            //    var notebookInfos = GetNotebooksInCache(notebookInfo.Cache).Where(n => n.NameComposite.ID == notebookInfo.Notebook.ID && n.NameComposite.OwnerTypeTag == "S");
            //    var pageFilePathsToCheck = new List<string>();

            //    foreach (var info in notebookInfos)
            //    {
            //        pageFilePathsToCheck.AddRange(Directory.EnumerateFiles(info.PagesFolderPath, "*.xml").ToList());
            //    }

            //    var submissions = LoadGivenSubmissionsForLoadedPages(notebookInfo, pageFilePathsToCheck);

            //    foreach (var page in notebookInfo.Notebook.Pages)
            //    {
            //        page.Submissions = new ObservableCollection<CLPPage>(submissions.Where(s => s.ID == page.ID && s.DifferentiationLevel == page.DifferentiationLevel).ToList());
            //    }
            //}
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
                var pageNameComposite = PageNameComposite.ParseFilePath(pageFilePath);
                if (pageNameComposite == null ||
                    pageNameComposite.VersionIndex == "0")
                {
                    continue;
                }

                var isPageToBeLoaded = notebookInfo.Notebook.Pages.Any(p => p.ID == pageNameComposite.ID);
                if (!isPageToBeLoaded)
                {
                    continue;
                }

                var page = CLPPage.LoadFromXML(pageFilePath);
                if (page == null)
                {
                    continue;
                }

                submissions.Add(page);
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

        public void SetCurrentNotebook(NotebookInfo notebookInfo)
        {
            CurrentNotebookInfo = notebookInfo;

            App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(CurrentNotebookInfo.Notebook);
            App.MainWindowViewModel.CurrentNotebookName = CurrentNotebookInfo.Notebook.Name;
            App.MainWindowViewModel.CurrentUser = CurrentNotebookInfo.Notebook.Owner;
            App.MainWindowViewModel.IsAuthoring = CurrentNotebookInfo.Notebook.OwnerID == Person.Author.ID;
            App.MainWindowViewModel.IsBackStageVisible = false;

            CurrentNotebookChanged.SafeInvoke(this);
        }

        #region Archival Methods

        public void ArchiveCache(CacheInfo cacheInfo)
        {
            //if (!Directory.Exists(cacheInfo.CacheFolderPath))
            //{
            //    return;
            //}

            //var archiveDirectory = Path.Combine(ArchivedCachesFolderPath, "ArchivedCaches");
            //var now = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            //var newCacheDirectory = Path.Combine(archiveDirectory, "Cache-" + now);
            //if (!Directory.Exists(archiveDirectory))
            //{
            //    Directory.CreateDirectory(archiveDirectory);
            //}
            //Directory.Move(cacheInfo.CacheFolderPath, newCacheDirectory);
        }

        public void MigrateCaches()
        {
            var cachesToMove = GetCachesInFolder(DesktopFolderPath);
            if (!cachesToMove.Any())
            {
                return;
            }

            var result = MessageBox.Show("Do you want to migrate caches to new location?", "Migrate Caches", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            
            Parallel.ForEach(cachesToMove,
                             cacheInfo =>
                             {
                                 var cacheFolderName = new DirectoryInfo(cacheInfo.CacheFolderPath).Name;
                                 var newCacheFolderPath = Path.Combine(CurrentCachesFolderPath, cacheFolderName);
                                 if (Directory.Exists(newCacheFolderPath))
                                 {
                                     return;
                                 }
                                 Directory.Move(cacheInfo.CacheFolderPath, newCacheFolderPath);
                             });
        }

        #endregion // Archival Methods

        #region Researcher Methods

        public void GenerateSubmissionsFromModifiedStudentPages()
        {
            foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.Notebook != null && n.Notebook.Owner.IsStudent))
            {
                foreach (var page in notebookInfo.Notebook.Pages.Where(p => p.VersionIndex == 0))
                {
                    if (page.Submissions.Any())
                    {
                        var mostRecentSubmission = page.Submissions.Last();
                        if (page.InkStrokes.StrokesWeight() != mostRecentSubmission.InkStrokes.StrokesWeight() ||
                            page.PageObjects.Count != mostRecentSubmission.PageObjects.Count)
                        {
                            page.TrimPage();
                            var submission = page.NextVersionCopy();
                            page.Submissions.Add(submission);
                        }
                    }
                    else
                    {
                        if (page.InkStrokes.Any(s => s.GetStrokeOwnerID() == page.OwnerID) ||
                            page.PageObjects.Any(p => p.OwnerID == page.OwnerID))
                        {
                            page.TrimPage();
                            var submission = page.NextVersionCopy();
                            page.Submissions.Add(submission);
                        }
                    }
                }
            }

            foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.Notebook != null && !n.Notebook.Owner.IsStudent))
            {
                foreach (var page in notebookInfo.Notebook.Pages.Where(p => p.VersionIndex == 0))
                {
                    var pageViewModels = page.GetAllViewModels();
                    foreach (var pageViewModel in pageViewModels)
                    {
                        var pageVM = pageViewModel as ACLPPageBaseViewModel;
                        if (pageVM == null)
                        {
                            continue;
                        }
                        pageVM.UpdateSubmissionCount();
                    }
                }
            }
        }

        public void PrintUsedHistoryItems()
        {
            var historyItemTypes = new List<Type>();
            foreach (var notebookInfo in LoadedNotebooksInfo.Where(n => n.Notebook != null && n.Notebook.Owner.IsStudent))
            {
                foreach (var page in notebookInfo.Notebook.Pages.Where(p => p.VersionIndex == 0))
                {
                    historyItemTypes.AddRange(page.History.CompleteOrderedHistoryItems.Select(h => h.GetType()));
                    foreach (var submission in page.Submissions)
                    {
                        historyItemTypes.AddRange(submission.History.CompleteOrderedHistoryItems.Select(h => h.GetType()));
                    }
                }
            }

            historyItemTypes = historyItemTypes.Distinct().ToList();
            historyItemTypes.ForEach(Console.WriteLine);
        }

        #endregion // Researcher Methods

        #region ClassPeriod Methods

        public static List<ClassPeriodInfo> GetClassPeriodsInFolder(CacheInfo cache)
        {
            if (!Directory.Exists(cache.ClassesFolderPath))
            {
                return new List<ClassPeriodInfo>();
            }

            var directoryInfo = new DirectoryInfo(cache.ClassesFolderPath);
            return directoryInfo.GetFiles().Select(fileInfo => new ClassPeriodInfo(cache, fileInfo.FullName)).Where(c => c != null).OrderBy(c => c.StartTime).ToList();
        }

        #endregion //ClassPeriod Methods

        #region Old ClassPeriod Methods

        //public void StartSoonestClassPeriod(string localCacheFolderPath)
        //{
        //    var classesFolderPath = Path.Combine(localCacheFolderPath, "Classes");
        //    var classPeriodFilePaths = Directory.GetFiles(classesFolderPath);
        //    ClassPeriodNameComposite closestClassPeriodNameComposite = null;
        //    var closestTimeSpan = TimeSpan.MaxValue;
        //    var now = DateTime.Now;
        //    foreach (var classPeriodFilePath in classPeriodFilePaths)
        //    {
        //        var classPeriodNameComposite = ClassPeriodNameComposite.ParseFilePath(classPeriodFilePath);
        //        if (classPeriodNameComposite == null)
        //        {
        //            continue;
        //        }
        //        var time = classPeriodNameComposite.StartTime;
        //        var timeParts = time.Split('.');
        //        var year = Int32.Parse(timeParts[0]);
        //        var month = Int32.Parse(timeParts[1]);
        //        var day = Int32.Parse(timeParts[2]);
        //        var hour = Int32.Parse(timeParts[3]);
        //        var minute = Int32.Parse(timeParts[4]);
        //        var dateTime = new DateTime(year, month, day, hour, minute, 0);

        //        var timeSpan = now - dateTime;
        //        var duration = timeSpan.Duration();
        //        var closestTimeSpanDuration = closestTimeSpan.Duration();
        //        if (duration >= closestTimeSpanDuration)
        //        {
        //            continue;
        //        }
        //        closestTimeSpan = timeSpan;
        //        closestClassPeriodNameComposite = classPeriodNameComposite;
        //    }

        //    if (closestClassPeriodNameComposite == null)
        //    {
        //        MessageBox.Show("ERROR: Could not find ClassPeriod .xml file.");
        //        return;
        //    }

        //    StartLocalClassPeriod(closestClassPeriodNameComposite, localCacheFolderPath);
        //}

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

        #region Testing

        public void TestJSON()
        {
            var currentPage = CurrentPage;

            var nameComposite = PageNameComposite.ParsePage(currentPage);
            var filePath = Path.Combine(DesktopFolderPath, nameComposite.ToFileName() + ".json");

            currentPage.ToJSON(filePath);

            var loadedPage = CLPPage.LoadFromJSON(filePath);
            if (loadedPage == null)
            {
                return;
            }

            var secondFilePath = Path.Combine(DesktopFolderPath, nameComposite.ToFileName() + " - Copy.json");
            loadedPage.ToJSON(secondFilePath);
        }

        #endregion // Testing

        #endregion //Methods

        //---------------------------------------------------------------------------------------------------------------------------------------------------

        #region Constants

        //private const string DEFAULT_CLP_DATA_FOLDER_NAME = "CLPData";
        //private const string DEFAULT_CACHE_FOLDER_NAME = "Cache";
        private const string DEFAULT_TEMP_CACHE_FOLDER_NAME = "TempCache";
        private const string DEFAULT_CONFIG_FOLDER_NAME = "Config";  // Config Service?
        private const string DEFAULT_ARCHIVE_FOLDER_NAME = "Archive";
        private const string DEFAULT_LOGS_FOLDER_NAME = "Logs";

        #endregion // Constants

        #region Properties

        #region Special Folder Paths

        private static string WindowsDriveFolderPath => Path.GetPathRoot(Environment.SystemDirectory);

        private static string DesktopFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static string CLPProgramFolderPath => typeof(MainWindowView).Assembly.GetDirectory();

        #endregion // Special Folder Paths

        #region Default Folder Paths

        private string DefaultCLPDataFolderPath
        {
            get
            {
                var folderPath = Path.Combine(DesktopFolderPath, DEFAULT_CLP_DATA_FOLDER_NAME);  // TODO: Change to WindowsDriveFolderPath
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        public string DefaultCacheFolderPath
        {
            get
            {
                var folderPath = Path.Combine(DefaultCLPDataFolderPath, DEFAULT_CACHE_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        #endregion // Default Folder Paths

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

        #region Notebook Properties

        //FilePathPair
        //AvailableNotebookSets   
        //LoadedNotebookSets      //Differentiate between a .clp file and the individual notebooks within?
        //CurrentNotebookSet



        //CurrentMultiDisplay

        #endregion // Notebook Properties

        #region Page Properties

        //CurrentNotebookPage
        //CurrentSubmissionPage
        //CurrentSelectedPage

        #endregion // Page Properties

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> CurrentNotebookChanged;
        public event EventHandler<EventArgs> CurrentPageChanged;

        #endregion // Events

        #region Static Methods

        public static List<FileInfo> GetNotebookSetsInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return new List<FileInfo>();
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            return directoryInfo.GetFiles("*.clp").ToList();
        }

        public static ClassRoster LoadNotebookSet(FileInfo fileInfo)
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

            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                if (!zip.ContainsEntry("classRoster.json"))
                {
                    return null;
                }

                var rosterEntry = zip.Entries.First(e => e.FileName == "classRoster.json");

                using (var memoryStream = new MemoryStream())
                {
                    rosterEntry.Extract(memoryStream);

                    var jsonString = Encoding.ASCII.GetString(memoryStream.ToArray());
                    return AEntityBase.FromJsonString<ClassRoster>(jsonString);
                }
            }
        }

        public void CreateTestNotebookSet()
        {
            var cacheFolderPath = DefaultCacheFolderPath;
            var fileName = "Test Notebook.clp";
            var fullFilePath = Path.Combine(cacheFolderPath, fileName);

            var classRoster = new ClassRoster
                              {
                                  SubjectName = "Math",
                                  GradeLevel = "3",
                                  SchoolName = "Northeastern",
                                  City = "Waltham",
                                  State = "Massachusetts"
                              };
            var teacher = new Person
                          {
                              FirstName = "Ann",
                              Nickname = "Mrs.",
                              LastName = "McNamara",
                              IsStudent = false
                          };

            var student1 = new Person
                           {
                               FirstName = "Steve",
                               LastName = "Chapman",
                               IsStudent = true
                           };

            var student2 = new Person
                           {
                               FirstName = "Lily",
                               LastName = "Ko",
                               IsStudent = true
                           };

            var student3 = new Person
                           {
                               FirstName = "Kimberle",
                               LastName = "Koile",
                               IsStudent = true
                           };

            classRoster.ListOfTeachers.Add(teacher);

            classRoster.ListOfStudents.Add(student1);
            classRoster.ListOfStudents.Add(student2);
            classRoster.ListOfStudents.Add(student3);

            var rosterString = classRoster.ToJsonString();

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;

                zip.AddDirectoryByName("sessions");
                zip.AddDirectoryByName("images");
                zip.AddDirectoryByName("notebooks");

                zip.AddEntry("classRoster.json", rosterString);

                zip.Save(fullFilePath);
            }

            var fileInfos = GetNotebookSetsInFolder(cacheFolderPath);
            var rosterFileInfo = fileInfos.FirstOrDefault();


            var loadedRoster = LoadNotebookSet(rosterFileInfo);

            foreach (var student in loadedRoster.ListOfStudents)
            {
                Console.WriteLine(student.DisplayName);
            }
        }

        #endregion // Static Methods
    }
}